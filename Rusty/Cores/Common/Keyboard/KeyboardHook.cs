﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace IronAHK.Rusty.Cores.Common.Keyboard
{
    // ToDo: Raise Events

    /// <summary>
    /// Platform independent keyboard Hook base.
    /// This Class is abstract.
    /// </summary>
    internal abstract class KeyboardHook
    {
        #region Properties

        List<HotkeyDefinition> hotkeys;
        List<HotstringDefinition> hotstrings;
        Dictionary<Keys, bool> pressed;

        StringBuilder history;
        const int retention = 1024;

        public string CurrentHotkey { get; set; }

        public string PriorHotkey { get; set; }

        public int CurrentHotkeyTime { get; set; }

        public int PriorHotkeyTime { get; set; }

        public bool Block { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a Hotkey is pressed
        /// </summary>
        public event EventHandler<HotkeyEventArgs> HotkeyEvent;

        /// <summary>
        /// Raised when a Hotstring is pressed
        /// </summary>
        public event EventHandler<HotstringEventArgs> HotstringEvent;

        #endregion

        #region Constructor/destructor

        public KeyboardHook() {
            hotkeys = new List<HotkeyDefinition>();
            hotstrings = new List<HotstringDefinition>();
            history = new StringBuilder(retention);
            pressed = new Dictionary<Keys, bool>();

            foreach(int i in Enum.GetValues(typeof(Keys)))
                if(!pressed.ContainsKey((Keys)i))
                    pressed.Add((Keys)i, false);

            RegisterHook();
        }

        ~KeyboardHook() {
            DeregisterHook();
        }

        #endregion

        #region Add/remove

        public HotkeyDefinition Add(HotkeyDefinition hotkey) {
            hotkeys.Add(hotkey);
            return hotkey;
        }

        public HotstringDefinition Add(HotstringDefinition hotstring) {
            hotstrings.Add(hotstring);
            return hotstring;
        }

        public void Remove(HotkeyDefinition hotkey) {
            hotkeys.Remove(hotkey);
        }

        public void Remove(HotstringDefinition hotstring) {
            hotstrings.Remove(hotstring);
        }

        #endregion

        #region Conversions

        char Letter(Keys key) {
            // HACK: remove Keys translation (and the overload) since it should be passed from the native handler

            bool caps = (key & Keys.Shift) == Keys.Shift || pressed[Keys.ShiftKey] || pressed[Keys.LShiftKey] || pressed[Keys.RShiftKey];
            key &= ~Keys.Modifiers;

            switch(key) {
                case Keys.Space: return ' ';
                case Keys.Enter: return '\n';
            }

            string letter = key.ToString();

            if(!caps)
                letter = letter.ToLower();

            return letter.Length == 1 ? letter[0] : (char)0;
        }

        #endregion

        #region Hotkey fired

        [Obsolete]
        protected bool KeyReceived(Keys key, bool down) {
            return KeyReceived(key, Letter(key).ToString(), down);
        }

        protected bool KeyReceived(Keys key, string typed, bool down) {
            if(Block)
                return true;

            bool block = false;

            if(Core.Suspended)
                goto next;

            pressed[key] = down;

            var exec = new List<HotkeyDefinition>();

            foreach(var hotkey in hotkeys) {
                bool match = KeyMatch(hotkey.Keys & ~Keys.Modifiers, key) ||
                    hotkey.Typed.Length != 0 && hotkey.Typed.Equals(typed, StringComparison.CurrentCultureIgnoreCase);
                bool up = (hotkey.EnabledOptions & HotkeyDefinition.Options.Up) == HotkeyDefinition.Options.Up;

                if(hotkey.Enabled && match && HasModifiers(hotkey) && up != down) {
                    exec.Add(hotkey);

                    if((hotkey.EnabledOptions & HotkeyDefinition.Options.PassThrough) != HotkeyDefinition.Options.PassThrough)
                        block = true;
                }
            }

            new Thread(delegate()
            {
                foreach(var hotkey in exec) {
                    PriorHotkeyTime = CurrentHotkeyTime;
                    CurrentHotkeyTime = Environment.TickCount;
                    PriorHotkey = CurrentHotkey;
                    CurrentHotkey = hotkey.ToString();

                    if(hotkey.Condition())
                        hotkey.Proc(new object[] { });
                }
            }).Start();

        next:

            if(!down)
                return block;

            #region Sequencing

            if(hotstrings.Count > 0) {
                if(key == Keys.Back && history.Length > 0)
                    history.Remove(history.Length - 1, 1);

                switch(key) {
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Down:
                    case Keys.Up:
                    case Keys.Next:
                    case Keys.Prior:
                    case Keys.Home:
                    case Keys.End:
                        history.Length = 0;
                        break;

                    case Keys.Alt:
                    case Keys.LMenu:
                    case Keys.RMenu:
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        break;

                    default:
                        int d = retention - history.Length;
                        if(d < 0)
                            history.Remove(history.Length + d, -d);
                        history.Append(typed);
                        break;
                }
            }

            if(Core.Suspended)
                return block;

            #endregion

            var expand = new List<HotstringDefinition>();

            foreach(var hotstring in hotstrings) {
                if(hotstring.Enabled && HasConditions(hotstring)) {
                    expand.Add(hotstring);

                    if((hotstring.EnabledOptions & HotstringDefinition.Options.Reset) == HotstringDefinition.Options.Reset)
                        history.Length = 0;
                }
            }

            string trigger = history.Length > 0 ? history[history.Length - 1].ToString() : null;

            foreach(var hotstring in expand) {
                block = true;

                new Thread(delegate()
                {
                    PriorHotkeyTime = CurrentHotkeyTime;
                    CurrentHotkeyTime = Environment.TickCount;
                    PriorHotkey = CurrentHotkey;
                    CurrentHotkey = hotstring.ToString();
                    int length = hotstring.Sequence.Length;
                    bool auto = (hotstring.EnabledOptions & HotstringDefinition.Options.AutoTrigger) == HotstringDefinition.Options.AutoTrigger;

                    if(auto)
                        length--;

                    if((hotstring.EnabledOptions & HotstringDefinition.Options.Backspace) == HotstringDefinition.Options.Backspace && length > 0) {
                        int n = length + 1;
                        history.Remove(history.Length - n, n);
                        Backspace(length);

                        // UNDONE: hook on Windows captures triggering key and blocks it, but X11 allows it through and needs an extra backspace
                        if(!auto && Environment.OSVersion.Platform != PlatformID.Win32NT)
                            Backspace(1);
                    }

                    hotstring.Proc(new object[] { });


                    if((hotstring.EnabledOptions & HotstringDefinition.Options.OmitEnding) == HotstringDefinition.Options.OmitEnding) {
                        if((hotstring.EnabledOptions & HotstringDefinition.Options.Backspace) == HotstringDefinition.Options.Backspace &&
                            (hotstring.EnabledOptions & HotstringDefinition.Options.AutoTrigger) != HotstringDefinition.Options.AutoTrigger) {
                            history.Remove(history.Length - 1, 1);
                            Backspace(1);
                        }
                    } else if(trigger != null && !auto)
                        SendMixed(trigger);
                }).Start();
            }

            return block;
        }

        bool KeyMatch(Keys expected, Keys received) {
            expected &= ~Keys.Modifiers;
            received &= ~Keys.Modifiers;

            if(expected == received)
                return true;

            switch(expected) {
                case Keys.ControlKey:
                    return received == Keys.LControlKey || received == Keys.RControlKey;

                case Keys.ShiftKey:
                    return received == Keys.LShiftKey || received == Keys.RShiftKey;
            }

            return false;
        }

        bool HasModifiers(HotkeyDefinition hotkey) {
            if(hotkey.Extra != Keys.None && !pressed[hotkey.Extra])
                return false;

            if((hotkey.EnabledOptions & HotkeyDefinition.Options.IgnoreModifiers) == HotkeyDefinition.Options.IgnoreModifiers)
                return true;

            bool[,] modifiers = { 
                                       { (hotkey.Keys & Keys.Alt) == Keys.Alt, pressed[Keys.Alt] || pressed[Keys.LMenu] || pressed[Keys.RMenu], (hotkey.Keys & Keys.LMenu) == Keys.LMenu },
                                       { (hotkey.Keys & Keys.Control) == Keys.Control, pressed[Keys.Control] || pressed[Keys.LControlKey] || pressed[Keys.RControlKey], (hotkey.Keys & Keys.ControlKey) == Keys.ControlKey },
                                       { (hotkey.Keys & Keys.Shift) == Keys.Shift, pressed[Keys.Shift] || pressed[Keys.LShiftKey] || pressed[Keys.RShiftKey], (hotkey.Keys & Keys.ShiftKey) == Keys.ShiftKey }
                                   };

            for(int i = 0; i < 3; i++)
                if((modifiers[i, 0] && !modifiers[i, 1]) || (modifiers[i, 1] && !modifiers[i, 0] && !modifiers[i, 2]))
                    return false;

            return true;
        }

        bool HasConditions(HotstringDefinition hotstring) {
            string history = this.history.ToString();

            if(history.Length == 0)
                return false;

            var compare = (hotstring.EnabledOptions & HotstringDefinition.Options.CaseSensitive) == HotstringDefinition.Options.CaseSensitive ?
                StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            int x = history.Length - hotstring.Sequence.Length - 1;

            if((hotstring.EnabledOptions & HotstringDefinition.Options.AutoTrigger) == HotstringDefinition.Options.AutoTrigger) {
                if(!history.EndsWith(hotstring.Sequence, compare))
                    return false;
            } else {
                if(history.Length < hotstring.Sequence.Length + 1)
                    return false;

                if(hotstring.EndChars.IndexOf(history[history.Length - 1]) == -1)
                    return false;

                if(!history.Substring(x--, hotstring.Sequence.Length).Equals(hotstring.Sequence, compare))
                    return false;
            }

            if((hotstring.EnabledOptions & HotstringDefinition.Options.Nested) != HotstringDefinition.Options.Nested)
                if(x > -1 && char.IsLetterOrDigit(history[x]))
                    return false;

            return true;
        }

        #endregion

        #region Send

        public void SendMixed(string sequence) {
            // TODO: modifiers in mixed mode send e.g. ^{a down}

            var buf = new[] { new StringBuilder(sequence.Length), new StringBuilder(16) };
            var scan = false;

            for(var i = 0; i < sequence.Length; i++) {
                var sym = sequence[i];

                switch(sym) {
                    case Core.Keyword_KeyNameOpen:
                        if(scan)
                            goto default;
                        scan = true;
                        break;

                    case Core.Keyword_KeyNameClose: {
                            if(!scan)
                                goto default;

                            var n = i + 1;

                            if(buf[1].Length == 0 && n < sequence.Length && sequence[n] == sym)
                                goto default;

                            scan = false;

                            if(buf[1].Length == 1) {
                                buf[0].Append(buf[1][0]);
                                buf[1].Length = 0;
                            }

                            if(buf[1].Length == 0)
                                continue;

                            switch(buf[1].ToString().ToLowerInvariant()) {
                                case Core.Keyword_Raw:
                                    buf[0].Append(sequence, i, sequence.Length - i);
                                    i = sequence.Length;
                                    break;
                            }

                            SendKey(buf[1].ToString());
                            buf[1].Length = 0;
                        }
                        break;

                    default:
                        buf[scan ? 1 : 0].Append(sym);
                        break;
                }
            }

            if(scan)
                buf[1].Append(Core.Keyword_KeyNameOpen);

            if(buf[1].Length == 1)
                buf[0].Append(buf[1][0]);
            else if(buf[1].Length > 1)
                SendKey(buf[1].ToString());

            if(buf[0].Length != 0)
                Send(buf[0].ToString());
        }

        void SendKey(string name) {
            var key = HotkeyDefinition.ParseKey(name);

            if(key != Keys.None)
                Send(key);
        }

        #endregion

        #region Abstract methods

        protected abstract void RegisterHook();

        protected abstract void DeregisterHook();

        protected internal abstract void Send(string keys);

        protected internal abstract void Send(Keys key);

        protected abstract void Backspace(int n);

        #endregion
    }

    internal class HotkeyEventArgs : EventArgs
    {
        Keys keys;

        public HotkeyEventArgs(Keys keys) {
            this.keys = keys;
        }

        public Keys Keys {
            get { return keys; }
        }
    }

    internal class HotstringEventArgs : EventArgs
    {
        string sequence;

        public HotstringEventArgs() : this(string.Empty) { }

        public HotstringEventArgs(string sequence) {
            this.sequence = sequence;
        }

        public string Sequence {
            get { return sequence; }
        }
    }

}