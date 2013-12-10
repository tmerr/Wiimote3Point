using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wiimote3Point
{
    /// <summary>
    /// Represents a Wiimote in 3D space using a triangular sensor bar.
    /// Using the wiimote's IR camera picking up 3 points, plus the real world
    /// characteristics of the sensor triangle, the wiimote's position and
    /// orientation can calculated. That information is then sent in eventargs
    /// along with the full wiimote state.
    /// </summary>
    public class Wiimote3P
    {
        /// <summary>
        /// Event raised when Wiimote state is changed.
        /// </summary>
        public event EventHandler<Wiimote3PChangedEventArgs> Wiimote3PChanged;

        /// <summary>
        /// Full access to the underlying wiimote.
        /// </summary>
        public WiimoteLib.Wiimote wiimote;

        private SensorTriangle sensorTriangle;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sensorTriangle">The sensor triangle.</param>
        public Wiimote3P(SensorTriangle sensorTriangle)
        {
            this.sensorTriangle = sensorTriangle;
        }

        /// <summary>
        /// Connect to the wiimote.
        /// </summary>
        public void Connect()
        {
            wiimote.WiimoteChanged += wm_WiimoteChanged;
            wiimote.Connect();
            wiimote.SetReportType(WiimoteLib.InputReport.IRAccel, true);
            wiimote.SetLEDs(false, true, true, false);
        }

        private void wm_WiimoteChanged(object sender, WiimoteLib.WiimoteChangedEventArgs args)
        {
            PositionOrientation po = new PositionOrientation(0,0,0,0,0,0);
            Wiimote3PChanged(this, new Wiimote3PChangedEventArgs(args.WiimoteState, po));
        }

    }



}