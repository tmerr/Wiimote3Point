using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace TestWiimote3Point
{
    public partial class IRSensorsView : UserControl
    {
        int vbo;

        Vector3[] vertices =
        {
            new Vector3(-1f, -1f, 0),
            new Vector3(1f, -1f, 0f),
            new Vector3(0f, 1f, 0f),
        };

        public IRSensorsView()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                this.Controls.Add(this.glControl1);
            }
        }

        private void IRSensorsView2_Load(object sender, EventArgs e)
        {
            glControl1.MakeCurrent();

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(-1f, 1f, -1f, 1f, -1f, 1f);
            GL.LoadMatrix(ref projection);

            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4 modelview = Matrix4.Identity;
            GL.LoadMatrix(ref modelview);

            vbo = GL.GenBuffer();

            this.Invalidate();
        }

        public void UpdatePoints(Vector3[] vertices)
        {
            if (vertices.Length != 3)
            {
                throw new ArgumentException("vertices");
            }
            this.vertices = vertices;
            this.Invalidate();
        }

        private void IRSensorsView2_Paint(object sender, PaintEventArgs e)
        {
            glControl1.MakeCurrent();
            GL.ClearColor(Color.Blue);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.Color4(Color.Black);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(vertices.Length * Vector3.SizeInBytes),
                vertices, BufferUsageHint.StaticDraw);
            GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DisableVertexAttribArray(0);
            glControl1.SwapBuffers();
        }
    }
}
