using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Collections;

namespace DS4MapperTest.ViewModels
{
    public class NewProfileCreateViewModel : INotifyDataErrorInfo
    {
        private Mapper mapper;
        public Mapper Mapper => mapper;

        private BackendManager manager;

        private string profilePath;
        public string ProfilePath
        {
            get => profilePath;
            set
            {
                profilePath = value;
                ProfilePathChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ProfilePathChanged;

        private string creator;
        public string Creator
        {
            get => creator;
            set
            {
                creator = value;
                CreatorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CreatorChanged;

        private bool profileCreated;
        public bool ProfileCreated
        {
            get => profileCreated;
            set
            {
                profileCreated = value;
            }
        }

        public string ProfilePathErrors
        {
            get
            {
                string result = string.Empty;
                if (errors.TryGetValue("ProfilePath", out List<string> errorList))
                {
                    result = string.Join("\n", errorList);
                }

                return result;
            }
        }
        public event EventHandler ProfilePathErrorsChanged;
        public bool HasProfilePathError
        {
            get => errors.ContainsKey("ProfilePath");
        }
        public event EventHandler HasProfilePathErrorChanged;

        public string CreatorErrors
        {
            get
            {
                string result = string.Empty;
                if (errors.TryGetValue("Creator", out List<string> errorList))
                {
                    result = string.Join("\n", errorList);
                }

                return result;
            }
        }
        public event EventHandler CreatorErrorsChanged;
        public bool HasCreatorError
        {
            get => errors.ContainsKey("Creator");
        }
        public event EventHandler HasCreatorErrorChanged;


        protected Dictionary<string, List<string>> errors =
            new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors => errors.Count > 0;

        public NewProfileCreateViewModel(Mapper mapper, BackendManager manager)
        {
            this.mapper = mapper;
            this.manager = manager;
        }

        public bool CreateProfile()
        {
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            Profile tempProfile = null;
            string profileName = string.Empty;
            mapper.ProcessMappingChangeAction(() =>
            {
                mapper.UseBlankProfile();
                tempProfile = mapper.ActionProfile;
                profileName = Path.GetFileNameWithoutExtension(profilePath);
                tempProfile.Name = profileName;
                tempProfile.Creator = creator;
                tempProfile.CreationDate = DateTime.UtcNow;
                tempProfile.Description = profileName;
                tempProfile.ActionSets[0].Name = "Main";
                tempProfile.ActionSets[0].ActionLayers[0].Name = "Default";

                mapper.AppGlobal.CreateBlankProfile(profilePath, tempProfile);

                resetEvent.Set();
            });

            resetEvent.Wait();

            manager.DeviceProfileListDict[mapper.DeviceType].CreateProfileItem(profilePath,
                    profileName,
                    mapper.DeviceType);

            profileCreated = true;

            return profileCreated;
        }

        public bool Validate()
        {
            bool result = false;
            if (profilePath.EndsWith(".json") && !File.Exists(profilePath))
            {
                result = true;
            }

            return result;
        }

        public bool ValidateForm()
        {
            bool result = false;
            ClearOldErrors();

            if (string.IsNullOrEmpty(profilePath))
            {
                List<string> tempList;
                if (!errors.TryGetValue("ProfilePath", out tempList))
                {
                    tempList = new List<string>();
                    errors.Add("ProfilePath", tempList);
                }

                tempList.Add("Profile Path not provided");
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("ProfilePath"));
            }
            else if (!profilePath.EndsWith(".json") || File.Exists(profilePath))
            {
                List<string> tempList;
                if (!errors.TryGetValue("ProfilePath", out tempList))
                {
                    tempList = new List<string>();
                    errors.Add("ProfilePath", tempList);
                }

                tempList.Add("Profile Path is invalid");
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("ProfilePath"));
            }

            if (string.IsNullOrEmpty(creator))
            {
                List<string> tempList;
                if (!errors.TryGetValue("Creator", out tempList))
                {
                    tempList = new List<string>();
                    errors.Add("Creator", tempList);
                }

                tempList.Add("No creator specified");
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("Creator"));
            }

            result = errors.Count == 0;
            if (!result)
            {
                RaiseErrorStatusEvents(errors.Keys.ToList());
            }

            return result;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            errors.TryGetValue(propertyName, out List<string> errorsForName);
            return errorsForName;
        }

        public void ClearOldErrors()
        {
            List<string> keys = errors.Keys.ToList();
            errors.Clear();

            foreach(string key in keys)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
            }

            RaiseErrorStatusEvents(keys);
        }

        private void RaiseErrorStatusEvents(List<string> keys)
        {
            foreach(string key in keys)
            {
                switch(key)
                {
                    case "ProfilePath":
                        ProfilePathErrorsChanged?.Invoke(this, EventArgs.Empty);
                        HasProfilePathErrorChanged?.Invoke(this, EventArgs.Empty);
                        break;
                    case "Creator":
                        CreatorErrorsChanged?.Invoke(this, EventArgs.Empty);
                        HasCreatorErrorChanged?.Invoke(this, EventArgs.Empty);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
