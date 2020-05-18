using Iros._7th.Workshop;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XInputDotNetPure;
using static SeventhHeaven.Classes.KeyboardInputSender;

namespace SeventhHeaven.Classes
{
    /// <summary>
    /// Class contains logic to handle intercepting XInput devices for controls that are not supported natively by FF7.
    /// These controls are D-pad and the left/right trigger (on standard ds4 or xbox controllers; but named 'button 11' and 'button 12')
    /// </summary>
    internal class ControllerInterceptor
    {
        public bool PollingInput { get; set; }

        /// <summary>
        /// Returns the <see cref="PlayerIndex"/> of the first controller detected. Returns null if no controllers are connected
        /// </summary>
        /// <returns></returns>
        internal static PlayerIndex? GetConnectedController()
        {
            if (GamePad.GetState(PlayerIndex.One).IsConnected)
            {
                return PlayerIndex.One;
            }
            else if (GamePad.GetState(PlayerIndex.Two).IsConnected)
            {
                return PlayerIndex.Two;
            }
            else if (GamePad.GetState(PlayerIndex.Three).IsConnected)
            {
                return PlayerIndex.Three;
            }
            else if (GamePad.GetState(PlayerIndex.Four).IsConnected)
            {
                return PlayerIndex.Four;
            }

            return null;
        }

        /// <summary>
        /// Polls for game pad input and maps the non-supported buttons to the keyboard binding.
        /// Non-supported buttons include D-pad, Left-stick click, Right-stick click
        /// </summary>
        internal Task PollForGamepadInput()
        {
            if (PollingInput)
            {
                return null; // already polling so just return
            }

            PollingInput = true;

            bool wasUpPressed = false;
            bool wasDownPressed = false;
            bool wasLeftPressed = false;
            bool wasRightPressed = false;
            bool wasLeftTriggerPressed = false;
            bool wasRightTriggerPressed = false;


            return Task.Factory.StartNew(() =>
            {
                ControlConfiguration loadedConfig = ControlMapper.LoadConfigurationFromFile(Path.Combine(Sys.PathToControlsFolder, Sys.Settings.GameLaunchSettings.InGameConfigOption));

                bool hasDpadBinded = loadedConfig.GamepadInputs.Values.Any(i => i?.GamepadInput.Value == GamePadButton.DPadUp ||
                                                                                i?.GamepadInput.Value == GamePadButton.DPadDown ||
                                                                                i?.GamepadInput.Value == GamePadButton.DPadLeft ||
                                                                                i?.GamepadInput.Value == GamePadButton.DPadRight);


                ScanCodeShort? upKey = null;
                bool upIsExtended = false;
                ScanCodeShort? downKey = null;
                bool downIsExtended = false;
                ScanCodeShort? leftKey = null;
                bool leftIsExtended = false;
                ScanCodeShort? rightKey = null;
                bool rightIsExtended = false;

                ScanCodeShort? leftTriggerKey = null;
                bool leftTriggerIsExtended = false;
                ScanCodeShort? rightTriggerKey = null;
                bool rightTriggerIsExtended = false;

                GameControl bindedControl;

                if (loadedConfig.GamepadInputs.Any(kv => kv.Value?.GamepadInput.Value == GamePadButton.LeftTrigger))
                {
                    bindedControl = loadedConfig.GamepadInputs.Where(kv => kv.Value?.GamepadInput.Value == GamePadButton.LeftTrigger).Select(kv => kv.Key).FirstOrDefault();
                    leftTriggerKey = loadedConfig.KeyboardInputs[bindedControl].KeyScanCode;
                    leftTriggerIsExtended = loadedConfig.KeyboardInputs[bindedControl].KeyIsExtended;
                }

                if (loadedConfig.GamepadInputs.Any(kv => kv.Value?.GamepadInput.Value == GamePadButton.RightTrigger))
                {
                    bindedControl = loadedConfig.GamepadInputs.Where(kv => kv.Value?.GamepadInput.Value == GamePadButton.RightTrigger).Select(kv => kv.Key).FirstOrDefault();
                    rightTriggerKey = loadedConfig.KeyboardInputs[bindedControl].KeyScanCode;
                    rightTriggerIsExtended = loadedConfig.KeyboardInputs[bindedControl].KeyIsExtended;
                }


                if (hasDpadBinded)
                {
                    if (loadedConfig.GamepadInputs.Any(kv => kv.Value?.GamepadInput.Value == GamePadButton.DPadUp))
                    {
                        bindedControl = loadedConfig.GamepadInputs.Where(kv => kv.Value?.GamepadInput.Value == GamePadButton.DPadUp).Select(kv => kv.Key).FirstOrDefault();
                        upKey = loadedConfig.KeyboardInputs[bindedControl].KeyScanCode;
                        upIsExtended = loadedConfig.KeyboardInputs[bindedControl].KeyIsExtended;
                    }

                    if (loadedConfig.GamepadInputs.Any(kv => kv.Value?.GamepadInput.Value == GamePadButton.DPadDown))
                    {
                        bindedControl = loadedConfig.GamepadInputs.Where(kv => kv.Value?.GamepadInput.Value == GamePadButton.DPadDown).Select(kv => kv.Key).FirstOrDefault();
                        downKey = loadedConfig.KeyboardInputs[bindedControl].KeyScanCode;
                        downIsExtended = loadedConfig.KeyboardInputs[bindedControl].KeyIsExtended;
                    }

                    if (loadedConfig.GamepadInputs.Any(kv => kv.Value?.GamepadInput.Value == GamePadButton.DPadLeft))
                    {
                        bindedControl = loadedConfig.GamepadInputs.Where(kv => kv.Value?.GamepadInput.Value == GamePadButton.DPadLeft).Select(kv => kv.Key).FirstOrDefault();
                        leftKey = loadedConfig.KeyboardInputs[bindedControl].KeyScanCode;
                        leftIsExtended = loadedConfig.KeyboardInputs[bindedControl].KeyIsExtended;
                    }

                    if (loadedConfig.GamepadInputs.Any(kv => kv.Value?.GamepadInput.Value == GamePadButton.DPadRight))
                    {
                        bindedControl = loadedConfig.GamepadInputs.Where(kv => kv.Value?.GamepadInput.Value == GamePadButton.DPadRight).Select(kv => kv.Key).FirstOrDefault();
                        rightKey = loadedConfig.KeyboardInputs[bindedControl].KeyScanCode;
                        rightIsExtended = loadedConfig.KeyboardInputs[bindedControl].KeyIsExtended;
                    }
                }
                else
                {
                    // user has not binded their DPAD buttons to any controls so just assume it is going to be used for movement
                    upKey = loadedConfig.KeyboardInputs[GameControl.Up].KeyScanCode;
                    upIsExtended = loadedConfig.KeyboardInputs[GameControl.Up].KeyIsExtended;

                    downKey = loadedConfig.KeyboardInputs[GameControl.Down].KeyScanCode;
                    downIsExtended = loadedConfig.KeyboardInputs[GameControl.Down].KeyIsExtended;

                    leftKey = loadedConfig.KeyboardInputs[GameControl.Left].KeyScanCode;
                    leftIsExtended = loadedConfig.KeyboardInputs[GameControl.Left].KeyIsExtended;

                    rightKey = loadedConfig.KeyboardInputs[GameControl.Right].KeyScanCode;
                    rightIsExtended = loadedConfig.KeyboardInputs[GameControl.Right].KeyIsExtended;
                }

                PlayerIndex? connectedController = GetConnectedController();

                while (PollingInput)
                {
                    if (connectedController == null)
                    {
                        // null means no connected controller found so just sleep for a little and check back later
                        Thread.Sleep(1000);
                        DS4ControllerService.Instance?.RootHub?.HotPlug();
                        connectedController = GetConnectedController();
                        continue;
                    }

                    GamePadState state = GamePad.GetState(connectedController.Value);

                    if (!state.IsConnected)
                    {
                        connectedController = null;
                        continue;
                    }

                    if (upKey != null && state.DPad.Up == ButtonState.Pressed && !wasUpPressed)
                    {
                        wasUpPressed = true;
                        SendKey(upKey.Value, upIsExtended);
                    }
                    else if (upKey != null && state.DPad.Up == ButtonState.Released && wasUpPressed)
                    {
                        ReleaseKey(upKey.Value, upIsExtended);
                        wasUpPressed = false;
                    }

                    if (downKey != null && state.DPad.Down == ButtonState.Pressed && !wasDownPressed)
                    {
                        wasDownPressed = true;
                        SendKey(downKey.Value, downIsExtended);
                    }
                    else if (downKey != null && state.DPad.Down == ButtonState.Released && wasDownPressed)
                    {
                        ReleaseKey(downKey.Value, downIsExtended);
                        wasDownPressed = false;
                    }

                    if (leftKey != null && state.DPad.Left == ButtonState.Pressed && !wasLeftPressed)
                    {
                        wasLeftPressed = true;
                        SendKey(leftKey.Value, leftIsExtended);
                    }
                    else if (leftKey != null && state.DPad.Left == ButtonState.Released && wasLeftPressed)
                    {
                        ReleaseKey(leftKey.Value, leftIsExtended);
                        wasLeftPressed = false;
                    }

                    if (rightKey != null && state.DPad.Right == ButtonState.Pressed && !wasRightPressed)
                    {
                        wasRightPressed = true;
                        SendKey(rightKey.Value, rightIsExtended);
                    }
                    else if (rightKey != null && state.DPad.Right == ButtonState.Released && wasRightPressed)
                    {
                        ReleaseKey(rightKey.Value, rightIsExtended);
                        wasRightPressed = false;
                    }

                    if (leftTriggerKey != null && state.Triggers.Left > 0 && !wasLeftTriggerPressed)
                    {
                        wasLeftTriggerPressed = true;
                        SendKey(leftTriggerKey.Value, leftTriggerIsExtended);
                    }
                    else if (leftTriggerKey != null && state.Triggers.Left == 0 && wasLeftTriggerPressed)
                    {
                        ReleaseKey(leftTriggerKey.Value, leftTriggerIsExtended);
                        wasLeftTriggerPressed = false;
                    }

                    if (rightTriggerKey != null && state.Triggers.Right > 0 && !wasRightTriggerPressed)
                    {
                        wasRightTriggerPressed = true;
                        SendKey(rightTriggerKey.Value, rightTriggerIsExtended);
                    }
                    else if (leftTriggerKey != null && state.Triggers.Right == 0 && wasRightTriggerPressed)
                    {
                        ReleaseKey(rightTriggerKey.Value, rightTriggerIsExtended);
                        wasRightTriggerPressed = false;
                    }
                }

            });

        }

        internal static void SendVibrationToConnectedController(int lengthInMilliseconds = 750)
        {
            PlayerIndex? connectedController = GetConnectedController();

            if (connectedController.HasValue)
            {
                GamePad.SetVibration(connectedController.Value, 0.75f, 0.75f);
                Thread.Sleep(lengthInMilliseconds);
                GamePad.SetVibration(connectedController.Value, 0f, 0f);
            }
        }
    }
}
