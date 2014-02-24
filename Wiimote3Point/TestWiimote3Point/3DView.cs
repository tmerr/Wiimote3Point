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
    public partial class WorldView : UserControl
    {
        private bool loaded = false;
        private Matrix4 projectionMatrix, modelviewMatrix;

        private List<Vector3> cubePositions = new List<Vector3>();
        private List<Vector3> cubeOrientations = new List<Vector3>();
        private Vector3 cameraRotation = new Vector3();
        private float cameraZoom = 1f;
        private bool dragging = false;
        private Point lastMouseLocation;

        #region Triangle data
        Vector3[] sensorTriangleVerts =
        {
            new Vector3(-1f, -1f, 0),
            new Vector3(1f, -1f, 0f),
            new Vector3(0f, 1f, 0f),
        };
        #endregion

        #region Cube data
        float[] cubeColors = {
			1.0f, 0.0f, 0.0f, 1.0f,
			0.0f, 1.0f, 0.0f, 1.0f,
			0.0f, 0.0f, 1.0f, 1.0f,
			0.0f, 1.0f, 1.0f, 1.0f,
			1.0f, 0.0f, 0.0f, 1.0f,
			0.0f, 1.0f, 0.0f, 1.0f,
			0.0f, 0.0f, 1.0f, 1.0f,
			0.0f, 1.0f, 1.0f, 1.0f,
		};
 
		byte[] triangles =
		{
			1, 0, 2, // front
			3, 2, 0,
			6, 4, 5, // back
			4, 6, 7,
			4, 7, 0, // left
			7, 3, 0,
			1, 2, 5, //right
			2, 6, 5,
			0, 1, 5, // top
			0, 5, 4,
			2, 3, 6, // bottom
			3, 7, 6,
		};
 
		float[] cube = {
			-0.5f,  0.5f,  0.5f, // vertex[0]
			 0.5f,  0.5f,  0.5f, // vertex[1]
			 0.5f, -0.5f,  0.5f, // vertex[2]
			-0.5f, -0.5f,  0.5f, // vertex[3]
			-0.5f,  0.5f, -0.5f, // vertex[4]
			 0.5f,  0.5f, -0.5f, // vertex[5]
			 0.5f, -0.5f, -0.5f, // vertex[6]
			-0.5f, -0.5f, -0.5f, // vertex[7]
		};
		#endregion

        public WorldView()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                this.Controls.Add(this.glControl1);
                loaded = true;
            }
            this.MouseWheel += WorldView_MouseWheel;
            cubePositions.Add(new Vector3(0, 0, 0));
            cubeOrientations.Add(new Vector3(0, 0, 0));

            timer1.Start();
        }

        void WorldView_MouseWheel(object sender, MouseEventArgs e)
        {
            cameraZoom += cameraZoom * e.Delta / 500f;
        }

        public void UpdatePoints(Vector3[] triangleVertices, List<Vector3> cubePositions, List<Vector3> cubeOrientations)
        {
            if (triangleVertices.Length != 3)
            {
                throw new ArgumentException("triangleVertices must have 3 elements.");
            }
            if (cubePositions.Count != cubeOrientations.Count)
            {
                throw new ArgumentException("cubeOrientations must have the same number of elements as cubePositions.");
            }
            //this.sensorTriangleVerts = triangleVertices;
            this.cubePositions = cubePositions;
            this.cubeOrientations = cubeOrientations;
        }

        public void Draw()
        {
            if (!loaded) { return; }

            glControl1.MakeCurrent();
            GL.ClearColor(Color.CornflowerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projectionMatrix);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var rotX = Matrix4.CreateRotationX(cameraRotation.X * (float)Math.PI / 180);
            var rotY = Matrix4.CreateRotationY(cameraRotation.Y * (float)Math.PI / 180);
            var rotZ = Matrix4.CreateRotationZ(cameraRotation.Z * (float)Math.PI / 180);
            var scaleMatrix = Matrix4.CreateScale(cameraZoom);

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1f, 100f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projectionMatrix);

            Vector3 eye = new Vector3(0, 0, -5);
            Vector3 target = new Vector3(0, 0, 0);
            Matrix4 lookAt = Matrix4.LookAt(eye, target, Vector3.UnitY);

            // Draw each cube
            GL.VertexPointer(3, VertexPointerType.Float, 0, cube);
            GL.ColorPointer(4, ColorPointerType.Float, 0, cubeColors);
            foreach (Vector3 cubePosition in cubePositions)
            {
                Matrix4 translation = Matrix4.CreateTranslation(cubePosition);
                modelviewMatrix = translation * rotY * rotZ * rotX * scaleMatrix * lookAt;
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadMatrix(ref modelviewMatrix);
                GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedByte, triangles);
            }

            // Draw sensor triangle
            modelviewMatrix = rotY * rotZ * rotX * scaleMatrix * lookAt;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelviewMatrix);

            GL.DisableClientState(ArrayCap.ColorArray);
            GL.Disable(EnableCap.CullFace);
            GL.VertexPointer(3, VertexPointerType.Float, 0, sensorTriangleVerts);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            glControl1.SwapBuffers();
        }

        private void WorldView_Load(object sender, EventArgs e)
        {
        }

        private void WorldView_Paint(object sender, PaintEventArgs e)
        {
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            lastMouseLocation = e.Location;
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                // Mouse moving over X rotates the shape about the Y axis.
                // Mouse moving over Y rotates the shape about the X axis.
                // It makes sense.

                Vector2 currentPosition = new Vector2(e.X, e.Y);
                Vector2 lastPosition = new Vector2(lastMouseLocation.X, lastMouseLocation.Y);
                Vector2 deltaPosition = currentPosition - lastPosition;

                // limit the rotation to 90 degrees so the user doesn't end up upside-down.
                float newRotationX = cameraRotation.X - deltaPosition.Y;
                if (newRotationX < -90f)
                {
                    cameraRotation.X = -90f;
                }
                else if (newRotationX > 90f)
                {
                    cameraRotation.X = 90f;
                }
                else
                {
                    cameraRotation.X = newRotationX;
                }

                cameraRotation.Y = (cameraRotation.Y < 360f) ? cameraRotation.Y + deltaPosition.X : 0;
                cameraRotation.Z = 0;
                Draw();
            }
            lastMouseLocation = e.Location;
        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Draw();
        }
    }
}
