using _7thHeaven.Code;
using System.Windows.Input;
using static SeventhHeaven.Classes.KeyboardInputSender;

namespace SeventhHeaven.Classes
{
    public class ControlInputSetting
    {
        public int ConfigValue { get; set; }
        public Key? KeyboardKey { get; set; }

        public ScanCodeShort KeyScanCode { get; set; }
        public bool KeyIsExtended { get; set; }

        public GamePadButton? GamepadInput { get; set; }

        public string DisplayText { get; set; }
        public StringKey? TranslationKey { get; set; }

        public ControlInputSetting(string displayText, int val, Key keyboardInput, StringKey? translationKey = null)
        {
            DisplayText = displayText;
            ConfigValue = val;
            KeyboardKey = keyboardInput;
            KeyIsExtended = false;
            TranslationKey = translationKey;
            GamepadInput = null;
        }

        public ControlInputSetting(string displayText, int val, GamePadButton padInput, StringKey? translationKey = null)
        {
            DisplayText = displayText;
            ConfigValue = val;
            KeyboardKey = null;
            KeyIsExtended = false;
            TranslationKey = translationKey;
            GamepadInput = padInput;
        }

    }

}



