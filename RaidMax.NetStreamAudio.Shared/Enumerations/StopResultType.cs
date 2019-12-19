namespace RaidMax.NetStreamAudio.Shared.Enumerations
{
    /// <summary>
    /// Specifies the types of stop results
    /// </summary>
    public enum StopResultType
    {
        /// <summary>
        /// The stop completed as expected
        /// </summary>
        Expected = 0x10,

        /// <summary>
        /// The stop encountered an issue or was not expected
        /// </summary>
        Unexpected = 0x20
    }
}
