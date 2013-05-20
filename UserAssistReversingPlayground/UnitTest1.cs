using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using DiffPlex;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Threading;
using CsvHelper;

namespace UserAssistReversingPlayground
{
	class RunSnap
	{
		public DateTime LastChanged
		{
			get;
			set;
		}
		public long Duration
		{
			get;
			set;
		}
		public long StartDate
		{
			get;
			set;
		}

	}

	class ValueLetter
	{
		public int Position
		{
			get;
			set;
		}
		public string Value1
		{
			get;
			set;
		}
		public string Value2
		{
			get;
			set;
		}
		public string IsDiff
		{
			get;
			set;
		}
	}
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void Quick()
		{
			int a = 0xF85D - 0x7257;
		}
		[TestMethod]
		public void CompareDiffSnapshot()
		{
			for(int i = 0 ; i < 2 ; i++)
			{
				var snap1 = i + "-snap1.csv";
				var snap2 = i + "-snap2.csv";
				var diff = i + "-diff.csv";
				UserAssist assist = new UserAssist();
				assist.Snapshot(snap1).Dispose();
				MessageBox.Show("Run manually");
				assist.Snapshot(snap2).Dispose();
				Diff(diff, snap1, snap2);
			}
			StartWindiff("0-diff.csv", "1-diff.csv");
		}
		[TestMethod]
		public void CompareSnapshot()
		{
			UserAssist assist = new UserAssist();
			assist.Snapshot("snap1.csv").Dispose();
			MessageBox.Show("Run manually");
			assist.Snapshot("snap2.csv").Dispose();
			StartWindiff("snap1.csv", "snap2.csv");
			Process.Start("snap1.csv");
		}

		[TestMethod]
		public void CompareChangingValues()
		{
			UserAssist assist = new UserAssist();
			using(CsvWriter writer = new CsvWriter(new StreamWriter(File.Open("ChangingValues.csv", FileMode.Create))))
			{
				writer.WriteHeader<ValueLetter>();
				var snap1Values =
					assist.Snapshot("snap1.csv")
					.FindProgram("cmd.exe")
					.GetValue();

				WaitOpenThenKill("cmd.exe");

				var snap2Values = assist.Snapshot("snap2.csv")
										.FindProgram("cmd.exe")
										.GetValue();
				for(int i = 0 ; i < snap1Values.Length ; i++)
				{
					writer.WriteRecord(new ValueLetter()
					{
						Position = i,
						Value1 = snap1Values[i].ToString("X2"),
						Value2 = snap2Values[i].ToString("X2"),
						IsDiff = (snap1Values[i] != snap2Values[i]) ? "x" : ""
					});
				}
			}
			Process.Start("ChangingValues.csv");
		}

		private void WaitOpenThenKill(string prog)
		{
			var p = WaitOpen(prog);
			WaitMaxSeconds(2);
			p.Kill();
		}

		[TestMethod]
		public void CompareUnknownDataWithTime()
		{
			UserAssist assist = new UserAssist();
			using(CsvWriter writer = new CsvWriter(new StreamWriter(File.Open("Experiment.csv", FileMode.Create))))
			{
				writer.WriteHeader<RunSnap>();
				var experimentStart = DateTime.UtcNow.Ticks;
				for(int i = 0 ; i < 5 ; i++)
				{
					RunSnap snap = new RunSnap();
					var process = WaitOpen("cmd.exe");
					snap.StartDate = DateTime.UtcNow.Ticks - experimentStart;
					WaitMaxSeconds(10);
					process.Kill();
					snap.Duration = (DateTime.UtcNow.Ticks - experimentStart) - snap.StartDate;
					snap.LastChanged =
					  assist.Snapshot()
						  .FindProgram("cmd.exe")
						  .ParseValue()
						  .LastChanged;
					writer.WriteRecord(snap);
				}
			}
		}

		Random rand = new Random();
		private void WaitMaxSeconds(int s)
		{
			Thread.Sleep((int)(rand.NextDouble() * s * 1000));
		}

		private Process WaitOpen(string processName)
		{
			var existingIds = Process.GetProcesses()
				.Where(p => p.ProcessName + ".exe" == processName)
				.Select(p => p.Id)
				.ToList();

			while(true)
			{
				var newProc =
					Process.GetProcesses()
					.Where(p => p.ProcessName + ".exe" == processName)
					.FirstOrDefault(p => existingIds.All(e => e != p.Id));


				if(newProc != null)
				{
					return newProc;
				}
				Thread.Sleep(1000);
			}

		}


		[TestMethod]
		public void TestROT()
		{
			Assert.AreEqual("pzq.rkr", ROT13.Toggle("cmd.exe"));
			Assert.AreEqual("cmd.exe", ROT13.Toggle("pzq.rkr"));
		}

		private void Diff(string diff, string snap1, string snap2)
		{
			new Differ()
					.CreateLineDiffs(File.ReadAllText(snap1), File.ReadAllText(snap2), false)
					.WriteTo(diff);
		}


		[TestMethod]
		public void EncodingByteTest()
		{
			var enc = new BytesToStringEncoding();
			var val = enc.GetBytes("0A 0B 0C");
			CollectionAssert.AreEquivalent(new byte[] { 0x0A, 0x0B, 0x0C }, val);

			var valStr = enc.GetString(val);
			Assert.AreEqual("0A 0B 0C", valStr);
		}

		[TestMethod]
		public void CreateDiff()
		{
			UserAssist assist = new UserAssist();
			new Differ()
				.CreateLineDiffs(File.ReadAllText("snap1.csv"), File.ReadAllText("snap2.csv"), false)
				.WriteTo("diff1.csv");
		}

		private void StartWindiff(string file1, string file2)
		{
			Process.Start(@"C:\Program Files\Microsoft SDKs\Windows\v7.1\Bin\x64\WinDiff.exe", file1 + "  " + file2);
		}
	}
}
