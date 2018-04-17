// Mesh.cs
using SharpDX;
using System.ComponentModel;

namespace Engin3D
{
    public interface Mesh
    {
        string Name { get; set; }
        Vector3 Rotation{ get; set; }
        Vector3 Position { get; set; }
        Vertex[] Vertices { get; set; }
        Face[] Faces { get; set; }
        void InitMesh();
    }

    public struct Vertex
    {
        public Vector3 Normal;
        public Vector3 Coordinates;
        public Vector3 WorldCoordinates;
        public Vector2 TextureCoordinates;
    }
}