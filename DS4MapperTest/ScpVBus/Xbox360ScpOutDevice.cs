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

using DS4MapperTest.MapperUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.ScpVBus
{
    public class Xbox360ScpOutDevice
    {
        private const int inputResolution = 127 - (-128);
        private const float reciprocalInputResolution = 1 / (float)inputResolution;
        private const int outputResolution = 32767 - (-32768);
        public const string devType = "X360";
        
        private byte[] report = new byte[28];
        private byte[] rumble = new byte[8];

        private X360BusDevice x360Bus;
        private int slotIdx = 0;
        public int SlotIdx => slotIdx;
        public int XinputSlotNum => slotIdx;

        public delegate void Xbox360FeedbackReceivedEventHandler(Xbox360ScpOutDevice sender, byte large, byte small, int idx);
        public event Xbox360FeedbackReceivedEventHandler FeedbackReceived;
        //public Xbox360FeedbackReceivedEventHandler forceFeedbackCall;
        // Input index, Xbox360FeedbackReceivedEventHandler instance
        public Dictionary<int, Xbox360FeedbackReceivedEventHandler> forceFeedbacksDict =
            new Dictionary<int, Xbox360FeedbackReceivedEventHandler>();

        public Xbox360ScpOutDevice(X360BusDevice client, int idx)
        {
            this.x360Bus = client;
            slotIdx = idx;
        }

        public void Connect()
        {
            x360Bus.Plugin(slotIdx);
        }

        public void ConvertandSendReport(IntermediateState state, int device)
        {
            x360Bus.Parse(state, report, slotIdx);
            if (x360Bus.Report(report, rumble))
            {
                unchecked
                {
                    byte Big = rumble[3];
                    byte Small = rumble[4];

                    if (rumble[1] == 0x08)
                    {
                        FeedbackReceived?.Invoke(this, Big, Small, slotIdx);
                    }
                }
            }
        }

        public void Disconnect()
        {
            foreach (KeyValuePair<int, Xbox360FeedbackReceivedEventHandler> pair in forceFeedbacksDict)
            {
                FeedbackReceived -= pair.Value;
            }

            forceFeedbacksDict.Clear();

            FeedbackReceived = null;
            x360Bus.Unplug(slotIdx);
        }

        public string GetDeviceType() => devType;

        private IntermediateState emptyState = new IntermediateState();
        public void ResetState(bool submit = true)
        {
            x360Bus.Parse(emptyState, report, slotIdx);
            if (submit)
            {
                x360Bus.Report(report, rumble);
            }
        }

        public void RemoveFeedbacks()
        {
            foreach (KeyValuePair<int, Xbox360FeedbackReceivedEventHandler> pair in forceFeedbacksDict)
            {
                FeedbackReceived -= pair.Value;
            }

            forceFeedbacksDict.Clear();
        }

        public void RemoveFeedback(int inIdx)
        {
            if (forceFeedbacksDict.TryGetValue(inIdx, out Xbox360FeedbackReceivedEventHandler handler))
            {
                FeedbackReceived -= handler;
                forceFeedbacksDict.Remove(inIdx);
            }
        }
    }
}
