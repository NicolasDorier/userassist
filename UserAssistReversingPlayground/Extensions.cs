using CsvHelper;
using DiffPlex.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAssistReversingPlayground
{
	public static class Extensions
	{
		public static void WriteTo(this DiffResult result, string file)
		{
			using(var fs = File.Open(file, FileMode.Create))
			{
				var streamWriter = new StreamWriter(fs);
				CsvWriter writer = new CsvWriter(streamWriter);
				writer.WriteHeader(typeof(DiffBlock));
				foreach(var block in result.DiffBlocks)
				{
					writer.WriteRecord(block);
				}
				streamWriter.Flush();
			}
		}
	}
}
