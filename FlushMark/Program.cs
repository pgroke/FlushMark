// Copyright (c) 2012, Paul Groke
// For conditions of distribution and use, see copyright notice in LICENSE.txt

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FlushMark
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FlushMarkForm());
		}
	}
}
