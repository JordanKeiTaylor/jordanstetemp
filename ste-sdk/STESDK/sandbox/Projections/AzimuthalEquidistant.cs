using System;
using System.Windows;

namespace Improbable.Sandbox.Projections
{
    /// <summary>
    /// Azimuthal equidistant projection.
    /// http://mathworld.wolfram.com/AzimuthalEquidistantProjection.html
    /// </summary>
    public class AzimuthalEquidistant : IMapProjection
    {
        private readonly double _earthRadius = 6371e3;

        private readonly double _latROrig;
        private readonly double _lonROrig;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Improbable.Sandbox.Projections.AzimuthalEquidistant"/>
        /// class centered at the specified lat/lon position.
        /// </summary>
        /// <param name="lat">Latitude origin.</param>
        /// <param name="lon">Longitude origin.</param>
        public AzimuthalEquidistant(double lat, double lon)
        {
            _latROrig = DegreesToRadians(lat);
            _lonROrig = DegreesToRadians(lon);
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
            return ConvertToPlane(lat, lon);
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
            return ConvertToSphere(x, y);
        }

        private static double DegreesToRadians(double n)
        {
            return n / 360.0f * 2 * Math.PI;
        }

        private static double RadiansToDegrees(double n)
        {
            return n / (2 * Math.PI) * 360.0f;
        }

        private Point ConvertToPlane(double lat, double lon)
        {
            var latR = DegreesToRadians(lat);
            var lonR = DegreesToRadians(lon);

            var c = Math.Acos(Math.Sin(_latROrig) * Math.Sin(latR) + Math.Cos(_latROrig) * Math.Cos(latR) * Math.Cos(lonR - _lonROrig));
            var k = c / Math.Sin(c);

            var x = k * Math.Cos(latR) * Math.Sin(lonR - _lonROrig);
            var y = k * (Math.Cos(_latROrig) * Math.Sin(latR) - Math.Sin(_latROrig) * Math.Cos(latR) * Math.Cos(lonR - _lonROrig));

            return new Point(x * _earthRadius, y * _earthRadius);
        }

        private Point ConvertToSphere(double x, double y)
        {
            var c = Math.Sqrt((x * x) + (y * y));

            var latR = Math.Asin((Math.Cos(c) * Math.Sin(_latROrig)) + (y * Math.Sin(c) * Math.Cos(_latROrig)) / c);
            var lonR = _lonROrig + Math.Atan((x * Math.Sin(c)) / (c * Math.Cos(_latROrig) * Math.Cos(c) - y * Math.Sin(_latROrig) * Math.Sin(c)));

            return new Point(RadiansToDegrees(latR), RadiansToDegrees(lonR));
        }
    }
}
