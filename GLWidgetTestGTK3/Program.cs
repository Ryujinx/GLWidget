using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Gtk;
using OpenTK;

namespace GLWidgetTestGTK3
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            GTKBindingHelper.InitXThreads();
            // GTK
            Application.Init();
			MainWindow win = MainWindow.Create();
			win.Show();
			Application.Run();
		}
	}
}