﻿//
//  MainWindow.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2016 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Gdk;
using GLWidgetTestGTK3.Data;
using GLWidgetTestGTK3.Debug;
using GLWidgetTestGTK3.World;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using KeyPressEventArgs = Gtk.KeyPressEventArgs;
using OpenTK.Graphics;

namespace GLWidgetTestGTK3
{
	public partial class MainWindow : Gtk.Window
	{
		[UI] readonly Box MainBox;
		[UI] readonly MenuBar MainMenuBar;

		[UI] readonly Alignment GLWidgetAlignment;
		[UI] readonly GLWidget MainGLWidget;

		private bool GLInit;

		private Scene Scene;

		private uint VertexArrayID;
		private uint ColourBufferID;

		private int ShaderProgramID;

        /* Default camera positions */
        private Vector3 cameraPosition;
		private Vector3 cameraLookDirection;
		private Vector3 cameraRightVector;

		private Vector3 cameraUpVector;
		private float horizontalViewAngle;
		private float verticalViewAngle;
		private const float defaultFOV = 45.0f;

		private const float defaultMovementSpeed = 10.0f;
		private const float defaultCameraSpeed = 0.5f;

		// Other variables
		private bool wantsToMove = false;
		private float deltaTime;

		private int mouseXLastFrame;
		private int mouseYLastFrame;

		private float rightAxis;
		private float forwardAxis;

		private static readonly float[] cubeColourBufferData =
		{
			0.583f,  0.771f,  0.014f,
			0.609f,  0.115f,  0.436f,
			0.327f,  0.483f,  0.844f,
			0.822f,  0.569f,  0.201f,
			0.435f,  0.602f,  0.223f,
			0.310f,  0.747f,  0.185f,
			0.597f,  0.770f,  0.761f,
			0.559f,  0.436f,  0.730f,
			0.359f,  0.583f,  0.152f,
			0.483f,  0.596f,  0.789f,
			0.559f,  0.861f,  0.639f,
			0.195f,  0.548f,  0.859f,
			0.014f,  0.184f,  0.576f,
			0.771f,  0.328f,  0.970f,
			0.406f,  0.615f,  0.116f,
			0.676f,  0.977f,  0.133f,
			0.971f,  0.572f,  0.833f,
			0.140f,  0.616f,  0.489f,
			0.997f,  0.513f,  0.064f,
			0.945f,  0.719f,  0.592f,
			0.543f,  0.021f,  0.978f,
			0.279f,  0.317f,  0.505f,
			0.167f,  0.620f,  0.077f,
			0.347f,  0.857f,  0.137f,
			0.055f,  0.953f,  0.042f,
			0.714f,  0.505f,  0.345f,
			0.783f,  0.290f,  0.734f,
			0.722f,  0.645f,  0.174f,
			0.302f,  0.455f,  0.848f,
			0.225f,  0.587f,  0.040f,
			0.517f,  0.713f,  0.338f,
			0.053f,  0.959f,  0.120f,
			0.393f,  0.621f,  0.362f,
			0.673f,  0.211f,  0.457f,
			0.820f,  0.883f,  0.371f,
			0.982f,  0.099f,  0.879f
		};

		public static MainWindow Create()
		{
			Builder builder = new Builder(null, "GLWidgetTestGTK3.interfaces.MainWindow.glade", null);
			return new MainWindow(builder, builder.GetObject("MainWindow").Handle);
		}

		protected MainWindow(Builder builder, IntPtr handle)
			: base(handle)
		{
			builder.Autoconnect(this);
			DeleteEvent += OnDeleteEvent;

			this.GLInit = false;
			ResetCamera();

            OpenTK.Toolkit.Init();

            this.MainGLWidget = new GLWidget(GraphicsMode.Default)
            {
                CanFocus = true,
                SingleBuffer = false,
                ColorBPP = 24,
                DepthBPP = 24,
                Samples = 4,
                GLVersionMajor = 3,
                GLVersionMinor = 3,
                GraphicsContextFlags = GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug,
				IsRenderHandler = true
            };

			this.MainGLWidget.Events |=
				EventMask.ButtonPressMask |
				EventMask.ButtonReleaseMask |
				EventMask.KeyPressMask |
				EventMask.KeyReleaseMask;

			this.MainGLWidget.Initialized += OnViewportInitialized;
			this.MainGLWidget.ButtonPressEvent += OnViewportButtonPressed;
			this.MainGLWidget.ButtonReleaseEvent += OnViewportButtonReleased;
			this.MainGLWidget.KeyPressEvent += OnViewportKeyPressed;
			this.MainGLWidget.KeyReleaseEvent += OnViewportKeyReleased;

			// Add the GL widget to the UI
			this.GLWidgetAlignment.Add(this.MainGLWidget);
			this.GLWidgetAlignment.ShowAll();

		}

		private List<Vertex> FloatArrayToVertexList(float[] vertexPositions)
		{
			if ((vertexPositions.Length % 3) != 0)
			{
				throw new ArgumentException("The input array must be of a 3-multiple length. Incomplete entries are not allowed.", nameof(vertexPositions));
			}

			List<Vertex> convertedVertices = new List<Vertex>();

			for (int i = 0; i < vertexPositions.Length; i += 3)
			{
				Vector3 Position = new Vector3(vertexPositions[i], vertexPositions[i + 1], vertexPositions[i + 2]);
				Vertex vertex = new Vertex(Position);

				convertedVertices.Add(vertex);
			}

			return convertedVertices;
		}

		private void OnViewportKeyReleased(object o, KeyReleaseEventArgs args)
		{
			if (args.Event.Type == EventType.KeyRelease)
			{
				if( args.Event.Key == Gdk.Key.w || args.Event.Key == Gdk.Key.W)
				{
					forwardAxis = 0.0f;
				}
				else if( args.Event.Key == Gdk.Key.s || args.Event.Key == Gdk.Key.S)
				{
					forwardAxis = 0.0f;
				}

				if( args.Event.Key == Gdk.Key.d || args.Event.Key == Gdk.Key.D)
				{
					rightAxis = 0.0f;
				}
				else if( args.Event.Key == Gdk.Key.a || args.Event.Key == Gdk.Key.A)
				{
					rightAxis = 0.0f;
				}
			}
		}

		private void OnViewportKeyPressed(object o, KeyPressEventArgs args)
		{
			if (args.Event.Type == EventType.KeyPress)
			{
				if( args.Event.Key == Gdk.Key.w || args.Event.Key == Gdk.Key.W)
				{
					forwardAxis = 1.0f;
				}
				else if( args.Event.Key == Gdk.Key.s || args.Event.Key == Gdk.Key.S)
				{
					forwardAxis = -1.0f;
				}

				if( args.Event.Key == Gdk.Key.d || args.Event.Key == Gdk.Key.D)
				{
					rightAxis = 1.0f;
				}
				else if( args.Event.Key == Gdk.Key.a || args.Event.Key == Gdk.Key.A)
				{
					rightAxis = -1.0f;
				}

				if( args.Event.Key == Gdk.Key.r || args.Event.Key == Gdk.Key.R)
				{
					if (wantsToMove)
					{
						ResetCamera();
					}
				}

				if (args.Event.Key == Gdk.Key.Escape)
				{
					Application.Quit();
				}
			}
		}

		private void ResetCamera()
		{
			this.cameraPosition = new Vector3(0.0f, 0.0f, 5.0f);
			this.horizontalViewAngle = MathHelper.DegreesToRadians(180.0f);
           	this.verticalViewAngle = MathHelper.DegreesToRadians(0.0f);

			this.cameraLookDirection = new Vector3(
				(float)(Math.Cos(this.verticalViewAngle) * Math.Sin(this.horizontalViewAngle)),
				(float)Math.Sin(this.verticalViewAngle),
				(float)(Math.Cos(this.verticalViewAngle) * Math.Cos(this.horizontalViewAngle)));

			this.cameraRightVector = new Vector3(
				(float)Math.Sin(horizontalViewAngle - MathHelper.PiOver2),
				0,
				(float)Math.Cos(horizontalViewAngle - MathHelper.PiOver2));

			this.cameraUpVector = Vector3.Cross(cameraRightVector, cameraLookDirection);
		}

		[GLib.ConnectBefore]
		private void OnViewportButtonReleased(object o, ButtonReleaseEventArgs args)
		{
			// Right click is released
			if (args.Event.Type == EventType.ButtonRelease && args.Event.Button == 3)
			{
				// Return the mouse pointer
				this.Window.Cursor = new Cursor(CursorType.Arrow);

				this.GrabFocus();
				this.wantsToMove = false;
			}
		}

		[GLib.ConnectBefore]
		private void OnViewportButtonPressed(object o, ButtonPressEventArgs args)
		{
			// Right click is pressed
			if (args.Event.Type == EventType.ButtonPress && args.Event.Button == 3)
			{
				// Hide the mouse pointer
				this.Window.Cursor = new Cursor(CursorType.BlankCursor);

				this.MainGLWidget.GrabFocus();
				this.wantsToMove = true;
				this.MainGLWidget.GetPointer(out this.mouseXLastFrame, out this.mouseYLastFrame);
			}
		}

		protected virtual void OnViewportInitialized(object sender, EventArgs e)
		{
			var version = GL.GetString(StringName.Version);

            var framebufferStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            this.Scene = new Scene();

			// Create the cube actor
			Actor cubeActor = new Actor(new Mesh(FloatArrayToVertexList(Shapes.UnindexedCube)));
			Actor cubeActor1 = new Actor(new Mesh(FloatArrayToVertexList(Shapes.UnindexedCube)));
			Actor cubeActor2 = new Actor(new Mesh(FloatArrayToVertexList(Shapes.UnindexedCube)));
			Actor cubeActor3 = new Actor(new Mesh(FloatArrayToVertexList(Shapes.UnindexedCube)));

			// Translate the cube actor
			cubeActor.Transform.Translation = new Vector3(4.0f, 0.0f, 0.0f);
			cubeActor1.Transform.Translation = new Vector3(0.0f, 4.0f, 0.0f);
			cubeActor2.Transform.Translation = new Vector3(0.0f, -4.0f, 0.0f);
			cubeActor3.Transform.Translation = new Vector3(-4.0f, 0.0f, 0.0f);

			Actor triangleActor = new Actor(new Mesh(FloatArrayToVertexList(Shapes.UnindexedTriangle)));

			this.Scene.Actors.Add(cubeActor);
			this.Scene.Actors.Add(cubeActor1);
			this.Scene.Actors.Add(cubeActor2);
			this.Scene.Actors.Add(cubeActor3);
			this.Scene.Actors.Add(triangleActor);

			// Generate the colour buffer
			GL.GenBuffers(1, out ColourBufferID);

			// Upload the colour data
			GL.BindBuffer(BufferTarget.ArrayBuffer, ColourBufferID);
			GL.BufferData(BufferTarget.ArrayBuffer,  (IntPtr)(cubeColourBufferData.Length * sizeof(float)), cubeColourBufferData, BufferUsageHint.StaticDraw);

			// Make sure we use the depth buffer when drawing
			GL.Enable(EnableCap.DepthTest);
			//GL.DepthFunc(DepthFunction.Less);

			// Enable backface culling for performance reasons
			//GL.Enable(EnableCap.CullFace);

			// Render wireframe
			//GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

			// Initialize the viewport
			int widgetWidth = this.GLWidgetAlignment.AllocatedWidth;
			int widgetHeight = this.GLWidgetAlignment.AllocatedHeight;

			GL.Viewport(0, 0, widgetWidth, widgetHeight);
			GL.ClearColor(0.522f, 0.573f, 0.678f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// Load the shaders
            ShaderProgramID = LoadShaders();

			// Add idle event handler to process rendering whenever and as long as time is available.
			GLInit = true;
			//GLib.Idle.Add(OnIdleProcessMain);

			MainGLWidget.ClearCurrent();

			System.Threading.Thread thread = new System.Threading.Thread(Start);
			thread.Start();
		}

		protected void RenderFrame()
        {
            MainGLWidget.MakeCurrent();

            if (MainGLWidget.Resize)
            {
                MainGLWidget.Update();
            }

            // Make sure the viewport is accurate for the current widget size on screen
            int widgetWidth = this.GLWidgetAlignment.AllocatedWidth;
			int widgetHeight = this.GLWidgetAlignment.AllocatedHeight;
            var error = GL.GetError();
            GL.Viewport(0, 0, widgetWidth, widgetHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// Activate the shaders
            GL.UseProgram(ShaderProgramID);

			// See if there's any movement to compute
			if (wantsToMove)
			{
				int mouseX;
				int mouseY;
				this.MainGLWidget.GetPointer(out mouseX, out mouseY);

				this.horizontalViewAngle += defaultCameraSpeed * this.deltaTime * (float)(mouseXLastFrame - mouseX);
				this.verticalViewAngle += defaultCameraSpeed * this.deltaTime * (float)(mouseYLastFrame - mouseY);

				if (verticalViewAngle > MathHelper.DegreesToRadians(90.0f))
				{
					verticalViewAngle = MathHelper.DegreesToRadians(90.0f);
				}
				else if (verticalViewAngle < MathHelper.DegreesToRadians(-90.0f))
				{
					verticalViewAngle = MathHelper.DegreesToRadians(-90.0f);
				}

				mouseXLastFrame = mouseX;
				mouseYLastFrame = mouseY;

				// Compute the look direction
				this.cameraLookDirection = new Vector3(
					(float)(Math.Cos(this.verticalViewAngle) * Math.Sin(this.horizontalViewAngle)),
					(float)Math.Sin(this.verticalViewAngle),
					(float)(Math.Cos(this.verticalViewAngle) * Math.Cos(this.horizontalViewAngle)));

				this.cameraRightVector = new Vector3(
					(float)Math.Sin(this.horizontalViewAngle - MathHelper.PiOver2),
					0,
					(float)Math.Cos(this.horizontalViewAngle - MathHelper.PiOver2));

				this.cameraUpVector = Vector3.Cross(this.cameraRightVector, this.cameraLookDirection);

				// Perform any movement
				if (forwardAxis > 0)
				{
					this.cameraPosition += this.cameraLookDirection * deltaTime * defaultMovementSpeed;
				}

				if (forwardAxis < 0)
				{
					this.cameraPosition -= this.cameraLookDirection * deltaTime * defaultMovementSpeed;
				}

				if (rightAxis > 0)
				{
					this.cameraPosition += this.cameraRightVector * deltaTime * defaultMovementSpeed;
				}

				if (rightAxis < 0)
				{
					this.cameraPosition -= this.cameraRightVector * deltaTime * defaultMovementSpeed;
				}
			}

			// Calculate the relative viewpoint
			float aspectRatio = (float)widgetWidth / (float)widgetHeight;
			Matrix4 Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(defaultFOV), aspectRatio, 0.1f, 1000.0f);
			Matrix4 View = Matrix4.LookAt(
				cameraPosition,
				cameraPosition + cameraLookDirection,
				cameraUpVector
			);

			// Enable the colour array
			GL.EnableVertexAttribArray(1);
			GL.BindBuffer(BufferTarget.ArrayBuffer, ColourBufferID);
			GL.VertexAttribPointer(
				1,
				3,
				VertexAttribPointerType.Float,
				false,
				0,
				0);

			// Tick the actors before any rendering is done
			this.Scene.Tick(deltaTime);

			foreach (Actor actor in this.Scene.Actors)
			{
				actor.Render(ShaderProgramID, View, Projection);
			}

			// Release the arrays
			GL.DisableVertexAttribArray(1);

            //swap
            MainGLWidget.Swapbuffers();

			MainGLWidget.ClearCurrent();
        }

		public void Start()
        {
			System.Threading.Thread.Sleep(1000);

			while (true)
            {
				RenderFrame();

				System.Threading.Thread.Sleep(5);
            }
        }

		protected bool OnIdleProcessMain()
		{
			if (!GLInit)
				return false;
			else
			{
                // Start deltaTime calculation
                Stopwatch deltaTimeWatcher = new Stopwatch();
				deltaTimeWatcher.Start();

                //System.Threading.Tasks.Task.Run(RenderFrame).Wait();

                RenderFrame();

                // End delta time calculation
                deltaTimeWatcher.Stop();
				this.deltaTime = (float)(deltaTimeWatcher.ElapsedMilliseconds * 0.001f);
				return true;
			}
		}

		private int LoadShaders()
		{
			int VertexShaderID = GL.CreateShader(ShaderType.VertexShader);
			int FragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);

			string vertexShaderSourceCode;
			using (Stream shaderStream =
					Assembly.GetExecutingAssembly().GetManifestResourceStream("GLWidgetTestGTK3.Shaders.VertexShader.glsl"))
			{
				using (StreamReader sr = new StreamReader(shaderStream))
				{
					vertexShaderSourceCode = sr.ReadToEnd();
				}
			}

			string fragmentShaderSourceCode;
			using (Stream shaderStream =
					Assembly.GetExecutingAssembly().GetManifestResourceStream("GLWidgetTestGTK3.Shaders.FragmentShader.glsl"))
			{
				using (StreamReader sr = new StreamReader(shaderStream))
				{
					fragmentShaderSourceCode = sr.ReadToEnd();
				}
			}


			int result = 0;
			int compilationLogLength;

			Console.WriteLine("Compiling vertex shader...");
			GL.ShaderSource(VertexShaderID, vertexShaderSourceCode);
			GL.CompileShader(VertexShaderID);

			GL.GetShader(VertexShaderID, ShaderParameter.CompileStatus, out result);
			GL.GetShader(VertexShaderID, ShaderParameter.InfoLogLength, out compilationLogLength);

			if (compilationLogLength > 0)
			{
				string compilationLog;
				GL.GetShaderInfoLog(VertexShaderID, out compilationLog);

				Console.WriteLine(compilationLog);
			}

			Console.WriteLine("Compiling fragment shader...");
			GL.ShaderSource(FragmentShaderID, fragmentShaderSourceCode);
			GL.CompileShader(FragmentShaderID);

			GL.GetShader(FragmentShaderID, ShaderParameter.CompileStatus, out result);
			GL.GetShader(FragmentShaderID, ShaderParameter.InfoLogLength, out compilationLogLength);

			if (compilationLogLength > 0)
			{
				string compilationLog;
				GL.GetShaderInfoLog(FragmentShaderID, out compilationLog);

				Console.WriteLine(compilationLog);
			}


			Console.WriteLine("Linking shader program...");
			int shaderProgramID = GL.CreateProgram();

			GL.AttachShader(shaderProgramID, VertexShaderID);
			GL.AttachShader(shaderProgramID, FragmentShaderID);
			GL.LinkProgram(shaderProgramID);

			GL.GetProgram(shaderProgramID, ProgramParameter.LinkStatus, out result);
			GL.GetProgram(shaderProgramID, ProgramParameter.InfoLogLength, out compilationLogLength);

			if (compilationLogLength > 0)
			{
				string compilationLog;
				GL.GetProgramInfoLog(shaderProgramID, out compilationLog);

				Console.WriteLine(compilationLog);
			}

			// Clean up the shader source code and unlinked object files from graphics memory
			GL.DetachShader(shaderProgramID, VertexShaderID);
			GL.DetachShader(shaderProgramID, FragmentShaderID);

			GL.DeleteShader(VertexShaderID);
			GL.DeleteShader(FragmentShaderID);

			return shaderProgramID;
		}

		/// <summary>
		/// Handles application shutdown procedures - terminating render threads, cleaning
		/// up the UI, etc.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="a">The deletion arguments.</param>
		protected void OnDeleteEvent(object sender, DeleteEventArgs a)
		{
			Application.Quit();
			a.RetVal = true;
		}
	}
}

