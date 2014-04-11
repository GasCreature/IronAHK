using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace IronAHK.Scripting
{
	partial class Parser : ICodeParser
	{
		#region Properties

		private const string mainScope = "";
		private const string className = "Program";
		private readonly Type bcl = typeof(Script).BaseType;
		private CodeEntryPointMethod main = new CodeEntryPointMethod();
		private Dictionary<string, CodeMemberMethod> methods = new Dictionary<string, CodeMemberMethod>();
		private CodeStatementCollection prepend = new CodeStatementCollection();
		private CodeAttributeDeclarationCollection assemblyAttributes = new CodeAttributeDeclarationCollection();

		private string fileName;
		private int line;

		#endregion Properties

		#region Getters

		/// <summary>
		/// Return a DOM representation of a script.
		/// </summary>
		public CodeCompileUnit CompileUnit
		{
			get
			{
				var unit = new CodeCompileUnit();

				var space = new CodeNamespace(bcl.Namespace + ".Instance");
				unit.Namespaces.Add(space);

				AddAssemblyAttribute(typeof(CLSCompliantAttribute), true);
				unit.AssemblyCustomAttributes.AddRange(assemblyAttributes);
				assemblyAttributes.Clear();

				var container = new CodeTypeDeclaration(className);
				container.BaseTypes.Add(bcl);
				container.Attributes = MemberAttributes.Private;
				space.Types.Add(container);

				if (!NoTrayIcon)
					prepend.Add(new CodeExpressionStatement((CodeMethodInvokeExpression) InternalMethods.CreateTrayMenu));

				for (int i = 0; i < prepend.Count; i++)
				{
					if (prepend[i] is CodeMethodReturnStatement)
						break;
					main.Statements.Insert(i, prepend[i]);
				}

				int r;

				for (r = prepend.Count; r < main.Statements.Count; r++)
					if (main.Statements[r] is CodeMethodReturnStatement)
						break;

				for (int i = main.Statements.Count - 1; i > r - 1; i--)
					main.Statements.RemoveAt(i);

				prepend.Clear();

				if (persistent)
				{
					var run = new CodeExpressionStatement((CodeMethodInvokeExpression) InternalMethods.Run);
					main.Statements.Add(run);
				}

				var exit = (CodeMethodInvokeExpression) InternalMethods.Exit;
				exit.Parameters.Add(new CodePrimitiveExpression(0));
				main.Statements.Add(new CodeExpressionStatement(exit));

				while (invokes.Count != 0)
					StdLib();

				foreach (var method in methods.Values)
					container.Members.Add(method);

				return unit;
			}
		}

		private CompilerParameters CompilerParameters
		{
			get;
			set;
		}

		#endregion Getters

		#region Methods

		public Parser(CompilerParameters options)
		{
			CompilerParameters = options;
			ScanLibrary();
			main.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(STAThreadAttribute))));
			methods.Add(mainScope, main);
		}

		public CodeCompileUnit Parse(TextReader codeStream)
		{
			return Parse(codeStream, string.Empty);
		}

		public CodeCompileUnit Parse(TextReader codeStream, string name)
		{
			var lines = Read(codeStream, name);
			Statements(lines);
			return CompileUnit;
		}

		#endregion Methods
	}
}