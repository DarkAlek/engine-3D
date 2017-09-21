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

namespace Engin3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow
    {
        private Device device;
        Mesh mesh = new Mesh("Cube", 8, 12);
        Camera mera = new Camera();
        Stopwatch fpsWatcher = new Stopwatch();
        Stopwatch fpsUpdateLabelWatcher = new Stopwatch();
        PhongWindow phongWindowSettings = null;

        double mouseLastX = double.NaN;
        double mouseLastY = double.NaN;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = BitmapFactory.New(600, 480);

            device = new Device(bmp);
            device.cameraWorld = mera;
            frontBuffer.Source = bmp;

            MouseWheel += MainWindow_MouseWheel;            

            mera.Position = new Vector3(0, 1.0f, 10.0f);
            mera.Target = Vector3.Zero;

            fpsWatcher.Start();
            fpsUpdateLabelWatcher.Start();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void FrontBuffer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mouseLastX = e.GetPosition(frontBuffer).X;
                mouseLastY = e.GetPosition(frontBuffer).Y;

                return;
            }

            double xOffset = e.GetPosition(frontBuffer).X - mouseLastX;
            double yOffset = e.GetPosition(frontBuffer).Y - mouseLastY;

            mouseLastX = e.GetPosition(frontBuffer).X;
            mouseLastY = e.GetPosition(frontBuffer).Y;

            mera.Position = new Vector3(
                mera.Position.X,
                mera.Position.Y + (float)yOffset / 1000,
                mera.Position.Z
            );

            mesh.Rotation = new Vector3(
                mesh.Rotation.X + (float)xOffset/1000,
                mesh.Rotation.Y,
                mesh.Rotation.Z
            );
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mera.Position = new Vector3(
                mera.Position.X,
                mera.Position.Y,
                mera.Position.Z + e.Delta / 1000
            );
        }

        void MoveCameraXY()
        {
            float offset = 0.02f;

            if (Keyboard.IsKeyDown(Key.Up))
            {
                mera.Target = new Vector3(
                mera.Target.X,
                mera.Target.Y + offset,
                mera.Target.Z);
            }
            else if (Keyboard.IsKeyDown(Key.Down))
            {
                mera.Target = new Vector3(
                mera.Target.X,
                mera.Target.Y - offset,
                mera.Target.Z);
            }

            if (Keyboard.IsKeyDown(Key.Left))
            {
                mera.Target = new Vector3(
                mera.Target.X + offset,
                mera.Target.Y,
                mera.Target.Z);
            }
            else if (Keyboard.IsKeyDown(Key.Right))
            {
                mera.Target = new Vector3(
                mera.Target.X - offset,
                mera.Target.Y,
                mera.Target.Z);
            }
        }

        private void MoveCameraRotate()
        {
            if (Keyboard.IsKeyDown(Key.NumPad2))
            {
                float offset = 0.03f;

                mesh.Rotation = new Vector3(
                    mesh.Rotation.X + offset,
                    mesh.Rotation.Y,
                    mesh.Rotation.Z
                );
            }
            else if (Keyboard.IsKeyDown(Key.NumPad8))
            {
                float offset = -0.03f;

                mesh.Rotation = new Vector3(
                    mesh.Rotation.X + offset,
                    mesh.Rotation.Y,
                    mesh.Rotation.Z
                );
            }

            if (Keyboard.IsKeyDown(Key.NumPad4))
            {
                float offset = 0.03f;

                mesh.Rotation = new Vector3(
                    mesh.Rotation.X,
                    mesh.Rotation.Y + offset,
                    mesh.Rotation.Z
                );
            }
            else if (Keyboard.IsKeyDown(Key.NumPad6))
            {
                float offset = -0.03f;

                mesh.Rotation = new Vector3(
                    mesh.Rotation.X,
                    mesh.Rotation.Y + offset,
                    mesh.Rotation.Z
                );
            }

            if (Keyboard.IsKeyDown(Key.Add))
            {
                float offset = -0.03f;

                mera.Position = new Vector3(
                    mera.Position.X,
                    mera.Position.Y,
                    mera.Position.Z + offset
                );
            }
            else if (Keyboard.IsKeyDown(Key.Subtract))
            {
                float offset = 0.03f;

                mera.Position = new Vector3(
                    mera.Position.X,
                    mera.Position.Y,
                    mera.Position.Z + offset
                );
            }
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(0, 0, 0, 255);

            mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y, mesh.Rotation.Z);

            mera.Position = new Vector3(
                mera.Position.X,
                mera.Position.Y,
                mera.Position.Z);
            
            MoveCameraXY();
            MoveCameraRotate();

            device.Render(mera, mesh);
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

                        var x = (float)vertices[actualVertice, 0];
                        var y = (float)vertices[actualVertice, 1];
                        var z = (float)vertices[actualVertice, 2];
                        
                        var nx = (float)0;
                        var ny = (float)0;
                        var nz = (float)0;
                        
                        mesh.Vertices[actualVertice] = new Vertex
                        {
                            Coordinates = new Vector3(x, y, z),
                            Normal = new Vector3(nx, ny, nz)
                        };

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

                for (int i = 0; i < mesh.Faces.Length; ++i)
                {
                    Face face = mesh.Faces[i];
                    Vector3 p1 = mesh.Vertices[face.A].Coordinates;
                    Vector3 p2 = mesh.Vertices[face.B].Coordinates;
                    Vector3 p3 = mesh.Vertices[face.C].Coordinates;

                    Vector3 u = p2 - p1;
                    Vector3 v = p3 - p1;

                    float nx = u.Y * v.Z - u.Z * v.Y;
                    float ny = u.Z * v.X - u.X * v.Z;
                    float nz = u.X * v.Y - u.Y * v.X;

                    mesh.Faces[i].Normal = new Vector3(nx, ny, nz);
                }

                for (int i = 0; i < mesh.Faces.Length; ++i)
                {
                    Face face = mesh.Faces[i];
                    mesh.Vertices[face.A].Normal += face.Normal;
                    mesh.Vertices[face.B].Normal += face.Normal;
                    mesh.Vertices[face.C].Normal += face.Normal;
                }

                for (int i = 0; i < mesh.Faces.Length; ++i)
                {
                    Face face = mesh.Faces[i];
                    mesh.Vertices[face.A].Normal.Normalize();
                    mesh.Vertices[face.B].Normal.Normalize();
                    mesh.Vertices[face.C].Normal.Normalize();
                }
            }
        }

        private void meshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            meshMenuItem.IsChecked = true;
            phongMenuItem.IsChecked = false;
            randomFacesMenuItem.IsChecked = false;
            textureMenuItem.IsChecked = false;
            if (phongWindowSettings != null)
                phongWindowSettings.Close();
            phongWindowSettings = null;

            GlobalSettings.currentMode = GlobalSettings.viewMode.meshMode;
        }

        private void randomFacesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            randomFacesMenuItem.IsChecked = true;
            phongMenuItem.IsChecked = false;
            meshMenuItem.IsChecked = false;
            textureMenuItem.IsChecked = false;
            if (phongWindowSettings != null)
                phongWindowSettings.Close();
            phongWindowSettings = null;

            GlobalSettings.currentMode = GlobalSettings.viewMode.randomFacesMode;
        }

        private void phongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            phongMenuItem.IsChecked = true;
            meshMenuItem.IsChecked = false;
            randomFacesMenuItem.IsChecked = false;
            textureMenuItem.IsChecked = false;

            if (phongWindowSettings != null)
                phongWindowSettings.Close();

            phongWindowSettings = new PhongWindow();
            phongWindowSettings.Show();

            GlobalSettings.currentMode = GlobalSettings.viewMode.phongMode;
            
        }

        private void textureMenuItem_Click(object sender, RoutedEventArgs e)
        {
            textureMenuItem.IsChecked = true;
            phongMenuItem.IsChecked = false;
            randomFacesMenuItem.IsChecked = false;
            meshMenuItem.IsChecked = false;
            if (phongWindowSettings != null)
                phongWindowSettings.Close();

            phongWindowSettings = new PhongWindow();
            phongWindowSettings.Show();

            GlobalSettings.currentMode = GlobalSettings.viewMode.textureMode;
        }

        private void opacityMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (opacityMenuItem.IsChecked)
                GlobalSettings.effectModes.Add(GlobalSettings.effectMode.opacityEffect);
            else
                GlobalSettings.effectModes.Remove(GlobalSettings.effectMode.opacityEffect);
        }

        private void fogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (fogMenuItem.IsChecked)
                GlobalSettings.effectModes.Add(GlobalSettings.effectMode.fogEffect);
            else
                GlobalSettings.effectModes.Remove(GlobalSettings.effectMode.fogEffect);
        }

        private void backfaceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (backfaceMenuItem.IsChecked)
                GlobalSettings.algorithmsUsed.Add(GlobalSettings.algorithm.backfaceCulling);
            else
                GlobalSettings.algorithmsUsed.Remove(GlobalSettings.algorithm.backfaceCulling);
        }
    }
}
