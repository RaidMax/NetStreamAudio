using RaidMax.NetStreamAudio.Shared.Enumerations;
using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace RaidMax.NetStreamAudio.Core.Helpers
{
    /// <summary>
    /// Implementation of IAudioCommand
    /// Provides the functionality of commands over UDP
    /// </summary>
    public class UdpAudioCommand : IAudioCommand
    {
        /// <inheritdoc/>
        public AudioCommandType Command { get; set; }

        /// <inheritdoc/>
        public int Future { get; set; }

        /// <inheritdoc/>
        public string Data { get; set; } = string.Empty;

        /// <inheritdoc/>
        public IPEndPoint Source { get; set; }

        /// <summary>
        /// Parses byte array payload into UdpAudioCommand
        /// </summary>
        /// <param name="payload">payload as received by UDP socket</param>
        /// <returns>parsed UDPAudioCommand</returns>
        public static UdpAudioCommand Parse(byte[] payload)
        {
            int currentIndex = 0;

            // get the command
            var commandType = (AudioCommandType)BitConverter.ToInt32(payload.Take(sizeof(int)).ToArray());
            currentIndex += sizeof(int);

            // get the future
            int future = BitConverter.ToInt32(payload.Skip(currentIndex).Take(sizeof(int)).ToArray());
            currentIndex += sizeof(int);

            // get the data
            string data = Encoding.ASCII.GetString(payload.Skip(currentIndex).ToArray()).TrimEnd('\0');

            var audioCommand = new UdpAudioCommand()
            {
                Command = commandType,
                Future = future,
                Data = data
            };

            return audioCommand;
        }

        /// <inheritdoc/>
        public byte[] GeneratePayload()
        {                    // command + future + data
            var bufferSize = sizeof(int) * 2 + Data.Length;
            byte[] buffer = new byte[bufferSize];

            int currentIndex = 0;  

            // copy the command
            var commandBytes = BitConverter.GetBytes((int)Command);
            Buffer.BlockCopy(commandBytes, 0, buffer, currentIndex, commandBytes.Length);
            currentIndex += commandBytes.Length;

            // copy the future :)
            var futureBytes = BitConverter.GetBytes(Future);
            Buffer.BlockCopy(futureBytes, 0, buffer, currentIndex, futureBytes.Length);
            currentIndex += futureBytes.Length;

            // copy the data
            var dataBytes = Encoding.ASCII.GetBytes(Data);
            Buffer.BlockCopy(dataBytes, 0, buffer, currentIndex, dataBytes.Length);

            return buffer;
        }
    }
}
