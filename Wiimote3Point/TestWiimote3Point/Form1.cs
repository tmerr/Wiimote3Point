using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wiimote3Point;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace TestWiimote3Point
{
    public partial class Form1 : Form
    {
        private Wiimote3P wiimote;
        bool loaded = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SensorTriangle s = new SensorTriangle(1.8, 1);
            wiimote = new Wiimote3P(s);
            wiimote.Wiimote3PChanged += wiimote_Wiimote3PChanged;
        }

        private void glControl_Load(object sender, EventArgs e)
        {
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded) // Play nice
                return;

        }

        private void wiimote_Wiimote3PChanged(object sender, Wiimote3PChangedEventArgs args)
        {
            lblPosition.BeginInvoke(new Action(() =>
            {
                string newlbl = "";
                foreach (PositionOrientation po in args.positionOrientations)
                {
                    newlbl += String.Format("X: {0}\nY: {1}\nZ: {2}\n\n", po.X, po.Y, po.Z);
                }
                lblPosition.Text = newlbl;
            }));
            var sensors = args.wiimoteState.IRState.IRSensors;
            if (sensors[0].Found && sensors[1].Found && sensors[2].Found)
            {
                var verts = new Vector3[3];
                verts[0] = new Vector3(sensors[0].Position.X, sensors[0].Position.Y, 0);
                verts[1] = new Vector3(sensors[1].Position.X, sensors[1].Position.Y, 0);
                verts[2] = new Vector3(sensors[2].Position.X, sensors[2].Position.Y, 0);
                irSensorsView21.UpdatePoints(verts);


                /*
                var pos = args.positionOrientations;
                vertices[0] = new Vector3((float)pos[0].X, (float)pos[0].Y, (float)pos[0].Z);
                vertices[1] = new Vector3((float)pos[1].X, (float)pos[1].Y, (float)pos[1].Z);
                vertices[2] = new Vector3((float)pos[2].X, (float)pos[2].Y, (float)pos[2].Z);
                 */
            }
            glControl.Invalidate();
        }

        private void shet()
        {
            Console.Out.WriteLine(GL.GetError().ToString());
        }

        
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                wiimote.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Wiimote not found.");
            }
        }

        private void danTimer_Tick(object sender, EventArgs e)
        {
            /*
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                                   new IntPtr(vertices.Length * Vector3.SizeInBytes),
                                   vertices, BufferUsageHint.StaticDraw);
            glControl.Invalidate();
             */
        }
    }
}
