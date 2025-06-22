using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.ViewModels
{
    public class TouchpadActionSelectViewModel
    {
        private Mapper mapper;
        private TouchpadMapAction action;

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

        public TouchpadActionSelectViewModel(Mapper mapper, TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public void PrepareView()
        {
            switch(action)
            {
                case TouchpadNoAction:
                    selectedIndex = 0;
                    break;
                case TouchpadStickAction:
                    selectedIndex = 1;
                    break;
                case TouchpadActionPad:
                    selectedIndex = 2;
                    break;
                case TouchpadMouseJoystick:
                    selectedIndex = 3;
                    break;
                case TouchpadMouse:
                    selectedIndex = 4;
                    break;
                case TouchpadCircular:
                    selectedIndex = 5;
                    break;
                case TouchpadAbsAction:
                    selectedIndex = 6;
                    break;
                case TouchpadDirectionalSwipe:
                    selectedIndex = 7;
                    break;
                case TouchpadSingleButton:
                    selectedIndex = 8;
                    break;
                case TouchpadFlickStick:
                    selectedIndex = 9;
                    break;
                default:
                    selectedIndex = -1;
                    break;
            }
        }
    }
}
