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
            var buttonPressed = false;
            var buttonPressedStart = DateTime.MinValue;

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
                {
                    mpc.PlayPauseToggle();
                    buttonPressed = true;
                    buttonPressedStart = DateTime.Now;
                }

                if (off && buttonPressed)
                {
                    if (buttonPressedStart.AddSeconds(1.5) < DateTime.Now)
                    {
                        // on press for 1.5 seconds, toggle port to enable/disable speaker (in favor of headphone out)
                        gpio?.Toggle(p40);
                        mpc.PlayPauseToggle();
                    }
                }

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
                    mpc.VolumeUp();
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

            mpc.ResetVolume();

            var blinkThread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                while (true)
                {
                    // blink power led when ready
                    gpio.Toggle(p03);
                    System.Threading.Thread.Sleep(1000);
                }
            }));
            blinkThread.Start();


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
