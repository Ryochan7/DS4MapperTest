using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;

namespace DS4MapperTest.MapperUtil
{
    public struct ActionFuncEnumerator : IEnumerator<ActionFunc>
    {
        private List<ActionFunc> _collection;
        private ActionFunc currentFunc;
        private int currentIndex;

        public ActionFuncEnumerator(List<ActionFunc> collection)
        {
            _collection = collection;
            currentIndex = -1;
            currentFunc = null;
        }

        public ActionFunc Current => currentFunc;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++currentIndex >= _collection.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                currentFunc = _collection[currentIndex];
            }
            return true;
        }

        public bool MovePrevious()
        {
            //Avoids going beyond the end of the collection.
            if (--currentIndex < 0)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                currentFunc = _collection[currentIndex];
            }
            return true;
        }

        public void MoveToEnd()
        {
            if (_collection.Count > 0)
            {
                currentIndex = _collection.Count;
                currentFunc = null;
            }
            else
            {
                Reset();
            }
        }

        public void Reset()
        {
            currentIndex = -1;
            currentFunc = null;
        }
    }
}
