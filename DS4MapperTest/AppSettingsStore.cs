using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace DS4MapperTest
{
    public class AppSettingsStore
    {
        private string configPath;
        private int configVersion = AppGlobalData.CONFIG_VERSION;

        public int ConfigVersion
        {
            get => configVersion;
            set => configVersion = value;
        }

        public AppSettingsStore()
        {
        }

        public AppSettingsStore(string configPath)
        {
            this.configPath = configPath;
        }

        public bool LoadConfig()
        {
            bool result = false;

            if (string.IsNullOrEmpty(configPath) ||
                !File.Exists(configPath))
            {
                throw new Exception($"Passed path {configPath} does not exist");
            }

            using (StreamReader sreader = new StreamReader(configPath))
            {
                string json = sreader.ReadToEnd();
                AppSettingsSerializer settingsSerializer =
                    new AppSettingsSerializer(this);

                try
                {
                    JsonConvert.PopulateObject(json, settingsSerializer);
                }
                catch (JsonSerializationException)
                {
                }
            }

            result = true;
            return result;
        }

        public bool SaveConfig()
        {
            bool result = false;

            if (string.IsNullOrEmpty(configPath))
            {
                return false;
            }

            AppSettingsSerializer settingsSerializer =
                    new AppSettingsSerializer(this);
            string json = JsonConvert.SerializeObject(settingsSerializer);
            using (StreamWriter writer = new StreamWriter(configPath))
            {
                writer.Write(json);
            }

            result = true;
            return result;
        }
    }

    public class AppSettingsSerializer
    {
        private AppSettingsStore settings;

        // Only serialize current app version. Don't care about reading value
        public string AppVersion
        {
            get => AppGlobalData.exeversion;
        }

        public int ConfigVersion
        {
            get => settings.ConfigVersion;
            set => settings.ConfigVersion = value;
        }

        public AppSettingsSerializer(AppSettingsStore appStore)
        {
            this.settings = appStore;
        }
    }

    public class AppSettingsMigration
    {
    }
}
