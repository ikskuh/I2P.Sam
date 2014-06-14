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
			SamBridge bridge = new SamBridge();

			bridge.Connect();
		}
	}
}
