namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// High level transmission of binary data
    /// </summary>
    public interface ITransmission
    {
        /// <summary>
        /// Generates the payload for the transmission
        /// </summary>
        /// <returns></returns>
        byte[] GeneratePayload();
    }
}
