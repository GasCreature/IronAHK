using IronAHK.Rusty.Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IronAHK.Rusty
{
	partial class Core
	{
		private static void InitVariables()
		{
			if (variables == null)
				variables = new Dictionary<string, object>();
		}

		private static void InitKeyboardHook()
		{
			if (hotkeys == null)
				hotkeys = new Dictionary<string, Keyboard.HotkeyDefinition>();

			if (hotstrings == null)
				hotstrings = new Dictionary<string, Keyboard.HotstringDefinition>();

			if (keyboardHook != null)
				return;

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				keyboardHook = new Windows.KeyboardHook();
			else
				keyboardHook = new Linux.KeyboardHook();

			Keyboard.IAInputCommand.Instance.Hook = keyboardHook;
		}

		private static void InitGui()
		{
			if (imageLists == null)
				imageLists = new Dictionary<long, ImageList>();
		}

		private static Random RandomGenerator
		{
			get
			{
				if (randomGenerator == null)
					randomGenerator = new Random();

				return randomGenerator;
			}
		}

		private static void InitDialoges()
		{
			if (progressDialgos == null)
				progressDialgos = new Dictionary<int, ProgressDialog>();

			if (splashDialogs == null)
				splashDialogs = new Dictionary<int, SplashDialog>();
		}
	}
}