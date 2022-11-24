using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace DS4MapperTest.DS4Library
{
    public class DS4Enumerator : DeviceEnumeratorBase
    {
        private const int SONY_VID = 0x054C;
        private const int SONY_DS4_V1_PID = 0x05C4;
        private const int SONY_DS4_V2_PID = 0x09CC;

        private Dictionary<string, DS4Device> foundDevices;
        private Dictionary<string, DS4Device> reservedDevices;
        private ReaderWriterLockSlim _foundDevlocker = new ReaderWriterLockSlim();

        public DS4Enumerator()
        {
            foundDevices = new Dictionary<string, DS4Device>();
            reservedDevices = new Dictionary<string, DS4Device>();
        }

        public override void FindControllers()
        {
            IEnumerable<HidDevice> hDevices = HidDevices.Enumerate(SONY_VID,
                SONY_DS4_V2_PID, SONY_DS4_V1_PID);
            List<HidDevice> tempList = hDevices.ToList();
            using (WriteLocker locker = new WriteLocker(_foundDevlocker))
            {
                foreach (HidDevice hDevice in tempList)
                {
                    if (!hDevice.IsOpen)
                    {
                        hDevice.OpenDevice(false);
                    }

                    if (foundDevices.ContainsKey(hDevice.DevicePath))
                    {
                        // Device is known. Skip
                        continue;
                    }

                    if (hDevice.IsOpen)
                    {
                        DS4Device tempDev = new DS4Device(hDevice);
                        foundDevices.Add(hDevice.DevicePath, tempDev);
                    }
                }
            }
        }

        public IEnumerable<DS4Device> GetFoundDevicesDS4()
        {
            return foundDevices.Values.ToList();
        }

        public override IEnumerable<InputDeviceBase> GetFoundDevices()
        {
            return foundDevices.Values.ToList();
        }

        public void RemoveDevice(DS4Device inputDevice)
        {
            inputDevice.Detach();
            inputDevice.HidDevice.CloseDevice();

            using (WriteLocker locker = new WriteLocker(_foundDevlocker))
            {
                foundDevices.Remove(inputDevice.HidDevice.DevicePath);
                reservedDevices.Remove(inputDevice.HidDevice.DevicePath);
            }
        }

        public override void StopControllers()
        {
            using (WriteLocker locker = new WriteLocker(_foundDevlocker))
            {
                foreach (DS4Device inputDevice in foundDevices.Values)
                {
                    inputDevice.Detach();
                    inputDevice.HidDevice.CloseDevice();
                }

                foreach (DS4Device inputDevice in reservedDevices.Values)
                {
                    inputDevice.Detach();
                    inputDevice.HidDevice.CloseDevice();
                }

                foundDevices.Clear();
                reservedDevices.Clear();
            }
        }

        public DS4Reader PrepareDeviceReader(DS4Device device)
        {
            return new DS4Reader(device);
        }

        public Mapper PrepareDeviceMapper(InputDeviceBase device, AppGlobalData appGlobal)
        {
            Mapper result = null;
            if (device is DS4Device ds4Device)
            {
                DS4Reader reader = new DS4Reader(ds4Device);
                result = new DS4Mapper(ds4Device, reader, appGlobal);
            }

            return result;
        }
    }
}
