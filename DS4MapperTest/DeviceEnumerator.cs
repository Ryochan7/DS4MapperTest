using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using DS4MapperTest.DS4Library;
using System.Runtime.InteropServices;
using static DS4MapperTest.VidPidMeta;

namespace DS4MapperTest
{
    internal class VidPidMeta
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct DelegateUnion
        {
            [FieldOffset(0)]
            public DeviceEnumerator.HidDeviceCheckHandler hidHandler;
            [FieldOffset(0)]
            public DeviceEnumerator.HidDeviceCheckHandler hidHandler2;
        }

        public enum UsedConnectionBus : ushort
        {
            HID,
            USB,
        }

        public int vid;
        public int pid;
        public string displayName;
        public InputDeviceType inputDevType;
        public UsedConnectionBus connectBus;
        public DelegateUnion testDelUnion;

        internal VidPidMeta(int vid, int pid, string displayName, InputDeviceType inputDevType,
            UsedConnectionBus connectBus)
        {
            this.vid = vid;
            this.pid = pid;
            this.displayName = displayName;
            this.inputDevType = inputDevType;
            this.connectBus = connectBus;
            this.testDelUnion = new DelegateUnion();
        }
    }

    public class DeviceEnumerator
    {
        private const int SONY_VID = 0x054C;
        private const int SONY_DS4_V1_PID = 0x05C4;
        private const int SONY_DS4_V2_PID = 0x09CC;

        internal delegate bool HidDeviceCheckHandler(HidDevice device, VidPidMeta meta);

        private HashSet<string> foundDevicePaths;
        private ReaderWriterLockSlim _foundDevlocker = new ReaderWriterLockSlim();

        private Dictionary<string, InputDeviceBase> foundKnownDevices;
        private Dictionary<InputDeviceBase, string> revFoundKnownDevices;
        private Dictionary<string, InputDeviceBase> newKnownDevices;
        private Dictionary<string, InputDeviceBase> removedKnownDevices;
        //private Dictionary<string, HidDeviceCheckHandler> vidPidDelDict;
        private Dictionary<string, VidPidMeta> vidPidMetaDict;

        private VidPidMeta[] knownDevicesMeta = new VidPidMeta[]
        {
            new VidPidMeta(SONY_VID, SONY_DS4_V1_PID, "DS4 v.1", InputDeviceType.DS4,
                VidPidMeta.UsedConnectionBus.HID),
            new VidPidMeta(SONY_VID, SONY_DS4_V2_PID, "DS4 v.2", InputDeviceType.DS4,
                VidPidMeta.UsedConnectionBus.HID),
        };

        public DeviceEnumerator()
        {
            foundDevicePaths = new HashSet<string>();
            foundKnownDevices = new Dictionary<string, InputDeviceBase>();
            revFoundKnownDevices = new Dictionary<InputDeviceBase, string>();
            newKnownDevices = new Dictionary<string, InputDeviceBase>();
            removedKnownDevices = new Dictionary<string, InputDeviceBase>();
            //vidPidDelDict = new Dictionary<string, HidDeviceCheckHandler>();
            vidPidMetaDict = new Dictionary<string, VidPidMeta>();
            foreach (VidPidMeta meta in knownDevicesMeta)
            {
                if (meta.inputDevType == InputDeviceType.DS4)
                {
                    meta.testDelUnion.hidHandler = DS4DeviceCheckHandler;
                    vidPidMetaDict.Add($"VID_{meta.vid}&PID_{meta.pid}", meta);
                    //vidPidDelDict.Add($"VID_{meta.vid}&PID_{meta.pid}", meta.testDelUnion.hidHandler);
                }
            }

            //vidPidDelDict = new Dictionary<string, HidDeviceCheckHandler>()
            //{
            //    [$"VID_{SONY_VID}&PID_{SONY_DS4_V1_PID}"] = DS4DeviceCheckHandler,
            //    [$"VID_{SONY_VID}&PID_{SONY_DS4_V2_PID}"] = DS4DeviceCheckHandler,
            //};
        }

        public void FindControllers()
        {
            using WriteLocker locker = new WriteLocker(_foundDevlocker);

            //Dictionary<string, InputDeviceBase> previousKnownDevices =
            //    new Dictionary<string, InputDeviceBase>(foundKnownDevices);
            HashSet<string> previousDevicePaths = new HashSet<string>(foundDevicePaths);
            HashSet<string> currentDevicePaths = new HashSet<string>();
            newKnownDevices.Clear();
            removedKnownDevices.Clear();

            IEnumerable<HidDevice> hidDevs = HidDevices.Enumerate();
            foreach(HidDevice hidDev in hidDevs)
            {
                currentDevicePaths.Add(hidDev.DevicePath);
            }

            IEnumerable<string> addedHidDevices = currentDevicePaths.Except(previousDevicePaths);
            IEnumerable<string> removedHidDevices = previousDevicePaths.Except(currentDevicePaths);

            foreach (string devicePath in removedHidDevices)
            {
                if (foundKnownDevices.Remove(devicePath, out InputDeviceBase tempDevice))
                {
                    revFoundKnownDevices.Remove(tempDevice);
                    foundDevicePaths.Remove(devicePath);

                    //removedKnownDevices.Add(devicePath, tempDevice);
                }
            }

            // Filter out devices already scanned in previous sessions
            hidDevs = hidDevs.Where((hidDevice) =>
            {
                return !foundDevicePaths.Contains(hidDevice.DevicePath);
            });

            foreach (HidDevice hidDev in hidDevs)
            {
                //if (foundDevicePaths.Contains(hidDev.DevicePath))
                //{
                //    continue;
                //}

                if (vidPidMetaDict.TryGetValue($"VID_{hidDev.Attributes.VendorId}&PID_{hidDev.Attributes.ProductId}",
                    out VidPidMeta value))
                {
                    if (!hidDev.IsOpen)
                    {
                        hidDev.OpenDevice(false);
                    }

                    if (hidDev.IsOpen)
                    {
                        if (value.inputDevType == InputDeviceType.DS4)
                        {
                            value.testDelUnion.hidHandler?.Invoke(hidDev, value);
                        }
                    }

                    //DS4Device tempDev = new DS4Device(hidDev);
                    //foundDevices.Add(hidDev.DevicePath, tempDev);
                    //newFoundDevices.Add(hidDev.DevicePath, tempDev);
                }

                foundDevicePaths.Add(hidDev.DevicePath);
            }

            //foreach(KeyValuePair<string, InputDeviceBase> pair in
            //    foundKnownDevices.Except(newKnownDevices))
            //{
            //    removedKnownDevices.Add(pair.Key, pair.Value);
            //    foundKnownDevices.Remove(pair.Key);
            //    revFoundKnownDevices.Remove(pair.Value);
            //    foundDevicePaths.Remove(pair.Key);
            //}

            //IEnumerable<HidDevice> hDevices = HidDevices.Enumerate(SONY_VID,
            //    SONY_DS4_V2_PID, SONY_DS4_V1_PID);
            //List<HidDevice> tempList = hDevices.ToList();
            //using (WriteLocker locker = new WriteLocker(_foundDevlocker))
            //{
            //    foreach (HidDevice hDevice in tempList)
            //    {
            //        if (!hDevice.IsOpen)
            //        {
            //            hDevice.OpenDevice(false);
            //        }

            //        if (foundDevices.ContainsKey(hDevice.DevicePath))
            //        {
            //            // Device is known. Skip
            //            continue;
            //        }

            //        if (hDevice.IsOpen)
            //        {
            //            DS4Device tempDev = new DS4Device(hDevice);
            //            foundDevices.Add(hDevice.DevicePath, tempDev);
            //        }
            //    }
            //}
        }

        public IEnumerable<InputDeviceBase> GetKnownDevices()
        {
            using WriteLocker locker = new WriteLocker(_foundDevlocker);
            return foundKnownDevices.Values.ToList();
        }

        public IEnumerable<InputDeviceBase> GetNewKnownDevices()
        {
            using WriteLocker locker = new WriteLocker(_foundDevlocker);
            return newKnownDevices.Values.ToList();
        }

        public IEnumerable<InputDeviceBase> GetRemoveKnownDevices()
        {
            using WriteLocker locker = new WriteLocker(_foundDevlocker);
            return removedKnownDevices.Values.ToList();
        }

        public void ClearRemovedDevicesReferences()
        {
            using WriteLocker locker = new WriteLocker(_foundDevlocker);
            removedKnownDevices.Clear();
        }

        public void RemoveDevice(InputDeviceBase inputDevice)
        {
            using (WriteLocker locker = new WriteLocker(_foundDevlocker))
            {
                if (revFoundKnownDevices.TryGetValue(inputDevice, out string temp))
                {
                    revFoundKnownDevices.Remove(inputDevice);
                    foundKnownDevices.Remove(temp);
                    foundDevicePaths.Remove(temp);
                }
            }
        }

        public void StopControllers()
        {
            using (WriteLocker locker = new WriteLocker(_foundDevlocker))
            {
                foreach (InputDeviceBase inputDevice in foundKnownDevices.Values)
                {
                    inputDevice.Detach();
                }

                revFoundKnownDevices.Clear();
                foundKnownDevices.Clear();
                newKnownDevices.Clear();
                removedKnownDevices.Clear();
                foundDevicePaths.Clear();
            }
        }

        internal bool DS4DeviceCheckHandler(HidDevice hidDev, VidPidMeta meta)
        {
            bool result = false;

            if (meta != null)
            {
                DS4Device tempDev = new DS4Device(hidDev, meta.displayName);
                foundKnownDevices.Add(hidDev.DevicePath, tempDev);
                revFoundKnownDevices.Add(tempDev, hidDev.DevicePath);
                newKnownDevices.Add(hidDev.DevicePath, tempDev);
                result = true;
            }

            return result;
        }

        public Mapper PrepareDeviceMapper(InputDeviceBase device, AppGlobalData appGlobal)
        {
            Mapper result = null;
            switch (device)
            {
                case DS4Device:
                    {
                        DS4Device ds4Device = device as DS4Device;
                        DS4Reader reader = new DS4Reader(ds4Device);
                        result = new DS4Mapper(ds4Device, reader, appGlobal);
                    }

                    break;
                default: break;
            }

            return result;
        }
    }
}
