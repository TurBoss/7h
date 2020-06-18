using SlimDX.DirectInput;
using SlimDX.XInput;
using System.Linq;

namespace SeventhHeaven.Classes
{
    public class GameController
    {
        private Joystick JoystickInstance;
        private JoystickState State;

        internal bool IsXInputDevice { get; set; }

        internal bool IsConnected
        {
            get
            {
                return JoystickInstance != null;
            }
        }

        internal void CreateDevice()
        {
            // sample reference: https://github.com/SlimDX/slimdx/blob/master/samples/DirectInput/Joystick/MainForm.cs
            // make sure that DirectInput has been initialized
            DirectInput dinput = new DirectInput();

            // search for devices
            foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                // create the device
                try
                {
                    JoystickInstance = new Joystick(dinput, device.InstanceGuid);
                    IsXInputDevice = JoystickInstance.Properties.InterfacePath.Contains("&ig_"); // XInput devices have IG_ in interface path (https://docs.microsoft.com/en-us/windows/win32/xinput/xinput-and-directinput)

                    break;
                }
                catch (DirectInputException e)
                {
                }
            }

            foreach (DeviceObjectInstance deviceObject in JoystickInstance.GetObjects())
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    JoystickInstance.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);
            }

            // acquire the device
            JoystickInstance.Acquire();
        }

        internal void ReleaseDevice()
        {
            // sample reference: https://github.com/SlimDX/slimdx/blob/master/samples/DirectInput/Joystick/MainForm.cs
            if (JoystickInstance != null)
            {
                JoystickInstance.Unacquire();
                JoystickInstance.Dispose();
            }
            JoystickInstance = null;
        }

        public JoystickState ReadState()
        {
            if (JoystickInstance.Acquire().IsFailure || JoystickInstance.Poll().IsFailure)
            {
                return null;
            }

            State = JoystickInstance.GetCurrentState();

            if (SlimDX.Result.Last.IsFailure)
            {
                ReleaseDevice();
                return null;
            }

            return State;
        }

        public GamePadButton? GetPressedButton()
        {
            if (State == null)
            {
                return null;
            }

            int pressedButton = State.GetButtons().ToList().FindIndex(b => b) + 1;

            switch (pressedButton)
            {
                case 1:
                    return GamePadButton.Button1;
                case 2:
                    return GamePadButton.Button2;
                case 3:
                    return GamePadButton.Button3;
                case 4:
                    return GamePadButton.Button4;
                case 5:
                    return GamePadButton.Button5;
                case 6:
                    return GamePadButton.Button6;
                case 7:
                    return GamePadButton.Button7;
                case 8:
                    return GamePadButton.Button8;
                case 9:
                    return GamePadButton.Button9;
                case 10:
                    return GamePadButton.Button10;
                case 11:
                    return GamePadButton.Button11;
                case 12:
                    return GamePadButton.Button12;
                case 13:
                    return GamePadButton.Button13;

                default:
                    break;
            }

            //// check dpad
            int pressedDPadButton = -1;

            if (State.GetPointOfViewControllers().Any(i => i != -1))
            {
                pressedDPadButton = State.GetPointOfViewControllers().First(i => i != -1);
            }

            switch (pressedDPadButton)
            {
                case -1:
                    break;
                case 0:
                    return GamePadButton.DPadUp;
                case 9000:
                    return GamePadButton.DPadRight;
                case 18000:
                    return GamePadButton.DPadDown;
                case 27000:
                    return GamePadButton.DPadLeft;
                default:
                    break;
            }

            //// check joystick for input
            int deadZone = 250; // the range is set between -1000 and 1000 so 250 is 25% deadzone

            if (State.X > deadZone)
            {
                return GamePadButton.Right;
            }
            else if (State.X < -deadZone)
            {
                return GamePadButton.Left;
            }
            else if (State.Y < -deadZone)
            {
                return GamePadButton.Up;
            }
            else if (State.Y > deadZone)
            {
                return GamePadButton.Down;
            }

            if (IsXInputDevice)
            {
                //// check triggers which are binded to Z axis when XInput mode (treated as buttons in DInput mode)
                if (State.Z > deadZone)
                {
                    return GamePadButton.LeftTrigger;
                }
                if (State.Z < -deadZone)
                {
                    return GamePadButton.RightTrigger;
                }
            }

            return null;
        }

        public bool IsButtonPressed(GamePadButton button)
        {
            int pressedDPadButton = -1;

            if (State.GetPointOfViewControllers().Any(i => i != -1))
            {
                pressedDPadButton = State.GetPointOfViewControllers().First(i => i != -1);
            }

            if ((pressedDPadButton == 0 || pressedDPadButton == 4500 || pressedDPadButton == 31500) && button == GamePadButton.DPadUp)
            {
                return true;
            }
            else if ((pressedDPadButton == 4500 || pressedDPadButton == 9000 || pressedDPadButton == 13500) && button == GamePadButton.DPadRight)
            {
                return true;
            }
            else if ((pressedDPadButton == 13500 || pressedDPadButton == 18000 || pressedDPadButton == 22500) && button == GamePadButton.DPadDown)
            {
                return true;
            }
            else if ((pressedDPadButton == 22500 || pressedDPadButton == 27000 || pressedDPadButton == 31500) && button == GamePadButton.DPadLeft)
            {
                return true;
            }

            // check for button1 - button14
            if ((int)button < (int)GamePadButton.Button14 && State.GetButtons()[(int)button])
            {
                return true;
            }

            //// check joystick for input
            int deadZone = 150; // the range is set between -1000 and 1000 so 150 is 15% deadzone

            if (State.X > deadZone && button == GamePadButton.Right)
            {
                return true;
            }
            else if (State.X < -deadZone && button == GamePadButton.Left)
            {
                return true;
            }
            else if (State.Y < -deadZone && button == GamePadButton.Up)
            {
                return true;
            }
            else if (State.Y > deadZone && button == GamePadButton.Down)
            {
                return true;
            }

            if (IsXInputDevice)
            {
                //// check triggers which are binded to Z axis when XInput mode (treated as buttons in DInput mode)
                if (State.Z > deadZone && button == GamePadButton.LeftTrigger)
                {
                    return true;
                }
                if (State.Z < -deadZone && button == GamePadButton.RightTrigger)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
