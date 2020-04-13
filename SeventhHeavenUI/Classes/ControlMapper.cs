using _7thHeaven.Code;
using System;
using System.Collections.Generic;
using System.IO;
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
            new ControlInputSetting("BACKSLASH", 43, Key.Oem5, StringKey.Backslash), // Note: Oem5 is backslash on my en-us keyboard but I suspect this will be different for other keyboard layouts/manufacturers
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
            new ControlInputSetting("F11", 87, Key.F11),
            new ControlInputSetting("F12", 88, Key.F12),
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

            new ControlInputSetting("UP", 227, GamePadButton.Up, StringKey.Up),
            new ControlInputSetting("DOWN", 228, GamePadButton.Down, StringKey.Down),
            new ControlInputSetting("LEFT", 229, GamePadButton.Left, StringKey.Left),
            new ControlInputSetting("RIGHT", 230, GamePadButton.Right, StringKey.Right),
            new ControlInputSetting("Button1", 235, GamePadButton.Button1, StringKey.Button1),
            new ControlInputSetting("Button2", 236, GamePadButton.Button2, StringKey.Button2),
            new ControlInputSetting("Button3", 237, GamePadButton.Button3, StringKey.Button3),
            new ControlInputSetting("Button4", 238, GamePadButton.Button4, StringKey.Button4),
            new ControlInputSetting("Button5", 239, GamePadButton.Button5, StringKey.Button5),
            new ControlInputSetting("Button6", 240, GamePadButton.Button6, StringKey.Button6),
            new ControlInputSetting("Button7", 241, GamePadButton.Button7, StringKey.Button7),
            new ControlInputSetting("Button8", 242, GamePadButton.Button8, StringKey.Button8),
            new ControlInputSetting("Button9", 243, GamePadButton.Button9, StringKey.Button9),
            new ControlInputSetting("Button10", 244, GamePadButton.Button10, StringKey.Button10),
        };

        public static ControlInputSetting GetControlInputFromKey(Key keyboardInput)
        {
            return ControlInputs.Where(c => c.KeyboardKey.HasValue && c.KeyboardKey.Value == keyboardInput).FirstOrDefault();
        }

        public static ControlInputSetting GetControlInputFromButton(GamePadButton button)
        {
            return ControlInputs.Where(c => c.GamepadInput.HasValue && c.GamepadInput.Value == button).FirstOrDefault();
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
            { GameControl.Menu, 0x11 },
            { GameControl.Ok, 0x15 },
            { GameControl.Cancel, 0x19 },
            { GameControl.Switch, 0x1D },
            { GameControl.Assist, 0x21 },
            { GameControl.Start, 0x2D },
            { GameControl.Up, 0x31 },
            { GameControl.Right, 0x35 },
            { GameControl.Down, 0x39 },
            { GameControl.Left, 0x3D }
        };

        public static Dictionary<GameControl, int> ControlGamepadOffsets = new Dictionary<GameControl, int>()
        {
            { GameControl.Camera, 0x65 },
            { GameControl.Target, 0x69 },
            { GameControl.Pageup, 0x6D },
            { GameControl.Pagedown, 0x71 },
            { GameControl.Menu, 0x75 },
            { GameControl.Ok, 0x79 },
            { GameControl.Cancel, 0x7D },
            { GameControl.Switch, 0x81 },
            { GameControl.Assist, 0x85 },
            { GameControl.Start, 0x91 },
            { GameControl.Up, 0x95 },
            { GameControl.Right, 0x99 },
            { GameControl.Down, 0x9D },
            { GameControl.Left, 0xA1 }
        };

        public static ControlConfiguration LoadConfigurationFromFile(string pathToFile)
        {
            ControlConfiguration loaded = new ControlConfiguration();

            byte[] fileBytes = File.ReadAllBytes(pathToFile);

            loaded.KeyboardInputs.Add(GameControl.Camera, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Camera]]));
            loaded.KeyboardInputs.Add(GameControl.Target, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Target]]));
            loaded.KeyboardInputs.Add(GameControl.Pageup, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Pageup]]));
            loaded.KeyboardInputs.Add(GameControl.Pagedown, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Pagedown]]));
            loaded.KeyboardInputs.Add(GameControl.Menu, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Menu]]));
            loaded.KeyboardInputs.Add(GameControl.Ok, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Ok]]));
            loaded.KeyboardInputs.Add(GameControl.Cancel, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Cancel]]));
            loaded.KeyboardInputs.Add(GameControl.Switch, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Switch]]));
            loaded.KeyboardInputs.Add(GameControl.Assist, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Assist]]));
            loaded.KeyboardInputs.Add(GameControl.Start, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Start]]));
            loaded.KeyboardInputs.Add(GameControl.Up, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Up]]));
            loaded.KeyboardInputs.Add(GameControl.Right, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Right]]));
            loaded.KeyboardInputs.Add(GameControl.Down, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Down]]));
            loaded.KeyboardInputs.Add(GameControl.Left, GetControlInputFromConfigValue(fileBytes[ControlKeyboardOffsets[GameControl.Left]]));

            loaded.GamepadInputs.Add(GameControl.Camera, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Camera]]));
            loaded.GamepadInputs.Add(GameControl.Target, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Target]]));
            loaded.GamepadInputs.Add(GameControl.Pageup, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Pageup]]));
            loaded.GamepadInputs.Add(GameControl.Pagedown, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Pagedown]]));
            loaded.GamepadInputs.Add(GameControl.Menu, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Menu]]));
            loaded.GamepadInputs.Add(GameControl.Ok, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Ok]]));
            loaded.GamepadInputs.Add(GameControl.Cancel, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Cancel]]));
            loaded.GamepadInputs.Add(GameControl.Switch, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Switch]]));
            loaded.GamepadInputs.Add(GameControl.Assist, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Assist]]));
            loaded.GamepadInputs.Add(GameControl.Start, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Start]]));
            loaded.GamepadInputs.Add(GameControl.Up, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Up]]));
            loaded.GamepadInputs.Add(GameControl.Right, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Right]]));
            loaded.GamepadInputs.Add(GameControl.Down, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Down]]));
            loaded.GamepadInputs.Add(GameControl.Left, GetControlInputFromConfigValue(fileBytes[ControlGamepadOffsets[GameControl.Left]]));

            return loaded;
        }

        public static bool SaveConfigurationToFile(string pathToFile, ControlConfiguration configToSave)
        {
            // ensure file exists
            if (!File.Exists(pathToFile))
            {
                return false;
            }

            byte[] fileBytes = File.ReadAllBytes(pathToFile);

            fileBytes[ControlKeyboardOffsets[GameControl.Camera]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Camera].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Target]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Target].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Pageup]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Pageup].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Pagedown]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Pagedown].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Menu]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Menu].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Ok]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Ok].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Cancel]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Cancel].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Switch]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Switch].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Assist]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Assist].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Start]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Start].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Up]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Up].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Right]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Right].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Down]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Down].ConfigValue);
            fileBytes[ControlKeyboardOffsets[GameControl.Left]] = Convert.ToByte(configToSave.KeyboardInputs[GameControl.Left].ConfigValue);

            fileBytes[ControlGamepadOffsets[GameControl.Camera]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Camera].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Target]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Target].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Pageup]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Pageup].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Pagedown]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Pagedown].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Menu]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Menu].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Ok]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Ok].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Cancel]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Cancel].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Switch]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Switch].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Assist]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Assist].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Start]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Start].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Up]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Up].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Right]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Right].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Down]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Down].ConfigValue);
            fileBytes[ControlGamepadOffsets[GameControl.Left]] = Convert.ToByte(configToSave.GamepadInputs[GameControl.Left].ConfigValue);

            File.WriteAllBytes(pathToFile, fileBytes);

            return true;
        }

        public static bool CopyConfigurationFileAndSaveAsNew(string pathToExistingFile, string newConfigName, ControlConfiguration configToSave)
        {
            // ensure file to copy exists
            if (!File.Exists(pathToExistingFile))
            {
                return false;
            }

            FileInfo existingFile = new FileInfo(pathToExistingFile);
            string newFilePath = Path.Combine(existingFile.DirectoryName, newConfigName);

            // ensure file with same name doesn't already exist
            if (File.Exists(newFilePath))
            {
                return false;
            }

            File.Copy(pathToExistingFile, newFilePath);

            return SaveConfigurationToFile(newFilePath, configToSave);
        }

    }

    public enum GamePadButton
    {
        Up,
        Down,
        Left,
        Right,
        Button1,
        Button2,
        Button3,
        Button4,
        Button5,
        Button6,
        Button7,
        Button8,
        Button9,
        Button10
    }

    public class ControlInputSetting
    {
        public int ConfigValue { get; set; }
        public Key? KeyboardKey { get; set; }

        public GamePadButton? GamepadInput { get; set; }

        public string DisplayText { get; set; }
        public StringKey? TranslationKey { get; set; }

        public ControlInputSetting(string displayText, int val, Key keyboardInput, StringKey? translationKey = null)
        {
            DisplayText = displayText;
            ConfigValue = val;
            KeyboardKey = keyboardInput;
            TranslationKey = translationKey;
            GamepadInput = null;
        }

        public ControlInputSetting(string displayText, int val, GamePadButton padInput, StringKey? translationKey = null)
        {
            DisplayText = displayText;
            ConfigValue = val;
            KeyboardKey = null;
            TranslationKey = translationKey;
            GamepadInput = padInput;
        }

    }

}



