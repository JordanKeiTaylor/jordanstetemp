using System;
using Shared.Projections;

namespace Shared
{
    public class Globe
    {
        public const double EarthRadiusM = 6371e3;
        public const double MilesPerHourToMetersPerSecondConversionFactor = 0.44704;

        public static double DegreesToRadians(double n)
        {
            return n / 360.0f * 2 * Math.PI;
        }

        public static double RadiansToDegrees(double n)
        {
            return n / (2 * Math.PI) * 360.0f;
        }
    }

    public class EquidistantProjection
    {
        private IMapProjection projection;

        public WorldPoint center;
        public SimulationPoint offset;

        public EquidistantProjection(double lat, double lon)
        {
            this.center.lat = Globe.DegreesToRadians(lat);
            this.center.lon = Globe.DegreesToRadians(lon);
            this.offset.x = 0;
            this.offset.y = 0;

            // idea here is to be able switch projection types
            this.projection = new AzimuthalEquidistant(lat, lon);
        }


        public SimulationPoint Convert(WorldPoint input)
        {
            SimulationPoint output = new SimulationPoint();
            var p = this.projection.ToPlane(input.lat, input.lon);
            output.x = offset.x + p.X;
            output.y = offset.y + p.Y;

            return output;
        }

        public WorldPoint Convert(SimulationPoint input)
        {
            WorldPoint output = new WorldPoint();

            var x = input.x - offset.x;
            var y = input.y - offset.y;
            var p = this.projection.ToSphere(x, y);
            output.lat = p.X;
            output.lon = p.Y;

            return output;
        }

        public struct SimulationPoint
        {
            public double x;
            public double y;
        }

        public struct WorldPoint
        {
            public double lon;
            public double lat;
        }

        internal void ReSetCenter(WorldPoint world, SimulationPoint sim)
        {
            this.center.lat = Globe.DegreesToRadians(world.lat);
            this.center.lon = Globe.DegreesToRadians(world.lon);
            this.offset.x = sim.x;
            this.offset.y = sim.y;
            this.projection = new AzimuthalEquidistant(this.center.lat, this.center.lon);
        }
    }
}