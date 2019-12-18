using RaidMax.NetStreamAudio.Shared.Enumerations;
using System.Net;

namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Defines the properties of an audio command
    /// </summary>
    public interface IAudioCommand : ICommandTransmission
    {
        /// <summary>
        /// Command type
        /// </summary>
        public AudioCommandType Command { get; set; }

        /// <summary>
        /// Reserved for future use
        /// </summary>
        public int Future { get; set; }

        /// <summary>
        /// Optional data for the command
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Source/Origin of the command
        /// </summary>
        public IPEndPoint Source { get; set; }
    }
}
