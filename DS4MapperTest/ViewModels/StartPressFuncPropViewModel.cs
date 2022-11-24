using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.ViewModels
{
    public class StartPressFuncPropViewModel
    {
        private Mapper mapper;
        private ButtonAction action;
        private StartPressFunc func;

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

        public StartPressFuncPropViewModel(Mapper mapper, ButtonAction action,
            StartPressFunc func)
        {
            this.mapper = mapper;
            this.action = action;
            this.func = func;
        }
    }
}
