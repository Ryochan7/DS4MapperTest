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
        public static string GetStringForKeyboardKey(uint keyCode)
        {
            string result = "";
            if (keyCode < FakerInputHandler.MODIFIER_MASK)
            {
                KeyboardKey tempKeyCode = (KeyboardKey)keyCode;
                switch (tempKeyCode)
                {
                    case KeyboardKey.A:
                        result = "A";
                        break;
                    case KeyboardKey.B:
                        result = "B";
                        break;
                    case KeyboardKey.C:
                        result = "C";
                        break;
                    case KeyboardKey.D:
                        result = "D";
                        break;
                    case KeyboardKey.E:
                        result = "E";
                        break;
                    case KeyboardKey.F:
                        result = "F";
                        break;
                    case KeyboardKey.G:
                        result = "G";
                        break;
                    case KeyboardKey.H:
                        result = "H";
                        break;
                    case KeyboardKey.I:
                        result = "I";
                        break;
                    case KeyboardKey.J:
                        result = "J";
                        break;
                    case KeyboardKey.K:
                        result = "K";
                        break;
                    case KeyboardKey.L:
                        result = "L";
                        break;
                    case KeyboardKey.M:
                        result = "M";
                        break;
                    case KeyboardKey.N:
                        result = "N";
                        break;
                    case KeyboardKey.O:
                        result = "O";
                        break;
                    case KeyboardKey.P:
                        result = "P";
                        break;
                    case KeyboardKey.Q:
                        result = "Q";
                        break;
                    case KeyboardKey.R:
                        result = "R";
                        break;
                    case KeyboardKey.S:
                        result = "S";
                        break;
                    case KeyboardKey.T:
                        result = "T";
                        break;
                    case KeyboardKey.U:
                        result = "U";
                        break;
                    case KeyboardKey.V:
                        result = "V";
                        break;
                    case KeyboardKey.W:
                        result = "W";
                        break;
                    case KeyboardKey.X:
                        result = "X";
                        break;
                    case KeyboardKey.Y:
                        result = "Y";
                        break;
                    case KeyboardKey.Z:
                        result = "Z";
                        break;
                    case KeyboardKey.Tilde:
                        result = "Grave";
                        break;
                    case KeyboardKey.Tab:
                        result = "Tab";
                        break;
                    case KeyboardKey.Spacebar:
                        result = "Space";
                        break;
                    case KeyboardKey.Escape:
                        result = "Escape";
                        break;
                    case KeyboardKey.Enter:
                        result = "Enter";
                        break;
                    case KeyboardKey.UpArrow:
                        result = "Up";
                        break;
                    case KeyboardKey.DownArrow:
                        result = "Down";
                        break;
                    case KeyboardKey.LeftArrow:
                        result = "Left";
                        break;
                    case KeyboardKey.RightArrow:
                        result = "Right";
                        break;
                    case KeyboardKey.CapsLock:
                        result = "CapsLock";
                        break;
                    case KeyboardKey.Subtract:
                        result = "Minus";
                        break;
                    case KeyboardKey.Equals:
                        result = "Equal";
                        break;
                    case KeyboardKey.OpenBrace:
                        result = "LeftBracket";
                        break;
                    case KeyboardKey.CloseBrace:
                        result = "RightBracket";
                        break;
                    case KeyboardKey.Backslash:
                        result = "Backslash";
                        break;
                    case KeyboardKey.Semicolon:
                        result = "Semicolon";
                        break;
                    case KeyboardKey.Quote:
                        result = "Quote";
                        break;
                    case KeyboardKey.Comma:
                        result = "Comma";
                        break;
                    case KeyboardKey.Dot:
                        result = "Period";
                        break;
                    case KeyboardKey.ForwardSlash:
                        result = "Slash";
                        break;
                    case KeyboardKey.Insert:
                        result = "Insert";
                        break;
                    case KeyboardKey.Delete:
                        result = "Delete";
                        break;
                    case KeyboardKey.Home:
                        result = "Home";
                        break;
                    case KeyboardKey.End:
                        result = "End";
                        break;
                    case KeyboardKey.PageUp:
                        result = "PageUp";
                        break;
                    case KeyboardKey.PageDown:
                        result = "PageDown";
                        break;
                    case KeyboardKey.PrintScreen:
                        result = "PrintScreen";
                        break;
                    case KeyboardKey.ScrollLock:
                        result = "ScrollLock";
                        break;
                    case KeyboardKey.Pause:
                        result = "Pause";
                        break;
                    case KeyboardKey.Number1:
                        result = "1";
                        break;
                    case KeyboardKey.Number2:
                        result = "2";
                        break;
                    case KeyboardKey.Number3:
                        result = "3";
                        break;
                    case KeyboardKey.Number4:
                        result = "4";
                        break;
                    case KeyboardKey.Number5:
                        result = "5";
                        break;
                    case KeyboardKey.Number6:
                        result = "6";
                        break;
                    case KeyboardKey.Number7:
                        result = "7";
                        break;
                    case KeyboardKey.Number8:
                        result = "8";
                        break;
                    case KeyboardKey.Number9:
                        result = "9";
                        break;
                    case KeyboardKey.Number0:
                        result = "0";
                        break;
                    case KeyboardKey.F1:
                        result = "F1";
                        break;
                    case KeyboardKey.F2:
                        result = "F2";
                        break;
                    case KeyboardKey.F3:
                        result = "F3";
                        break;
                    case KeyboardKey.F4:
                        result = "F4";
                        break;
                    case KeyboardKey.F5:
                        result = "F5";
                        break;
                    case KeyboardKey.F6:
                        result = "F6";
                        break;
                    case KeyboardKey.F7:
                        result = "F7";
                        break;
                    case KeyboardKey.F8:
                        result = "F8";
                        break;
                    case KeyboardKey.F9:
                        result = "F9";
                        break;
                    case KeyboardKey.F10:
                        result = "F10";
                        break;
                    case KeyboardKey.F11:
                        result = "F11";
                        break;
                    case KeyboardKey.F12:
                        result = "F12";
                        break;
                    default:
                        result = "Empty";
                        break;
                }
            }
            else if (keyCode < FakerInputHandler.MODIFIER_ENHANCED)
            {
                KeyboardModifier tempKeyCode = (KeyboardModifier)keyCode;
                switch (tempKeyCode)
                {
                    case KeyboardModifier.LShift:
                        result = "LShift";
                        break;
                    case KeyboardModifier.RShift:
                        result = "RShift";
                        break;
                    case KeyboardModifier.LAlt:
                        result = "LAlt";
                        break;
                    case KeyboardModifier.RAlt:
                        result = "RAlt";
                        break;
                    case KeyboardModifier.LControl:
                        result = "LCtrl";
                        break;
                    case KeyboardModifier.RControl:
                        result = "RCtrl";
                        break;
                    case KeyboardModifier.LWin:
                        result = "LWin";
                        break;
                    case KeyboardModifier.RWin:
                        result = "RWin";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                EnhancedKey tempKeyCode = (EnhancedKey)(keyCode & ~FakerInputHandler.MODIFIER_ENHANCED);
                switch (tempKeyCode)
                {
                    default:
                        break;
                }
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
