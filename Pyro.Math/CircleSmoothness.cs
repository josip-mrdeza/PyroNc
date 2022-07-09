using System;

namespace Pyro.Math
{
    public enum CircleSmoothness
    {
        /// <summary>
        /// Represents a circle plotted with 45 points in space, or a point for every 8 degrees up til 360 degrees.
        /// </summary>
        Rough = 45,
        /// <summary>
        /// Represents a circle plotted with 90 points in space, or a point for every 4 degrees up til 360 degrees.
        /// </summary>
        Crude = 90,
        /// <summary>
        /// Represents a circle plotted with 180 points in space, or a point for every 2 degrees up til 360 degrees.
        /// </summary>
        Standard = 180,
        /// <summary>
        /// Represents a circle plotted with 360 points in space, or a point for every degree up til 360 degrees.
        /// </summary>
        Fine = 360,
        /// <summary>
        /// Represents a circle plotted with 720 points in space, or a point for every half a degree up til 360 degrees.
        /// </summary>
        [Obsolete] Perfect = 720
    }
}