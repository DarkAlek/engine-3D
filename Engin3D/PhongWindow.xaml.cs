using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SoftEngine
{
    /// <summary>
    /// Interaction logic for PhongWindow.xaml
    /// </summary>
    public partial class PhongWindow : Window
    {
        public PhongWindow()
        {
            InitializeComponent();

            lightPosX.Text = GlobalSettings.lightPos.X.ToString();
            lightPosY.Text = GlobalSettings.lightPos.Y.ToString();
            lightPosZ.Text = GlobalSettings.lightPos.Z .ToString();

            ambientR.Text = ((int)(GlobalSettings.ambientColor.X * 255)).ToString();
            ambientG.Text = ((int)(GlobalSettings.ambientColor.Y * 255)).ToString();
            ambientB.Text = ((int)(GlobalSettings.ambientColor.Z * 255)).ToString();

            diffuseR.Text = ((int)(GlobalSettings.diffuseColor.X * 255)).ToString();
            diffuseG.Text = ((int)(GlobalSettings.diffuseColor.Y * 255)).ToString();
            diffuseB.Text = ((int)(GlobalSettings.diffuseColor.Z * 255)).ToString();

            specularR.Text = ((int)(GlobalSettings.diffuseColor.X * 255)).ToString();
            specularG.Text = ((int)(GlobalSettings.diffuseColor.Y * 255)).ToString();
            specularB.Text = ((int)(GlobalSettings.diffuseColor.Z * 255)).ToString();

            specularPow.Text = GlobalSettings.specularPower.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Vector3 lightPos;
            Vector3 ambient;
            Vector3 diffusion;
            Vector3 specular;
            int specularPow;

            try
            {
                lightPos = GetLightPos();
                ambient = GetAmbientColor();
                diffusion = GetDiffuseColor();
                specular = GetSpecularColor();
                specularPow = GetSpecularPow();
            }
            catch (InvalidCastException)
            {
                return;
            }

            GlobalSettings.lightPos = lightPos;
            GlobalSettings.ambientColor = ambient;
            GlobalSettings.diffuseColor = diffusion;
            GlobalSettings.specularColor = specular;
            GlobalSettings.specularPower = specularPow;
        }

        private Vector3 GetLightPos(){
            var x = lightPosX.Text;
            var y = lightPosY.Text;
            var z = lightPosZ.Text;

            float x_out;
            float y_out;
            float z_out;

            if (!float.TryParse(x, out x_out))
                throw new InvalidCastException();
            if (!float.TryParse(y, out y_out))
                throw new InvalidCastException();
            if (!float.TryParse(z, out z_out))
                throw new InvalidCastException();

            return new Vector3(x_out, y_out, z_out);
        }

        private Vector3 GetAmbientColor()
        {
            var x = ambientR.Text;
            var y = ambientG.Text;
            var z = ambientB.Text;

            int x_out;
            int y_out;
            int z_out;

            if (!int.TryParse(x, out x_out))
                throw new InvalidCastException();
            if (!int.TryParse(y, out y_out))
                throw new InvalidCastException();
            if (!int.TryParse(z, out z_out))
                throw new InvalidCastException();

            return new Vector3(x_out / 255.0f, y_out / 255.0f, z_out / 255.0f);
        }

        private Vector3 GetDiffuseColor()
        {
            var x = diffuseR.Text;
            var y = diffuseG.Text;
            var z = diffuseB.Text;

            int x_out;
            int y_out;
            int z_out;

            if (!int.TryParse(x, out x_out))
                throw new InvalidCastException();
            if (!int.TryParse(y, out y_out))
                throw new InvalidCastException();
            if (!int.TryParse(z, out z_out))
                throw new InvalidCastException();

            return new Vector3(x_out / 255.0f, y_out / 255.0f, z_out / 255.0f);
        }

        private Vector3 GetSpecularColor()
        {
            var x = specularR.Text;
            var y = specularG.Text;
            var z = specularB.Text;

            float x_out;
            float y_out;
            float z_out;

            if (!float.TryParse(x, out x_out))
                throw new InvalidCastException();
            if (!float.TryParse(y, out y_out))
                throw new InvalidCastException();
            if (!float.TryParse(z, out z_out))
                throw new InvalidCastException();

            return new Vector3(x_out / 255.0f, y_out / 255.0f, z_out / 255.0f);
        }

        private int GetSpecularPow()
        {
            var x = specularPow.Text;
            int x_out;

            if (!int.TryParse(x, out x_out))
                throw new InvalidCastException();

            return x_out;
        }
    }
}
