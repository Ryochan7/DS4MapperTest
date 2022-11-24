using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest
{
    public abstract class DeviceEnumeratorBase
    {
        public abstract void FindControllers();
        public abstract IEnumerable<InputDeviceBase> GetFoundDevices();
        public abstract void StopControllers();
    }
}
