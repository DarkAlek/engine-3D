using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftEngine
{
    static class GlobalSettings
    {
        public enum viewMode
        {
            meshMode,
            randomFacesMode,
            phongMode,
            textureMode,
        }

        public static viewMode currentMode = viewMode.meshMode;
    }
}
