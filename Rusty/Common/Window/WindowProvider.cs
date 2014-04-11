using System;

namespace IronAHK.Rusty.Common
{
	partial class Window
	{
		public static class WindowProvider
		{
			// UNDONE: organise WindowProvider

			/// <summary>
			/// Creates a WindowManager for the current Platform
			/// </summary>
			public static WindowManagerBase CreateWindowManager()
			{
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					return new Windows.WindowManager();
				else
					return new Linux.WindowManager();
			}
		}
	}
}