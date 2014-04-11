using System.CodeDom;
using System.Reflection.Emit;

namespace IronAHK.Scripting
{
	partial class MethodWriter
	{
		private LabelMetadata LookupLabel(string Name)
		{
			if (!Labels.ContainsKey(Name))
			{
				// Create the label if it does not exist yet.
				// Remember, labels can be referenced before they are declared

				var Add = new LabelMetadata();
				Add.Label = Generator.DefineLabel();
				Add.Name = Name;
				Add.Resolved = false;

				Labels.Add(Name, Add);

				return Add;
			}
			else return Labels[Name];
		}

		private void EmitGotoStatement(CodeGotoStatement Goto)
		{
			Depth++;
			Debug("Emitting goto: " + Goto.Label);

			LabelMetadata Meta = LookupLabel(Goto.Label);
			Generator.Emit(OpCodes.Br, Meta.Label);
			Meta.From = Goto;

			Depth--;
		}

		private void EmitLabeledStatement(CodeLabeledStatement Labeled)
		{
			Depth++;
			Debug("Marking label: " + Labeled.Label);

			LabelMetadata Meta = LookupLabel(Labeled.Label);

			if (Meta.Resolved)
				throw new CompileException(Labeled, "Can not mark the label " + Meta.Name + " twice");

			Generator.MarkLabel(Meta.Label);
			EmitStatement(Labeled.Statement);

			Meta.Resolved = true;
			Meta.Bound = Labeled.Statement;

			Depth--;
		}

		private void CheckLabelsResolved()
		{
			foreach (var Meta in Labels.Values)
			{
				if (!Meta.Resolved)
					throw new CompileException(Meta.From, "Unresolved label: " + Meta.Name);
			}
		}
	}
}