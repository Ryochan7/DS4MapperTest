using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;
using DS4MapperTest.DS4Library;
using System.Windows.Threading;

namespace DS4MapperTest
{
    public class BackendManager
    {
        public const int CONTROLLER_LIMIT = 8;

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

        private ViGEmClient vigemTestClient = null;
        private ArgumentParser _argParser;

        public delegate void HotplugControllerHandler(InputDeviceBase device, int ind);
        public event HotplugControllerHandler HotplugController;
        public event HotplugControllerHandler UnplugController;

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
            changingService = true;
            bool checkConnect = fakerInputHandler.Connect();

            // Change thread affinity of bus object to not be tied
            // to GUI thread
            vbusThr = new Thread(() =>
            {
                vigemTestClient = new ViGEmClient();
            });

            vbusThr.Priority = ThreadPriority.AboveNormal;
            vbusThr.IsBackground = true;
            vbusThr.Start();
            vbusThr.Join(); // Wait for bus object start

            Thread temper = new Thread(() =>
            {
                //enumerator.FindControllers();
                testEnumerator.FindControllers();
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

                Mapper testMapper = testEnumerator.PrepareDeviceMapper(device,
                    appGlobal);
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
                testMapper.Start(vigemTestClient, fakerInputHandler);
                testMapper.ProfileChanged += (object sender, string e) => {
                    appGlobal.activeProfiles[tempInd] = e;
                    appGlobal.SaveControllerDeviceSettings(device, device.DeviceOptions);
                };

                mapperDict.Add(ind, testMapper);
                deviceReadersMap.Add(device, reader);

                controllerList[ind] = device;
                ind++;
            }

            isRunning = true;
            changingService = false;

            ServiceStarted?.Invoke(this, EventArgs.Empty);
        }

        private void Device_Removal(object sender, EventArgs e)
        {
            InputDeviceBase device = sender as InputDeviceBase;
            if (device != null)
            {
                deviceReadersMap.Remove(device);
                if (mapperDict.TryGetValue(device.Index, out Mapper tempMapper))
                {
                    Task tempTask = Task.Run(() =>
                    {
                        tempMapper.Stop();
                        tempMapper = null;
                    });
                    //tempTask.Wait();

                    //tempMapper.BaseReader.StopUpdate();
                    mapperDict.Remove(device.Index);
                }

                testEnumerator.RemoveDevice(device);
                //enumerator.RemoveDevice(device as DS4Device);
                //UnplugController?.Invoke(device, device.Index);
                eventDispatcher.Invoke(() =>
                {
                    if (device.Index >= 0)
                    {
                        controllerList[device.Index] = null;
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

            vigemTestClient?.Dispose();
            vigemTestClient = null;

            fakerInputHandler.Sync();
            fakerInputHandler.Disconnect();

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
            mapper.Start(vigemTestClient, fakerInputHandler);
            mapper.ProfileChanged += (object sender, string e) => {
                appGlobal.activeProfiles[tempInd] = e;
                appGlobal.SaveControllerDeviceSettings(device, device.DeviceOptions);
            };

            DeviceReaderBase reader = mapper.BaseReader;
            mapperDict.Add(ind, mapper);
            deviceReadersMap.Add(device, reader);

            controllerList[ind] = device;
        }
    }
}
