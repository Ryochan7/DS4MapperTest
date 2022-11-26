using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DS4MapperTest
{
    public class InputControllerDeviceOptions
    {
        private SteamControllerDeviceOptions steamControllerDevOpts =
            new SteamControllerDeviceOptions();
        public SteamControllerDeviceOptions SteamControllerDevOpts
        {
            get => steamControllerDevOpts;
        }
    }

    public class SteamControllerDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;
    }

    public abstract class ControllerOptionsStore
    {
        protected InputDeviceType deviceType;
        [JsonIgnore]
        public InputDeviceType DeviceType { get => deviceType; }

        public ControllerOptionsStore(InputDeviceType deviceType)
        {
            this.deviceType = deviceType;
        }

        public abstract void PersistSettings(JObject controllerJObj);

        public abstract void LoadSettings(JObject controllerJObj);
    }

    public class DS4ControllerOptions : ControllerOptionsStore
    {
        public DS4ControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public override void LoadSettings(JObject controllerJObj)
        {
        }

        public override void PersistSettings(JObject controllerJObj)
        {
        }
    }

    public class SteamControllerControllerOptions : ControllerOptionsStore
    {
        public const string SETTINGS_PROP_NAME = "SteamControllerSettings";

        private int leftTouchpadRotation = -15;
        public int LeftTouchpadRotation
        {
            get => leftTouchpadRotation;
            set
            {
                leftTouchpadRotation = Math.Clamp(-180, value, 180);
                LeftTouchpadRotationChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler LeftTouchpadRotationChanged;

        private int rightTouchpadRotation = 15;
        public int RightTouchpadRotation
        {
            get => rightTouchpadRotation;
            set
            {
                rightTouchpadRotation = Math.Clamp(-180, value, 180);
                RightTouchpadRotationChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RightTouchpadRotationChanged;

        private int ledBrightness = 50;
        public int LEDBrightness
        {
            get => ledBrightness;
            set
            {
                ledBrightness = value;
                LEDBrightnessChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler LEDBrightnessChanged;

        public SteamControllerControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public override void PersistSettings(JObject controllerJObj)
        {
            if (controllerJObj.SelectToken(SETTINGS_PROP_NAME) == null ||
                controllerJObj[SETTINGS_PROP_NAME].Type != JTokenType.Object)
            {
                controllerJObj[SETTINGS_PROP_NAME] = new JObject();
            }

            string output = JsonConvert.SerializeObject(this);
            controllerJObj[SETTINGS_PROP_NAME].Replace(JObject.Parse(output));
        }

        public override void LoadSettings(JObject controllerJObj)
        {
            if (controllerJObj.TryGetValue(SETTINGS_PROP_NAME,
                out JToken settingsToken) && settingsToken.Type == JTokenType.Object)
            {
                string json = settingsToken.ToString();
                JsonConvert.PopulateObject(json, this);
            }
        }
    }
}
