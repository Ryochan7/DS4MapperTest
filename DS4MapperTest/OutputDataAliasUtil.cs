using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;
using FakerInputWrapper;

namespace DS4MapperTest
{
    public static class OutputDataAliasUtil
    {
        private static Dictionary<VirtualKeys, string> keyboardStringAliasDict =
            new Dictionary<VirtualKeys, string>()
            {
                [VirtualKeys.A] = "A",
                [VirtualKeys.B] = "B",
                [VirtualKeys.C] = "C",
                [VirtualKeys.D] = "D",
                [VirtualKeys.E] = "E",
                [VirtualKeys.F] = "F",
                [VirtualKeys.G] = "G",
                [VirtualKeys.H] = "H",
                [VirtualKeys.I] = "I",
                [VirtualKeys.J] = "J",
                [VirtualKeys.K] = "K",
                [VirtualKeys.L] = "L",
                [VirtualKeys.M] = "M",
                [VirtualKeys.N] = "N",
                [VirtualKeys.O] = "O",
                [VirtualKeys.P] = "P",
                [VirtualKeys.Q] = "Q",
                [VirtualKeys.R] = "R",
                [VirtualKeys.S] = "S",
                [VirtualKeys.T] = "T",
                [VirtualKeys.U] = "U",
                [VirtualKeys.V] = "V",
                [VirtualKeys.W] = "W",
                [VirtualKeys.X] = "X",
                [VirtualKeys.Y] = "Y",
                [VirtualKeys.Z] = "Z",
                [VirtualKeys.N0] = "N0",
                [VirtualKeys.N1] = "N1",
                [VirtualKeys.N2] = "N2",
                [VirtualKeys.N3] = "N3",
                [VirtualKeys.N4] = "N4",
                [VirtualKeys.N5] = "N5",
                [VirtualKeys.N6] = "N6",
                [VirtualKeys.N7] = "N7",
                [VirtualKeys.N8] = "N8",
                [VirtualKeys.N9] = "N9",
                [VirtualKeys.Escape] = "Escape",
                [VirtualKeys.Space] = "Space",
                [VirtualKeys.Tab] = "Tab",
                [VirtualKeys.OEM3] = "Grave",
                [VirtualKeys.CapsLock] = "CapsLock",
                [VirtualKeys.OEMMinus] = "Minus",
                [VirtualKeys.NEC_Equal] = "Equal",
                [VirtualKeys.OEM4] = "LeftBracket",
                [VirtualKeys.OEM6] = "RightBracket",
                [VirtualKeys.OEM5] = "Backslash",
                [VirtualKeys.OEM1] = "Semicolon",
                [VirtualKeys.OEM7] = "Quote",
                [VirtualKeys.OEMComma] = "Comma",
                [VirtualKeys.OEM2] = "Slash",
                [VirtualKeys.OEMPlus] = "Equal",
                [VirtualKeys.OEMPeriod] = "Period",
                [VirtualKeys.Insert] = "Insert",
                [VirtualKeys.Delete] = "Delete",
                [VirtualKeys.Home] = "Home",
                [VirtualKeys.End] = "End",
                [VirtualKeys.Prior] = "PageUp",
                [VirtualKeys.Next] = "PageDown",
                [VirtualKeys.Return] = "Enter",
                [VirtualKeys.Snapshot] = "PrintScreen",
                [VirtualKeys.ScrollLock] = "ScrollLock",
                [VirtualKeys.Pause] = "Pause",
                [VirtualKeys.LeftMenu] = "LeftAlt",
                [VirtualKeys.RightMenu] = "RightAlt",
                [VirtualKeys.LeftShift] = "LeftShift",
                [VirtualKeys.RightShift] = "RightShift",
                [VirtualKeys.LeftControl] = "LeftControl",
                [VirtualKeys.RightControl] = "RightControl",
                [VirtualKeys.LeftWindows] = "LeftWindows",
                [VirtualKeys.RightWindows] = "RightWindows",
                [VirtualKeys.Up] = "Up",
                [VirtualKeys.Down] = "Down",
                [VirtualKeys.Left] = "Left",
                [VirtualKeys.Right] = "Right",

                [VirtualKeys.F1] = "F1",
                [VirtualKeys.F2] = "F2",
                [VirtualKeys.F3] = "F3",
                [VirtualKeys.F4] = "F4",
                [VirtualKeys.F5] = "F5",
                [VirtualKeys.F6] = "F6",
                [VirtualKeys.F7] = "F7",
                [VirtualKeys.F8] = "F8",
                [VirtualKeys.F9] = "F9",
                [VirtualKeys.F10] = "F10",
                [VirtualKeys.F11] = "F11",
                [VirtualKeys.F12] = "F12",
        };

        public static Dictionary<VirtualKeys, string> KeyboardStringAliasDict => keyboardStringAliasDict;

        public static string GetDisplayStringForKeyboardKey(uint keycode)
        {
            string result = "";
            VirtualKeys key = (VirtualKeys)keycode;
            switch(key)
            {
                case VirtualKeys.A:
                    result = "A";
                    break;
                case VirtualKeys.B:
                    result = "B";
                    break;
                case VirtualKeys.C:
                    result = "C";
                    break;
                case VirtualKeys.D:
                    result = "D";
                    break;
                case VirtualKeys.E:
                    result = "E";
                    break;
                case VirtualKeys.F:
                    result = "F";
                    break;
                case VirtualKeys.G:
                    result = "G";
                    break;
                case VirtualKeys.H:
                    result = "H";
                    break;
                case VirtualKeys.I:
                    result = "I";
                    break;
                case VirtualKeys.J:
                    result = "J";
                    break;
                case VirtualKeys.K:
                    result = "K";
                    break;
                case VirtualKeys.L:
                    result = "L";
                    break;
                case VirtualKeys.M:
                    result = "M";
                    break;
                case VirtualKeys.N:
                    result = "N";
                    break;
                case VirtualKeys.O:
                    result = "O";
                    break;
                case VirtualKeys.P:
                    result = "P";
                    break;
                case VirtualKeys.Q:
                    result = "Q";
                    break;
                case VirtualKeys.R:
                    result = "R";
                    break;
                case VirtualKeys.S:
                    result = "S";
                    break;
                case VirtualKeys.T:
                    result = "T";
                    break;
                case VirtualKeys.U:
                    result = "U";
                    break;
                case VirtualKeys.V:
                    result = "V";
                    break;
                case VirtualKeys.W:
                    result = "W";
                    break;
                case VirtualKeys.X:
                    result = "X";
                    break;
                case VirtualKeys.Y:
                    result = "Y";
                    break;
                case VirtualKeys.Z:
                    result = "Z";
                    break;

                case VirtualKeys.OEM3:
                    result = "Grave";
                    break;
                case VirtualKeys.Tab:
                    result = "Tab";
                    break;
                case VirtualKeys.Space:
                    result = "Space";
                    break;
                case VirtualKeys.Escape:
                    result = "Escape";
                    break;
                case VirtualKeys.Return:
                    result = "Enter";
                    break;
                case VirtualKeys.Up:
                    result = "Up";
                    break;
                case VirtualKeys.Down:
                    result = "Down";
                    break;
                case VirtualKeys.Left:
                    result = "Left";
                    break;
                case VirtualKeys.Right:
                    result = "Right";
                    break;
                case VirtualKeys.CapsLock:
                    result = "CapsLock";
                    break;
                case VirtualKeys.Subtract:
                    result = "Minus";
                    break;

                case VirtualKeys.OEMPlus:
                    result = "Equal";
                    break;

                case VirtualKeys.OEM4:
                    result = "LeftBracket";
                    break;
                case VirtualKeys.OEM6:
                    result = "RightBracket";
                    break;
                case VirtualKeys.OEM5:
                    result = "Backslash";
                    break;
                case VirtualKeys.OEM1:
                    result = "Semicolon";
                    break;
                case VirtualKeys.OEM7:
                    result = "Quote";
                    break;
                case VirtualKeys.OEMComma:
                    result = "Comma";
                    break;

                case VirtualKeys.OEMPeriod:
                    result = "Period";
                    break;

                case VirtualKeys.OEM2:
                    result = "Slash";
                    break;
                case VirtualKeys.Insert:
                    result = "Insert";
                    break;
                case VirtualKeys.Delete:
                    result = "Delete";
                    break;
                case VirtualKeys.Home:
                    result = "Home";
                    break;
                case VirtualKeys.End:
                    result = "End";
                    break;
                case VirtualKeys.Prior:
                    result = "PageUp";
                    break;
                case VirtualKeys.Next:
                    result = "PageDown";
                    break;
                case VirtualKeys.Snapshot:
                    result = "PrintScreen";
                    break;
                case VirtualKeys.ScrollLock:
                    result = "ScrollLock";
                    break;
                case VirtualKeys.Pause:
                    result = "Pause";
                    break;
                case VirtualKeys.N1:
                    result = "1";
                    break;
                case VirtualKeys.N2:
                    result = "2";
                    break;
                case VirtualKeys.N3:
                    result = "3";
                    break;
                case VirtualKeys.N4:
                    result = "4";
                    break;
                case VirtualKeys.N5:
                    result = "5";
                    break;
                case VirtualKeys.N6:
                    result = "6";
                    break;
                case VirtualKeys.N7:
                    result = "7";
                    break;
                case VirtualKeys.N8:
                    result = "8";
                    break;
                case VirtualKeys.N9:
                    result = "9";
                    break;
                case VirtualKeys.N0:
                    result = "0";
                    break;
                case VirtualKeys.F1:
                    result = "F1";
                    break;
                case VirtualKeys.F2:
                    result = "F2";
                    break;
                case VirtualKeys.F3:
                    result = "F3";
                    break;
                case VirtualKeys.F4:
                    result = "F4";
                    break;
                case VirtualKeys.F5:
                    result = "F5";
                    break;
                case VirtualKeys.F6:
                    result = "F6";
                    break;
                case VirtualKeys.F7:
                    result = "F7";
                    break;
                case VirtualKeys.F8:
                    result = "F8";
                    break;
                case VirtualKeys.F9:
                    result = "F9";
                    break;
                case VirtualKeys.F10:
                    result = "F10";
                    break;
                case VirtualKeys.F11:
                    result = "F11";
                    break;
                case VirtualKeys.F12:
                    result = "F12";
                    break;

                case VirtualKeys.LeftShift:
                    result = "LShift";
                    break;
                case VirtualKeys.RightShift:
                    result = "RShift";
                    break;
                case VirtualKeys.LeftMenu:
                    result = "LAlt";
                    break;
                case VirtualKeys.RightMenu:
                    result = "RAlt";
                    break;
                case VirtualKeys.LeftControl:
                    result = "LCtrl";
                    break;
                case VirtualKeys.RightControl:
                    result = "RCtrl";
                    break;
                case VirtualKeys.LeftWindows:
                    result = "LWin";
                    break;
                case VirtualKeys.RightWindows:
                    result = "RWin";
                    break;
                default: break;
            }
            return result;
        }

        public static string GetStringForMouseButton(int mouseBtnCode)
        {
            string result = "";
            switch(mouseBtnCode)
            {
                case MouseButtonCodes.MOUSE_LEFT_BUTTON:
                    result = "LMB";
                    break;
                case MouseButtonCodes.MOUSE_MIDDLE_BUTTON:
                    result = "MMB";
                    break;
                case MouseButtonCodes.MOUSE_RIGHT_BUTTON:
                    result = "RMB";
                    break;
                case MouseButtonCodes.MOUSE_XBUTTON1:
                    result = "XButton1";
                    break;
                case MouseButtonCodes.MOUSE_XBUTTON2:
                    result = "XButton2";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string GetStringForX360GamepadCode(JoypadActionCodes code)
        {
            string result = "";
            switch(code)
            {
                case JoypadActionCodes.X360_A:
                    result = "A";
                    break;
                case JoypadActionCodes.X360_B:
                    result = "B";
                    break;
                case JoypadActionCodes.X360_X:
                    result = "X";
                    break;
                case JoypadActionCodes.X360_Y:
                    result = "Y";
                    break;
                case JoypadActionCodes.X360_LB:
                    result = "LB";
                    break;
                case JoypadActionCodes.X360_RB:
                    result = "RB";
                    break;
                case JoypadActionCodes.X360_Back:
                    result = "Back";
                    break;
                case JoypadActionCodes.X360_Start:
                    result = "Start";
                    break;
                case JoypadActionCodes.X360_Guide:
                    result = "Guide";
                    break;
                case JoypadActionCodes.X360_LT:
                    result = "Left Trigger";
                    break;
                case JoypadActionCodes.X360_RT:
                    result = "Right Trigger";
                    break;
                case JoypadActionCodes.X360_ThumbL:
                    result = "LStick Click";
                    break;
                case JoypadActionCodes.X360_ThumbR:
                    result = "RStick Click";
                    break;
                case JoypadActionCodes.X360_DPAD_UP:
                    result = "DPad Up";
                    break;
                case JoypadActionCodes.X360_DPAD_DOWN:
                    result = "DPad Down";
                    break;
                case JoypadActionCodes.X360_DPAD_LEFT:
                    result = "DPad Left";
                    break;
                case JoypadActionCodes.X360_DPAD_RIGHT:
                    result = "DPad Right";
                    break;
                case JoypadActionCodes.X360_LX_NEG:
                    result = "LX-";
                    break;
                case JoypadActionCodes.X360_LX_POS:
                    result = "LX+";
                    break;
                case JoypadActionCodes.X360_LY_NEG:
                    result = "LY-";
                    break;
                case JoypadActionCodes.X360_LY_POS:
                    result = "LY+";
                    break;

                case JoypadActionCodes.X360_RX_NEG:
                    result = "RX-";
                    break;
                case JoypadActionCodes.X360_RX_POS:
                    result = "RX+";
                    break;
                case JoypadActionCodes.X360_RY_NEG:
                    result = "RY-";
                    break;
                case JoypadActionCodes.X360_RY_POS:
                    result = "RY+";
                    break;
                case JoypadActionCodes.Empty:
                    result = "Empty";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string GetStringForGamepadStick(StickActionCodes code)
        {
            string result = "";
            switch(code)
            {
                case StickActionCodes.X360_LS:
                    result = "LS";
                    break;
                case StickActionCodes.X360_RS:
                    result = "RS";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string GetStringForMouseWheelBtn(int code)
        {
            string result = "";
            MouseWheelCodes tempCode = (MouseWheelCodes)code;
            switch(tempCode)
            {
                case MouseWheelCodes.WheelUp:
                    result = "Wheel Up";
                    break;
                case MouseWheelCodes.WheelDown:
                    result = "Wheel Down";
                    break;
                case MouseWheelCodes.WheelLeft:
                    result = "Wheel Left";
                    break;
                case MouseWheelCodes.WheelRight:
                    result = "Wheel Right";
                    break;
                case MouseWheelCodes.None:
                    result = "None";
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
