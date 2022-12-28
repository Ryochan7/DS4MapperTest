using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace DS4MapperTest
{
    public class ProfileList
    {
        private object _proLockobj = new object();
        private ObservableCollection<ProfileEntity> profileListCol =
            new ObservableCollection<ProfileEntity>();

        public ObservableCollection<ProfileEntity> ProfileListCol { get => profileListCol; set => profileListCol = value; }

        private InputDeviceType inputDeviceType;

        public ProfileList(InputDeviceType inputDeviceType)
        {
            this.inputDeviceType = inputDeviceType;
            BindingOperations.EnableCollectionSynchronization(profileListCol, _proLockobj);
        }

        public void Refresh()
        {
            profileListCol.Clear();
            string tempDirPath = AppGlobalDataSingleton.Instance.GetDeviceProfileFolderLocation(inputDeviceType);
            if (Directory.Exists(tempDirPath))
            {
                string[] profiles = Directory.GetFiles(tempDirPath);
                foreach (string s in profiles)
                {
                    if (s.EndsWith(".json"))
                    {
                        string json = File.ReadAllText(s);

                        try
                        {
                            ProfilePreview tempPreview =
                                JsonConvert.DeserializeObject<ProfilePreview>(json);

                            ProfileEntity item = new ProfileEntity(path: s, name: tempPreview.Name, inputDeviceType);
                            profileListCol.Add(item);
                        }
                        catch (JsonReaderException)
                        {
                        }
                    }
                }
            }
        }

        public void CreateProfileItem(string profilePath, string profileName,
            InputDeviceType deviceType)
        {
            lock (_proLockobj)
            {
                ProfileEntity tempEntity =
                    new ProfileEntity(profilePath, profileName, deviceType);
                int insertIdx = profileListCol.TakeWhile((item) => string.Compare(item.Name, profileName) < 0).Count();
                if (insertIdx > 0 && insertIdx < profileListCol.Count-1)
                {
                    profileListCol.Insert(insertIdx, tempEntity);
                }
                else
                {
                    profileListCol.Add(tempEntity);
                }
            }
        }
    }

    public class ProfilePreview
    {
        private string name;
        public string Name
        {
            get => name;
            set => name = value;
        }

        private string controllerType;
        public string ControllerType
        {
            get => controllerType;
            set => controllerType = value;
        }
    }
}
