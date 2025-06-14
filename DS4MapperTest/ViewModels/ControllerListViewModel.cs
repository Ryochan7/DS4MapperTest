using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;
//using DS4MapperTest.SteamControllerLibrary;
using DS4MapperTest.Common;

namespace DS4MapperTest.ViewModels
{
    public class ReadProfileFailException : Exception
    {
        private JsonException innerJsonException;
        public JsonException InnerJsonException => innerJsonException;

        private string extraMessage;
        public string ExtraMessage => extraMessage;

        public ReadProfileFailException(JsonException e, string extraMessage)
        {
            innerJsonException = e;
            this.extraMessage = extraMessage;
        }
    }

    public class ControllerListViewModel
    {
        private ReaderWriterLockSlim _colListLocker = new ReaderWriterLockSlim();
        private ObservableCollection<DeviceListItem> controllerList =
            new ObservableCollection<DeviceListItem>();
        public ObservableCollection<DeviceListItem> ControllerList
        {
            get => controllerList;
        }

        private Dictionary<int, DeviceListItem> controllerDict =
            new Dictionary<int, DeviceListItem>();
        public DeviceListItem CurrentItem
        {
            get
            {
                if (selectedIndex == -1) return null;
                controllerDict.TryGetValue(selectedIndex, out DeviceListItem item);
                return item;
            }
        }

        public Dictionary<int, DeviceListItem> ControllerDict { get => controllerDict; set => controllerDict = value; }

        private BackendManager backendManager;
        private int selectedIndex = -1;

        //public ProfileList DeviceProfileList
        //{
        //    get => backendManager.DeviceProfileList;
        //}

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value == selectedIndex) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedIndexChanged;
        public event EventHandler<ReadProfileFailException> ReadProfileFailure;
        public event EventHandler<DeviceListItem> EditProfileRequested;

        public ControllerListViewModel(BackendManager manager)
        {
            backendManager = manager;

            backendManager.ServiceStarted += BackendManager_ServiceStarted;
            backendManager.ServiceStopped += BackendManager_ServiceStopped;
            backendManager.HotplugController += BackendManager_HotplugController;
            backendManager.UnplugController += BackendManager_UnplugController;

            BindingOperations.EnableCollectionSynchronization(controllerList, _colListLocker,
                            ColLockCallback);
        }

        private void BackendManager_UnplugController(InputDeviceBase device, int ind)
        {
            using (WriteLocker locker = new WriteLocker(_colListLocker))
            {
                //int ind = controllerList.Where((item) => item.ItemIndex == device.Index)
                //    .Select((item) => item.ItemIndex).DefaultIfEmpty(-1).First();
                if (ind >= 0)
                {
                    controllerDict.Remove(ind);
                    controllerList.RemoveAt(ind);
                }
            }
        }

        private void BackendManager_ServiceStopped(object sender, EventArgs e)
        {
            using (WriteLocker locker = new WriteLocker(_colListLocker))
            {
                controllerList.Clear();
                controllerDict.Clear();
            }
        }

        private void BackendManager_ServiceStarted(object sender, EventArgs e)
        {
            using (WriteLocker locker = new WriteLocker(_colListLocker))
            {
                int i = 0;
                foreach (InputDeviceBase device in backendManager.ControllerList)
                {
                    if (device != null)
                    {
                        DeviceListItem devItem = new DeviceListItem(device, i,
                            backendManager.DeviceProfileListDict[device.DeviceType]);

                        if (backendManager.MapperDict.ContainsKey(device.Index))
                        {
                            Mapper map = backendManager.MapperDict[device.Index];
                            if (map.ProfileFile != string.Empty)
                            {
                                devItem.PostInit(map.ProfileFile);

                                devItem.ProfileIndexChanged += DevItem_ProfileIndexChanged;
                                devItem.EditProfileRequested += DevItem_EditProfileRequested;
                            }
                        }

                        //if (!string.IsNullOrWhiteSpace(backendManager.ProfileFile))
                        //{
                        //    devItem.PostInit(backendManager.ProfileFile);
                        //}
                        device.Removal += Device_Removal;
                        controllerList.Add(devItem);
                        controllerDict[i] = devItem;

                        i++;
                    }
                }
            }
        }

        private void DevItem_EditProfileRequested(object sender, EventArgs e)
        {
            EditProfileRequested?.Invoke(this, sender as DeviceListItem);
        }

        private void Device_Removal(object sender, EventArgs e)
        {
            InputDeviceBase device = sender as InputDeviceBase;
            using (WriteLocker locker = new WriteLocker(_colListLocker))
            {
                int ind = -1;
                int findInd = 0;
                foreach(DeviceListItem devItem in controllerList)
                {
                    if (devItem.ItemIndex == device.Index)
                    {
                        ind = findInd;
                        break;
                    }

                    findInd++;
                }
                //int ind = controllerList.Where((item) => item.ItemIndex == device.Index)
                //    .Select((item) => item.ItemIndex).DefaultIfEmpty(-1).First();
                if (device.Synced && ind >= 0)
                {
                    controllerList.RemoveAt(ind);
                }
            }
        }

        private void BackendManager_HotplugController(InputDeviceBase device, int ind)
        {
            // Engage write lock pre-maturely
            using (WriteLocker readLock = new WriteLocker(_colListLocker))
            {
                DeviceListItem devItem = new DeviceListItem(device, ind,
                    backendManager.DeviceProfileListDict[device.DeviceType]);
                Mapper map = backendManager.MapperDict[device.Index];
                if (!string.IsNullOrWhiteSpace(map.ProfileFile))
                {
                    devItem.PostInit(map.ProfileFile);
                }

                devItem.ProfileIndexChanged += DevItem_ProfileIndexChanged;
                devItem.EditProfileRequested += DevItem_EditProfileRequested;
                device.Removal += Device_Removal;
                controllerList.Add(devItem);
                controllerDict[ind] = devItem;
            }
        }

        private void DevItem_ProfileIndexChanged(object sender, EventArgs e)
        {
            DeviceListItem item = sender as DeviceListItem;
            Mapper map = backendManager.MapperDict[item.Device.Index];
            string profilePath = backendManager.DeviceProfileListDict[item.Device.DeviceType].ProfileListCol[item.ProfileIndex].ProfilePath;

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            map.QueueEvent(() =>
            {
                {
                    //map.UseBlankProfile();
                    //ReadProfileFailure?.Invoke(this, new ReadProfileFailException(new JsonException(), $"Failed to read profile {profilePath}"));
                    try
                    {
                        map.ChangeProfile(profilePath);
                    }
                    catch (JsonException e)
                    {
                        ReadProfileFailure?.Invoke(this, new ReadProfileFailException(e, $"Failed to read profile {profilePath}"));
                    }
                    //backendManager.ProfileFile = DeviceProfileList.ProfileListCol[item.ProfileIndex].ProfilePath;
                }

                resetEvent.Set();
            });

            resetEvent.Wait(AppGlobalData.RESET_WAIT_TIMEOUT);
        }

        private void ColLockCallback(IEnumerable collection, object context,
            Action accessMethod, bool writeAccess)
        {
            if (writeAccess)
            {
                using (WriteLocker locker = new WriteLocker(_colListLocker))
                {
                    accessMethod?.Invoke();
                }
            }
            else
            {
                using (ReadLocker locker = new ReadLocker(_colListLocker))
                {
                    accessMethod?.Invoke();
                }
            }
        }

        public void WaitMapperEvent(DeviceListItem item)
        {
            Mapper map = backendManager.MapperDict[item.Device.Index];
            map.ProcessMappingChangeAction(() =>
            {
            });
        }
    }

    public class DeviceListItem
    {
        private int itemIndex;
        private InputDeviceBase device;
        private ProfileList profileListHolder;
        private int profileIndex = -1;

        public InputDeviceBase Device
        {
            get => device;
        }

        public string DisplayName
        {
            get => $"{device.DevTypeStr} ({device.Serial})";
        }

        public int DisplayIndex
        {
            get => device.Index + 1;
        }

        public int ItemIndex
        {
            get => itemIndex;
        }

        public string Battery
        {
            get => $"{device.Battery}%";
        }
        public event EventHandler BatteryChanged;

        public int ProfileIndex
        {
            get => profileIndex;
            set
            {
                if (value == profileIndex) return;
                profileIndex = value;
                ProfileIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ProfileIndexChanged;

        public ObservableCollection<ProfileEntity> DevProfileList
        {
            get => profileListHolder.ProfileListCol;
        }

        //private EditProfileCommand editProfCommand;
        //public EditProfileCommand EditProfCommand => editProfCommand;

        private BasicActionCommand editProfCommand;
        public BasicActionCommand EditProfCommand => editProfCommand;

        public event EventHandler EditProfileRequested;

        public bool PrimaryDevice
        {
            get => device.PrimaryDevice;
        }

        public DeviceListItem(InputDeviceBase device, int itemIndex, ProfileList profileListHolder)
        {
            this.device = device;
            this.itemIndex = itemIndex;
            this.profileListHolder = profileListHolder;
            device.BatteryChanged += Device_BatteryChanged;

            editProfCommand = new BasicActionCommand((parameter) =>
            {
                EditProfileRequested?.Invoke(this, EventArgs.Empty);
            });
        }

        private void Device_BatteryChanged(object sender, EventArgs e)
        {
            BatteryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void PostInit(string profilePath)
        {
            ProfileEntity temp = profileListHolder.ProfileListCol.SingleOrDefault((item) => item.ProfilePath == profilePath);
            if (temp != null)
            {
                int ind = profileListHolder.ProfileListCol.IndexOf(temp);
                ProfileIndex = ind;
            }
            else
            {
                ProfileIndex = -1;
            }
        }
    }
}
