using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wiimote3Point
{
    /// <summary>
    /// Argument sent through the Wiimote3PChangedEvent
    /// </summary>
    public class Wiimote3PChangedEventArgs : EventArgs
    {        
        /// <summary>
        /// The current state of the Wiimote and extension controllers.
        /// </summary>
        public WiimoteLib.WiimoteState wiimoteState;

        /// <summary>
        /// 3D position and orientation derived from the wiimote state.
        /// </summary>
        public List<Pose> positionOrientations;

        /// <summary>
        /// Constructor
        /// </summary>
        public Wiimote3PChangedEventArgs(WiimoteLib.WiimoteState ws, List<Pose> po)
        {
            wiimoteState = ws;
            positionOrientations = po;
        }

    }
}
