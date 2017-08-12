using SharpDX;
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

        public static Vector3 lightPos = new Vector3(0, 1, 1);
        public static Vector3 ambientColor = new Vector3(0.1f, 0.3f, 0.1f);
        public static Vector3 diffuseColor = new Vector3(0.3f, 0.8f, 0.6f);
        public static Vector3 specularColor = new Vector3(0.7f, 0.3f, 0.2f);
        public static int specularPower = 1;
    }
}
