using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ViewModels.Common
{
    public class OutputStickSelectionItem
    {
        private string displayName;
        public string DisplayName
        {
            get => displayName;
        }

        private StickActionCodes code;
        public StickActionCodes Code => code;

        public OutputStickSelectionItem(string displayName, StickActionCodes code)
        {
            this.displayName = displayName;
            this.code = code;
        }
    }

    public class OutputStickSelectionItemList
    {
        private List<OutputStickSelectionItem> outputStickItems;
        public List<OutputStickSelectionItem> OutputStickItems => outputStickItems;

        public OutputStickSelectionItemList()
        {
            outputStickItems = new List<OutputStickSelectionItem>();

            outputStickItems.AddRange(new OutputStickSelectionItem[]
            {
                new OutputStickSelectionItem("Unbound", StickActionCodes.Empty),
                new OutputStickSelectionItem("Left Stick", StickActionCodes.X360_LS),
                new OutputStickSelectionItem("Right Stick", StickActionCodes.X360_RS),
            });
        }

        public int StickAliasIndex(StickActionCodes code)
        {
            int result = 0;
            switch (code)
            {
                case StickActionCodes.Empty:
                    result = 0;
                    break;
                case StickActionCodes.X360_LS:
                    result = 1;
                    break;
                case StickActionCodes.X360_RS:
                    result = 2;
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
