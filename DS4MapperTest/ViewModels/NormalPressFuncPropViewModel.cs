using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ViewModels
{
    public class NormalPressFuncPropViewModel
    {
        private Mapper mapper;
        private ButtonAction action;
        private NormalPressFunc func;

        public string Name
        {
            get => func.Name;
            set
            {
                func.Name = value;
            }
        }

        public string DisplayBind
        {
            get
            {
                string result = "";
                result = func.DescribeOutputActions(mapper);
                return result;
            }
        }

        public bool ToggleEnabled
        {
            get => func.toggleEnabled;
            set
            {
                func.toggleEnabled = value;
            }
        }

        public bool TurboEnabled
        {
            get => func.TurboEnabled;
            set
            {
                if (func.TurboEnabled == value) return;
                func.TurboEnabled = value;
                TurboEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TurboEnabledChanged;

        public int TurboDurationMs
        {
            get => func.TurboDurationMs;
            set
            {
                func.TurboDurationMs = value;
            }
        }

        public int FireDelayMs
        {
            get => func.FireDelayMs;
            set
            {
                func.FireDelayMs = value;
            }
        }

        public NormalPressFuncPropViewModel(Mapper mapper, ButtonAction action,
            NormalPressFunc func)
        {
            this.mapper = mapper;
            this.action = action;
            this.func = func;
        }
    }
}
