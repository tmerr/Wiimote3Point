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

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SensorTriangle s = new SensorTriangle(1.3, 1);
            wiimote = new Wiimote3P(s);
            wiimote.Wiimote3PChanged += wiimote_Wiimote3PChanged;
            UpdateZ();
            UpdateSensorTriangle();
        }

        private void wiimote_Wiimote3PChanged(object sender, Wiimote3PChangedEventArgs args)
        {
            try
            {
                lblPosition.BeginInvoke(new Action(() =>
                {
                    string newlbl = "";
                    foreach (Pose po in args.positionOrientations)
                    {
                        newlbl += String.Format("X: {0}\nY: {1}\nZ: {2}\n\n", po.X, po.Y, po.Z);
                    }
                    lblPosition.Text = newlbl;
                }));
            }
            catch (System.InvalidOperationException e)
            {
                Console.WriteLine(e.ToString());
            }
            var sensors = args.wiimoteState.IRState.IRSensors;
            if (sensors[0].Found && sensors[1].Found && sensors[2].Found)
            {
                var verts = new Vector3[3];
                verts[0] = new Vector3(sensors[0].Position.X, sensors[0].Position.Y, 0);
                verts[1] = new Vector3(sensors[1].Position.X, sensors[1].Position.Y, 0);
                verts[2] = new Vector3(sensors[2].Position.X, sensors[2].Position.Y, 0);
                irSensorsView21.UpdatePoints(verts);

                var pos = args.positionOrientations;
                var cubePositions = new List<Vector3>();
                var orientations = new List<Matrix3>();
                foreach (Pose po in args.positionOrientations)
                {
                    cubePositions.Add(new Vector3((float)po.X, (float)po.Y, (float)po.Z));
                    var o = po.Orientation;
                    orientations.Add(new Matrix3((float)o[0,0], (float)o[0,1], (float)o[0,2],
                                                      (float)o[1, 0], (float)o[1, 1], (float)o[1, 2],
                                                      (float)o[2, 0], (float)o[2, 1], (float)o[2, 2]));
                }
                var v = wiimote.SensorTriangle.Vertices;
                Vector3[] sensorTriangle =
                {
                    new Vector3(v[0,0], v[0,1], v[0,2]),
                    new Vector3(v[1,0], v[1,1], v[1,2]),
                    new Vector3(v[2,0], v[2,1], v[2,2])
                };
                worldView1.UpdatePoints(sensorTriangle, cubePositions, orientations);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                wiimote.Connect();
            }
            catch (WiimoteLib.WiimoteNotFoundException ex)
            {
                MessageBox.Show(this, "Wiimote not found.");
            }
        }

        private void danTimer_Tick(object sender, EventArgs e)
        {
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            UpdateZ();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            UpdateSensorTriangle();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            UpdateSensorTriangle();
        }

        private void UpdateZ()
        {
            wiimote.PIXELS_Z = Convert.ToInt32(numericUpDown1.Value);
        }

        private void UpdateSensorTriangle()
        {
            wiimote.SensorTriangle = new SensorTriangle(
                    Convert.ToDouble(numericUpDown2.Value),
                    Convert.ToDouble(numericUpDown3.Value));
        }
    }
}
