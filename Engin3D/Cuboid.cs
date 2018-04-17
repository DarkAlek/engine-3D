using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engin3D
{
    class Cuboid : Mesh, INotifyPropertyChanged
    {
        private string name;
        private float a;
        private float b;
        private float c;
        private Vector3 position;
        private Vector3 rotation;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                OnPropertyChanged("Position");
            }
        }
        public Vector3 Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                OnPropertyChanged("Position");
            }
        }

        public float A
        {
            get { return a; }
            set
            {
                a = value;
                InitMesh();
                OnPropertyChanged("A");
            }
        }
        public float B
        {
            get { return b; }
            set
            {
                b = value;
                InitMesh();
                OnPropertyChanged("B");
            }
        }
        public float C
        {
            get { return c; }
            set
            {
                c = value;
                InitMesh();
                OnPropertyChanged("C");
            }
        }


        public Vertex[] Vertices { get; set; }
        public Face[] Faces { get; set; }

        public Cuboid(string name, float a, float b, float c)
        {
            Vertices = new Vertex[8];
            Faces = new Face[12];
            Name = name;
            this.a = a;
            this.b = b;
            this.c = c;
            this.Rotation = new Vector3(0, 0.5f, 0);
            InitMesh();
        }

        public void InitMesh()
        {
            this.Vertices[0].Coordinates = new Vector3(-a * .5f, -b * .5f, c * .5f);
            this.Vertices[1].Coordinates = new Vector3(a * .5f, -b * .5f, c * .5f);
            this.Vertices[2].Coordinates = new Vector3(a* .5f, -b * .5f, -c * .5f);
            this.Vertices[3].Coordinates = new Vector3(-a * .5f, -b * .5f, -c * .5f);

            this.Vertices[4].Coordinates = new Vector3(-a * .5f, b * .5f, c * .5f);
            this.Vertices[5].Coordinates = new Vector3(a * .5f, b * .5f, c * .5f);
            this.Vertices[6].Coordinates = new Vector3(a * .5f, b * .5f, -c * .5f);
            this.Vertices[7].Coordinates = new Vector3(-a * .5f, b * .5f, -c * .5f);

            this.Faces[0] = new Face { A = 3, B = 1, C = 0 };
            this.Faces[1] = new Face { A = 3, B = 2, C = 1 };

            this.Faces[2] = new Face { A = 3, B = 4, C = 7 };
            this.Faces[3] = new Face { A = 3, B = 0, C = 4 };

            this.Faces[4] = new Face { A = 0, B = 5, C = 4 };
            this.Faces[5] = new Face { A = 0, B = 1, C = 5 };

            this.Faces[6] = new Face { A = 2, B = 7, C = 6 };
            this.Faces[7] = new Face { A = 2, B = 3, C = 7 };

            this.Faces[8] = new Face { A = 1, B = 6, C = 5 };
            this.Faces[9] = new Face { A = 1, B = 2, C = 6 };

            this.Faces[10] = new Face { A = 4, B = 6, C = 7 };
            this.Faces[11] = new Face { A = 4, B = 5, C = 6 };

            for (int i = 0; i < this.Faces.Length; ++i)
            {
                Face face = this.Faces[i];
                Vector3 p1 = this.Vertices[face.A].Coordinates;
                Vector3 p2 = this.Vertices[face.B].Coordinates;
                Vector3 p3 = this.Vertices[face.C].Coordinates;

                Vector3 u = p2 - p1;
                Vector3 v = p3 - p1;

                float nx = u.Y * v.Z - u.Z * v.Y;
                float ny = u.Z * v.X - u.X * v.Z;
                float nz = u.X * v.Y - u.Y * v.X;

                this.Faces[i].Normal = new Vector3(nx, ny, nz);
            }

            for (int i = 0; i < this.Faces.Length; ++i)
            {
                Face face = this.Faces[i];
                this.Vertices[face.A].Normal += face.Normal;
                this.Vertices[face.B].Normal += face.Normal;
                this.Vertices[face.C].Normal += face.Normal;
            }

            for (int i = 0; i < this.Faces.Length; ++i)
            {
                Face face = this.Faces[i];
                this.Vertices[face.A].Normal.Normalize();
                this.Vertices[face.B].Normal.Normalize();
                this.Vertices[face.C].Normal.Normalize();
            }

            //this.Position = new Vector3(1, 0, 0);
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
    }
}
