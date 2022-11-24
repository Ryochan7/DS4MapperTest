using System;

namespace DS4MapperTest.ViewModels.Common
{
    public class EnumChoiceSelection<T>
    {
        private string displayName;
        public string DisplayName { get => displayName; }

        private T choiceValue;
        public T ChoiceValue
        {
            get => choiceValue;
            set => choiceValue = value;
        }

        public EnumChoiceSelection(string name, T currentValue)
        {
            displayName = name;
            choiceValue = currentValue;
        }
    }
}
