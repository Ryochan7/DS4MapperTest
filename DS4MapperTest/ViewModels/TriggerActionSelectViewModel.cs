using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.TriggerActions;

namespace DS4MapperTest.ViewModels
{
    public class TriggerActionSelectViewModel
    {
        private Mapper mapper;
        private TriggerMapAction action;

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

        public TriggerActionSelectViewModel(Mapper mapper, TriggerMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public void PrepareView()
        {
            switch (action)
            {
                case TriggerNoAction:
                    selectedIndex = 0;
                    break;
                case TriggerTranslate:
                    selectedIndex = 1;
                    break;
                case TriggerDualStageAction:
                    selectedIndex = 2;
                    break;
                case TriggerButtonAction:
                    selectedIndex = 3;
                    break;
                default:
                    selectedIndex = -1;
                    break;
            }
        }
    }
}
