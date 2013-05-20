using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAssistReversingPlayground
{
	class RecordData
	{
		public int Count
		{
			get;
			set;
		}
		public DateTime LastChanged
		{
			get;
			set;
		}
	}
	class SnapshotRecord
	{
		public string GUID
		{
			get;
			set;
		}
		public string ValueName
		{
			get;
			set;
		}
		public string Value
		{
			get;
			set;
		}

		public int ValueLength
		{
			get;
			set;
		}
		public RecordData ParseValue()
		{
			var data = new RecordData();
			BigBinaryReader reader = new BigBinaryReader(new MemoryStream(new BytesToStringEncoding().GetBytes(Value)));
			reader.ReadByte();
			data.Count = reader.ReadInt32();
			reader.ReadBytes(55);
			data.LastChanged = DateTime.FromFileTimeUtc((long)reader.ReadUInt64());	
			return data;
		}

		public byte[] GetValue()
		{
			return new BytesToStringEncoding().GetBytes(Value);
		}
	}

	class Snapshot
	{
		private Stream _Stream;
		private long _Initial;

		public Snapshot(Stream stream, long initialPosition)
		{
			this._Stream = stream;
			this._Initial = initialPosition;
		}
		public static Snapshot Take(Stream stream)
		{
			Snapshot snap = new Snapshot(stream, stream.Position);
			snap.Make();
			return snap;
		}

		void WithCsvWriter(Action<CsvWriter> act)
		{
			_Stream.Position = _Initial;
			var writer = new StreamWriter(_Stream);
			var csv = new CsvWriter(writer, Configuration);
			act(csv);
			writer.Flush();
		}

		public static CsvConfiguration Configuration = new CsvHelper.Configuration.CsvConfiguration()
			{
				Delimiter = ";"
			};
		CsvReader CreateCsvReader()
		{
			_Stream.Position = _Initial;
			var reader = new StreamReader(_Stream);
			return new CsvReader(reader, Configuration);
		}
		private void Make()
		{
			WithCsvWriter(writer =>
			{
				var userAssist = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist");
				writer.WriteHeader(typeof(SnapshotRecord));

				foreach(var guidKeyName in userAssist.GetSubKeyNames())
				{
					var countKey = userAssist.OpenSubKey(guidKeyName).OpenSubKey("Count");
					foreach(var valueName in countKey.GetValueNames())
					{
						var value = (byte[])countKey.GetValue(valueName);
						writer.WriteRecord(new SnapshotRecord()
						{
							GUID = guidKeyName,
							ValueName = ROT13.Toggle(valueName),
							Value = ToString(value),
							ValueLength = value.Length
						});
					}
				}
			});
		}

		public IEnumerable<SnapshotRecord> All()
		{
			var reader = CreateCsvReader();
			while(reader.Read())
			{
				yield return reader.GetRecord<SnapshotRecord>();
			}
		}

		public SnapshotRecord FindProgram(string program)
		{
			return All().FirstOrDefault(p => p.ValueName.EndsWith(program));
		}

		private string ToString(byte[] value)
		{
			return new BytesToStringEncoding().GetString(value);
		}

		public void Dispose()
		{
			_Stream.Close();
		}
	}
	class UserAssist
	{

		public Snapshot Snapshot()
		{
			return UserAssistReversingPlayground.Snapshot.Take(new MemoryStream());
		}
		public Snapshot Snapshot(string outputFile)
		{
			var fs = File.Open(outputFile, FileMode.Create);
			return Snapshot(fs);
		}

		private Snapshot Snapshot(Stream stream)
		{
			return UserAssistReversingPlayground.Snapshot.Take(stream);
		}





	}
}
