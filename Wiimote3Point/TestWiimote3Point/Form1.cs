using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wiimote3Point;

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
            SensorTriangle s = new SensorTriangle(20, 30);
            wiimote = new Wiimote3P(s);
            wiimote.Wiimote3PChanged += wiimote_Wiimote3PChanged;
            wiimote.Connect();
        }

        private void wiimote_Wiimote3PChanged(object sender, Wiimote3PChangedEventArgs args)
        {
            lblPosition.Invoke(new Action(() =>
            {
                lblPosition.Text = String.Format("{0}\n{1}\n{2}", args.positionOrientation.X, args.positionOrientation.Y, args.positionOrientation.Z);
            }));
        }
    }
}
