using NUnit.Framework;

namespace Improbable.GeographicLib.Test
{
    [TestFixture]
    public class LocalCartesianTest
    {
        readonly double TOLERANCE = 0.001;
        
        double lat0 = 51.509865, lon0 = -0.118092; // London (origin)

        double[] latlon =
        {
            52.205067,  0.107760,   // Cambridge (N)
            50.827930, -0.168749,   // Brighton (S)
            51.279797,  1.082800,   // Canterbury (E)
            51.456250, -0.971130,   // Reading (W)
        };

        double[] xy =
        {
             15440.051477363928,  77373.321513011237,  // Cambridge (N)
            -3569.1398748964539, -75863.269927878515,  // Brighton (S)
             83785.667624222406, -24909.045408962455,  // Canterbury (E)
            -59289.906656069172,  -5619.602110058262   // Reading (W)
        };

        [Test]
        public void Should_ConvertLatLonToXY()
        {
            var proj = new LocalCartesian(lat0, lon0);

            var length = latlon.Length;
            for (int i = 0; i < length - 1; i += 2)
            {
                var lat = latlon[i];
                var lon = latlon[i + 1];

                double x, y, z;
                proj.Forward(lat, lon, 0, out x, out y, out z);

                Assert.AreEqual(xy[i], x, TOLERANCE);
                Assert.AreEqual(xy[i + 1], y, TOLERANCE);
            }
        }

        [Test]
        public void Should_ConvertXYToLatLon()
        {
            var proj = new LocalCartesian(lat0, lon0);

            var length = xy.Length;
            for (int i = 0; i < length - 1; i += 2)
            {
                var x = xy[i];
                var y = xy[i + 1];

                double lat, lon, h;
                proj.Reverse(x, y, 0, out lat, out lon, out h);

                Assert.AreEqual(latlon[i], lat, TOLERANCE);
                Assert.AreEqual(latlon[i + 1], lon, TOLERANCE);
            }
        }
    }
}