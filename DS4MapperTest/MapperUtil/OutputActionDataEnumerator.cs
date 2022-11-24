using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.MapperUtil
{
    public struct OutputActionDataEnumerator : IEnumerator<OutputActionData>
    {
        private List<OutputActionData> _collection;
        private OutputActionData currentOutputAction;
        private int currentIndex;

        public OutputActionDataEnumerator(List<OutputActionData> collection)
        {
            _collection = collection;
            currentIndex = -1;
            currentOutputAction = null;
        }

        public OutputActionData Current => currentOutputAction;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool AtStart()
        {
            return currentIndex <= 0;
        }

        public bool AtEnd()
        {
            return (currentIndex+1) == _collection.Count;
        }

        public bool MoveNext()
        {
            // Avoids going beyond the end of the collection.
            if (++currentIndex >= _collection.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                currentOutputAction = _collection[currentIndex];
            }
            return true;
        }

        public bool MovePrevious()
        {
            // Avoids going beyond the end of the collection.
            if (--currentIndex < 0)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                currentOutputAction = _collection[currentIndex];
            }
            return true;
        }

        public void MoveToEnd()
        {
            if (_collection.Count > 0)
            {
                currentIndex = _collection.Count;
                currentOutputAction = null;
            }
            else
            {
                Reset();
            }
        }

        public bool MoveToStep(int index)
        {
            // Avoids going beyond the end of the collection
            if (index >= _collection.Count)
            {
                return false;
            }
            else
            {
                currentIndex = index;
                // Set current box to desired item in collection.
                currentOutputAction = _collection[currentIndex];
            }

            return true;
        }

        public void Reset()
        {
            currentIndex = -1;
            currentOutputAction = null;
        }
    }
}
