using IronAHK.Rusty.Linux.X11;
using IronAHK.Rusty.Linux.X11.Events;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace IronAHK.Rusty.Linux
{
	internal delegate void XEventHandler(XEvent Event);

	/// <summary>
	/// Singleton class to keep track of all active windows and their
	/// events to help the hotkey and window management code
	/// </summary>
	internal class XConnectionSingleton : IDisposable
	{
		#region Singleton boilerplate

		private static XConnectionSingleton Instance;

		public static XConnectionSingleton GetInstance()
		{
			if (Instance == null)
				Instance = new XConnectionSingleton();

			return Instance;
		}

		#endregion Singleton boilerplate

		// These are the events we subscribe to, in order to allow hotkey/hotstring support
		private static readonly EventMasks SelectMask = EventMasks.KeyPress |
			EventMasks.FocusChange | EventMasks.SubstructureNofity |
			EventMasks.KeyRelease | EventMasks.Exposure;

		private IntPtr Display;
		private Thread Listener;
		private XErrorHandler OldHandler;
		private List<int> Windows;

		internal event XEventHandler OnEvent;

		// HACK: X sometimes throws a BadWindow Error because windows are quickly deleted
		// We set a placeholder errorhandler for some time and restore it later
		private bool mSurpressErrors = false;

		private bool Success = true;

		public bool SurpressErrors
		{
			set
			{
				if (value && !mSurpressErrors)
					OldHandler = Xlib.XSetErrorHandler(delegate
					{
						Success = false; return 0;
					});
				else if (!value && mSurpressErrors)
					Xlib.XSetErrorHandler(OldHandler);
				mSurpressErrors = value;
			}
			get
			{
				return mSurpressErrors;
			}
		}

		public IntPtr Handle
		{
			get
			{
				return Display;
			}
		}

		private XConnectionSingleton()
		{
			Windows = new List<int>();

			// Kick off a thread listening to X events
			Listener = new Thread(Listen);
			Listener.Start();
		}

		public void Dispose()
		{
			Listener.Abort();
			Xlib.XCloseDisplay(Display);
		}

		private void Listen()
		{
			Display = Xlib.XOpenDisplay(IntPtr.Zero);

			// Select all the windows already present
			SurpressErrors = true;
			RecurseTree(Display, Xlib.XDefaultRootWindow(Display));
			SurpressErrors = false;

			while (true)
			{
				FishEvent();
				Thread.Sleep(10); // Be polite
			}
		}

		private void FishEvent()
		{
			var Event = new XEvent();
			Xlib.XNextEvent(Display, ref Event);

			if (OnEvent != null)
				OnEvent(Event);

			if (Event.type == XEventName.CreateNotify)
			{
				int Window = Event.CreateWindowEvent.window;
				Success = true;

				Windows.Add(Window);

				SurpressErrors = true;
				if (Success)
					RecurseTree(Display, Window);

				SurpressErrors = false;
			}
			else if (Event.type == XEventName.DestroyNotify)
			{
				Windows.Remove(Event.DestroyWindowEvent.window);
			}
		}

		/// <summary>
		/// In the X Window system, windows can have sub windows. This function crawls a
		/// specific function, and then recurses on all child windows. It is called to
		/// select all initial windows. It make some time (~0.5s)
		/// </summary>
		/// <param name="Display"></param>
		/// <param name="RootWindow"></param>
		private void RecurseTree(IntPtr Display, int RootWindow)
		{
			int RootWindowRet, ParentWindow, NChildren;
			IntPtr ChildrenPtr;
			int[] Children;

			if (!Windows.Contains(RootWindow))
				Windows.Add(RootWindow);

			// Request all children of the given window, along with the parent
			Xlib.XQueryTree(Display, RootWindow, out RootWindowRet, out ParentWindow, out ChildrenPtr, out NChildren);

			if (NChildren != 0)
			{
				// Fill the array with zeroes to prevent NullReferenceException from glue layer
				Children = new int[NChildren];
				Marshal.Copy(ChildrenPtr, Children, 0, NChildren);

				Xlib.XSelectInput(Display, RootWindow, SelectMask);

				// Subwindows shouldn't be forgotten, especially since everything is a subwindow of RootWindow
				for (int i = 0; i < NChildren; i++)
				{
					if (Children[i] != 0)
					{
						Xlib.XSelectInput(Display, Children[i], SelectMask);
						RecurseTree(Display, Children[i]);
					}
				}
			}
		}
	}
}