using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAssistReversingPlayground
{
	public class BigBinaryReader : BinaryReader
	{
		public BigBinaryReader(Stream stream)
			: base(stream)
		{

		}
		public override int ReadInt32()
		{
			var a = base.ReadBytes(4);
			Array.Reverse(a);
			return BitConverter.ToInt32(a, 0);
		}
		public override Int16 ReadInt16()
		{
			var a = base.ReadBytes(2);
			Array.Reverse(a);
			return BitConverter.ToInt16(a, 0);
		}
		public override Int64 ReadInt64()
		{
			var a = base.ReadBytes(8);
			Array.Reverse(a);
			return BitConverter.ToInt64(a, 0);
		}
		//public override UInt64 ReadUInt64()
		//{
		//	var a = base.ReadBytes(8);
		//	Array.Reverse(a);
		//	return BitConverter.ToUInt64(a, 0);
		//}
		public override UInt32 ReadUInt32()
		{
			var a = base.ReadBytes(4);
			Array.Reverse(a);
			return BitConverter.ToUInt32(a, 0);
		}


	}
}
