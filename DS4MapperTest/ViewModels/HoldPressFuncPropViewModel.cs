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
    public class HoldPressFuncPropViewModel
    {
        private Mapper mapper;
        private ButtonAction action;
        private HoldPressFunc func;

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

        public int HoldMs
        {
            get => func.DurationMs;
            set
            {
                func.DurationMs = value;
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

        public HoldPressFuncPropViewModel(Mapper mapper, ButtonAction action,
            HoldPressFunc func)
        {
            this.mapper = mapper;
            this.action = action;
            this.func = func;
        }
    }
}
