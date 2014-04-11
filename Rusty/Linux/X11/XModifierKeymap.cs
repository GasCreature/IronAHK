using System;
using System.Runtime.InteropServices;

namespace IronAHK.Rusty.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XModifierKeymap
	{
		public int max_keypermod;
		public IntPtr modifiermap;
	}
}