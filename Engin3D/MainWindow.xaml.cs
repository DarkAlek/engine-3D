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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using Microsoft.Win32;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Engin3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private Device device;
        ObservableCollection<Mesh> meshes = new ObservableCollection<Mesh>();
        Camera mera = new Camera();
        Stopwatch fpsWatcher = new Stopwatch();
        Stopwatch fpsUpdateLabelWatcher = new Stopwatch();
        PhongWindow phongWindowSettings = null;
        Mesh selectedmesh;

        public ObservableCollection<Mesh> Meshes
        {
            get { return meshes; }

            set
            {
                meshes = value;
                OnPropertyChanged("Meshes");
            }
        }

        public Mesh SelectedMesh
        {
            get { return selectedmesh; }
            set
            {
                selectedmesh = value;
                OnPropertyChanged("SelectedMesh");
            }
        }

        double mouseLastX = 0;
        double mouseLastY = 0;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = BitmapFactory.New(1000, 1000);

            device = new Device(bmp, ref frontBuffer);
            device.cameraWorld = mera;
            frontBuffer.Source = bmp;
            frontBuffer.Stretch = Stretch.Fill;

            //MouseWheel += MainWindow_MouseWheel;         

            mera.Position = new Vector3(0, 1f, 10f);
            mera.Target = Vector3.Zero;
            //meshes.Add(new Sphere("Sphere", 1, 24, 24));
            //meshes.Add(new Cuboid("Cuboid", 1, 1, 1));
            //meshesList.SelectedItem = meshes[0];

            fpsWatcher.Start();
            fpsUpdateLabelWatcher.Start();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void FrontBuffer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double xOffset = e.GetPosition(frontBuffer).X - mouseLastX;
                double yOffset = e.GetPosition(frontBuffer).Y - mouseLastY;

                mouseLastX = e.GetPosition(frontBuffer).X;
                mouseLastY = e.GetPosition(frontBuffer).Y;

                mera.Position = new Vector3(
                    mera.Position.X + (float)xOffset / 100,
                    mera.Position.Y + (float)yOffset / 100,
                    mera.Position.Z
                );
            }
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
                float offset = 0.05f;

                Vector3 vectorRef = new Vector3(
                     mera.Position.X,
                     mera.Position.Y,
                     mera.Position.Z
                 );

                SharpDX.Matrix matrix;
                SharpDX.Matrix.RotationX(offset, out matrix);

                Vector3 newVector = Vector3.TransformCoordinate(mera.Position, matrix);
                mera.Position = newVector;


            }
            else if (Keyboard.IsKeyDown(Key.NumPad8))
            {
                float offset = 0.05f;

                Vector3 vectorRef = new Vector3(
                     mera.Position.X,
                     mera.Position.Y,
                     mera.Position.Z
                 );

                SharpDX.Matrix matrix;
                SharpDX.Matrix.RotationX(-offset, out matrix);

                Vector3 newVector = Vector3.TransformCoordinate(mera.Position, matrix);
                mera.Position = newVector;
            }
            else if (Keyboard.IsKeyDown(Key.NumPad4))
            {
                float offset = 0.05f;

                Vector3 vectorRef = new Vector3(
                     mera.Position.X,
                     mera.Position.Y,
                     mera.Position.Z
                 );

                SharpDX.Matrix matrix;
                SharpDX.Matrix.RotationY(-offset, out matrix);

                Vector3 newVector = Vector3.TransformCoordinate(mera.Position, matrix);
                mera.Position = newVector;

            }
            else if (Keyboard.IsKeyDown(Key.NumPad6))
            {
                float offset = 0.05f;

                Vector3 vectorRef = new Vector3(
                     mera.Position.X,
                     mera.Position.Y,
                     mera.Position.Z
                 );

                SharpDX.Matrix matrix;
                SharpDX.Matrix.RotationY(offset, out matrix);

                Vector3 newVector = Vector3.TransformCoordinate(mera.Position, matrix);
                mera.Position = newVector;
            }

            if (Keyboard.IsKeyDown(Key.Add))
            {
                mera.Position = new Vector3(
                    mera.Position.X,
                    mera.Position.Y,
                    mera.Position.Z) - new Vector3(0.01f * mera.Position.X,
                                                   0.01f * mera.Position.Y,
                                                   0.01f * mera.Position.Z);
            }
            else if (Keyboard.IsKeyDown(Key.Subtract))
            {
                mera.Position = new Vector3(
                    mera.Position.X,
                    mera.Position.Y,
                    mera.Position.Z) + new Vector3(0.01f * mera.Position.X,
                                                   0.01f * mera.Position.Y,
                                                   0.01f * mera.Position.Z);
            }
        }
        private void MoveSelectedMeshRotate()
        {
            if (SelectedMesh != null)
            {
                if (Keyboard.IsKeyDown(Key.W))
                {
                    SelectedMesh.Rotation = new Vector3(
                        SelectedMesh.Rotation.X + 0.02f,
                        SelectedMesh.Rotation.Y,
                        SelectedMesh.Rotation.Z);
                }
                else if (Keyboard.IsKeyDown(Key.S))
                {
                    SelectedMesh.Rotation = new Vector3(
                        SelectedMesh.Rotation.X - 0.02f,
                        SelectedMesh.Rotation.Y,
                        SelectedMesh.Rotation.Z);
                }
                else if (Keyboard.IsKeyDown(Key.A))
                {
                    SelectedMesh.Rotation = new Vector3(
                        SelectedMesh.Rotation.X,
                        SelectedMesh.Rotation.Y + 0.02f,
                        SelectedMesh.Rotation.Z);
                }
                if (Keyboard.IsKeyDown(Key.D))
                {
                    SelectedMesh.Rotation = new Vector3(
                        SelectedMesh.Rotation.X,
                        SelectedMesh.Rotation.Y - 0.02f,
                        SelectedMesh.Rotation.Z);
                }
            }
        }


        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(0, 0, 0, 255);

            //selectedmesh = meshesList.SelectedItem as Mesh;
            //SelectedMesh.Rotation = new Vector3(SelectedMesh.Rotation.X, SelectedMesh.Rotation.Y, SelectedMesh.Rotation.Z);

            mera.Position = new Vector3(
                mera.Position.X,
                mera.Position.Y,
                mera.Position.Z);

            MoveCameraXY();
            MoveCameraRotate();
            MoveSelectedMeshRotate();

            device.Render(mera, ref meshes);
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
            //meshesList.ItemsSource = meshes;

        }

        private void SaveFiguresToFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("    ");
                using (XmlWriter writer = XmlWriter.Create(saveFileDialog.FileName, settings))
                {
                    writer.WriteStartElement("Scene");
                    // Write XML data.
                    foreach (var figure in meshes)
                    {
                        if (figure is Cuboid)
                        {
                            writer.WriteStartElement(figure.Name);
                            writer.WriteStartElement("Position");
                            writer.WriteElementString("X", (figure as Cuboid).Position.X.ToString());
                            writer.WriteElementString("Y", (figure as Cuboid).Position.Y.ToString());
                            writer.WriteElementString("Z", (figure as Cuboid).Position.Z.ToString());
                            writer.WriteEndElement();
                            writer.WriteStartElement("Rotation");
                            writer.WriteElementString("X", (figure as Cuboid).Rotation.X.ToString());
                            writer.WriteElementString("Y", (figure as Cuboid).Rotation.Y.ToString());
                            writer.WriteElementString("Z", (figure as Cuboid).Rotation.Z.ToString());
                            writer.WriteEndElement();
                            writer.WriteStartElement("Parameters");
                            writer.WriteElementString("A", (figure as Cuboid).A.ToString());
                            writer.WriteElementString("B", (figure as Cuboid).B.ToString());
                            writer.WriteElementString("C", (figure as Cuboid).C.ToString());
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }
                        else
                        {
                            writer.WriteStartElement(figure.Name);
                            writer.WriteStartElement("Position");
                            writer.WriteElementString("X", (figure as Sphere).Position.X.ToString());
                            writer.WriteElementString("Y", (figure as Sphere).Position.Y.ToString());
                            writer.WriteElementString("Z", (figure as Sphere).Position.Z.ToString());
                            writer.WriteEndElement();
                            writer.WriteStartElement("Rotation");
                            writer.WriteElementString("X", (figure as Sphere).Rotation.X.ToString());
                            writer.WriteElementString("Y", (figure as Sphere).Rotation.Y.ToString());
                            writer.WriteElementString("Z", (figure as Sphere).Rotation.Z.ToString());
                            writer.WriteEndElement();
                            writer.WriteStartElement("Parameters");
                            writer.WriteElementString("Radius", (figure as Sphere).Radius.ToString());
                            writer.WriteElementString("SegmentsLong", (figure as Sphere).SegmentsLong.ToString());
                            writer.WriteElementString("SegmentsLat", (figure as Sphere).SegmentsLat.ToString());
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }

                    }
                    writer.WriteEndElement();
                    writer.Flush();
                }
            }
        }

        private void LoadFiguresFromFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                meshes = new ObservableCollection<Mesh>();
                meshesList.ItemsSource = meshes;
                using (XmlReader reader = XmlReader.Create(openFileDialog.FileName))
                {
                    reader.MoveToContent();
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "Cuboid" || reader.Name == "Sphere")
                            {
                                XElement el = XNode.ReadFrom(reader) as XElement;
                                if (el.Name == "Cuboid")
                                {
                                    float postitionX = float.Parse(el.Element("Position").Element("X").Value);
                                    float postitionY = float.Parse(el.Element("Position").Element("Y").Value);
                                    float postitionZ = float.Parse(el.Element("Position").Element("Z").Value);

                                    float rotationX = float.Parse(el.Element("Rotation").Element("X").Value);
                                    float rotationY = float.Parse(el.Element("Rotation").Element("Y").Value);
                                    float rotationZ = float.Parse(el.Element("Rotation").Element("Z").Value);

                                    float A = float.Parse(el.Element("Parameters").Element("A").Value);
                                    float B = float.Parse(el.Element("Parameters").Element("B").Value);
                                    float C = float.Parse(el.Element("Parameters").Element("C").Value);

                                    Cuboid cuboid = new Cuboid("Cuboid", A, B, C);
                                    cuboid.Position = new Vector3(postitionX, postitionY, postitionZ);
                                    cuboid.Rotation = new Vector3(rotationX, rotationY, rotationZ);
                                    meshes.Add(cuboid);
                                }
                                else
                                {
                                    float postitionX = float.Parse(el.Element("Position").Element("X").Value);
                                    float postitionY = float.Parse(el.Element("Position").Element("Y").Value);
                                    float postitionZ = float.Parse(el.Element("Position").Element("Z").Value);

                                    float rotationX = float.Parse(el.Element("Rotation").Element("X").Value);
                                    float rotationY = float.Parse(el.Element("Rotation").Element("Y").Value);
                                    float rotationZ = float.Parse(el.Element("Rotation").Element("Z").Value);

                                    float radius = float.Parse(el.Element("Parameters").Element("Radius").Value);
                                    int segmentsLong = int.Parse(el.Element("Parameters").Element("SegmentsLong").Value);
                                    int segmentsLat = int.Parse(el.Element("Parameters").Element("SegmentsLat").Value);

                                    Sphere sphere = new Sphere("Sphere", radius, segmentsLong, segmentsLat);
                                    sphere.Position = new Vector3(postitionX, postitionY, postitionZ);
                                    sphere.Rotation = new Vector3(rotationX, rotationY, rotationZ);
                                    meshes.Add(sphere);
                                }
                            }
                        }
                    }
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

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (meshesList.SelectedIndex == -1)
            {
                return;
            }
            meshes.RemoveAt(meshesList.SelectedIndex);
        }

        private void MenuItemAddCuboid_Click(object sender, RoutedEventArgs e)
        {
            meshes.Add(new Cuboid("Cuboid", 1, 1, 1));
        }

        private void MenuItemAddSphere_Click(object sender, RoutedEventArgs e)
        {
            meshes.Add(new Sphere("Sphere", 1, 24, 16));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }

    public class vectorContainer
    {
        public static Vector3 vector;
    }

    public class XFloatToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            vectorContainer.vector = (Vector3)value;
            return vectorContainer.vector.X.ToString("n4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value as string == "") return false;
            try
            {
                return new Vector3(float.Parse(value as string), vectorContainer.vector.Y, vectorContainer.vector.Z);
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public class YFloatToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            vectorContainer.vector = (Vector3)value;
            return vectorContainer.vector.Y.ToString("n4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value as string == "") return false;
            try
            {
                return new Vector3(vectorContainer.vector.X, float.Parse(value as string), vectorContainer.vector.Z);
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public class ZFloatToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            vectorContainer.vector = (Vector3)value;
            return vectorContainer.vector.Z.ToString("n4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value as string == "") return false;
            try
            {
                return new Vector3(vectorContainer.vector.X, vectorContainer.vector.Y, float.Parse(value as string));
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public class FloatToFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float param = System.Convert.ToSingle(value);
            return param.ToString("n4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value as string == "") return false;
            try
            {
                return float.Parse(value as string);
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public class VisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Hidden;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return true;
        }
    }

    public class CuboidVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Cuboid) return Visibility.Visible;
            else return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return true;
        }
    }

    public class SphereVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Sphere) return Visibility.Visible;
            else return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return true;
        }
    }
}
