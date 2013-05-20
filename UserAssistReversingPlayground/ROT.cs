using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAssistReversingPlayground
{
	public class ROT13
	{
		
		public static string Toggle(string input)
		{
			char[] array = input.ToCharArray();
			for(int i = 0 ; i < array.Length ; i++)
			{
				int number = (int)array[i];

				if(number >= 'a' && number <= 'z')
				{
					if(number > 'm')
					{
						number -= 13;
					}
					else
					{
						number += 13;
					}
				}
				else if(number >= 'A' && number <= 'Z')
				{
					if(number > 'M')
					{
						number -= 13;
					}
					else
					{
						number += 13;
					}
				}
				array[i] = (char)number;
			}
			return new string(array);
		}
	}
}
