using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

namespace FlushMark
{
	static class FlushMarkCore
	{
		public const uint FLUSHMARKCORE_TESTTYPE_RANDOM = 0;
		public const uint FLUSHMARKCORE_TESTTYPE_LINEAR = 1;

		[StructLayout(LayoutKind.Sequential)]
		public struct Settings
		{
			public Settings(string filePath_)
			{
				filePath = filePath_;

				testMode = 0;
				flags = 0;

				testSize = 1024 * 1024 * 1024;
				pageSize = 4096;

				minPageCount = 0;
				minMilliseconds = 0;
				flushFrequency = 1;
			}

			[MarshalAs(UnmanagedType.LPWStr)]
			public string filePath;

			public uint testMode;
			public uint flags;

			public ulong testSize;
			public uint pageSize;

			public uint minPageCount;
			public uint minMilliseconds;
			public uint flushFrequency;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Result
		{
			public uint writtenPageCount;
			public uint elapsedMilliseconds;
		};

		[DllImport("FlushMarkCore.dll", SetLastError = true)]
		public static extern bool FlushMark_PrepareFile(ref Settings settings, [MarshalAs(UnmanagedType.BStr)] out string errorMessage);

		[DllImport("FlushMarkCore.dll", SetLastError = true)]
		public static extern bool FlushMark_RunBenchmark(ref Settings settings, out Result result, [MarshalAs(UnmanagedType.BStr)] out string errorMessage);
	}
}
