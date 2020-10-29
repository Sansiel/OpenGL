using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace KursachOpenTK
{
    public enum MaterialType
    {
        /// <summary>
        /// Represents a Diffuse Texture
        /// </summary>
        Diffuse,
        /// <summary>
        /// Represents a Normal Texture
        /// </summary>
        Normal
    }

    public class Material
    {
        /// <summary>
        /// The name of the Material
        /// </summary>
        public string Name;
        /// <summary>
        /// The Diffuse Texture
        /// </summary>
        public int Diffuse;
        /// <summary>
        /// The Normal Texture
        /// </summary>
        public int NormalMap;
        /// <summary>
        /// The Ambient Color for the Material
        /// </summary>
        public Color AmbientColor;

        public Material(string materialName)
        {
            this.Name = materialName;
            this.AmbientColor = Color.White;
            this.Diffuse = 0;
            this.NormalMap = 0;
        }

        /// <summary>
        /// Loads a Bitmap as a Diffuse texture.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        public void LoadDiffuse(Bitmap bitmap)
        {
            GL.Enable(EnableCap.Texture2D);
            //GL.Hint( HintTarget.PerspectiveCorrectionHint , HintMode.Nicest );
            GL.GenTextures(1, out Diffuse);
            GL.BindTexture(TextureTarget.Texture2D, Diffuse);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
