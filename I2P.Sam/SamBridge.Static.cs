using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace I2P.Sam
{
	partial class SamBridge
	{
		/// <summary>
		/// Sends a datagram to the SAM bridge.
		/// </summary>
		/// <param name="sessionID"></param>
		/// <param name="target"></param>
		/// <param name="payload"></param>
		public static void SendDatagram(string sessionID, string target, byte[] payload)
		{
			using (var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
			{
				// TODO: Expose socket address.
				udpSocket.Connect("127.0.0.1", 7655);

				using (var stream = new MemoryStream())
				{
					// Write header
					StreamWriter w = new StreamWriter(stream, Encoding.ASCII, 512, true);
					w.NewLine = "\n";
					w.WriteLine("3.0 {0} {1}", sessionID, target);

					// Write payload
					stream.Write(payload, 0, payload.Length);

					// Send datagram
					udpSocket.Send(stream.ToArray());
				}
			}
		}
	}
}
