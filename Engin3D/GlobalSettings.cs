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
        public static Vector3 ambientColor = new Vector3(0, 0.196f, 0);
        public static Vector3 diffuseColor = new Vector3(0, 0.274f, 0);
        public static Vector3 specularColor = new Vector3(0, 0.1f, 0);
        public static int specularPower = 6;
    }
}
