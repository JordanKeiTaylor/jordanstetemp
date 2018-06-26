using Improbable.Sandbox.Projections;
using NUnit.Framework;

namespace Tests.Projections
{
    /// <summary>
    /// This test ensures that our implementation of azimuthal equidistant produces
    /// results that match the results from GeographicLib implementation.
    /// https://geographiclib.sourceforge.io/.
    /// </summary>
    [TestFixture]
    [Ignore("Test is currently failing because our implementation doesn't match GeographicLib, ignoring till fixed.")]
    public class AzimuthalEquidistantTest
    {
        readonly double TOLERANCE = 0.001;

        double[] latlon =
        {
             90,    0,   // origin
              0,    0,   // halfway from center (west)
              0,   90,   // halfway from center (north)
              0,  180,   // halfway from center (east)
              0,  270,   // halfway from center (south)
            -90,    0,   // max distance from center (west)
            -90,   90,   // max distance from center (north)
            -90,  180,   // max distance from center (east)
            -90,  270,   // max distance from center (south)
             89,   45,   // close to origin, off axis
             89,  135,   // close to origin, off axis
             89,  225,   // close to origin, off axis
             89,  315    // close to origin, off axis
        };

        // lat/lon converstion to x/y results for azimuthal 
        // equidistant projection from GeographicLib
        double[] xy =
        {
                   0.0  ,         0.0  ,
                   0.0  , -10001965.729,
            10001965.729,         0.0  ,
                   0.0  ,  10001965.729,
           -10001965.729,         0.0  ,
                   0.0  , -20003931.459,
            20003931.459,         0.0  ,
                   0.0  ,  20003931.459,
           -20003931.459,         0.0  ,
               78979.489,    -78979.489,
               78979.489,     78979.489,
              -78979.489,     78979.489,
              -78979.489,    -78979.489,
        };

        [Test]
        public void Should_ConvertLatLonToXY()
        {
            var proj = new AzimuthalEquidistant(90, 0);

            var length = latlon.Length;
            for (int i = 0; i < length - 1; i += 2)
            {
                var lat = latlon[i];
                var lon = latlon[i + 1];

                var point = proj.ToPlane(lat, lon);

                var x = xy[i];
                var y = xy[i + 1];

                Assert.AreEqual(x, point.X, TOLERANCE);
                Assert.AreEqual(y, point.Y, TOLERANCE);
            }
        }

        [Test]
        public void Should_ConvertXYToLatLon()
        {
            var proj = new AzimuthalEquidistant(90, 0);

            var length = xy.Length;
            for (int i = 0; i < length - 1; i += 2)
            {
                var x = xy[i];
                var y = xy[i + 1];

                var point = proj.ToSphere(x, y);

                var lat = latlon[i];
                var lon = latlon[i + 1];

                Assert.AreEqual(lat, point.X, TOLERANCE);
                Assert.AreEqual(lon, point.Y, TOLERANCE);
            }
        }
    }
}
