namespace IronAHK.Rusty.Common
{
	/// <summary>
	/// Serialize JSON strings.
	/// </summary>
	static partial class SimpleJson
	{
		#region Tokens

		private const char ObjectOpen = '{';
		private const char ObjectClose = '}';
		private const char MemberSeperator = ',';
		private const char MemberAssign = ':';
		private const char MemberAssignAlt = '=';
		private const char ArrayOpen = '[';
		private const char ArrayClose = ']';
		private const char StringBoundary = '"';
		private const char StringBoundaryAlt = '\'';
		private const char Escape = '\\';
		private const string True = "true";
		private const string False = "false";
		private const string Null = "null";
		private const char Space = ' ';

		#endregion Tokens

		#region Exceptions

		private const string ExUntermField = "Unterminated field";
		private const string ExNoMemberVal = "Expected member value";
		private const string ExNoKeyPair = "Expected key pair";
		private const string ExUnexpectedToken = "Unexpected token";

		private const string ExSeperator = " at position ";

		#endregion Exceptions

		#region Helpers

		private static string ErrorMessage(string text, int position)
		{
			return string.Concat(text, ExSeperator, position.ToString());
		}

		#endregion Helpers
	}
}