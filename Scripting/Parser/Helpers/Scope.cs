namespace IronAHK.Scripting
{
	partial class Parser
	{
		internal const string ScopeVar = ".";
		internal const string VarProperty = "Vars";
		private int internalID;

		private string InternalID
		{
			get
			{
				return "e" + internalID++;
			}
		}

		private string Scope
		{
			get
			{
				foreach (var block in blocks)
				{
					if (block.Kind == CodeBlock.BlockKind.Function)
						return block.Method ?? mainScope;
				}

				return mainScope;
			}
		}
	}
}