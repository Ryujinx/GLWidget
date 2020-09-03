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
            InitializeGlBindings();

            // GTK
            Application.Init();
			MainWindow win = MainWindow.Create();
			win.Show();
			Application.Run();
		}

        private static void InitializeGlBindings()
        {
            // We don't put a hard dependency on OpenTK.Graphics here.
            // So we need to use reflection to initialize the GL bindings, so users don't have to.

            // Try to load OpenTK.Graphics assembly.
            Assembly assembly;
            try
            {
                assembly = Assembly.Load("OpenTK.Graphics");
            }
            catch
            {
                // Failed to load graphics, oh well.
                // Up to the user I guess?
                // TODO: Should we expose this load failure to the user better?
                return;
            }

            var provider = new GTKBindingContext();

            void LoadBindings(string typeNamespace)
            {
                var type = assembly.GetType($"OpenTK.Graphics.{typeNamespace}.GL");
                if (type == null)
                {
                    return;
                }

                var load = type.GetMethod("LoadBindings");
                load.Invoke(null, new object[] { provider });
            }

            LoadBindings("ES11");
            LoadBindings("ES20");
            LoadBindings("ES30");
            LoadBindings("OpenGL");
            LoadBindings("OpenGL4");
        }
	}
}