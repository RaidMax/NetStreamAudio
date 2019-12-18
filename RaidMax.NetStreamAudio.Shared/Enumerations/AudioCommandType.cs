namespace RaidMax.NetStreamAudio.Shared.Enumerations
{
    /// <summary>
    /// Specifies the type of commands for audio server/client connections
    /// </summary>
    public enum AudioCommandType
    {
        /// <summary>
        /// Attaches the client to the server (hello)
        /// </summary>
        Attach = 0x10,

        /// <summary>
        /// Detaches the client from the server (goodbye)
        /// </summary>
        Detach = 0x20,

        /// <summary>
        /// todo:
        /// </summary>
        Start = 0x30,

        /// <summary>
        /// todo:
        /// </summary>
        Stop = 0x40,

        /// <summary>
        /// Dummy command to keep the socket open if nothing is playing
        /// </summary>
        KeepAlive = 0x50,
    }
}
