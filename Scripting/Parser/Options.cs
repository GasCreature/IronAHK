﻿namespace IronAHK.Scripting
{
	partial class Parser
	{
		private const string Legacy = "LEGACY";

		private const bool LaxExpressions =
#if LEGACY
 true
#endif
#if !LEGACY
 false
#endif
;

		private const bool LegacyIf = LaxExpressions;

		private const bool LegacyLoop = LaxExpressions;

		private bool DynamicVars = LaxExpressions;
	}
}