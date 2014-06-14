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
	public sealed partial class SamBridge : IDisposable
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
		/// <param name="bridgePortNumber">Port number of the SAM bridge.</param>
		public SamBridge(string hostName, int bridgePortNumber)
		{
			this.hostName = hostName;
			this.portNumber = bridgePortNumber;
		}

		~SamBridge()
		{
			this.Dispose();
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

			SamMessage response;
			SamMessage msg = new SamMessage("HELLO", "VERSION");
			msg["MIN"] = "3.0";
			msg["MAX"] = "3.0";

			response = ReadWrite(msg, 50);

			if (!response.Validate("HELLO", "REPLY", new[] { "RESULT", "OK" }, new[] { "VERSION", "3.0" }))
			{
				throw new InvalidDataException("Handshake failed.");
			}
		}

		private SamMessage ReadWrite(SamMessage msg, int timeout)
		{
			SamMessage response;
			lock (@lock)
			{
				this.writer.WriteLine(msg);
				response = this.ReadMessage(timeout);
			}
			return response;
		}

		/// <summary>
		/// Looks up a name in the I2P address book.
		/// </summary>
		/// <param name="name">Name of the address.</param>
		/// <returns>Base64 public key.</returns>
		public string LookUp(string name)
		{
			SamMessage request = new SamMessage("NAMING", "LOOKUP");
			request["NAME"] = name.ToASCII();

			var response = this.ReadWrite(request, 250);

			if (!response.Validate("NAMING", "REPLY", new[] { "RESULT" }))
			{
				throw new InvalidDataException("Invalid NAMING response.");
			}
			if (!response.Validate("NAMING", "REPLY", new[] { "RESULT", "OK" }))
			{
				switch (response["RESULT"])
				{
					case "KEY_NOT_FOUND":
						return null;
					default:
						throw new InvalidDataException("Invalid NAMING RESULT: " + response["RESULT"]);
				}
			}

			return response["VALUE"];
		}

		public KeyPair GenerateKeyPair()
		{
			var request = new SamMessage("DEST", "GENERATE");
			var response = this.ReadWrite(request, 250);

			if (!response.Validate("DEST", "REPLY", new[] { "PUB" }, new[] { "PRIV" }))
			{
				throw new InvalidDataException("Invalid DEST response.");
			}

			return new KeyPair(response["PUB"], response["PRIV"]);
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

		public void Dispose()
		{
			if (this.writer != null)
			{
				this.writer.Dispose();
				this.writer = null;
			}
			if (this.reader != null)
			{
				this.reader.Dispose();
				this.reader = null;
			}
			if (this.stream != null)
			{
				this.stream.Dispose();
				this.stream = null;
			}
			if (this.bridgeClient != null)
			{
				((IDisposable)this.bridgeClient).Dispose();
				this.bridgeClient = null;
			}

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Gets a value that indicates whether the bridge is connected or not. 
		/// </summary>
		public bool IsConnected { get { return this.bridgeClient != null; } }
	}
}
