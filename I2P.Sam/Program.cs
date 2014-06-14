using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I2P.Sam
{
	class Program
	{
		static void Main(string[] args)
		{
			// Create new SAM bridge interface
			SamBridge bridge = new SamBridge();
			bridge.Connect();

			// Naming lookup supports punycode
			var pubKey = bridge.LookUp("😺😺😺.i2p");

			// Generate a key pair.
			var keypair = bridge.GenerateKeyPair();
		}
	}
}
