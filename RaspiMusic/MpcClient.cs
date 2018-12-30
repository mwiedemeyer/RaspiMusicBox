using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RaspiMusic
{
    public class MpcClient
    {
        private readonly string _ip;

        public MpcClient(string ip = "127.0.0.1")
        {
            _ip = ip;
        }

        public string ExecuteCommand(string command)
        {
            var result = string.Empty;
            using (var tcp = new TcpClient(_ip, 6600))
            using (var stream = tcp.GetStream())
            {
                var data = new byte[256];
                var bytes = stream.Read(data, 0, data.Length);
                while (bytes > 0)
                {
                    var responseData = Encoding.ASCII.GetString(data, 0, bytes);
                    result += responseData;
                    Console.WriteLine("Received: {0}", responseData);
                    bytes = 0;
                    if (!stream.DataAvailable)
                        break;
                    bytes = stream.Read(data, 0, data.Length);
                }

                Console.WriteLine($"Sending: {command}");

                var buffer = Encoding.ASCII.GetBytes($"{command}\r\n");
                stream.Write(buffer, 0, buffer.Length);

                data = new byte[256];
                bytes = stream.Read(data, 0, data.Length);
                while (bytes > 0)
                {
                    var responseData = Encoding.ASCII.GetString(data, 0, bytes);
                    result += responseData;
                    Console.WriteLine("Received: {0}", responseData);
                    bytes = 0;
                    if (!stream.DataAvailable)
                        break;
                    bytes = stream.Read(data, 0, data.Length);
                }
            }
            return result;
        }

        public void ResetVolume()
        {
            ExecuteCommand("volume -100");
            ExecuteCommand("volume +" + ConfigurationManager.AppSettings["Volume:StartWithPercent"]);
        }

        public void PlayPauseToggle()
        {
            ExecuteCommand("pause");
        }

        public void VolumeUp()
        {
            ExecuteCommand("volume +" + ConfigurationManager.AppSettings["Volume:PlusMinus"]);
        }

        public void VolumeDown()
        {
            ExecuteCommand("volume -" + ConfigurationManager.AppSettings["Volume:PlusMinus"]);
        }

        public void Stop()
        {
            ExecuteCommand("stop");
        }

        public void Play()
        {
            ExecuteCommand("play");
        }

        public void LoadPlaylist(string name)
        {
            ExecuteCommand($"load {name}");
        }

        public void ClearPlaylist()
        {
            ExecuteCommand("clear");
        }

        public void Next()
        {
            ExecuteCommand("next");
        }

        public void Previous()
        {
            ExecuteCommand("previous");
        }
    }
}
