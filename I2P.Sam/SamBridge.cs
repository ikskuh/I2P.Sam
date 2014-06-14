using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace I2P.Sam
{
	/// <summary>
	/// Basic connection to all SAM services.
	/// </summary>
	public sealed class SamBridge
	{
		private readonly object @lock = new object();
		private readonly string hostName;
		private readonly int portNumber;

		private TcpClient bridgeClient = null;
		private NetworkStream stream = null;
		private StreamWriter writer = null;
		private StreamReader reader = null;

		/// <summary>
		/// Instantiates a new SAM bridge connection with the default settings.
		/// </summary>
		public SamBridge()
			: this("127.0.0.1", 7656)
		{

		}

		/// <summary>
		/// Instantiates a new SAM bridge connection with custom host and port settings.
		/// </summary>
		/// <param name="hostName">Hostname or IP where the SAM bridge is hostet.</param>
		/// <param name="portNumber">Port number of the SAM bridge.</param>
		public SamBridge(string hostName, int portNumber)
		{
			this.hostName = hostName;
			this.portNumber = portNumber;
		}

		/// <summary>
		/// Connects to the SAM bridge and performs the handshake.
		/// </summary>
		public void Connect()
		{
			if (this.bridgeClient != null)
			{
				throw new InvalidOperationException("Cannot connect to an already connected SAM bridge. Disconnect before reconnecting.");
			}

			this.bridgeClient = new TcpClient(hostName, portNumber);

			this.stream = this.bridgeClient.GetStream();

			this.writer = new StreamWriter(this.stream, Encoding.ASCII, 512, true);
			this.writer.AutoFlush = true;
			this.writer.NewLine = "\n";

			this.reader = new StreamReader(this.stream, Encoding.ASCII, false, 512, true);

			// Handshake

			SamMessage msg = new SamMessage("HELLO", "VERSION");
			msg["MIN"] = "3.0";
			msg["MAX"] = "3.0";
			this.writer.WriteLine(msg);

			var response = this.ReadMessage(50);
			if(!response.Validate("HELLO", "REPLY", new[] { "RESULT", "OK" }, new[] { "VERSION", "3.0" }))
			{
				throw new InvalidDataException("Handshake failed.");
			}
		}

		/// <summary>
		/// Reads a message from the bridge.
		/// </summary>
		/// <param name="timeout">The timeout in milliseconds.</param>
		/// <returns>Parsed sam message.</returns>
		public SamMessage ReadMessage(int timeout)
		{
			string line = this.ReadLine(timeout);
			return new SamMessage(line);
		}

		/// <summary>
		/// Reads a string from the SAM bridge.
		/// </summary>
		/// <param name="timeOut">The timeout in milliseconds.</param>
		/// <returns></returns>
		private string ReadLine(int timeout)
		{
			Stopwatch watch = new Stopwatch();
			watch.Start();
			do
			{
				if (this.stream.DataAvailable)
					return this.reader.ReadLine();
				Thread.Sleep(0);
			} while (watch.ElapsedMilliseconds <= timeout);
			throw new TimeoutException();
		}

		/// <summary>
		/// Gets a value that indicates whether the bridge is connected or not. 
		/// </summary>
		public bool IsConnected { get { return this.bridgeClient != null; } }
	}
}
