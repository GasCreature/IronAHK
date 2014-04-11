namespace IronAHK.Rusty.Common
{
	partial class Window
	{
		/// <summary>
		/// Singleton Facade for easy accessing current Platform's WindowManager
		/// </summary>
		public class WindowItemProvider
		{
			private static readonly WindowManagerBase instance = WindowProvider.CreateWindowManager();

			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static WindowItemProvider()
			{
			}

			// private constructor
			private WindowItemProvider()
			{
			}

			public static WindowManagerBase Instance
			{
				get
				{
					return instance;
				}
			}
		}
	}
}