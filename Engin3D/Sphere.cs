using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engin3D
{
    class Sphere : Mesh, INotifyPropertyChanged
    {
        private string name;
        private float radius;
        private Vector3 position;
        private Vector3 rotation;
        private int segmentsLong;
        private int segmentsLat;

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

        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                InitMesh();
                OnPropertyChanged("Radius");
            }
        }
        public int SegmentsLong
        {
            get { return segmentsLong; }
            set
            {
                segmentsLong = value;
                InitMesh();
                OnPropertyChanged("SegmentsLong");
            }
        }
        public int SegmentsLat
        {
            get { return segmentsLat; }
            set
            {
                segmentsLat = value;
                InitMesh();
                OnPropertyChanged("SegmentsLat");
            }
        }

        public Vertex[] Vertices { get; set; }
        public Face[] Faces { get; set; }

        public Sphere(string name, float r, int segmentsLong, int segmentsLat)
        {
            this.segmentsLong = segmentsLong;
            this.segmentsLat = segmentsLat;
            this.radius = r;
            Name = name;
            InitMesh();
        }

        public void InitMesh()
        {
            if (segmentsLong == 0 || segmentsLat == 0) return;
            int verticesCount = (segmentsLong + 1) * segmentsLat + 2;
            int facesCount = 2 * verticesCount;
            Vertices = new Vertex[verticesCount];
            Faces = new Face[facesCount];

            // vertices
            this.Vertices[0].Coordinates = new Vector3(0, 1, 0) * radius;
            for (int lat = 0; lat < segmentsLat; lat++)
            {
                float a1 = (float)Math.PI * (float)(lat + 1) / (segmentsLat + 1);
                float sin1 = (float)Math.Sin(a1);
                float cos1 = (float)Math.Cos(a1);

                for (int lon = 0; lon <= segmentsLong; lon++)
                {
                    float a2 = 2 * (float)Math.PI * (float)(lon == segmentsLong ? 0 : lon) / segmentsLong;
                    float sin2 = (float)Math.Sin(a2);
                    float cos2 = (float)Math.Cos(a2);

                    this.Vertices[lon + lat * (segmentsLong + 1) + 1].Coordinates = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
                }
            }
            this.Vertices[verticesCount - 1].Coordinates = new Vector3(0, 1, 0) * -radius;

            //Top Cap
            int it = 0;
            for (int lon = 0; lon < segmentsLong; lon++)
            {
                this.Faces[it++] = new Face { A = lon + 2, B = lon + 1, C = 0 };

            }

            //Middle
            for (int lat = 0; lat < segmentsLat - 1; lat++)
            {
                for (int lon = 0; lon < segmentsLong; lon++)
                {
                    int current = lon + lat * (segmentsLong + 1) + 1;
                    int next = current + segmentsLong + 1;

                    this.Faces[it++] = new Face { A = current, B = current + 1, C = next + 1 };
                    this.Faces[it++] = new Face { A = current, B = next + 1, C = next };
                }
            }

            //Bottom Cap
            for (int lon = 0; lon < segmentsLong; lon++)
            {
                this.Faces[it++] = new Face { A = verticesCount - 1, B = verticesCount - (lon + 2) - 1, C = verticesCount - (lon + 1) - 1 };
            }

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
