using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DS4MapperTest
{
    public class ProfileEntity
    {
        private string profilePath;
        private string name;
        private InputDeviceType inputDeviceType;

        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public InputDeviceType InputDeviceType
        {
            get => inputDeviceType;
        }

        public string ProfilePath
        {
            get => profilePath;
        }

        public event EventHandler NameChanged;

        public ProfileEntity(string path, string name, InputDeviceType inputDeviceType)
        {
            this.profilePath = path;
            this.name = name;
            this.inputDeviceType = inputDeviceType;
        }
    }
}
