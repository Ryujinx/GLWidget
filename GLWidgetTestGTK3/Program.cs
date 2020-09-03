using System;
using System.Reflection;
using Gtk;
using OpenTK;

namespace GLWidgetTestGTK3
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            // GTK
            Application.Init();
			MainWindow win = MainWindow.Create();
			win.Show();
			Application.Run();
		}
	}
}