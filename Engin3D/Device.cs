using System.Windows.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using SharpDX;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Engin3D
{
    public class Device
    {
        private byte[] backBuffer;
        private readonly float[] depthBuffer;
        private WriteableBitmap bmp;
        private readonly int renderWidth;
        private readonly int renderHeight;
        Vector3 lightPos = GlobalSettings.lightPos;
        public Camera cameraWorld;
        private WriteableBitmap currentTexture;
        private byte[] currentTextureBuffer;

        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            renderWidth = bmp.PixelWidth;
            renderHeight = bmp.PixelHeight;

            backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
            depthBuffer = new float[bmp.PixelWidth * bmp.PixelHeight];

            BitmapImage texture = new BitmapImage(new Uri("H:/MiNI/Vsem/GK I - 3D/Engin3D/Engin3D/res/tecza.png", UriKind.Absolute));
            currentTexture = new WriteableBitmap(texture);
            var stride = currentTexture.PixelWidth * ((currentTexture.Format.BitsPerPixel + 7) / 8);
            currentTextureBuffer = new byte[currentTexture.PixelHeight * stride];
            currentTexture.CopyPixels(currentTextureBuffer, stride, 0);
        }

        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                backBuffer[index] = r;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = b;
                backBuffer[index + 3] = a;
            }
            
            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
            }
        }

        public void PutPixel(int x, int y, float z, Color4 color)
        {
            var index = (x + y * renderWidth);
            var index4 = index * 4;

            if (GlobalSettings.effectModes.Contains(GlobalSettings.effectMode.opacityEffect))
            {
                double alpha = .3;

                if (depthBuffer[index] != float.MaxValue)
                {
                    int blue = (int)((color.Blue * 255 * alpha) + backBuffer[index4]);
                    int green = (int)((color.Green * 255 * alpha) + backBuffer[index4 + 1]);
                    int red = (int)((color.Red * 255 * alpha) + backBuffer[index4 + 2]);

                    backBuffer[index4] = blue > 255 ? (byte)255 : (byte)blue;
                    backBuffer[index4 + 1] = blue > 255 ? (byte)255 : (byte)green;
                    backBuffer[index4 + 2] = blue > 255 ? (byte)255 : (byte)red;
                    backBuffer[index4 + 3] = 255;
                }
                else
                {
                    backBuffer[index4] = (byte)((color.Blue * 255 * alpha));
                    backBuffer[index4 + 1] = (byte)((color.Green * 255 * alpha));
                    backBuffer[index4 + 2] = (byte)((color.Red * 255 * alpha));
                    backBuffer[index4 + 3] = 255;
                }

                depthBuffer[index] = z;
            }
            else
            {
                if (depthBuffer[index] < z)
                {
                    return;
                }

                depthBuffer[index] = z;

                backBuffer[index4] = (byte)((color.Blue * 255));
                backBuffer[index4 + 1] = (byte)((color.Green * 255));
                backBuffer[index4 + 2] = (byte)((color.Red * 255));
                backBuffer[index4 + 3] = (byte)((color.Alpha * 255));
            }
        }

        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        Vector3 Interpolate(Vector3 min, Vector3 max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Color4 color)
        {
            Vector3 pa = va.Coordinates;
            Vector3 pb = vb.Coordinates;
            Vector3 pc = vc.Coordinates;
            Vector3 pd = vd.Coordinates;

            var gradient1 = pa.Y != pb.Y ? (data.currentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.currentY - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var sNormal = Interpolate(va.Normal, vb.Normal, gradient1);
            var eNormal = Interpolate(vc.Normal, vd.Normal, gradient2);

            var su = Interpolate(data.ua, data.ub, gradient1);
            var eu = Interpolate(data.uc, data.ud, gradient2);
            var sv = Interpolate(data.va, data.vb, gradient1);
            var ev = Interpolate(data.vc, data.vd, gradient2);

            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var pNormal = Interpolate(sNormal, eNormal, gradient);

                if (GlobalSettings.currentMode == GlobalSettings.viewMode.phongMode)
                {
                    var L = GlobalSettings.lightPos;
                    var n = pNormal;
                    var R = 2 * Vector3.Dot(L, n)*n - L;
                    var A = GlobalSettings.ambientColor;      // can be set as (R, G, B)
                    var D = GlobalSettings.diffuseColor;      // can be set as (R, G, B)
                    var S = GlobalSettings.specularColor;     // can be set as (R, G, B)
                    var p = GlobalSettings.specularPower;     // can be set as Int
                    Vector3 specularMax = Vector3.Max(new Vector3(0, 0, 0), R * L);
                    Vector3 specularPowered = specularMax;

                    /*
                    for (int i = 1; i < p; ++i)
                        specularPowered *= specularMax;
                        */

                    float xda = Math.Max(0, Vector3.Dot(R, L));
                    float xdb = 1;
                    for (int i = 1; i <= p; ++i)
                        xdb = xdb * xda;

                    //Vector3 colorOut = A + D * Vector3.Max(new Vector3(0, 0, 0), n * L) + S * specularPowered;
                    //Vector3 colorOut = A + Vector3.Dot(L, n) * D + S * specularPowered;
                    Vector3 colorOut = A + D * Math.Max(0, Vector3.Dot(n, L)) + S * xdb;
                    // Target => Position

                    if (GlobalSettings.effectModes.Contains(GlobalSettings.effectMode.fogEffect))
                    {
                        var dist = Vector3.Distance(cameraWorld.Position, pNormal);
                        //colorOut = colorOut * (1 / dist) * 5;

                        // TEST DRIVEN
                        colorOut = colorOut * dist / 5;
                    }

                    Color colorDraw = new Color(colorOut.X, colorOut.Y, colorOut.Z);
                    DrawPoint(new Vector3(x, data.currentY, z), colorDraw);
                }
                else if (GlobalSettings.currentMode == GlobalSettings.viewMode.textureMode)
                {
                    float leftX = Math.Min(pa.X, pb.X);

                    // INTERPOLACJA JEST ZLA
                    // I TORY SA ZLE
                    // I POCIAG TEZ BYL ZLY
                    var u = Interpolate(su, eu, gradient);
                    var v = Interpolate(sv, sv, gradient);

                    //var multiply = 1;
                    //var u = x * z * 10/ multiply;
                    //var v = data.currentY * z * 10 / multiply;

                    if (u > 1)
                    {
                        u = u / 10;
                        return;
                    }

                    if (v > 1)
                    {
                        v = v / 10;
                        return;
                    }

                    int textureX = Math.Abs((int)(u * currentTexture.PixelWidth)) % currentTexture.PixelWidth;
                    int textureY = Math.Abs((int)(v * currentTexture.PixelHeight)) % currentTexture.PixelHeight;

                    int colorB = currentTextureBuffer[textureY * currentTexture.BackBufferStride + textureX * 4];
                    int colorG = currentTextureBuffer[textureY * currentTexture.BackBufferStride + textureX * 4 + 1];
                    int colorR = currentTextureBuffer[textureY * currentTexture.BackBufferStride + textureX * 4 + 2];

                    /*
                    GlobalSettings.lightPos = new Vector3(0, 1, 1);
                    GlobalSettings.ambientColor = new Vector3(0, 0.196f, 0);
                    GlobalSettings.diffuseColor = new Vector3(0, 0.274f, 0);
                    GlobalSettings.specularColor = new Vector3(0, 0.1f, 0);
                    GlobalSettings.specularPower = 6;

                    var L = GlobalSettings.lightPos;          // can be set as (X, Y, Z)
                    var n = pNormal;
                    var R = 2 * n * Vector3.Dot(n, L) - L;   // TODO: leave on last 
                    var A = new Vector3(colorR / 255.0f, colorR / 255.0f, colorB / 255.0f);      // can be set as (R, G, B)
                    var D = GlobalSettings.diffuseColor;      // can be set as (R, G, B)
                    var S = GlobalSettings.specularColor;     // can be set as (R, G, B)
                    var p = GlobalSettings.specularPower;     // can be set as Int
                    Vector3 specularMax = Vector3.Max(new Vector3(0, 0, 0), R * L);
                    Vector3 specularPowered = specularMax;

                    for (int i = 1; i < p; ++i)
                        specularPowered *= specularMax;

                    Vector3 colorOut = A + D * Vector3.Max(new Vector3(0, 0, 0), n * L) + S * specularPowered;
                    */

                    var colorOut = new Vector3(colorR / 255.0f, colorR / 255.0f, colorB / 255.0f);

                    if (GlobalSettings.effectModes.Contains(GlobalSettings.effectMode.fogEffect))
                    {
                        var dist = Vector3.Distance(cameraWorld.Position, pNormal);
                        //colorOut = colorOut * (1 / dist) * 5;

                        colorOut = colorOut * dist / 3;
                    }

                    Color colorDraw = new Color(colorOut.X, colorOut.Y, colorOut.Z);
                    DrawPoint(new Vector3(x, data.currentY, z), colorDraw);
                }
                else
                {
                    Vector3 colorOut = new Vector3(color.Red, color.Green, color.Blue);

                    if (GlobalSettings.effectModes.Contains(GlobalSettings.effectMode.fogEffect))
                    {
                        var dist = Vector3.Distance(cameraWorld.Position, pNormal);

                        //colorOut = colorOut * (1 / dist) * 5;
                        colorOut = colorOut * dist / 5;
                    }

                    DrawPoint(new Vector3(x, data.currentY, z), new Color(colorOut.X, colorOut.Y, colorOut.Z));
                }
            }
        }

        float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return Math.Max(0, Vector3.Dot(normal, lightDirection));
        }

        public Vertex Project(Vertex vertex, Matrix transMat, Matrix world)
        {
            var point2d = Vector3.TransformCoordinate(vertex.Coordinates, transMat);
            var point3dWorld = Vector3.TransformCoordinate(vertex.Coordinates, world);
            var normal3dWorld = Vector3.TransformCoordinate(vertex.Normal, world);
            var x = point2d.X * renderWidth + renderWidth / 2.0f;
            var y = -point2d.Y * renderHeight + renderHeight / 2.0f;

            return new Vertex
            {
                Coordinates = new Vector3(x, y, point2d.Z),
                Normal = normal3dWorld,
                WorldCoordinates = point3dWorld,
                TextureCoordinates = new Vector2(vertex.Coordinates.X, vertex.Coordinates.Z)
            };
        }

        public void DrawPoint(Vector3 point, Color4 color)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
            {
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }

        public void Present()
        {
            bmp.Lock();
            bmp.Clear(System.Windows.Media.Color.FromRgb(0, 0, 0));
            Marshal.Copy(backBuffer, 0, bmp.BackBuffer, backBuffer.Length);
            bmp.Unlock();
        }

        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Color4 color)
        {
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            Vector3 p1 = v1.Coordinates;
            Vector3 p2 = v2.Coordinates;
            Vector3 p3 = v3.Coordinates;

            float nl1 = ComputeNDotL(v1.WorldCoordinates, v1.Normal, lightPos);
            float nl2 = ComputeNDotL(v2.WorldCoordinates, v2.Normal, lightPos);
            float nl3 = ComputeNDotL(v3.WorldCoordinates, v3.Normal, lightPos);

            var data = new ScanLineData { };

            float dP1P2, dP1P3;

            // http://en.wikipedia.org/wiki/Slope
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            // Slope case:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl2;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v2.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v2.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v3, v1, v2, color);
                    }
                    else
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl2;
                        data.ndotld = nl3;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v2.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v2.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v3, v2, v3, color);
                    }
                }
            }
            // Slop case:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl2;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v2.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v2.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v2, v1, v3, color);
                    }
                    else
                    {
                        data.ndotla = nl2;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;

                        data.ua = v2.Coordinates.X;
                        data.ub = v3.Coordinates.X;
                        data.uc = v1.Coordinates.X;
                        data.ud = v3.Coordinates.X;

                        data.va = v2.Coordinates.Y;
                        data.vb = v3.Coordinates.Y;
                        data.vc = v1.Coordinates.Y;
                        data.vd = v3.Coordinates.Y;

                        ProcessScanLine(data, v2, v3, v1, v3, color);
                    }
                }
            }
        }

        private float Angle(Vector3 v1, Vector3 v2)
        {
            v1.Normalize();
            v2.Normalize();
            var cosA = Vector3.Dot(v1, v2);

            return cosA;
        }

        public void Render(Camera camera, params Mesh[] meshes)
        {
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix.PerspectiveFovLH(0.78f,
                                                           (float)bmp.PixelWidth / bmp.PixelHeight,
                                                           0.01f, 1.0f);

            foreach (Mesh mesh in meshes)
            {
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) *
                                  Matrix.Translation(mesh.Position);

                var worldView = worldMatrix * viewMatrix;
                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                var faceIndex = 0;
                foreach (var face in mesh.Faces)
                {
                    Vector3 normalR = Vector3.TransformCoordinate(face.Normal, worldMatrix);
                    Vector3 cameraPos = camera.Position - mesh.Vertices[face.A].WorldCoordinates;
                    float angle = Angle(normalR, cameraPos);

                    if (GlobalSettings.algorithmsUsed.Contains(GlobalSettings.algorithm.backfaceCulling) && angle < 0)
                        continue;

                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = Project(vertexA, transformMatrix, worldMatrix);
                    var pixelB = Project(vertexB, transformMatrix, worldMatrix);
                    var pixelC = Project(vertexC, transformMatrix, worldMatrix);

                    if (GlobalSettings.currentMode == GlobalSettings.viewMode.meshMode)
                    {
                        DrawLine(pixelA, pixelB);
                        DrawLine(pixelB, pixelC);
                        DrawLine(pixelC, pixelA);
                    }
                    else if (GlobalSettings.currentMode == GlobalSettings.viewMode.randomFacesMode)
                    {
                        var colorR = 0.25f + (faceIndex % mesh.Faces.Length) * 0.75f / mesh.Faces.Length;
                        var colorG = 0.15f + (faceIndex % mesh.Faces.Length) * 0.85f / mesh.Faces.Length;
                        var colorB = 0.45f + (faceIndex % mesh.Faces.Length) * 0.55f / mesh.Faces.Length;

                        DrawTriangle(pixelA, pixelB, pixelC, new Color4(colorR, colorG, colorB, 1));
                    }
                    else if (GlobalSettings.currentMode == GlobalSettings.viewMode.textureMode)
                    {
                        var offsetX = (faceIndex * 45 % (renderWidth - 100));
                        var offsetY = (faceIndex * 55 % (renderHeight - 100));

                        DrawTriangle(pixelA, pixelB, pixelC, new Color4());
                    }
                    else
                    {
                        DrawTriangle(pixelA, pixelB, pixelC, new Color4(0.0f, 0.0f, 1.0f, 1));
                    }

                    faceIndex++;
                }
            }
        }

        public void DrawLine(Vertex point0, Vertex point1)
        {
            int x0 = (int)point0.Coordinates.X;
            int y0 = (int)point0.Coordinates.Y;
            int x1 = (int)point1.Coordinates.X;
            int y1 = (int)point1.Coordinates.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                DrawPoint(new Vector3(x0, y0, point0.Coordinates.Z), new Color4(0.0f, 0.0f, 1.0f, 1));

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }
    }
}
