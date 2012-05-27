using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace FlushMark
{
	public partial class FlushMarkForm : Form
	{
		public FlushMarkForm()
		{
			InitializeComponent();

			m_driveComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			m_driveComboBox.Items.Clear();

			foreach (DriveInfo di in DriveInfo.GetDrives())
			{
				switch (di.DriveType)
				{
					case DriveType.Fixed:
					case DriveType.Network:
					case DriveType.Ram:
					case DriveType.Removable:
						m_driveComboBox.Items.Add(di.RootDirectory.FullName);
						break;
				}
			}

			m_driveComboBox.SelectedIndex = 0;
			
			SelectDefaultItem(m_testSizeComboBox);
			SelectDefaultItem(m_pageSizeComboBox);
			SelectDefaultItem(m_testModeComboBox);
			SelectDefaultItem(m_flushFrequencyComboBox);
		}

		private void m_goButton_Click(object sender, EventArgs e)
		{
			bool running = false;

			try
			{
				EnableControls(false);
				running = StartBenchmark();
			}
			finally
			{
				if (!running)
					EnableControls(true);
			}
		}

		private void m_clearButton_Click(object sender, EventArgs e)
		{
			m_messageTextBox.Clear();
		}

		private void FlushMarkForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (m_workerThread != null)
			{
				m_workerThread.Abort();
				m_workerThread.Join();
				m_workerThread = null;
			}
		}

		private bool StartBenchmark()
		{
			if (m_workerThread != null)
				throw new ApplicationException("internal error");

			FlushMarkCore.Settings settings;
			if (!GetSettings(out settings))
				return false;

			Thread t = new Thread(delegate()
			{
				try
				{
					RunBenchmark(settings);
				}
				catch (ThreadAbortException)
				{
					AddMessage("ABORTED");
					throw;
				}
				catch (Exception e)
				{
					BeginInvoke(new MethodInvoker(() => OnBenchmarkCompleted(e)));
					return;
				}

				BeginInvoke(new MethodInvoker(() => OnBenchmarkCompleted(null)));
			});

			t.Start();

			m_workerThread = t;
			return true;
		}

		private void RunBenchmark(FlushMarkCore.Settings settings)
		{
			AddMessage("------------------------------");
			AddMessage(string.Format("Path = {0}", settings.filePath));
			AddMessage(string.Format("Test size = {0}", settings.testSize));
			AddMessage(string.Format("Page size = {0}", settings.pageSize));

			string testMode;
			if (settings.testMode == FlushMarkCore.FLUSHMARKCORE_TESTTYPE_RANDOM)
				testMode = "random";
			else if (settings.testMode == FlushMarkCore.FLUSHMARKCORE_TESTTYPE_LINEAR)
				testMode = "linear";
			else
				throw new Exception("Unknown test mode");

			AddMessage(string.Format("Test mode = {0}", testMode));
			AddMessage(string.Format("Flush frequency = {0}", settings.flushFrequency));

			AddMessage("...preparing test file...");

			Directory.CreateDirectory(Path.GetDirectoryName(settings.filePath));
			string errorMessage;
			if (!FlushMarkCore.FlushMark_PrepareFile(ref settings, out errorMessage))
				throw new Exception(errorMessage);

			string contigPath = LocateContigExe();
			if (contigPath != null)
			{
				AddMessage("...contig.exe found - defragmenting test file...");
				string str = RunContigExe(contigPath, settings.filePath);
				AddMessage("");
				AddMessage(str);
				AddMessage("");
			}

			AddMessage("...running test...");

			FlushMarkCore.Result result;
			if (!FlushMarkCore.FlushMark_RunBenchmark(ref settings, out result, out errorMessage))
				throw new Exception(errorMessage);

			double iops = (result.writtenPageCount * 1000.0) / result.elapsedMilliseconds;

			AddMessage("");
			AddMessage(string.Format("IOPS = {0:0.00}", iops));
			AddMessage("");
		}

		private void OnBenchmarkCompleted(Exception e)
		{
			if (m_workerThread != null)
			{
				m_workerThread.Join();
				m_workerThread = null;
			}

			if (e != null)
			{
				AddMessage("ERROR: " + e.ToString() + "\n\n");
				MessageBox.Show(e.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			EnableControls(true);
		}

		private void EnableControls(bool doEnable)
		{
			m_goButton.Enabled = doEnable;

			m_driveComboBox.Enabled = doEnable;

			m_testSizeComboBox.Enabled = doEnable;
			m_pageSizeComboBox.Enabled = doEnable;
			m_testModeComboBox.Enabled = doEnable;
			m_flushFrequencyComboBox.Enabled = doEnable;
		}

		private void AddMessage(string text)
		{
			text = text + "\r\n";
			if (m_messageTextBox.InvokeRequired)
				m_messageTextBox.BeginInvoke(new MethodInvoker(() => m_messageTextBox.AppendText(text)));
			else
				m_messageTextBox.AppendText(text);
		}

		private bool GetSettings(out FlushMarkCore.Settings settings)
		{
			// Get test directory

			string drivePath = m_driveComboBox.Text;

			if (string.IsNullOrEmpty(drivePath))
			{
				MessageBox.Show("No drive selected!?!");
				settings = new FlushMarkCore.Settings();
				return false;
			}

			settings = new FlushMarkCore.Settings(drivePath + "FlushMarkTestData" + Path.DirectorySeparatorChar + "test.dat");

			// Get test size

			long testSize = GetTestSize();
			if (testSize <= 0)
			{
				MessageBox.Show("No test size selected!?!");
				return false;
			}

			settings.testSize = (ulong)testSize;

			// Get page size

			int pageSize = GetPageSize();
			if (pageSize <= 0)
			{
				MessageBox.Show("No page size selected!?!");
				return false;
			}

			settings.pageSize = (uint)pageSize;

			// Get test mode

			int testMode = GetTestMode();
			if (testMode < 0)
			{
				MessageBox.Show("No test mode selected!?!");
				return false;
			}

			settings.testMode = (uint)testMode;

			// Get flush frequency

			int flushFrequency = GetFlushFrequency();
			if (flushFrequency < 0)
			{
				MessageBox.Show("No flush frequency selected!?!");
				return false;
			}

			settings.flushFrequency = (uint)flushFrequency;

			return true;
		}

		private long GetTestSize()
		{
			return ParseSizeString(StripAnnotations(m_testSizeComboBox.Text));
		}

		private int GetPageSize()
		{
			long pageSize = ParseSizeString(StripAnnotations(m_pageSizeComboBox.Text));
			if (pageSize > int.MaxValue)
				return -1;
			else
				return (int) pageSize;
		}

		private int GetTestMode()
		{
			string str = StripAnnotations(m_testModeComboBox.Text).ToLower();
			if (str == "random")
				return (int)FlushMarkCore.FLUSHMARKCORE_TESTTYPE_RANDOM;
			else if (str == "linear")
				return (int)FlushMarkCore.FLUSHMARKCORE_TESTTYPE_LINEAR;
			else
				return -1;
		}

		private int GetFlushFrequency()
		{
			string str = StripAnnotations(m_flushFrequencyComboBox.Text);

			int value = 0;
			if (!int.TryParse(str, out value))
				return -1;
			else
				return value;
		}

		private static long ParseSizeString(string str)
		{
			if (string.IsNullOrEmpty(str))
				return -1;

			KeyValuePair<string, long>[] scaleChars = new KeyValuePair<string, long>[]
			{
				new KeyValuePair<string, long>("B", 1),
				new KeyValuePair<string, long>("K", 1024),
				new KeyValuePair<string, long>("M", 1024*1024),
				new KeyValuePair<string, long>("G", 1024*1024*1024),
			};

			long scale = 1;

			foreach (KeyValuePair<string, long> sc in scaleChars)
			{
				if (str.EndsWith(sc.Key))
				{
					str = str.Substring(0, str.Length - sc.Key.Length);
					scale = sc.Value;
					break;
				}
			}

			str = str.Trim();

			long value = 0;
			if (!long.TryParse(str, out value))
				return -1;

			return value * scale;
		}

		private static void SelectDefaultItem(ComboBox comboBox)
		{
			for (int i = 0; i < comboBox.Items.Count; i++)
			{
				string str = comboBox.Items[i].ToString().ToLower();
				if (str.Contains("(default)"))
				{
					comboBox.SelectedIndex = i;
					return;
				}
			}
		}

		private static string StripAnnotations(string str)
		{
			Regex r = new Regex(@"\([^\)]*\)");
			return r.Replace(str, "").Trim();
		}

		private static string LocateContigExe()
		{
			string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string exeDir = Path.GetDirectoryName(exePath);
			string contigPath = exeDir + Path.DirectorySeparatorChar + "contig.exe";
			if (File.Exists(contigPath))
				return contigPath;
			else
				return null;
		}

		private static string RunContigExe(string contigPath, string filePath)
		{
			if (!IsElevated())
				return "ERROR: contig.exe requires elevation\n";

			ProcessStartInfo pi = new ProcessStartInfo(contigPath);

			pi.Arguments = "\"" + filePath + "\"";
			pi.CreateNoWindow = true;
			pi.UseShellExecute = false;
			pi.WorkingDirectory = Path.GetDirectoryName(filePath);
			pi.RedirectStandardOutput = true;
			pi.RedirectStandardError = true;

			Process p = Process.Start(pi);
			p.WaitForExit();

			string stdout = p.StandardOutput.ReadToEnd();
			string stderr = p.StandardError.ReadToEnd();
			return (stdout + stderr).Trim() + "\n";
		}

		private static bool IsElevated()
		{
			System.Security.Principal.WindowsIdentity currentIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
			if (currentIdentity == null)
				return false;

			System.Security.Principal.WindowsPrincipal pricipal = new System.Security.Principal.WindowsPrincipal(currentIdentity);
			return pricipal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
		}

		Thread m_workerThread;
	}
}
