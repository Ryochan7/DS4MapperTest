using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.MapperUtil
{
    public enum JoypadActionCodes : uint
    {
        Empty, AlwaysOn = Empty,
        Btn1, BtnNorth = Btn1, X360_Y = Btn1,
        Btn2, BtnEast = Btn2, X360_B = Btn2,
        Btn3, BtnSouth = Btn3, X360_A = Btn3,
        Btn4, BtnWest = Btn4, X360_X = Btn4,
        Btn5, BtnLShoulder = Btn5, X360_LB = Btn5,
        Btn6, BtnRShoulder = Btn6, X360_RB = Btn6,
        Btn7, BtnMode = Btn7, BtnSteam = Btn7, X360_Guide = Btn7,
        Btn8, BtnStart = Btn8, X360_Start = Btn8,
        Btn9, BtnSelect = Btn9, X360_Back = Btn9,
        Btn10, BtnThumbL = Btn10, X360_ThumbL = Btn10,
        Btn11, BtnThumbR = Btn11, X360_ThumbR = Btn11,
        Btn12, BtnDPadUp = Btn12, X360_DPAD_UP = Btn12,
        Btn13, BtnDPadDown = Btn13, X360_DPAD_DOWN = Btn13,
        Btn14, BtnDPadLeft = Btn14, X360_DPAD_LEFT = Btn14,
        Btn15, BtnDPadRight = Btn15, X360_DPAD_RIGHT = Btn15,

        Btn16, BtnHome = Btn16,
        Btn17, BtnCapture = Btn17,

        Btn18, BtnLGrip = Btn18,
        Btn19, BtnRGrip = Btn19,

        // Steam Controller Touchpads. DS4 and DS Touchpad Regions
        Btn20, LPadTouch = Btn20,
        Btn21, LPadClick = Btn21,
        Btn22, RPadTouch = Btn22,
        Btn23, RPadClick = Btn23,

        Btn24, LTFullPull = Btn24,
        Btn25, RTFullPull = Btn25,

        // Use for FnL and FnR for now
        Btn26, BtnMode2 = Btn26,
        Btn27, BtnMode3 = Btn27,

        // Adding extra grip / paddle button aliases early
        Btn28, BtnLGrip2 = Btn28,
        Btn29, BtnRGrip2 = Btn29,

        Btn30, BtnLSideL = Btn30,
        Btn31, BtnLSideR = Btn31,
        Btn32, BtnRSideL = Btn32,
        Btn33, BtnRSideR = Btn33,

        // Mainly DS4 and DS Whole Touchpad
        Btn34, CenterPadTouch = Btn34,
        Btn35, CenterPadClick = Btn35,

        BtnMax = 255,

        Axis1 = 1 << 8, AxisLX = Axis1, X360_LX = Axis1,
        Axis2, AxisLY = Axis2, X360_LY = Axis2,
        Axis3, AxisRX = Axis3, X360_RX = Axis3,
        Axis4, AxisRY = Axis4, X360_RY = Axis4,
        Axis5, AxisLTrigger = Axis5, X360_LT = Axis5,
        Axis6, AxisRTrigger = Axis6, X360_RT = Axis6,

        AxisMax = 511,

        // Apply axis direction aliases. Mainly for button to axis bindings
        Axis1Neg, AxisLXNeg = Axis1Neg, X360_LX_NEG = Axis1Neg,
        Axis1Pos, AxisLXPos = Axis1Pos, X360_LX_POS = Axis1Pos,

        Axis2Neg, AxisLYNeg = Axis2Neg, X360_LY_NEG = Axis2Neg,
        Axis2Pos, AxisLYPos = Axis2Pos, X360_LY_POS = Axis2Pos,

        Axis3Neg, AxisRXNeg = Axis3Neg, X360_RX_NEG = Axis3Neg,
        Axis3Pos, AxisRXPos = Axis3Pos, X360_RX_POS = Axis3Pos,

        Axis4Neg, AxisRYNeg = Axis4Neg, X360_RY_NEG = Axis4Neg,
        Axis4Pos, AxisRYPos = Axis4Pos, X360_RY_POS = Axis4Pos,
    }

    public enum StickActionCodes : uint
    {
        Empty,
        Stick1, LS = Stick1, X360_LS = Stick1,
        Stick2, RS = Stick2, X360_RS = Stick2,

        StickMax = 127,
    }

    public enum DPadActionCodes : uint
    {
        Empty,
        DPad, DPad1 = DPad, X360_DPAD = DPad,
        DPad2,

        DPadMax = 127,
    }

    public enum TouchpadActionCodes : uint
    {
        Empty,
        Touch1, Touch = Touch1, TouchL = Touch1,
        Touch2, TouchR = Touch2,
        Touch3, TouchCenterWhole = Touch3,
    }

    public enum TriggerActionCodes : uint
    {
        Empty,
        Trigger1, Trigger = Trigger1, LeftTrigger = Trigger1, LT = Trigger1,
        Trigger2, RightTrigger = Trigger2, RT = RightTrigger,
    }

    public class MouseButtonCodes
    {
        public const int MOUSE_LEFT_BUTTON = 1;
        public const int MOUSE_MIDDLE_BUTTON = 2;
        public const int MOUSE_RIGHT_BUTTON = 3;

        public const int MOUSE_XBUTTON1 = 6;
        public const int MOUSE_XBUTTON2 = 7;
    }

    public enum MouseWheelCodes : uint
    {
        None = 0,
        WheelUp,
        WheelDown,
        WheelLeft,
        WheelRight,
    }

    public enum JoypadAxesCodes : uint
    {
        Empty,
        Axis1 = 1 << 8, AxisLX = Axis1, X360_LX = Axis1,
        Axis2, AxisLY = Axis2, X360_LY = Axis2,
        Axis3, AxisRX = Axis3, X360_RX = Axis3,
        Axis4, AxisRY = Axis4, X360_RY = Axis4,
        Axis5, AxisLTrigger = Axis5, X360_LT = Axis5,
        Axis6, AxisRTrigger = Axis6, X360_RT = Axis6, AxisMax = Axis6,
    }

    public enum SplitAxesAnalogBindingCodes : uint
    {
        None,
        Axis1 = 1 << 8, AxisLX = Axis1, X360_LX = Axis1,
        Axis2, AxisLY = Axis2, X360_LY = Axis2,
        Axis3, AxisRX = Axis3, X360_RX = Axis3,
        Axis4, AxisRY = Axis4, X360_RY = Axis4,
        Axis5, AxisLTrigger = Axis5, X360_LT = Axis5,
        Axis6, AxisRTrigger = Axis6, X360_RT = Axis6, AxisMax = Axis6,
        MouseX,
        MouseY,
        MouseWheelX,
        MouseWheelY,
    }

    public static class OutputJoypadActionCodeHelper
    {
        public static string ALWAYS_ON_TEXT = "AlwaysOn";

        public static Dictionary<JoypadActionCodes, string> X360Mappings = new Dictionary<JoypadActionCodes, string>()
        {
            {JoypadActionCodes.Empty, "Empty"},
            {JoypadActionCodes.X360_A, "X360_A"},
            {JoypadActionCodes.X360_B, "X360_B"},
            {JoypadActionCodes.X360_X, "X360_X"},
            {JoypadActionCodes.X360_Y, "X360_Y"},
            {JoypadActionCodes.X360_LB, "X360_LB"},
            {JoypadActionCodes.X360_RB, "X360_RB"},
            {JoypadActionCodes.X360_Guide, "X360_Guide"},
            {JoypadActionCodes.X360_Start, "X360_Start"},
            {JoypadActionCodes.X360_Back, "X360_Back"},
            {JoypadActionCodes.X360_ThumbL, "X360_ThumbL"},
            {JoypadActionCodes.X360_ThumbR, "X360_ThumbR"},
            {JoypadActionCodes.X360_DPAD_UP, "X360_DPAD_UP"},
            {JoypadActionCodes.X360_DPAD_DOWN, "X360_DPAD_DOWN"},
            {JoypadActionCodes.X360_DPAD_LEFT, "X360_DPAD_LEFT"},
            {JoypadActionCodes.X360_DPAD_RIGHT, "X360_DPAD_RIGHT"},
            {JoypadActionCodes.X360_LX, "X360_LX"},
            {JoypadActionCodes.X360_LY, "X360_LY"},
            {JoypadActionCodes.X360_RX, "X360_RX"},
            {JoypadActionCodes.X360_RY, "X360_RY"},
            {JoypadActionCodes.X360_LT, "X360_LT"},
            {JoypadActionCodes.X360_RT, "X360_RT"},
            {JoypadActionCodes.X360_LX_NEG, "X360_LX_NEG"},
            {JoypadActionCodes.X360_LX_POS, "X360_LX_POS"},
            {JoypadActionCodes.X360_RX_NEG, "X360_RX_NEG"},
            {JoypadActionCodes.X360_RX_POS, "X360_RX_POS"},
            {JoypadActionCodes.X360_RY_NEG, "X360_RY_NEG"},
            {JoypadActionCodes.X360_RY_POS, "X360_RY_POS"},
        };


        public static string Convert(JoypadActionCodes code)
        {
            string result = code.ToString();
            if (X360Mappings.TryGetValue(code, out string tempResult))
            {
                result = tempResult;
            }

            return result;
        }
    }

    public static class StickCodeHelper
    {
        public static string Convert(StickActionCodes code)
        {
            string result = code.ToString();
            switch(code)
            {
                case StickActionCodes.Stick1:
                    result = "X360_LS";
                    break;
                case StickActionCodes.Stick2:
                    result = "X360_RS";
                    break;
                default:
                    break;
            }
            return result;
        }
    }

    public static class DPadCodeHelper
    {
        public static string Convert(DPadActionCodes code)
        {
            string result = code.ToString();
            switch(code)
            {
                case DPadActionCodes.DPad1:
                    result = "X360_DPAD";
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
