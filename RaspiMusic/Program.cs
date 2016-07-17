using Raspberry.IO.GeneralPurpose;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspiMusic
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            //var mpc = new MpcClient("192.168.1.123");
            var mpc = new MpcClient();

            GpioConnection gpio = null;

            var p11First = true;
            var p12First = true;
            var p13First = true;
            var p15First = true;
            var p16First = true;
            var volumeUpPressed = false;
            var volumeUpPressedStart = DateTime.MinValue;

            var p40 = ConnectorPin.P1Pin40.Output().Disable();

            var p11 = ConnectorPin.P1Pin11.Input().PullUp();
            p11.OnStatusChanged((off) =>
            {
                if (p11First)
                {
                    p11First = false;
                    return;
                }
                if (!off)
                    mpc.PlayPauseToggle();
            });
            var p12 = ConnectorPin.P1Pin12.Input().PullUp();
            p12.OnStatusChanged((off) =>
            {
                if (p12First)
                {
                    p12First = false;
                    return;
                }
                if (!off)
                {
                    mpc.VolumeUp();
                    volumeUpPressed = true;
                    volumeUpPressedStart = DateTime.Now;
                }

                Console.WriteLine(volumeUpPressedStart);

                if (off && volumeUpPressed)
                {
                    if (volumeUpPressedStart.AddSeconds(2) < DateTime.Now)
                    {
                        // on volume up press for 2 seconds, toggle port to enable/disable speaker (in favor of headphone out)
                        gpio?.Toggle(p40);
                    }
                }
            });

            var p13 = ConnectorPin.P1Pin13.Input().PullUp();
            p13.OnStatusChanged((off) =>
            {
                if (p13First)
                {
                    p13First = false;
                    return;
                }
                if (!off)
                    mpc.VolumeDown();
            });
            var p15 = ConnectorPin.P1Pin15.Input().PullUp();
            p15.OnStatusChanged((off) =>
            {
                if (p15First)
                {
                    p15First = false;
                    return;
                }
                if (!off)
                    mpc.Next();
            });
            var p16 = ConnectorPin.P1Pin16.Input().PullUp();
            p16.OnStatusChanged((off) =>
            {
                if (p16First)
                {
                    p16First = false;
                    return;
                }
                if (!off)
                    mpc.Previous();
            });

            var p03 = ConnectorPin.P1Pin03.Output();

            gpio = new GpioConnection(p03, p11, p12, p13, p15, p16, p40);

            // blink power led on startup
            for (int i = 0; i < 7; i++)
            {
                gpio.Toggle(p03);
                System.Threading.Thread.Sleep(300);
            }

            mpc.ResetVolume();

            while (true)
            {
                Console.WriteLine("Waiting for playlist name...");
                var inp = Console.ReadLine();

                mpc.Stop();
                mpc.ClearPlaylist();
                mpc.LoadPlaylist(inp);
                mpc.Play();
            }
        }
    }
}
