using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Defines the capabilities of an audio server
    /// </summary>
    public interface IAudioServer : IStartStop
    {
        /// <summary>
        /// Processes a command sent to the audio server
        /// </summary>
        /// <param name="audioCommand"></param>
        void ProcessCommand(IAudioCommand audioCommand);

        /// <summary>
        /// Serves the audio data to any listening clients
        /// </summary>
        /// <param name="audioEventArgs">audio data</param>
        /// <returns>Task to run that serves data</returns>
        Task Serve(IAudioCaptureEventArgs audioEventArgs);
    }
}
