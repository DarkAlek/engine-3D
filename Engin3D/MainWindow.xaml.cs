using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows;
using System.Drawing;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoftEngine
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow
    {
        private Device device;
        Mesh mesh = new Mesh("Cube", 8, 12);
        Camera mera = new Camera();
        int showFpsCounter = 0;
        Stopwatch fpsWatcher = new Stopwatch();
        Stopwatch fpsUpdateLabelWatcher = new Stopwatch();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Choose the back buffer resolution here
            WriteableBitmap bmp = BitmapFactory.New(600, 480);

            device = new Device(bmp);

            // Our Image XAML control
            frontBuffer.Source = bmp;

            mesh.Vertices[0] = new Vector3(-1, 1, 1);
            mesh.Vertices[1] = new Vector3(1, 1, 1);
            mesh.Vertices[2] = new Vector3(-1, -1, 1);
            mesh.Vertices[3] = new Vector3(1, -1, 1);
            mesh.Vertices[4] = new Vector3(-1, 1, -1);
            mesh.Vertices[5] = new Vector3(1, 1, -1);
            mesh.Vertices[6] = new Vector3(1, -1, -1);
            mesh.Vertices[7] = new Vector3(-1, -1, -1);

            mesh.Faces[0] = new Face { A = 0, B = 1, C = 2 };
            mesh.Faces[1] = new Face { A = 1, B = 2, C = 3 };
            mesh.Faces[2] = new Face { A = 1, B = 3, C = 6 };
            mesh.Faces[3] = new Face { A = 1, B = 5, C = 6 };
            mesh.Faces[4] = new Face { A = 0, B = 1, C = 4 };
            mesh.Faces[5] = new Face { A = 1, B = 4, C = 5 };

            mesh.Faces[6] = new Face { A = 2, B = 3, C = 7 };
            mesh.Faces[7] = new Face { A = 3, B = 6, C = 7 };
            mesh.Faces[8] = new Face { A = 0, B = 2, C = 7 };
            mesh.Faces[9] = new Face { A = 0, B = 4, C = 7 };
            mesh.Faces[10] = new Face { A = 4, B = 5, C = 6 };
            mesh.Faces[11] = new Face { A = 4, B = 6, C = 7 };

            mera.Position = new Vector3(0, 0, 10.0f);
            mera.Target = Vector3.Zero;

            fpsWatcher.Start();
            fpsUpdateLabelWatcher.Start();

            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        // Rendering loop handler
        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(0, 0, 0, 255);

            // rotating slightly the cube during each frame rendered
            // add 0.01f to X if You want more robust
            mesh.Rotation = new Vector3(mesh.Rotation.X + 0.01f, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);

            // Doing the various matrix operations
            device.Render(mera, mesh);
            // Flushing the back buffer into the front buffer
            device.Present();

            long timeElapsed = fpsWatcher.ElapsedMilliseconds;
            fpsWatcher.Restart();

            if (timeElapsed != 0 && fpsUpdateLabelWatcher.ElapsedMilliseconds > 500)
            {
                long currentFps = 1000 / timeElapsed;
                fpsTextLabel.Content = currentFps.ToString();
                fpsUpdateLabelWatcher.Restart();
            }

        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ImportOffFileClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".off";
            dlg.Filter = "OFF Files (*.off)|*.off";

            var result = dlg.ShowDialog();

            if (!(result.HasValue && result.Value == true))
                return;

            var filename = dlg.FileName;

            int dataToRead = 0;
            int verticesCount;
            int facetsCount;

            float[,] vertices = null;
            int[,] facets = null;
            int actualVertice = 0;
            int actualFacet = 0;

            using (StreamReader sr = File.OpenText(filename))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    // ommit comments
                    if (s.StartsWith("#"))
                        continue;

                    if (s == "OFF")
                    {
                        dataToRead = 1;
                        continue;
                    }

                    if (dataToRead == 1)
                    {
                        var splitted = s.Split(' ');
                        verticesCount = int.Parse(splitted[0]);
                        facetsCount = int.Parse(splitted[1]);

                        vertices = new float[verticesCount, 3];
                        facets = new int[facetsCount, 3];

                        // set mesh
                        mesh = new Mesh("MeshName", verticesCount, facetsCount);

                        dataToRead = 2;
                        continue;
                    }

                    if (dataToRead == 2 && s != String.Empty)
                    {
                        var splitted = s.Split(' ');

                        vertices[actualVertice, 0] = float.Parse(splitted[0].Replace('.', ','));  //x 
                        vertices[actualVertice, 1] = float.Parse(splitted[1].Replace('.', ','));  //y
                        vertices[actualVertice, 2] = float.Parse(splitted[2].Replace('.', ','));  //z

                        // TODO
                        // should be divided by max abs value?
                        // 
                        // notice there is minus
                        mesh.Vertices[actualVertice] = new Vector3(
                            -vertices[actualVertice, 0],
                            -vertices[actualVertice, 1],
                            -vertices[actualVertice, 2]);

                        actualVertice++;
                        continue;
                    }

                    if (dataToRead == 2 && actualVertice != 0 && s == String.Empty)
                    {
                        dataToRead = 3;
                        continue;
                    }

                    if (dataToRead == 3 && s != String.Empty)
                    {
                        var splitted = s.Split(' ');

                        facets[actualFacet, 0] = int.Parse(splitted[2]);  //v1
                        facets[actualFacet, 1] = int.Parse(splitted[3]);  //v2
                        facets[actualFacet, 2] = int.Parse(splitted[4]);  //v3

                        mesh.Faces[actualFacet] = new Face {
                            A = facets[actualFacet, 0],
                            B = facets[actualFacet, 1],
                            C = facets[actualFacet, 2]
                        };

                        actualFacet++;
                        continue;
                    }
                }
            }

            // i have:
            // facets
            // vertices
        }
    }
}
