using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.ViewModels
{
    public class OSDTestViewModel
    {
        public string CurrentTime
        {
            get
            {
                DateTime current = DateTime.Now;
                return current.ToString("HH:mm:ss");
            }
        }
        public event EventHandler CurrentTimeChanged;

        private string layerMessage;
        public string LayerMessage
        {
            get => layerMessage;
            set
            {
                if (value == layerMessage) return;
                layerMessage = value;
                LayerMessageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler LayerMessageChanged;

        public void RefreshTime()
        {
            CurrentTimeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
