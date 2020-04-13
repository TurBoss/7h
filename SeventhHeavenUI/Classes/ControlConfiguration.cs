using EasyHook;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace SeventhHeaven.Classes
{
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
            RemoveExistingBinding(key); // check if this key is binded to another control and remove it there first

            if (KeyboardInputs.ContainsKey(control))
            {
                KeyboardInputs[control] = ControlMapper.GetControlInputFromKey(key);
            }
            else
            {
                KeyboardInputs.Add(control, ControlMapper.GetControlInputFromKey(key));
            }
        }

        internal void Set(GameControl control, ControlInputSetting newSetting)
        {
            RemoveExistingBinding(newSetting.ConfigValue); // check if this key is binded to another control and remove it there first


            if (KeyboardInputs.ContainsKey(control))
            {
                KeyboardInputs[control] = newSetting;
            }
            else
            {
                KeyboardInputs.Add(control, newSetting);
            }
        }

        private void RemoveExistingBinding(Key key)
        {
            if (KeyboardInputs.Any(kv => kv.Value != null && kv.Value.KeyboardKey.HasValue && kv.Value.KeyboardKey.Value == key))
            {
                GameControl existingBinding = KeyboardInputs.Where(kv => kv.Value != null && kv.Value.KeyboardKey.HasValue && kv.Value.KeyboardKey.Value == key)
                                                            .Select(kv => kv.Key)
                                                            .FirstOrDefault();
                KeyboardInputs[existingBinding] = null;
            }
        }

        private void RemoveExistingBinding(int configVal)
        {
            if (KeyboardInputs.Any(kv => kv.Value != null && kv.Value.ConfigValue == configVal))
            {
                GameControl existingBinding = KeyboardInputs.Where(kv => kv.Value != null && kv.Value.ConfigValue == configVal)
                                                            .Select(kv => kv.Key)
                                                            .FirstOrDefault();
                KeyboardInputs[existingBinding] = null;
            }
        }
    }

}



