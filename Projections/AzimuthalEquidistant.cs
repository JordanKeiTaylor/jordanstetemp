using System;
using System.Windows;

namespace Shared.Projections
{
    /// <summary>
    /// Azimuthal equidistant projection.
    /// http://mathworld.wolfram.com/AzimuthalEquidistantProjection.html
    /// </summary>
    public class AzimuthalEquidistant : IMapProjection
    {
        private readonly double EarthRadius = 6371e3;

        private double latROrig;
        private double lonROrig;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Shared.Projections.AzimuthalEquidistant"/> 
        /// class centered at the specified lat/lon position.
        /// </summary>
        /// <param name="lat">Latitude origin.</param>
        /// <param name="lon">Longitude origin.</param>
        public AzimuthalEquidistant(double lat, double lon)
        {
            latROrig = degreesToRadians(lat);
            lonROrig = degreesToRadians(lon);
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
            return convertToPlane(lat, lon);
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
            return convertToSphere(x, y);
        }

        private Point convertToPlane(double lat, double lon)
        {
            var latR = degreesToRadians(lat);
            var lonR = degreesToRadians(lon);

            var c = Math.Acos(Math.Sin(latROrig) * Math.Sin(latR) + Math.Cos(latROrig) * Math.Cos(latR) * Math.Cos(lonR - lonROrig));
            var k = c / Math.Sin(c);

            var x = k * Math.Cos(latR) * Math.Sin(lonR - lonROrig);
            var y = k * (Math.Cos(latROrig) * Math.Sin(latR) - Math.Sin(latROrig) * Math.Cos(latR) * Math.Cos(lonR - lonROrig));

            return new Point(x * EarthRadius, y * EarthRadius);
        }

        private Point convertToSphere(double x, double y)
        {
            var c = Math.Sqrt(x * x + y * y);

            var latR = Math.Asin(Math.Cos(c) * Math.Sin(latROrig) + (y * Math.Sin(c) * Math.Cos(latROrig)) / c);
            var lonR = lonROrig + Math.Atan((x * Math.Sin(c)) / (c * Math.Cos(latROrig) * Math.Cos(c) - y * Math.Sin(latROrig) * Math.Sin(c)));

            return new Point(radiansToDegrees(latR), radiansToDegrees(lonR));
        }

        private double degreesToRadians(double n)
        {
            return n / 360.0f * 2 * Math.PI;
        }

        private double radiansToDegrees(double n)
        {
            return n / (2 * Math.PI) * 360.0f;
        }
    }
}
