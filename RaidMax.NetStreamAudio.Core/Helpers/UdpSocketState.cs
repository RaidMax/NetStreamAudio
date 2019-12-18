using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RaidMax.NetStreamAudio.Core.Helpers
{
    /// <summary>
    /// Defines the necessary properties to maintain track of a UDP Socket
    /// </summary>
    public class UdpSocketState
    {
        ~UdpSocketState()
        {
            SendWaiter.Dispose();
            ReceiveWaiter.Dispose();
        }

        /// <summary>
        /// Specifies the largest buffer size we want to transmit or receive
        /// </summary>
        public const int MAX_BUFFER_LENGTH = 32500;

        /// <summary>
        /// Source socket of the state (where is data being transmitted through)
        /// </summary>
        public Socket StateSocket { get; set; }

        /// <summary>
        /// Client used for the player (abstracted UDP socket)
        /// </summary>
        public UdpClient UdpClient { get; set; }

        /// <summary>
        /// Destination of data (or source of remote data)
        /// </summary>
        public EndPoint RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        /// <summary>
        /// Buffer used when transmitting or receiving data
        /// </summary>
        public byte[] Buffer { get; set; } = new byte[MAX_BUFFER_LENGTH];

        /// <summary>
        /// Semaphore that unlocks once the send operation is complete
        /// </summary>
        public SemaphoreSlim SendWaiter { get; } = new SemaphoreSlim(0, 1);

        /// <summary>
        /// Semaphore that unlocks once the receive operation is complete
        /// </summary>
        public SemaphoreSlim ReceiveWaiter { get; } = new SemaphoreSlim(0, 1);

        /// <summary>
        /// Specifies the last time that a message was received on the remote endpoint
        /// </summary>
        public DateTime LastMessageTime { get; set; }
    }
}
