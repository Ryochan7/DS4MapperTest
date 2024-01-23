using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HidLibrary;

namespace DS4MapperTest.SteamControllerLibrary
{
    public class SteamControllerEnumerator
    {
        private const int STEAM_CONTROLLER_VENDOR_ID = 0x28DE;
        private const int STEAM_CONTROLLER_PRODUCT_ID = 0x1102;
        public const int STEAM_DONGLE_CONTROLLER_PRODUCT_ID = 0x1142;
        public const int STEAM_BT_CONTROLLER_PRODUCT_ID = 0x1106;

        private Dictionary<string, SteamControllerDevice> foundDevices;
        private Dictionary<string, SteamControllerDevice> reservedDevices;
        private ReaderWriterLockSlim _foundDevlocker = new ReaderWriterLockSlim();

        public SteamControllerEnumerator()
        {
            foundDevices = new Dictionary<string, SteamControllerDevice>();
            reservedDevices = new Dictionary<string, SteamControllerDevice>();
        }

        public void FindControllers()
        {
            //int endpointIdx = 1;

            IEnumerable<HidDevice> hDevices = HidDevices.Enumerate(STEAM_CONTROLLER_VENDOR_ID,
                STEAM_CONTROLLER_PRODUCT_ID, STEAM_DONGLE_CONTROLLER_PRODUCT_ID,
                STEAM_BT_CONTROLLER_PRODUCT_ID);
            hDevices = hDevices.Where(hDevice => hDevice.IsConnected && hDevice.Capabilities.Usage == 1);
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
                        //string serial = hDevice.ReadSerial();
                        if (hDevice.Attributes.ProductId == STEAM_CONTROLLER_PRODUCT_ID)
                        {
                            SteamControllerDevice tempDev = new SteamControllerDevice(hDevice, "");
                            foundDevices.Add(hDevice.DevicePath, tempDev);
                        }
                        else if (hDevice.Attributes.ProductId == STEAM_DONGLE_CONTROLLER_PRODUCT_ID)
                        {
                            // Only care about interface 1 for this test
                            //if (endpointIdx == 1)
                            //if (hDevice.DevicePath.Contains("&mi_02#"))
                            //if (SteamControllerDevice.TestDongleSCConnected(hDevice))
                            {
                                SteamControllerDevice tempDev = new SteamControllerDevice(hDevice, "");
                                foundDevices.Add(hDevice.DevicePath, tempDev);
                                //reservedDevices.Add(hDevice.DevicePath, tempDev);
                                //tempDev.Synced = true;
                                //endpointIdx++;
                            }
                        }
                        else if (hDevice.Attributes.ProductId ==
                            STEAM_BT_CONTROLLER_PRODUCT_ID)
                        {
                            SteamControllerBTDevice tempDev = new SteamControllerBTDevice(hDevice, "");
                            foundDevices.Add(hDevice.DevicePath, tempDev);
                        }
                    }
                }
            }
        }

        public IEnumerable<SteamControllerDevice> GetFoundDevices()
        {
            return foundDevices.Values.ToList();
        }

        public void RemoveDevice(SteamControllerDevice inputDevice)
        {
            inputDevice.Detach();
            inputDevice.HidDevice.CloseDevice();

            using (WriteLocker locker = new WriteLocker(_foundDevlocker))
            {
                foundDevices.Remove(inputDevice.HidDevice.DevicePath);
                reservedDevices.Remove(inputDevice.HidDevice.DevicePath);
            }
        }

        public void StopControllers()
        {
            using (WriteLocker locker = new WriteLocker(_foundDevlocker))
            {
                foreach (SteamControllerDevice inputDevice in foundDevices.Values)
                {
                    inputDevice.Detach();
                    inputDevice.HidDevice.CloseDevice();
                }

                foreach (SteamControllerDevice inputDevice in reservedDevices.Values)
                {
                    inputDevice.Detach();
                    inputDevice.HidDevice.CloseDevice();
                }

                foundDevices.Clear();
                reservedDevices.Clear();
            }
        }
    }
}
