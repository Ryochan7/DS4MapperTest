using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DS4MapperTest.ViewModels
{
    public class MainWindowViewModel
    {
        public const string DEFAULT_START_TEXT = "Start";
        public const string DEFAULT_STOP_TEXT = "Stop";

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

        private string serviceBtnText = DEFAULT_START_TEXT;
        public string ServiceBtnText
        {
            get => serviceBtnText;
            set
            {
                serviceBtnText = value;
                ServiceBtnTextChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ServiceBtnTextChanged;

        public string VersionText
        {
            get => AppGlobalData.exeversion;
        }

        public MainWindowViewModel()
        {
        }

        public bool CheckProfilePath(string testPath)
        {
            bool result = false;
            if (File.Exists(testPath))
            {
                ProfilePath = testPath;
                result = true;
            }

            return result;
        }
    }
}
