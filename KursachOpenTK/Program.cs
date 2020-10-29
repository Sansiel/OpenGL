using System;
using System.IO;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Drawing.Text;
using OpenTK.Graphics.ES11;

namespace KursachOpenTK
{
    class Program : GameWindow
    {
        [STAThread]
        static void Main()
        {
            using (var Program = new Program())
            {
                Program.Run(30);
            }
        }

        protected override void OnLoad(EventArgs E)
        {
            base.OnLoad(E);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            New();
        }


        protected override void OnResize(EventArgs E)
        { }
        protected override void OnUpdateFrame(FrameEventArgs E)
        { }
        protected override void OnRenderFrame(FrameEventArgs E)
        {
            base.OnRenderFrame(E);
        }
        private void New()
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Color4(Color4.Black);

            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(StickLength * SolidSize, 0);
            GL.Vertex2(StickLength * SolidSize, SolidSize);
            GL.Vertex2(0, SolidSize);
            GL.End();

            GL.Enable(EnableCap.Texture2D);
        }
    }
}
