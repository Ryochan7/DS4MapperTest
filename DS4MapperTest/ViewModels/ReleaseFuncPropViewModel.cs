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
    public class ReleaseFuncPropViewModel
    {
        private Mapper mapper;
        private ButtonAction action;
        private ReleaseFunc func;

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

        public string DurationMs
        {
            get => func.DurationMs.ToString();
            set
            {
                if (int.TryParse(value, out int temp))
                {
                    func.DurationMs = temp;
                    DurationMsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DurationMsChanged;

        public bool Interruptable
        {
            get => func.interruptable;
            set
            {
                func.interruptable = value;
                InterruptableChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler InterruptableChanged;

        public ReleaseFuncPropViewModel(Mapper mapper, ButtonAction action,
            ReleaseFunc func)
        {
            this.mapper = mapper;
            this.action = action;
            this.func = func;
        }
    }
}
