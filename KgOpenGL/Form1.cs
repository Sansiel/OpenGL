using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL;

namespace KgOpenGL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //0 - go
        //1 - wall
        //2 - place for turrel
        //3 - start
        //4 - finish
        int[,] map = new int[,] 
        { 
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            { 1, 1, 2, 1, 1, 0, 0, 0, 1, 1},
            { 3, 0, 0, 2, 2, 0, 2, 0, 0, 1},
            { 1, 2, 0, 0, 0, 0, 1, 2, 0, 4},
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        };

        private void openGLControl_OpenGLDraw(object sender, RenderEventArgs args)
        {
            OpenGL gl = this.openGLControl.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            int height = openGLControl.Height/10;
            int width = openGLControl.Width/5;
            float x = 0;
            float y = 0;

            gl.Translate(0.0f, 0.0f, -6.0f);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    int m = map[i, j];

                    x = i / 10;
                    y = j / 10;

                    //if (m==1) gl.Color(0, 0, 0);
                    if (m==2) gl.Color(0.3, 0, 0.5);


                    gl.Begin(OpenGL.GL_QUADS);
                    gl.Vertex(x, y, 0.0f);
                    gl.Vertex(x+1, y, 0.0f);
                    gl.Vertex(x+1, y-1, 0.0f);
                    gl.Vertex(x, y-1, 0.0f);
                    gl.End();

                }
            }

            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex(-1.0f, 1.0f, 0.0f);  // Слева вверху
            gl.Vertex(1.0f, 1.0f, 0.0f);  // Справа вверху
            gl.Vertex(1.0f, -1.0f, 0.0f);  // Справа внизу
            gl.Vertex(-1.0f, -1.0f, 0.0f);  // Слева внизу
            gl.End();
        }

        private void openGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            OpenGL gl = openGLControl.OpenGL;
            gl.ClearColor(0.1f, 0.5f, 1.0f, 0);
        }

        private void openGLControl_Resized(object sender, EventArgs e)
        {
            //  Возьмём OpenGL объект
            OpenGL gl = openGLControl.OpenGL;

            //  Зададим матрицу проекции
            gl.MatrixMode(OpenGL.GL_PROJECTION);

            //  Единичная матрица для последующих преобразований
            gl.LoadIdentity();

            //  Преобразование
            gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);

            //  Данная функция позволяет установить камеру и её положение
            //gl.LookAt(5, 4, 0, 
                        //0, 1, 0, 
                        //0, 1, 0);

            //  Зададим модель отображения
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }
    }
}
