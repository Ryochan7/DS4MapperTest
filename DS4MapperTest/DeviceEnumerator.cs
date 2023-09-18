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
using DS4MapperTest.SwitchProLibrary;
using DS4MapperTest.DualSense;
using Nefarius.Utilities.DeviceManagement.PnP;
using DS4MapperTest.JoyConLibrary;

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
        private const int SONY_DUALSENSE_PID = 0x0CE6;
        private const int SONY_DUALSENSE_EDGE_PID = 0x0DF2;

        private const int NINTENDO_VENDOR_ID = 0x57e;
        private const int SWITCH_PRO_PRODUCT_ID = 0x2009;
        private const int JOYCON_L_PRODUCT_ID = 0x2006;
        private const int JOYCON_R_PRODUCT_ID = 0x2007;
        private const int JOYCON_CHARGING_GRIP_PRODUCT_ID = 0x200E;

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
            new VidPidMeta(SONY_VID, SONY_DUALSENSE_PID, "DualSense", InputDeviceType.DualSense,
                VidPidMeta.UsedConnectionBus.HID),
            new VidPidMeta(SONY_VID, SONY_DUALSENSE_EDGE_PID, "DualSense Edge", InputDeviceType.DualSense,
                VidPidMeta.UsedConnectionBus.HID),
            new VidPidMeta(NINTENDO_VENDOR_ID, SWITCH_PRO_PRODUCT_ID, "Switch Pro", InputDeviceType.SwitchPro,
                VidPidMeta.UsedConnectionBus.HID),
            new VidPidMeta(NINTENDO_VENDOR_ID, JOYCON_L_PRODUCT_ID, "JoyCon L", InputDeviceType.JoyCon,
                VidPidMeta.UsedConnectionBus.HID),
            new VidPidMeta(NINTENDO_VENDOR_ID, JOYCON_R_PRODUCT_ID, "JoyCon R", InputDeviceType.JoyCon,
                VidPidMeta.UsedConnectionBus.HID),
            new VidPidMeta(NINTENDO_VENDOR_ID, JOYCON_CHARGING_GRIP_PRODUCT_ID, "JoyCon Charging Grip", InputDeviceType.JoyCon,
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
                else if (meta.inputDevType == InputDeviceType.DualSense)
                {
                    meta.testDelUnion.hidHandler = DualSenseDeviceCheckHandler;
                    vidPidMetaDict.Add($"VID_{meta.vid}&PID_{meta.pid}", meta);
                    //vidPidDelDict.Add($"VID_{meta.vid}&PID_{meta.pid}", meta.testDelUnion.hidHandler);
                }
                else if (meta.inputDevType == InputDeviceType.SwitchPro)
                {
                    meta.testDelUnion.hidHandler = SwitchProDeviceCheckHandler;
                    vidPidMetaDict.Add($"VID_{meta.vid}&PID_{meta.pid}", meta);
                    //vidPidDelDict.Add($"VID_{meta.vid}&PID_{meta.pid}", meta.testDelUnion.hidHandler);
                }
                else if (meta.inputDevType == InputDeviceType.JoyCon)
                {
                    meta.testDelUnion.hidHandler = JoyConDeviceCheckHandler;
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

        private bool IsRealDev(HidDevice hDevice)
        {
            bool result = !Util.CheckIfVirtualDevice(hDevice.DevicePath);
            return result;
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
                return !foundDevicePaths.Contains(hidDevice.DevicePath) && hidDevice.IsConnected;
            });

            // Attempt to filter out virtual devices
            hidDevs = hidDevs.Where(IsRealDev);

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
                        if (value.inputDevType == InputDeviceType.DS4 ||
                            value.inputDevType == InputDeviceType.DualSense)
                        {
                            value.testDelUnion.hidHandler?.Invoke(hidDev, value);
                        }
                        else if (value.inputDevType == InputDeviceType.SwitchPro)
                        {
                            value.testDelUnion.hidHandler?.Invoke(hidDev, value);
                        }
                        else if (value.inputDevType == InputDeviceType.JoyCon)
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

        private bool DualSenseDeviceCheckHandler(HidDevice hidDev, VidPidMeta meta)
        {
            bool result = false;

            if (meta != null)
            {
                DualSenseDevice tempDev = new DualSenseDevice(hidDev, meta.displayName);
                foundKnownDevices.Add(hidDev.DevicePath, tempDev);
                revFoundKnownDevices.Add(tempDev, hidDev.DevicePath);
                newKnownDevices.Add(hidDev.DevicePath, tempDev);
                result = true;
            }

            return result;
        }

        private bool SwitchProDeviceCheckHandler(HidDevice hidDev, VidPidMeta meta)
        {
            bool result = false;

            if (meta != null)
            {
                SwitchProDevice tempDev = new SwitchProDevice(hidDev, meta.displayName);
                foundKnownDevices.Add(hidDev.DevicePath, tempDev);
                revFoundKnownDevices.Add(tempDev, hidDev.DevicePath);
                newKnownDevices.Add(hidDev.DevicePath, tempDev);
                result = true;
            }

            return result;
        }

        private bool JoyConDeviceCheckHandler(HidDevice hidDev, VidPidMeta meta)
        {
            bool result = false;

            if (meta != null)
            {
                JoyConDevice tempDev = new JoyConDevice(hidDev, meta.displayName);
                foundKnownDevices.Add(hidDev.DevicePath, tempDev);
                revFoundKnownDevices.Add(tempDev, hidDev.DevicePath);
                newKnownDevices.Add(hidDev.DevicePath, tempDev);
                result = true;
            }

            return result;
        }

        // TODO: Possibly move to BackendManager. Mainly to deal with possible Joined JoyCon
        // Mapper type in the future
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
                case DualSenseDevice:
                    {
                        DualSenseDevice dsDevice = device as DualSenseDevice;
                        DualSenseReader reader = new DualSenseReader(dsDevice);
                        result = new DualSenseMapper(dsDevice, reader, appGlobal);
                    }

                    break;
                case SwitchProDevice:
                    {
                        SwitchProDevice switchDevice = device as SwitchProDevice;
                        SwitchProReader reader = new SwitchProReader(switchDevice);
                        result = new SwitchProMapper(switchDevice, reader, appGlobal);
                    }

                    break;
                case JoyConDevice:
                    {
                        JoyConDevice joyConDevice = device as JoyConDevice;
                        JoyConReader reader = new JoyConReader(joyConDevice);
                        result = new JoyConMapper(joyConDevice, reader, appGlobal);
                    }

                    break;
                default: break;
            }

            return result;
        }
    }
}
