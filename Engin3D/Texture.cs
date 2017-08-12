

using SharpDX;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

public class Texture
{
    private byte[] internalBuffer = null;
    private int width;
    private int height;

    // Working with a fix sized texture (512x512, 1024x1024, etc.).
    public Texture(string filename, int width, int height)
    {
        this.width = width;
        this.height = height;
        Load(filename);
    }

    async void Load(string filename)
    {
        BitmapImage bitmap = new BitmapImage(new Uri(filename, UriKind.Absolute));
        WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap);
        internalBuffer = new byte[4 * width * height];

        Marshal.Copy(writeableBitmap.BackBuffer, internalBuffer, 0, 4 * width * height);
    }

    // Takes the U & V coordinates exported by Blender
    // and return the corresponding pixel color in the texture
    public Color4 Map(float tu, float tv)
    {
        // Image is not loaded yet
        if (internalBuffer == null)
        {
            return Color4.White;
        }
        // using a % operator to cycle/repeat the texture if needed
        int u = Math.Abs((int)(tu * width) % width);
        int v = Math.Abs((int)(tv * height) % height);

        int pos = (u + v * width) * 4;
        byte b = internalBuffer[pos];
        byte g = internalBuffer[pos + 1];
        byte r = internalBuffer[pos + 2];
        byte a = internalBuffer[pos + 3];

        return new Color4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    public bool IsSetTexture
    {
        get { return internalBuffer != null; }
    }
}