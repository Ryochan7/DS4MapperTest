using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Nefarius.ViGEm.Client;
using DS4MapperTest.DS4Library;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DS4MapperTest.JoyConLibrary;
using DS4MapperTest.ScpVBus;

namespace DS4MapperTest
{
    public class DebugEventArgs : EventArgs
    {
        protected DateTime m_Time = DateTime.Now;
        protected string message = string.Empty;
        protected bool warning = false;
        //protected bool temporary = false;
        //public DebugEventArgs(string message, bool warn, bool temporary = false)
        public DebugEventArgs(string message, bool warn)
        {
            this.message = message;
            warning = warn;
            //this.temporary = temporary;
        }

        public DateTime Time => m_Time;
        public string Message => message;
        public bool Warning => warning;
        //public bool Temporary => temporary;
    }

    public class BackendManager
    {
        public const int CONTROLLER_LIMIT = 8;
        private const bool JOYCON_JOINED = true;

        private Thread vbusThr;
        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
        }

        private bool changingService;
        public bool ChangingService
        {
            get => changingService;
        }

        public event EventHandler ServiceStarted;
        public event EventHandler PreServiceStop;
        public event EventHandler ServiceStopped;
        //public event EventHandler HotplugFinished;

        private FakerInputHandler fakerInputHandler = new FakerInputHandler();

        private Dictionary<int, Mapper> mapperDict;
        public Dictionary<int, Mapper> MapperDict
        {
            get => mapperDict;
        }
        private Dictionary<InputDeviceBase, DeviceReaderBase> deviceReadersMap;
        private InputDeviceBase[] controllerList =
            new InputDeviceBase[CONTROLLER_LIMIT];
        public InputDeviceBase[] ControllerList
        {
            get => controllerList;
        }

        private Dictionary<InputDeviceType, ProfileList> deviceProfileListDict;
        public Dictionary<InputDeviceType, ProfileList> DeviceProfileListDict
        {
            get => deviceProfileListDict;
        }

        private Thread eventDispatchThread;
        private Dispatcher eventDispatcher;
        public Dispatcher EventDispatcher
        {
            get => eventDispatcher;
        }

        private AppGlobalData appGlobal;
        //private DS4Enumerator enumerator;
        private DeviceEnumerator testEnumerator;
        //private List<DeviceEnumeratorBase> enumeratorList;

        //private ViGEmClient vigemTestClient = null;
        private X360BusDevice x360BusDevice = null;
        private ArgumentParser _argParser;

        public delegate void HotplugControllerHandler(InputDeviceBase device, int ind);
        public event HotplugControllerHandler HotplugController;
        public event HotplugControllerHandler UnplugController;
        public event EventHandler<DebugEventArgs> Debug;

        public BackendManager(ArgumentParser argParse, AppGlobalData appGlobal)
        {
            _argParser = argParse;
            this.appGlobal = appGlobal;

            mapperDict = new Dictionary<int, Mapper>();
            deviceReadersMap = new Dictionary<InputDeviceBase, DeviceReaderBase>();
            deviceProfileListDict = new Dictionary<InputDeviceType, ProfileList>();
            ProfileList deviceProfileList = new ProfileList(InputDeviceType.DS4);
            deviceProfileList.Refresh();
            deviceProfileListDict.Add(InputDeviceType.DS4, deviceProfileList);

            ProfileList dsDeviceProfileList = new ProfileList(InputDeviceType.DualSense);
            dsDeviceProfileList.Refresh();
            deviceProfileListDict.Add(InputDeviceType.DualSense, dsDeviceProfileList);

            ProfileList switchDeviceProfileList = new ProfileList(InputDeviceType.SwitchPro);
            switchDeviceProfileList.Refresh();
            deviceProfileListDict.Add(InputDeviceType.SwitchPro, switchDeviceProfileList);

            ProfileList joyconDeviceProfileList = new ProfileList(InputDeviceType.JoyCon);
            joyconDeviceProfileList.Refresh();
            deviceProfileListDict.Add(InputDeviceType.JoyCon, joyconDeviceProfileList);
            //enumeratorList = new List<DeviceEnumeratorBase>()
            //{
            //    new DS4Enumerator(),
            //};

            //enumerator = new DS4Enumerator();
            testEnumerator = new DeviceEnumerator();

            // Initialize Crc32 table for app
            Crc32Algorithm.InitializeTable(Crc32Algorithm.DefaultPolynomial);

            eventDispatchThread = new Thread(() =>
            {
                Dispatcher currentDis = Dispatcher.CurrentDispatcher;
                eventDispatcher = currentDis;
                Dispatcher.Run();
            });
            eventDispatchThread.IsBackground = true;
            eventDispatchThread.Priority = ThreadPriority.BelowNormal;
            eventDispatchThread.Name = "BackendManager Events";
            eventDispatchThread.Start();

            while (eventDispatcher == null)
            {
                Thread.SpinWait(500);
            }
        }

        public void Start()
        {
            LogDebug("Starting service");

            changingService = true;
            bool checkConnect = fakerInputHandler.Connect();

            // Change thread affinity of bus object to not be tied
            // to GUI thread
            vbusThr = new Thread(() =>
            {
                //vigemTestClient = new ViGEmClient();
                x360BusDevice = new X360BusDevice();
                x360BusDevice.Open();
                x360BusDevice.Start();
            });

            vbusThr.Priority = ThreadPriority.AboveNormal;
            vbusThr.IsBackground = true;
            vbusThr.Start();
            vbusThr.Join(); // Wait for bus object start

            Thread temper = new Thread(() =>
            {
                //enumerator.FindControllers();
                testEnumerator.FindControllers();
                testEnumerator.ClearRemovedDevicesReferences();
            });
            temper.IsBackground = true;
            temper.Priority = ThreadPriority.Normal;
            temper.Name = "HID Device Opener";
            temper.Start();
            temper.Join();

            int ind = 0;
            //foreach (InputDeviceBase device in enumerator.GetFoundDevices())
            foreach (InputDeviceBase device in testEnumerator.GetKnownDevices())
            {
                if (ind >= CONTROLLER_LIMIT)
                {
                    break;
                }

                device.Index = ind;
                device.Removal += Device_Removal;

                if (device.DeviceType == InputDeviceType.JoyCon &&
                    JOYCON_JOINED)
                {
                    bool existingJoy = JoyConMapperCheck(device as JoyConDevice, out JoyConMapper joyMapper, out JoyConReader joyReader);
                    if (existingJoy)
                    {
                        device.PrimaryDevice = false;
                        appGlobal.LoadControllerDeviceSettings(device, device.DeviceOptions);
                        mapperDict.Add(ind, joyMapper);
                        deviceReadersMap.Add(device, joyReader);
                        controllerList[ind] = device;
                        LogDebug($"Plugged in controller #{ind + 1} ({device.Serial})");

                        ind++;
                        continue;
                    }
                }

                Mapper testMapper = testEnumerator.PrepareDeviceMapper(device, appGlobal);
                DeviceReaderBase reader = testMapper.BaseReader;
                //DeviceReaderBase reader = enumerator.PrepareDeviceReader(device as DS4Device);
                //Mapper testMapper = new DS4Mapper(device as DS4Device,
                //    reader as DS4Reader, AppGlobalDataSingleton.Instance);
                //if (!string.IsNullOrEmpty(_argParser.ProfilePath))
                //{
                //    testMapper.ProfileFile = _argParser.ProfilePath;
                //}

                appGlobal.LoadControllerDeviceSettings(device, device.DeviceOptions);

                string tempProfilePath = string.Empty;
                if (appGlobal.activeProfiles.TryGetValue(ind, out tempProfilePath))
                {
                    testMapper.ProfileFile = tempProfilePath;
                }
                else if (deviceProfileListDict[device.DeviceType].ProfileListCol.Count > 0)
                {
                    tempProfilePath = deviceProfileListDict[device.DeviceType].ProfileListCol[0].ProfilePath;
                    testMapper.ProfileFile = tempProfilePath;
                }

                int tempInd = ind;
                testMapper.Start(x360BusDevice, fakerInputHandler);
                testMapper.ProfileChanged += (object sender, string e) => {
                    appGlobal.activeProfiles[tempInd] = e;
                    appGlobal.SaveControllerDeviceSettings(device, device.DeviceOptions);
                };

                mapperDict.Add(ind, testMapper);
                deviceReadersMap.Add(device, reader);

                controllerList[ind] = device;
                LogDebug($"Plugged in controller #{ind + 1} ({device.Serial})");

                ind++;
            }

            isRunning = true;
            changingService = false;

            ServiceStarted?.Invoke(this, EventArgs.Empty);

            LogDebug("Service started");
        }

        private void Device_Removal(object sender, EventArgs e)
        {
            InputDeviceBase device = sender as InputDeviceBase;
            if (device != null)
            {
                DeviceReaderBase reader = deviceReadersMap[device];
                deviceReadersMap.Remove(device);
                if (mapperDict.TryGetValue(device.Index, out Mapper tempMapper))
                {
                    if (device.PrimaryDevice)
                    {
                        Task tempTask = Task.Run(() =>
                        {
                            tempMapper.Stop(true);
                            tempMapper = null;
                        });

                        //tempTask.Wait();
                    }
                    //else
                    //{
                    //    eventDispatcher.Invoke(() =>
                    //    {
                    //        reader.StopUpdate();
                    //    });
                    //    //reader.StopUpdate();
                    //}
                    //tempMapper.BaseReader.StopUpdate();
                    mapperDict.Remove(device.Index);
                }

                reader = null;
                testEnumerator.RemoveDevice(device);
                //enumerator.RemoveDevice(device as DS4Device);
                //UnplugController?.Invoke(device, device.Index);
                eventDispatcher.Invoke(() =>
                {
                    if (device.Index >= 0)
                    {
                        controllerList[device.Index] = null;
                        if (appGlobal.activeProfiles.ContainsKey(device.Index))
                        {
                            appGlobal.activeProfiles.Remove(device.Index);
                            LogDebug($"Desynced controller #{device.Index + 1} ({device.Serial})");
                        }
                    }
                });
            }
        }

        public void Stop()
        {
            changingService = true;
            isRunning = false;

            PreServiceStop?.Invoke(this, EventArgs.Empty);

            foreach (Mapper mapper in mapperDict.Values)
            {
                mapper.Stop();
            }

            foreach (DeviceReaderBase reader in deviceReadersMap.Values)
            {
                reader.StopUpdate();
            }

            mapperDict.Clear();
            deviceReadersMap.Clear();
            testEnumerator.StopControllers();
            //enumerator.StopControllers();
            Array.Clear(controllerList, 0, CONTROLLER_LIMIT);

            appGlobal.activeProfiles.Clear();

            //vigemTestClient?.Dispose();
            //vigemTestClient = null;
            x360BusDevice?.UnplugAll();
            x360BusDevice = null;

            fakerInputHandler.Sync();
            Thread.Sleep(100);
            try
            {
                fakerInputHandler.Disconnect();
            }
            catch (SEHException)
            {
                // Ignore
            }

            changingService = false;

            ServiceStopped?.Invoke(this, EventArgs.Empty);
        }

        public void PreAppStopDown()
        {
            PreServiceStop = null;
            ServiceStopped = null;
        }

        public void ShutDown()
        {
        }

        public void Hotplug()
        {
            if (isRunning)
            {
                Task temp = Task.Run(() =>
                {
                    //enumerator.FindControllers();
                    testEnumerator.FindControllers();
                });
                temp.Wait();

                //IEnumerable<InputDeviceBase> devices =
                //    enumerator.GetFoundDevices();
                IEnumerable<InputDeviceBase> devices =
                    testEnumerator.GetNewKnownDevices();
                testEnumerator.ClearRemovedDevicesReferences();

                for (var devEnum = devices.GetEnumerator(); devEnum.MoveNext();)
                {
                    InputDeviceBase device = devEnum.Current;
                    //if (!device.Synced)
                    //{
                    //    continue;
                    //}

                    Func<bool> tempFoundDevFunc = () =>
                    {
                        bool found = false;
                        for (int i = 0, arlen = controllerList.Length; i < arlen; i++)
                        {
                            if (controllerList[i] != null &&
                                controllerList[i].Serial == device.Serial)
                            {
                                found = true;
                            }
                        }

                        return found;
                    };

                    if (tempFoundDevFunc())
                    {
                        continue;
                    }

                    for (int ind = 0, arlen = controllerList.Length; ind < arlen; ind++)
                    {
                        // No controller in input slot. Insert newly created
                        // device in slot
                        if (controllerList[ind] == null)
                        {
                            if (device.DeviceType == InputDeviceType.JoyCon &&
                                JOYCON_JOINED)
                            {
                                bool existingJoy = JoyConMapperCheck(device as JoyConDevice, out JoyConMapper joyMapper, out JoyConReader reader);
                                if (existingJoy)
                                {
                                    device.PrimaryDevice = false;
                                    PrepareAddInputDeviceMini(device, joyMapper, reader, ind);
                                    HotplugController?.Invoke(device, ind);
                                    break;
                                }
                            }

                            Mapper mapper = testEnumerator.PrepareDeviceMapper(device, appGlobal);
                            PrepareAddInputDevice(device, mapper, ind);
                            HotplugController?.Invoke(device, ind);
                            break;
                        }
                    }
                }
            }
        }

        private void PrepareAddInputDevice(InputDeviceBase device, Mapper mapper, int ind)
        {
            //SteamControllerReader reader;
            //if (device.ConType == SteamControllerDevice.ConnectionType.Bluetooth)
            //{
            //    reader = new SteamControllerBTReader(device as SteamControllerBTDevice);
            //}
            //else
            //{
            //    reader = new SteamControllerReader(device);
            //}

            device.Index = ind;
            //device.SetOperational();
            device.Removal += Device_Removal;
            //if (device.CheckForSyncChange)
            //{
            //    device.SyncedChanged += Device_SyncedChanged;
            //}

            appGlobal.LoadControllerDeviceSettings(device, device.DeviceOptions);

            //string tempProfilePath = profileFile;
            //if (string.IsNullOrEmpty(profileFile) &&
            //    deviceProfileList.ProfileListCol.Count > 0)
            //{
            //    tempProfilePath = deviceProfileList.ProfileListCol[0].ProfilePath;
            //}

            //string tempProfilePath = string.Empty;
            //if (deviceProfileList.ProfileListCol.Count > 0)
            //{
            //    tempProfilePath = deviceProfileList.ProfileListCol[0].ProfilePath;
            //}

            //Mapper testMapper = new Mapper(device, tempProfilePath, appGlobal);
            ////testMapper.Start(device, reader);
            //testMapper.Start(vigemTestClient, fakerInputHandler, device, reader);
            //testMapper.RequestOSD += TestMapper_RequestOSD;

            string tempProfilePath = string.Empty;
            if (appGlobal.activeProfiles.TryGetValue(ind, out tempProfilePath))
            {
                mapper.ProfileFile = tempProfilePath;
            }
            else if (deviceProfileListDict[device.DeviceType].ProfileListCol.Count > 0)
            {
                tempProfilePath = deviceProfileListDict[device.DeviceType].ProfileListCol[0].ProfilePath;
                mapper.ProfileFile = tempProfilePath;
            }

            int tempInd = ind;
            mapper.Start(x360BusDevice, fakerInputHandler);
            mapper.ProfileChanged += (object sender, string e) => {
                appGlobal.activeProfiles[tempInd] = e;
                appGlobal.SaveControllerDeviceSettings(device, device.DeviceOptions);
            };

            DeviceReaderBase reader = mapper.BaseReader;
            mapperDict.Add(ind, mapper);
            deviceReadersMap.Add(device, reader);

            controllerList[ind] = device;
            LogDebug($"Plugged in controller #{ind + 1} ({device.Serial})");
        }

        private void PrepareAddInputDeviceMini(InputDeviceBase device, Mapper mapper, DeviceReaderBase reader, int ind)
        {
            device.Index = ind;
            //device.SetOperational();
            device.Removal += Device_Removal;
            //if (device.CheckForSyncChange)
            //{
            //    device.SyncedChanged += Device_SyncedChanged;
            //}

            appGlobal.LoadControllerDeviceSettings(device, device.DeviceOptions);

            mapperDict.Add(ind, mapper);
            deviceReadersMap.Add(device, reader);

            controllerList[ind] = device;
            LogDebug($"Plugged in controller #{ind + 1} ({device.Serial})");
        }

        public void LogDebug(string message, bool warning = false)
        {
            DebugEventArgs args = new DebugEventArgs(message, warning);
            Debug?.Invoke(this, args);
        }

        public bool JoyConMapperCheck(JoyConDevice device, out JoyConMapper deviceMapper,
            out JoyConReader reader)
        {
            bool result = false;
            deviceMapper = null;
            reader = null;

            List<JoyConMapper> tempList = mapperDict.Where((t) => t.Value.DeviceType == InputDeviceType.JoyCon)
                .Select(t => t.Value as JoyConMapper).ToList();
            foreach(JoyConMapper mapper in tempList)
            {
                JoyConDevice otherDevice = mapper.JoyDevice;
                if (mapper.SecondaryJoyDevice == null)
                {
                    if (device.SideType == JoyConSide.Left &&
                        otherDevice.SideType == JoyConSide.Right)
                    {
                        reader = new JoyConReader(device);
                        mapper.AssignSecondaryJoyCon(device, reader);
                        deviceMapper = mapper;
                        result = true;
                        break;
                    }
                    else if (device.SideType == JoyConSide.Right &&
                        otherDevice.SideType == JoyConSide.Left)
                    {
                        reader = new JoyConReader(device);
                        mapper.AssignSecondaryJoyCon(device, reader);
                        deviceMapper = mapper;
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
