using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace IronAHK.Scripting
{
	partial class Parser
	{
		private List<CodeMethodInvokeExpression> invokes = new List<CodeMethodInvokeExpression>();
		private const string invokeCommand = "IsCommand";

		#region DOM

		private CodeMemberMethod LocalMethod(string name)
		{
			var method = new CodeMemberMethod
			{
				Name = name,
				ReturnType = new CodeTypeReference(typeof(object))
			};
			var param = new CodeParameterDeclarationExpression(typeof(object[]), args);
			param.UserData.Add(Parser.RawData, typeof(object[]));
			method.Parameters.Add(param);
			return method;
		}

		private CodeMethodInvokeExpression LocalLabelInvoke(string name)
		{
			var invoke = (CodeMethodInvokeExpression) InternalMethods.LabelCall;
			invoke.Parameters.Add(new CodePrimitiveExpression(name));
			return invoke;
		}

		private CodeMethodInvokeExpression LocalMethodInvoke(string name)
		{
			var invoke = new CodeMethodInvokeExpression();
			invoke.Method.MethodName = name;
			invoke.Method.TargetObject = null;
			return invoke;
		}

		#endregion DOM

		#region Resolve

		private void StdLib()
		{
			#region Paths

			var search = new StringBuilder();

			search.Append(Environment.GetEnvironmentVariable(LibEnv));
			search.Append(Path.PathSeparator);
			search.Append(Path.Combine(Assembly.GetExecutingAssembly().Location, LibDir));

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				search.Append(Path.PathSeparator);
				search.Append(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.Combine("AutoHotkey", LibDir)));
			}
			else if (Path.DirectorySeparatorChar == '/' && Environment.OSVersion.Platform == PlatformID.Unix)
			{
				search.Append(Path.PathSeparator);
				search.Append("/usr/" + LibDir + "/" + LibExt);
				search.Append(Path.PathSeparator);
				search.Append("/usr/local/" + LibDir + "/" + LibExt);
				search.Append(Path.PathSeparator);
				search.Append(Path.Combine(Environment.GetEnvironmentVariable("HOME"), Path.Combine(LibDir, LibExt)));
			}

			var paths = search.ToString().Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
			search = null;

			#endregion Paths

			foreach (var invoke in invokes)
			{
				string name = invoke.Method.MethodName;

				if (IsLocalMethodReference(name))
				{
					var obj = new CodeArrayCreateExpression
					{
						Size = invoke.Parameters.Count,
						CreateType = new CodeTypeReference(typeof(object))
					};
					obj.Initializers.AddRange(invoke.Parameters);
					invoke.Parameters.Clear();
					invoke.Parameters.Add(obj);
					continue;
				}

				if (invoke.Method.TargetObject != null)
					continue;

				int z = name.IndexOf(LibSeperator);

				if (z != -1)
					name = name.Substring(0, z);

				foreach (var dir in paths)
				{
					if (!Directory.Exists(dir))
						continue;

					string file = Path.Combine(dir, string.Concat(name, ".", LibExt));

					if (File.Exists(file))
					{
						Parse(new StreamReader(file), Path.GetFileName(file));
						return;
					}
				}
			}

			invokes.Clear();
		}

		private bool IsLocalMethodReference(string name)
		{
			foreach (var method in methods)
				if (method.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
					return true;
			return false;
		}

		#endregion Resolve
	}
}