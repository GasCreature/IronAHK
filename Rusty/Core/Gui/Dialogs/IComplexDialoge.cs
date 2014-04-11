using System;

namespace IronAHK.Rusty
{
	internal interface IComplexDialoge
	{
		string MainText
		{
			get;
			set;
		}

		string Subtext
		{
			get;
			set;
		}

		string Title
		{
			get;
			set;
		}

		#region Form

		//DialogResult ShowDialog();
		void Show();

		void Close();

		void Dispose();

		bool Visible
		{
			get;
			set;
		}

		bool TopMost
		{
			get;
			set;
		}

		#region Invokable

		bool InvokeRequired
		{
			get;
		}

		object Invoke(Delegate Method, params object[] obj);

		object Invoke(Delegate Method);

		#endregion Invokable

		#endregion Form
	}
}