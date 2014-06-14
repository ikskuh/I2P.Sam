using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace I2P.Sam
{
	/// <summary>
	/// Provides extension methods to convert from or to PunyCode.
	/// </summary>
	public static class PunyCode
	{
		static System.Globalization.IdnMapping mapping = new System.Globalization.IdnMapping();

		/// <summary>
		/// Converts a string to Punycode.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static string ToASCII(this string p)
		{
			return mapping.GetAscii(p);
		}

		/// <summary>
		/// Converts a string from Punycode.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static string ToUnicode(this string p)
		{
			return mapping.GetUnicode(p);
		}
	}
}
