using IronAHK.Rusty.Common;
using System;
using System.Collections.Generic;
using System.Security;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace IronAHK.Rusty
{
	partial class Core
	{
		#region Delegates

		/// <summary>
		/// A function.
		/// </summary>
		/// <param name="args">Parameters.</param>
		/// <returns>A value.</returns>
		public delegate object GenericFunction(object[] args);

		private delegate void SimpleDelegate();

		/// <summary>
		///
		/// </summary>
		public static event EventHandler ApplicationExit;

		#endregion Delegates

		#region Error

		[ThreadStatic]
		private static int error;

		/// <summary>
		/// Indicates the success or failure of a command.
		/// </summary>
		static public int ErrorLevel
		{
			get
			{
				return error;
			}
			set
			{
				error = value;
			}
		}

		#endregion Error

		#region Hooks

		private static Dictionary<string, object> variables;

		private static Dictionary<string, Keyboard.HotkeyDefinition> hotkeys;

		private static Dictionary<string, Keyboard.HotstringDefinition> hotstrings;

		private static GenericFunction keyCondition;

		private static Keyboard.KeyboardHook keyboardHook;

		private static bool suspended;

		/// <summary>
		/// Is the Script currently suspended?
		/// </summary>
		public static bool Suspended
		{
			get
			{
				return suspended;
			}
		}

		[ThreadStatic]
		private static int? _KeyDelay;

		[ThreadStatic]
		private static int? _KeyPressDuration;

		[ThreadStatic]
		private static int? _MouseDelay;

		[ThreadStatic]
		private static int? _DefaultMouseSpeed;

		#endregion Hooks

		#region Guis

		[ThreadStatic]
		private static Form dialogOwner;

		private static Dictionary<long, ImageList> imageLists;

		private static Dictionary<string, Form> guis;

		[ThreadStatic]
		private static string defaultGui;

		private static string DefaultGuiId
		{
			get
			{
				return defaultGui ?? "1";
			}
			set
			{
				defaultGui = value;
				defaultTreeView = null;
			}
		}

		private static Form DefaultGui
		{
			get
			{
				if (guis == null)
					return null;

				string key = DefaultGuiId;
				return guis.ContainsKey(key) ? guis[key] : null;
			}
		}

		[ThreadStatic]
		private static long lastFoundForm = 0;

		private static long LastFoundForm
		{
			get
			{
				return lastFoundForm;
			}
			set
			{
				lastFoundForm = value;
			}
		}

		[ThreadStatic]
		private static TreeView defaultTreeView;

		private static TreeView DefaultTreeView
		{
			get
			{
				if (defaultTreeView != null)
					return defaultTreeView;

				var gui = DefaultGui;

				if (gui == null)
					return null;

				TreeView tv = null;

				foreach (var control in gui.Controls)
					if (control is TreeView)
						tv = (TreeView) control;

				return tv;
			}
			set
			{
				defaultTreeView = value;
			}
		}

		[ThreadStatic]
		private static ListView defaultListView;

		private static ListView DefaultListView
		{
			get
			{
				if (defaultListView != null)
					return defaultListView;

				var gui = DefaultGui;

				if (gui == null)
					return null;

				ListView lv = null;

				foreach (var control in gui.Controls)
					if (control is ListView)
						lv = (ListView) control;

				return lv;
			}
			set
			{
				defaultListView = value;
			}
		}

		private static StatusBar DefaultStatusBar
		{
			get
			{
				var gui = DefaultGui;

				if (gui == null)
					return null;

				return ((GuiInfo) gui.Tag).StatusBar;
			}
		}

		private static NotifyIcon Tray;

		#endregion Guis

		#region Dialogs

		private static Dictionary<int, ProgressDialog> progressDialgos;
		private static Dictionary<int, SplashDialog> splashDialogs;

		#endregion Dialogs

		#region Tips

		private static ToolTip persistentTooltip;

		private static Form tooltip;

		#endregion Tips

		#region RunAs

		[ThreadStatic]
		private static string runUser;

		[ThreadStatic]
		private static SecureString runPassword;

		[ThreadStatic]
		private static string runDomain;

		#endregion RunAs

		#region Windows

		[ThreadStatic]
		private static int? _ControlDelay;

		[ThreadStatic]
		private static int? _WinDelay;

		[ThreadStatic]
		private static bool? _DetectHiddenText;

		[ThreadStatic]
		private static bool? _DetectHiddenWindows;

		[ThreadStatic]
		private static int? _TitleMatchMode;

		[ThreadStatic]
		private static bool? _TitleMatchModeSpeed;

		#endregion Windows

		#region Strings

		[ThreadStatic]
		private static StringComparison? _StringComparison;

		[ThreadStatic]
		private static string _FormatNumeric;

		[ThreadStatic]
		private static string _UserAgent;

		#endregion Strings

		#region Misc

		private static Dictionary<string, Timer> timers;

		private static EventHandler onExit;

		private const int LoopFrequency = 50;

		[ThreadStatic]
		private static Random randomGenerator;

		#endregion Misc

		#region Coordmode

		[ThreadStatic]
		private static CoordModes coords;

		private struct CoordModes
		{
			public CoordModeType Tooltip
			{
				get;
				set;
			}

			public CoordModeType Pixel
			{
				get;
				set;
			}

			public CoordModeType Mouse
			{
				get;
				set;
			}

			public CoordModeType Caret
			{
				get;
				set;
			}

			public CoordModeType Menu
			{
				get;
				set;
			}
		}

		private enum CoordModeType
		{
			Relative = 0,
			Screen
		}

		#endregion Coordmode
	}
}