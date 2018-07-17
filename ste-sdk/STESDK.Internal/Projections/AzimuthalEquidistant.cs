using System;
using System.Windows;

namespace Improbable.Projections
{
    /// <summary>
    /// Azimuthal equidistant projection.
    /// http://mathworld.wolfram.com/AzimuthalEquidistantProjection.html
    /// </summary>
    public class AzimuthalEquidistant : IMapProjection
    {
        private readonly double _latROrig;
        private readonly double _lonROrig;
        private readonly GeographicLib.AzimuthalEquidistant _projection;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Improbable.Projections.AzimuthalEquidistant"/>
        /// class centered at the specified lat/lon position.
        /// </summary>
        /// <param name="lat">Latitude origin (degrees).</param>
        /// <param name="lon">Longitude origin (degrees).</param>
        public AzimuthalEquidistant(double lat, double lon)
        {
            _latROrig = lat;
            _lonROrig = lon;
            _projection = new GeographicLib.AzimuthalEquidistant();
        }

        /// <summary>
        /// Converts a spherical lat/lon position to planar x/y position.
        /// </summary>
        /// <returns>X/Y position on a plane.</returns>
        /// <param name="point">Lat/Lon position on a sphere (degrees).</param>
        public Point ToPlane(Point point)
        {
            return ToPlane(point.X, point.Y);
        }

        /// <summary>
        /// Converts a spherical lat/lon position to planar x/y position.
        /// </summary>
        /// <returns>X/Y position on a plane.</returns>
        /// <param name="lat">Latitude coordinate (degrees).</param>
        /// <param name="lon">Longitude coordinate (degrees).</param>
        public Point ToPlane(double lat, double lon)
        {
            double x, y;
            _projection.Forward(_latROrig, _lonROrig, lat, lon, out x, out y);
            return new Point(x, y);
        }

        /// <summary>
        /// Converts a planar x/y position to a spherical lat/lon position.
        /// </summary>
        /// <returns>Lat/Lon position on a sphere (degrees).</returns>
        /// <param name="point">X/Y point on a plane.</param>
        public Point ToSphere(Point point)
        {
            return ToSphere(point.X, point.Y);
        }

        /// <summary>
        /// Converts a planar x/y position to a spherical lat/lon position.
        /// </summary>
        /// <returns>Lat/Lon position on a sphere (degrees).</returns>
        /// <param name="x">Planar x coordinate.</param>
        /// <param name="y">Planar y coordinate.</param>
        public Point ToSphere(double x, double y)
        {
            double lat1, lon1;
            _projection.Reverse(_latROrig, _lonROrig, x, y, out lat1, out lon1);
            return new Point(lat1, lon1);
        }
    }
}
