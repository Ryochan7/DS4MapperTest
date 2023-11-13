/*
DS4MapperTest
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ScpVBus
{
    [SuppressUnmanagedCodeSecurity]
    public class X360BusDevice : ScpDevice
    {
        private const String DS3_BUS_CLASS_GUID = "{F679F562-3164-42CE-A4DB-E7DDBE723909}";
        private const int CONTROLLER_OFFSET = 1; // Device 0 is the virtual USB hub itself, and we leave devices 1-10 available for other software (like the Scarlet.Crush DualShock driver itself)
        private const int inputResolution = 127 - (-128);
        private const float reciprocalInputResolution = 1 / (float)inputResolution;
        private const int outputResolution = 32767 - (-32768);

        private const int X360_STICK_MAX = 32767;
        private const int X360_STICK_MIN = -32768;

        private int firstController = 1;
        // Device 0 is the virtual USB hub itself, and we can leave more available for other software (like the Scarlet.Crush DualShock driver)
        public int FirstController
        {
            get { return firstController; }
            set { firstController = value > 0 ? value : 1; }
        }

        protected Int32 Scale(Int32 Value, Boolean Flip)
        {
            unchecked
            {
                Value -= 0x80;

                //float temp = (Value - (-128)) / (float)inputResolution;
                float temp = (Value - (-128)) * reciprocalInputResolution;
                if (Flip) temp = (temp - 0.5f) * -1.0f + 0.5f;

                return (Int32)(temp * outputResolution + (-32768));
            }
        }


        public X360BusDevice()
            : base(DS3_BUS_CLASS_GUID)
        {
        }

        /* public override Boolean Open(int Instance = 0)
        {
            if (base.Open(Instance))
            {
            }

            return true;
        } */

        public override Boolean Open(String DevicePath)
        {
            m_Path = DevicePath;
            m_WinUsbHandle = (IntPtr)INVALID_HANDLE_VALUE;

            if (GetDeviceHandle(m_Path))
            {
                m_IsActive = true;
            }

            return true;
        }

        public override Boolean Start()
        {
            if (IsActive)
            {
            }

            return true;
        }

        public override Boolean Stop()
        {
            if (IsActive)
            {
                //Unplug(0);
            }

            return base.Stop();
        }

        public override Boolean Close()
        {
            if (IsActive)
            {
                Unplug(0);
            }

            return base.Close();
        }


        public void Parse(IntermediateState state, Byte[] Output, int device)
        {
            unchecked
            {
                Output[0] = 0x1C;
                Output[4] = (Byte)(device + firstController);
                Output[9] = 0x14;

                for (int i = 10; i < 28; i++)
                {
                    Output[i] = 0;
                }

                if (state.BtnSelect) Output[10] |= (Byte)(1 << 5); // Back
                if (state.BtnThumbL) Output[10] |= (Byte)(1 << 6); // Left  Thumb
                if (state.BtnThumbR) Output[10] |= (Byte)(1 << 7); // Right Thumb
                if (state.BtnStart) Output[10] |= (Byte)(1 << 4); // Start

                if (state.DpadUp) Output[10] |= (Byte)(1 << 0); // Up
                if (state.DpadRight) Output[10] |= (Byte)(1 << 3); // Down
                if (state.DpadDown) Output[10] |= (Byte)(1 << 1); // Right
                if (state.DpadLeft) Output[10] |= (Byte)(1 << 2); // Left

                if (state.BtnLShoulder) Output[11] |= (Byte)(1 << 0); // Left  Shoulder
                if (state.BtnRShoulder) Output[11] |= (Byte)(1 << 1); // Right Shoulder

                if (state.BtnNorth) Output[11] |= (Byte)(1 << 7); // Y
                if (state.BtnEast) Output[11] |= (Byte)(1 << 5); // B
                if (state.BtnSouth) Output[11] |= (Byte)(1 << 4); // A
                if (state.BtnWest) Output[11] |= (Byte)(1 << 6); // X

                if (state.BtnMode) Output[11] |= (Byte)(1 << 2); // Guide     

                Int32 ThumbLX;
                Int32 ThumbLY;
                Int32 ThumbRX;
                Int32 ThumbRY;

                Output[12] = (byte)(state.LTrigger * 255.0); // Left Trigger
                Output[13] = (byte)(state.RTrigger * 255.0); // Right Trigger

                ThumbLX = (short)(state.LX * (state.LX >= 0 ? X360_STICK_MAX : -X360_STICK_MIN));
                ThumbLY = (short)(state.LY * (state.LY >= 0 ? X360_STICK_MAX : -X360_STICK_MIN));

                ThumbRX = (short)(state.RX * (state.RX >= 0 ? X360_STICK_MAX : -X360_STICK_MIN));
                ThumbRY = (short)(state.RY * (state.RY >= 0 ? X360_STICK_MAX : -X360_STICK_MIN));

                Output[14] = (Byte)((ThumbLX >> 0) & 0xFF); // LX
                Output[15] = (Byte)((ThumbLX >> 8) & 0xFF);
                Output[16] = (Byte)((ThumbLY >> 0) & 0xFF); // LY
                Output[17] = (Byte)((ThumbLY >> 8) & 0xFF);
                Output[18] = (Byte)((ThumbRX >> 0) & 0xFF); // RX
                Output[19] = (Byte)((ThumbRX >> 8) & 0xFF);
                Output[20] = (Byte)((ThumbRY >> 0) & 0xFF); // RY
                Output[21] = (Byte)((ThumbRY >> 8) & 0xFF);
            }
        }

        public Boolean Plugin(Int32 Serial)
        {
            if (IsActive)
            {
                Int32 Transfered = 0;
                Byte[] Buffer = new Byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                Serial += firstController;
                Buffer[4] = (Byte)((Serial >> 0) & 0xFF);
                Buffer[5] = (Byte)((Serial >> 8) & 0xFF);
                Buffer[6] = (Byte)((Serial >> 16) & 0xFF);
                Buffer[7] = (Byte)((Serial >> 24) & 0xFF);

                return DeviceIoControl(m_FileHandle, 0x2A4000, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }

        public Boolean Unplug(Int32 Serial)
        {
            if (IsActive)
            {
                Int32 Transfered = 0;
                Byte[] Buffer = new Byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                Serial += firstController;
                Buffer[4] = (Byte)((Serial >> 0) & 0xFF);
                Buffer[5] = (Byte)((Serial >> 8) & 0xFF);
                Buffer[6] = (Byte)((Serial >> 16) & 0xFF);
                Buffer[7] = (Byte)((Serial >> 24) & 0xFF);

                return DeviceIoControl(m_FileHandle, 0x2A4004, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }

        public Boolean UnplugAll() //not yet implemented, not sure if will
        {
            if (IsActive)
            {
                Int32 Transfered = 0;
                Byte[] Buffer = new Byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                return DeviceIoControl(m_FileHandle, 0x2A4004, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }


        public Boolean Report(Byte[] Input, Byte[] Output)
        {
            if (IsActive)
            {
                Int32 Transfered = 0;

                return DeviceIoControl(m_FileHandle, 0x2A400C, Input, Input.Length, Output, Output.Length, ref Transfered, IntPtr.Zero) && Transfered > 0;
            }

            return false;
        }
    }
}
