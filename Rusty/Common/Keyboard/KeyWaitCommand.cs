using System;
using System.Threading;
using System.Windows.Forms;

namespace IronAHK.Rusty.Common
{
	partial class Keyboard
	{
		/// <summary>
		/// Command which holds the current Thread until requested Key was pressed
		/// </summary>
		public class KeyWaitCommand
		{
			#region Fields

			private readonly KeyboardHook _keyboardHook;

			private bool abort = false;
			private object abortLock = new object();

			private System.Threading.Timer _timeoutTimer;
			private int? _timeOutVal = null;
			private bool _triggerOnKeyDown = false;
			private Keys _endKey;

			#endregion Fields

			#region Constructor

			internal KeyWaitCommand(KeyboardHook keyboardHook)
			{
				if (keyboardHook == null)
					throw new ArgumentNullException("keyboardHook");

				_keyboardHook = keyboardHook;
			}

			#endregion Constructor

			#region Public Methods

			/// <summary>
			/// Wait until the key gets pressed.
			/// </summary>
			/// <param name="k"></param>
			public void Wait(Keys k)
			{
				if (k == Keys.None)
					throw new ArgumentException("Must submit valid Key definition!");
				_endKey = k;

				if (this.TimeOutVal.HasValue)
				{
					_timeoutTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTimoutTick));
					_timeoutTimer.Change(this.TimeOutVal.Value, Timeout.Infinite);
				}
				_keyboardHook.IAKeyEvent += OnKeyEvent;

				while (true)
				{
					lock (abortLock)
					{
						if (abort)
							break;
					}
					Application.DoEvents(); // This is necessary if the Wait Method gets called on the Main GUI Thread
					Thread.Sleep(2);
				}
				_keyboardHook.IAKeyEvent -= OnKeyEvent;
			}

			#endregion Public Methods

			#region Properties

			/// <summary>
			/// Define the Timeout when the wait process should timeout.
			/// Default is null, means no timeout.
			/// </summary>
			public int? TimeOutVal
			{
				get
				{
					return _timeOutVal;
				}
				set
				{
					_timeOutVal = value;
				}
			}

			/// <summary>
			/// Trigger on the key Down Event. Default is false, what means Key Up event must be fired.
			/// </summary>
			public bool TriggerOnKeyDown
			{
				get
				{
					return _triggerOnKeyDown;
				}
				set
				{
					_triggerOnKeyDown = value;
				}
			}

			#endregion Properties

			#region Private Helper Methods

			private void Abort()
			{
				lock (abortLock)
				{
					abort = true;
				}
			}

			#endregion Private Helper Methods

			#region Event Handlers

			private void OnTimoutTick(object state)
			{
				var timeoutTimer = (System.Threading.Timer) state;
				timeoutTimer.Dispose();
				Abort();
			}

			/// <summary>
			/// Occurs when a Key is pressed/released
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			private void OnKeyEvent(object sender, IAKeyEventArgs e)
			{
				if (e.Block || e.Handeled)
					return;

				if ((e.Keys == _endKey) && (e.Down == TriggerOnKeyDown))
				{
					Abort();
				}
			}

			#endregion Event Handlers
		}
	}
}