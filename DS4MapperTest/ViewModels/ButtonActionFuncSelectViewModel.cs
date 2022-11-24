using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ViewModels
{
    public class ButtonActionFuncSelectViewModel
    {
        private ActionFunc func;

        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedIndexChanged;

        public ButtonActionFuncSelectViewModel(ActionFunc func)
        {
            this.func = func;

            ChangeIndex();
        }

        private void ChangeIndex()
        {
            switch (func)
            {
                case NormalPressFunc:
                    selectedIndex = 1;
                    break;
                case HoldPressFunc:
                    selectedIndex = 2;
                    break;
                case StartPressFunc:
                    selectedIndex = 3;
                    break;
                case ReleaseFunc:
                    selectedIndex = 4;
                    break;
                case DistanceFunc:
                    selectedIndex = 5;
                    break;
                default:
                    break;
            }
        }
    }
}
