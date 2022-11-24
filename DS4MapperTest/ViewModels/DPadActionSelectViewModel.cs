using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.DPadActions;

namespace DS4MapperTest.ViewModels
{
    public class DPadActionSelectViewModel
    {
        private Mapper mapper;
        private DPadMapAction action;

        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedIndexChanged;

        public DPadActionSelectViewModel(Mapper mapper, DPadMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public void PrepareView()
        {
            switch (action)
            {
                case DPadNoAction:
                    selectedIndex = 0;
                    break;
                case DPadTranslate:
                    selectedIndex = 1;
                    break;
                case DPadAction:
                    selectedIndex = 2;
                    break;
                // TODO: FIX
                //case StickMouse:
                //    selectedIndex = 3;
                //    break;
                default:
                    selectedIndex = -1;
                    break;
            }
        }
    }
}
