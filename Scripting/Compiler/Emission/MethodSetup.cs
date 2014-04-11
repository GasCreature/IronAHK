using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace IronAHK.Scripting
{
	partial class MethodWriter
	{
		private ILGenerator Generator;

		private MethodInfo ForceDecimal;
		private MethodInfo ForceLong;
		private MethodInfo ForceInt;
		private MethodInfo ForceBool;
		private MethodInfo ForceString;

		private MethodInfo SetVariable;
		private MethodInfo GetVariable;
		private LocalBuilder VarsProperty;

		private MethodCollection Lookup;
		private ILMirror Mirror;

		public bool IsEntryPoint;
		public MethodBuilder Method;
		public Dictionary<string, MethodWriter> Methods; // Set in TypeEmitter.cs
		public Dictionary<string, Type[]> ParameterTypes;
		public CodeMemberMethod Member;

		private int Depth;

		private Stack<LoopMetadata> Loops;

		private Dictionary<string, LocalBuilder> Locals;
		private Dictionary<string, LabelMetadata> Labels;

		public MethodWriter(TypeBuilder Parent, CodeMemberMethod Member, MethodCollection Lookup, ILMirror Mirror)
		{
			Loops = new Stack<LoopMetadata>();

			this.Member = Member;
			this.Lookup = Lookup;
			this.Mirror = Mirror;

			if (Member is CodeEntryPointMethod)
			{
				Method = Parent.DefineMethod("Main", MethodAttributes.Private | MethodAttributes.Static, typeof(void), null);
				IsEntryPoint = true;
			}
			else Method = Parent.DefineMethod(Member.Name, MethodAttributes.Public | MethodAttributes.Static, typeof(object), new[] { typeof(object[]) });

			Generator = Method.GetILGenerator();

			ForceString = typeof(Script).GetMethod("ForceString");
			ForceDecimal = typeof(Script).GetMethod("ForceDecimal");
			ForceLong = typeof(Script).GetMethod("ForceLong");
			ForceInt = typeof(Script).GetMethod("ForceInt");
			ForceBool = typeof(Script).GetMethod("ForceBool");

			Locals = new Dictionary<string, LocalBuilder>();
			Labels = new Dictionary<string, LabelMetadata>();

			Type Variables = typeof(Script.Variables);

			if (IsEntryPoint)
				GenerateEntryPointHeader(Generator);

			SetVariable = typeof(Script.Variables).GetMethod("SetVariable");
			GetVariable = typeof(Script.Variables).GetMethod("GetVariable");

			MethodInfo GetVars = typeof(Script).GetMethod("get_Vars");

			if (Mirror != null)
			{
				ForceString = Mirror.GrabMethod(ForceString);
				ForceDecimal = Mirror.GrabMethod(ForceDecimal);
				ForceLong = Mirror.GrabMethod(ForceLong);
				ForceInt = Mirror.GrabMethod(ForceInt);
				ForceBool = Mirror.GrabMethod(ForceBool);
				SetVariable = Mirror.GrabMethod(SetVariable);
				GetVariable = Mirror.GrabMethod(GetVariable);
				Variables = Mirror.GrabType(Variables);
				GetVars = Mirror.GrabMethod(GetVars);
			}

			VarsProperty = Generator.DeclareLocal(Variables);
			Generator.Emit(OpCodes.Call, GetVars);
			Generator.Emit(OpCodes.Stloc, VarsProperty);
		}

		private void GenerateEntryPointHeader(ILGenerator Generator)
		{
			ConstructorInfo StatThreadConstructor = typeof(STAThreadAttribute).GetConstructor(Type.EmptyTypes);
			var Attribute = new CustomAttributeBuilder(StatThreadConstructor, new object[] { });
			Method.SetCustomAttribute(Attribute);
		}

		[Conditional("DEBUG")]
		private void Debug(string Message)
		{
			Console.Write(new string(' ', Depth));
			Console.WriteLine(Message);
		}
	}
}