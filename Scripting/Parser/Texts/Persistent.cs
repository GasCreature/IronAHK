namespace IronAHK.Scripting
{
	partial class Parser
	{
		private bool persistent;

		private void CheckPersistent(string name)
		{
			if (persistent)
				return;

			switch (name.ToLowerInvariant())
			{
				case "settimer":
				case "menu":
				case "hotkey":
				case "hotstring":
				case "onmessage":
				case "gui":
					persistent = true;
					break;
			}
		}
	}
}