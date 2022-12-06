using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.ViewModels.Common
{
    public enum InvertChoices
    {
        None,
        InvertX,
        InvertY,
        InvertXY,
    }

    public class InvertChoiceItem
    {
        private string displayName;
        public string DisplayName => displayName;

        private InvertChoices choice;
        public InvertChoices Choice => choice;

        public InvertChoiceItem(string displayName, InvertChoices choice)
        {
            this.displayName = displayName;
            this.choice = choice;
        }
    }
}
