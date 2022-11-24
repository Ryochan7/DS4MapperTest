using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest
{
    public abstract class DeviceReaderBase
    {
        public abstract void StartUpdate();
        public abstract void StopUpdate();
        public abstract void WriteRumbleReport();
    }
}
