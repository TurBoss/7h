using _7thHeaven.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SeventhHeaven.Classes
{
    public enum GameControl
    {
        Ok,
        Cancel,
        Menu,
        Switch,
        Pageup,
        Pagedown,
        Camera,
        Target,
        Assist,
        Start,
        Up,
        Down,
        Left,
        Right
    }

    public class ControlMapper
    {
        public static List<ControlInputSetting> ControlInputs = new List<ControlInputSetting>()
        {
            new ControlInputSetting("ESC", 1, Key.Escape, StringKey.Esc),
            new ControlInputSetting("1", 2, Key.D1, StringKey.D1),
            new ControlInputSetting("2", 3, Key.D2, StringKey.D2),
            new ControlInputSetting("3", 4, Key.D3, StringKey.D3),
            new ControlInputSetting("4", 5, Key.D4, StringKey.D4),
            new ControlInputSetting("5", 6, Key.D5, StringKey.D5),
            new ControlInputSetting("6", 7, Key.D6, StringKey.D6),
            new ControlInputSetting("7", 8, Key.D7, StringKey.D7),
            new ControlInputSetting("8", 9, Key.D8, StringKey.D8),
            new ControlInputSetting("9", 10, Key.D9, StringKey.D9),
            new ControlInputSetting("0", 11, Key.D0, StringKey.D0),
            new ControlInputSetting("MINUS", 12, Key.OemMinus, StringKey.Minus),
            new ControlInputSetting("EQUALS", 13, Key.OemPlus, StringKey.Equal),
            new ControlInputSetting("BACKSPACE", 14, Key.Back, StringKey.Backspace),
            new ControlInputSetting("TAB", 15, Key.Tab, StringKey.Tab),
            new ControlInputSetting("Q", 16, Key.Q),
            new ControlInputSetting("W", 17, Key.W),
            new ControlInputSetting("E", 18, Key.E),
            new ControlInputSetting("R", 19, Key.R),
            new ControlInputSetting("T", 20, Key.T),
            new ControlInputSetting("Y", 21, Key.Y),
            new ControlInputSetting("U", 22, Key.U),
            new ControlInputSetting("I", 23, Key.I),
            new ControlInputSetting("O", 24, Key.O),
            new ControlInputSetting("P", 25, Key.P),
            new ControlInputSetting("LEFTBRACKET", 26, Key.OemOpenBrackets, StringKey.LeftBracket),
            new ControlInputSetting("RIGHTBRACKET", 27, Key.OemCloseBrackets, StringKey.RightBracket),
            new ControlInputSetting("RETURN", 28, Key.Return, StringKey.Return),
            new ControlInputSetting("LEFTCTRL", 29, Key.LeftCtrl, StringKey.LeftCtrl),
            new ControlInputSetting("A", 30, Key.A),
            new ControlInputSetting("S", 31, Key.S),
            new ControlInputSetting("D", 32, Key.D),
            new ControlInputSetting("F", 33, Key.F),
            new ControlInputSetting("G", 34, Key.G),
            new ControlInputSetting("H", 35, Key.H),
            new ControlInputSetting("J", 36, Key.J),
            new ControlInputSetting("K", 37, Key.K),
            new ControlInputSetting("L", 38, Key.L),
            new ControlInputSetting("SEMICOLON", 39, Key.OemSemicolon, StringKey.Semicolon),
            new ControlInputSetting("APOSTROPHE", 40, Key.OemQuotes, StringKey.Apostrophe),
            new ControlInputSetting("BACKQUOTE", 41, Key.OemTilde, StringKey.Backquote),
            new ControlInputSetting("LEFTSHIFT", 42, Key.LeftShift, StringKey.LeftShift),
            new ControlInputSetting("BACKSLASH", 43, Key.OemBackslash, StringKey.Backslash),
            new ControlInputSetting("Z", 44, Key.Z),
            new ControlInputSetting("X", 45, Key.X),
            new ControlInputSetting("C", 46, Key.C),
            new ControlInputSetting("V", 47, Key.V),
            new ControlInputSetting("B", 48, Key.B),
            new ControlInputSetting("N", 49, Key.N),
            new ControlInputSetting("M", 50, Key.M),
            new ControlInputSetting("COMMA", 51, Key.OemComma, StringKey.Comma),
            new ControlInputSetting("PERIOD", 52, Key.OemPeriod, StringKey.Period),
            new ControlInputSetting("SLASH (FORWARD)", 53, Key.OemQuestion, StringKey.Slash),
            new ControlInputSetting("RIGHTSHIFT", 54, Key.RightShift, StringKey.RightShift),
            new ControlInputSetting("NUMPADMULTIPLY", 55, Key.Multiply, StringKey.NumpadMultiply),
            new ControlInputSetting("LEFTALT", 56, Key.LeftAlt, StringKey.LeftAlt),
            new ControlInputSetting("SPACE", 57, Key.Space, StringKey.Space),
            new ControlInputSetting("CAPS", 58, Key.CapsLock, StringKey.Caps),
            new ControlInputSetting("F1", 59, Key.F1),
            new ControlInputSetting("F2", 60, Key.F2),
            new ControlInputSetting("F3", 61, Key.F3),
            new ControlInputSetting("F4", 62, Key.F4),
            new ControlInputSetting("F5", 63, Key.F5),
            new ControlInputSetting("F6", 64, Key.F6),
            new ControlInputSetting("F7", 65, Key.F7),
            new ControlInputSetting("F8", 66, Key.F8),
            new ControlInputSetting("F9", 67, Key.F9),
            new ControlInputSetting("F10", 68, Key.F10),
            new ControlInputSetting("NUMLOCK", 69, Key.NumLock, StringKey.Numlock),
            new ControlInputSetting("SCROLLLOCK", 70, Key.Scroll, StringKey.Scrolllock),
            new ControlInputSetting("NUMPAD7", 71, Key.NumPad7, StringKey.Numpad7),
            new ControlInputSetting("NUMPAD8", 72, Key.NumPad8, StringKey.Numpad8),
            new ControlInputSetting("NUMPAD9", 73, Key.NumPad9, StringKey.Numpad9),
            new ControlInputSetting("NUMPADSUBTRACT", 74, Key.Subtract, StringKey.NumpadSubtract),
            new ControlInputSetting("NUMPAD4", 75, Key.NumPad4, StringKey.Numpad0),
            new ControlInputSetting("NUMPAD5", 76, Key.NumPad5, StringKey.Numpad5),
            new ControlInputSetting("NUMPAD6", 77, Key.NumPad6, StringKey.Numpad6),
            new ControlInputSetting("NUMPADADD", 78, Key.Add, StringKey.NumpadAdd),
            new ControlInputSetting("NUMPAD1", 79, Key.NumPad1, StringKey.Numpad1),
            new ControlInputSetting("NUMPAD2", 80, Key.NumPad2, StringKey.Numpad2),
            new ControlInputSetting("NUMPAD3", 81, Key.NumPad3, StringKey.Numpad3),
            new ControlInputSetting("NUMPAD0", 82, Key.NumPad0, StringKey.Numpad0),
            new ControlInputSetting("NUMPADDECIMAL", 83, Key.Decimal, StringKey.NumpadDecimal),
            new ControlInputSetting("F11", 87, Key.F10),
            new ControlInputSetting("F12", 88, Key.F10),
            new ControlInputSetting("NUMPADENTER", 156, Key.Enter, StringKey.NumpadEnter),
            new ControlInputSetting("RIGHTCTRL", 157, Key.RightCtrl, StringKey.RightCtrl),
            new ControlInputSetting("NUMPADDIVIDE", 181, Key.Divide, StringKey.NumpadDivide),
            new ControlInputSetting("PRTSCN", 183, Key.PrintScreen, StringKey.PrtScn),
            new ControlInputSetting("RIGHTALT", 184, Key.RightAlt, StringKey.RightAlt),
            new ControlInputSetting("PAUSEBREAK", 197, Key.Pause, StringKey.PauseBreak),
            new ControlInputSetting("HOME", 199, Key.Home, StringKey.Home),
            new ControlInputSetting("UP", 200, Key.Up, StringKey.Up),
            new ControlInputSetting("PAGEUP", 201, Key.PageUp, StringKey.PageUp),
            new ControlInputSetting("LEFT", 203, Key.Left, StringKey.Left),
            new ControlInputSetting("RIGHT", 205, Key.Right, StringKey.Right),
            new ControlInputSetting("END", 207, Key.End, StringKey.End),
            new ControlInputSetting("DOWN", 208, Key.Down, StringKey.Down),
            new ControlInputSetting("PAGEDOWN", 209, Key.PageDown, StringKey.PageDown),
            new ControlInputSetting("INSERT", 210, Key.Insert, StringKey.Insert),
            new ControlInputSetting("DELETE", 211, Key.Delete, StringKey.Delete),
            new ControlInputSetting("LEFTWINKEY", 219, Key.LWin, StringKey.LeftWinKey),
            new ControlInputSetting("RIGHTWINKEY", 220, Key.RWin, StringKey.RightWinKey),
            new ControlInputSetting("APPS (CONTEXT MENU)", 221, Key.Apps, StringKey.Apps),
            new ControlInputSetting("MOUSE_B1", 223, MouseButton.Left, StringKey.MouseB1),
            new ControlInputSetting("MOUSE_B2", 224, MouseButton.Middle, StringKey.MouseB2),
            new ControlInputSetting("MOUSE_B3", 225, MouseButton.Right, StringKey.MouseB3),
        };

        public static ControlInputSetting GetControlInputFromKey(Key keyboardInput)
        {
            return ControlInputs.Where(c => c.KeyboardKey.HasValue && c.KeyboardKey.Value == keyboardInput).FirstOrDefault();
        }

        public static ControlInputSetting GetControlInputFromConfigValue(int configValue)
        {
            return ControlInputs.Where(c => c.ConfigValue == configValue).FirstOrDefault();
        }

        public static Dictionary<GameControl, int> ControlKeyboardOffsets = new Dictionary<GameControl, int>()
        {
            { GameControl.Camera, 0x01 },
            { GameControl.Target, 0x05 },
            { GameControl.Pageup, 0x09 },
            { GameControl.Pagedown, 0x0D },
            { GameControl.Menu, 11 },
            { GameControl.Ok, 15 },
            { GameControl.Cancel, 19 },
            { GameControl.Switch, 0x1D },
            { GameControl.Assist, 21 },
            { GameControl.Start, 0x2D },
            { GameControl.Up, 31 },
            { GameControl.Right, 35 },
            { GameControl.Down, 39 },
            { GameControl.Left, 0x3D }
        };

        public static Dictionary<GameControl, int> ControlGamepadOffsets = new Dictionary<GameControl, int>()
        {
            { GameControl.Camera, 65 },
            { GameControl.Target, 69 },
            { GameControl.Pageup, 0x6D },
            { GameControl.Pagedown, 71 },
            { GameControl.Menu, 75 },
            { GameControl.Ok, 79 },
            { GameControl.Cancel, 0x7D },
            { GameControl.Switch, 81 },
            { GameControl.Assist, 85 },
            { GameControl.Start, 91 },
            { GameControl.Up, 95 },
            { GameControl.Right, 99 },
            { GameControl.Down, 0x9D },
            { GameControl.Left, 0xA1 }
        };

    }

    public class ControlConfiguration
    {
        public Dictionary<GameControl, ControlInputSetting> KeyboardInputs;
        public Dictionary<GameControl, ControlInputSetting> GamepadInputs;

        public ControlConfiguration()
        {
            KeyboardInputs = new Dictionary<GameControl, ControlInputSetting>();
            GamepadInputs = new Dictionary<GameControl, ControlInputSetting>();
        }

        internal void Set(GameControl control, Key key)
        {
            if (KeyboardInputs.ContainsKey(control))
            {
                KeyboardInputs[control] = ControlMapper.GetControlInputFromKey(key);
            }
            else
            {
                KeyboardInputs.Add(control, ControlMapper.GetControlInputFromKey(key));
            }
        }
    }

    public class ControlInputSetting
    {
        public int ConfigValue { get; set; }
        public Key? KeyboardKey { get; set; }

        public MouseButton? MouseButtonKey { get; set; }

        public string DisplayText { get; set; }
        public StringKey? TranslationKey { get; set; }

        public ControlInputSetting(string displayText, int val, Key keyboardInput, StringKey? translationKey = null)
        {
            DisplayText = displayText;
            ConfigValue = val;
            KeyboardKey = keyboardInput;
            TranslationKey = translationKey;
            MouseButtonKey = null;
        }

        public ControlInputSetting(string displayText, int val, MouseButton mouseInput, StringKey? translationKey = null)
        {
            DisplayText = displayText;
            ConfigValue = val;
            KeyboardKey = null;
            TranslationKey = translationKey;
            MouseButtonKey = mouseInput;
        }
    }
}
