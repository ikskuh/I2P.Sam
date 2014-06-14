using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace I2P.Sam
{
	/// <summary>
	/// Represents a asymmetric key pair.
	/// </summary>
	public sealed class KeyPair
	{
		/// <summary>
		/// Creates a new asymmetric key pair.
		/// </summary>
		/// <param name="pub">Public key.</param>
		/// <param name="priv">Private key.</param>
		public KeyPair(string pub, string priv)
		{
			this.Public = pub;
			this.Private = priv;
		}

		public string Public { get; private set; }

		public string Private { get; private set; }
	}
}
