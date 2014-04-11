namespace IronAHK.Scripting
{
	partial class Parser
	{
		#region Generic

		private const char CR = '\r';
		private const char LF = '\n';
		internal const char SingleSpace = ' ';
		private const char Reserved = '\0';
		private readonly char[] Spaces = { CR, LF, SingleSpace, '\t', '\xA0' };

		public const string RawData = "raw";

		private const string LibEnv = "AHKLIBPATH";
		private const string LibDir = "lib";
		private const string LibExt = "ahk";
		private const char LibSeperator = '_';

		internal const char StringBound = '"';
		internal const char ParenOpen = '(';
		internal const char ParenClose = ')';
		internal const char BlockOpen = '{';
		internal const char BlockClose = '}';
		internal const char ArrayOpen = '[';
		internal const char ArrayClose = ']';
		internal const char MultiComA = '/';
		internal const char MultiComB = '*';
		internal const char TernaryA = '?';
		internal const char TernaryB = ':';
		internal const char HotkeyBound = ':';
		internal const string HotkeySignal = "::";
		internal const char Directive = '#';

#if !LEGACY
        internal const char LastVar = '$';
#endif

		internal const char DefaultEscpe = '`';
		internal const char DefaultComment = ';';
		internal const char DefaultResolve = '%';
		internal const char DefaultMulticast = ',';

#if !LEGACY
        const
#endif
		private char Escape = DefaultEscpe;

#if !LEGACY
        const char Comment = DefaultComment;
#endif
		private string Comment = DefaultComment.ToString();

#if !LEGACY
        const
#endif
		internal char Resolve = DefaultResolve;

#if !LEGACY
        const
#endif
		internal char Multicast = DefaultMulticast;

#if LEGACY
		internal const string VarExt = "#_@$?"; // []
#endif
#if !LEGACY
        internal const string VarExt = "#_$";
#endif

		#endregion Generic

		#region Operators

		//internal const char Power = "**";
		internal const char Multiply = '*';

		internal const char Divide = '/';

		//internal const string FloorDivide = "//";
		internal const char Add = '+';

		internal const char Subtract = '-';
		internal const char BitAND = '&';

		//internal const string And = "&&";
		internal const char BitXOR = '^';

		internal const char BitOR = '|';

		//internal const string Or = "||";
		internal const char BitNOT = '~';

		internal const char Concatenate = '.';
		internal const char Greater = '>';
		internal const char Less = '<';

		//internal const string BitShiftLeft = "<<";
		//internal const string BitShiftRight = ">>";
		//internal const string GreaterOrEqual = ">=";
		//internal const string LessOrEqual = "<=";
		internal const char Equal = '=';

		internal const char AssignPre = ':';
		//internal const string CaseSensitiveEqual = "==";
		//internal const string NotEqual = "!=";

		internal const string AndTxt = "and";
		internal const string OrTxt = "or";
		internal const string NotTxt = "not";
		internal const string TrueTxt = "true";
		internal const string FalseTxt = "false";
		internal const string NullTxt = "null";

		internal const string BetweenTxt = "between";
		internal const string InTxt = "in";
		internal const string ContainsTxt = "contains";
		internal const string IsTxt = "is";

		internal const char Minus = '-';
		internal const char Not = '!';
		internal const char Address = '&';
		internal const char Dereference = '*';

		internal const string ErrorLevel = "ErrorLevel";

		#region Assignments

		//readonly string AssignEqual = ":" + Equal;
		//readonly string AssignAdd = "+" + Equal;
		//readonly string AssignSubtract = "-" + Equal;
		//readonly string AssignMultiply = "*=" + Equal;
		//readonly string AssignDivide = "/" + Equal;
		//readonly string AssignFloorDivide = "//" + Equal;
		//readonly string AssignConcatenate = "." + Equal;
		//readonly string AssignBitOR = "|" + Equal;
		//readonly string AssignBitAND = "&" + Equal;
		//readonly string AssignBitXOR = "^" + Equal;
		//readonly string AssignShiftLeft = "<<" + Equal;
		//readonly string AssignShiftRight = ">>" + Equal;

		#endregion Assignments

		#endregion Operators

		#region Words

		private const string MsgBox = "msgbox";

		#region Flow

		internal const string FlowBreak = "break";
		internal const string FlowContinue = "continue";
		internal const string FlowElse = "else";
		internal const string FlowGosub = "gosub";
		internal const string FlowGoto = "goto";
		internal const string FlowIf = "if";
		internal const string FlowLoop = "loop";
		internal const string FlowReturn = "return";
		internal const string FlowWhile = "while";

		#endregion Flow

		#region Functions

		internal const string FunctionLocal = "local";
		internal const string FunctionGlobal = "global";
		internal const string FunctionStatic = "static";
		internal const string FunctionParamRef = "byref";

		#endregion Functions

		#region Directives

		private const string DirvCommentFlag = "commentflag";
		private const string DirvEscapeChar = "escapechar";
		private const string DirvInclude = "include";
		private const string DirvIncludeAgain = "includeagain";

		#endregion Directives

		#region Multiline string

		private const string LTrim = "ltrim";
		private const string RTrim = "rtrim";
		private const string Join = "join";
		private const string Comments0 = "comments";
		private const string Comments1 = "comment";
		private const string Comments2 = "com";
		private const string Comments3 = "c";

		#endregion Multiline string

		#endregion Words

		#region Exceptions

		private const string ExGeneric = "Unexpected exception";
		private const string ExUnexpected = "Unexpected symbol";
		private const string ExMultiStr = "Unrecognized multiline string option";
		private const string ExFlowArgReq = "Argument is required";
		private const string ExFlowArgNotReq = "Argument not expected";
		private const string ExUnbalancedParens = "Unbalanaced parentheses in expression";
		private const string ExUntermStr = "Unterminated string";
		private const string ExUnknownDirv = "Unrecognized directive";
		private const string ExInvalidVarName = "Invalid variable name";
		private const string ExInvalidVarToken = "Invalid character in variable name";
		private const string ExEmptySource = "No code to parse";
		private const string ExEmptyVarRef = "Empty variable reference";
		private const string ExEnd = "Unexpected end of file";
		private const string ExSymbolMismatch = "Parser returned incorrect token";
		private const string ExFileNotFound = "File or directory not found";
		private const string ExCommand = "Invalid command name";
		private const string ExUnclosedBlock = "Unclosed block";
		private const string ExInvalidExpression = "Invalid expression term";
		private const string ExInvalidExponent = "Invalid exponent.";
		private const string ExIntlLineMismatch = "Line and index counts mismatched";
		private const string ExContJoinTooLong = "Join string for continuation section is too long";
		private const string ExTooFewParams = "Too few parameters passed to function";
		private const string ExIncludeNotFound = "Include file not found";
		private const string ExNoDynamicVars = "Dynamic variables are not permitted";
		private const string ExIllegalCommentFlag = "Illegal comment flag";

		#endregion Exceptions
	}
}