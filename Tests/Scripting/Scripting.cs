using IronAHK.Rusty;
using IronAHK.Scripting;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace IronAHK.Tests
{
	[TestFixture, Category("Scripting")]
	public partial class Scripting
	{
		private string path = string.Format("..{0}..{0}Code{0}", Path.DirectorySeparatorChar);
		private const string ext = ".ahk";

		private bool TestScript(string source)
		{
			return HasPassed(RunScript(string.Concat(path, source, ext), true));
		}

		private bool ValidateScript(string source)
		{
			RunScript(string.Concat(path, source, ext), false);
			return true;
		}

		private bool HasPassed(string output)
		{
			if (string.IsNullOrEmpty(output))
				return false;

			const string pass = "pass";
			foreach (var remove in new[] { pass, " ", "\n" })
				output = output.Replace(remove, string.Empty);

			return output.Length == 0;
		}

		private string RunScript(string source, bool execute)
		{
			CompilerResults results;

			using (var provider = new IACodeProvider())
			{
				var options = new IACompilerParameters
				{
					GenerateExecutable = false,
					GenerateInMemory = true,
					Merge = true,
					MergeFallbackToLink = true
				};
				results = provider.CompileAssemblyFromFile(options, source);
			}

			var buffer = new StringBuilder();

			using (var writer = new StringWriter(buffer))
			{
				Console.SetOut(writer);

				if (execute)
					results.CompiledAssembly.EntryPoint.Invoke(null, null);

				writer.Flush();
			}

			string output = buffer.ToString();

			using (var console = Console.OpenStandardOutput())
			{
				var stdout = new StreamWriter(console);
				stdout.AutoFlush = true;
				Console.SetOut(stdout);
			}

			return output;
		}
	}
}