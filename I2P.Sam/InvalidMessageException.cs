using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace I2P.Sam
{
	[Serializable]
	public sealed class InvalidMessageException : Exception
	{
		public InvalidMessageException()
		{
		}

		public InvalidMessageException(string message)
			: base(message)
		{
		}
	}
}
