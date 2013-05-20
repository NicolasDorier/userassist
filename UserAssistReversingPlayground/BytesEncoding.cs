using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAssistReversingPlayground
{
	public class BytesToStringEncoding : Encoding
	{
		public override int GetByteCount(char[] chars, int index, int count)
		{
			var str = new String(chars);
			str = str.Replace(" ","");
			return str.Length / 2;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			var str = new String(chars);
			str = str.Replace(" ", "");
			for(int i = charIndex ; i < charIndex + str.Length ; i += 2)
			{
				var charOffset = i - charIndex;
				var byteString = new String(new char[] { str[i], str[i + 1] });
				var b = byte.Parse(byteString.TrimEnd(), NumberStyles.HexNumber);
				bytes[byteIndex + charOffset / 2] = b;
			}
			return str.Length / 2;
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return count * 2 + count - 1;
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			for(int i = byteIndex ; i < byteIndex + byteCount ; i++)
			{
				bool isLast = i + 1 >= byteIndex + byteCount;

				int b = bytes[i];
				int byteOffset = i - byteIndex;
				int charOffset = byteOffset * 3;
				var byteString = b.ToString("X2");
				chars[charIndex + charOffset] = byteString[0];
				chars[charIndex + charOffset + 1] = byteString[1];
				if(!isLast)
					chars[charIndex + charOffset + 2] = ' ';
			}
			return byteCount * 3 - 1;
		}

		public override int GetMaxByteCount(int charCount)
		{

			return charCount / 2;
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return byteCount * 2;
		}
	}
}
