namespace Improbable.Projections
{
    public class EquidistantProjection
    {
        private IMapProjection _projection;

        private WorldPoint _center;
        private SimulationPoint _offset;

        public EquidistantProjection(double lat, double lon)
        {
            _center.Lat = Globe.DegreesToRadians(lat);
            _center.Lon = Globe.DegreesToRadians(lon);
            _offset.X = 0;
            _offset.Y = 0;
            _projection = new AzimuthalEquidistant(lat, lon);
        }

        public SimulationPoint Convert(WorldPoint input)
        {
            var p = _projection.ToPlane(input.Lat, input.Lon);
            return new SimulationPoint
            {
                X = _offset.X + p.X,
                Y = _offset.Y + p.Y,
            };
        }

        public WorldPoint Convert(SimulationPoint input)
        {
            var x = input.X - _offset.X;
            var y = input.Y - _offset.Y;
            var p = _projection.ToSphere(x, y);

            return new WorldPoint
            {
                Lat = p.X,
                Lon = p.Y,
            };
        }

        internal void ReSetCenter(WorldPoint world, SimulationPoint sim)
        {
            _center.Lat = Globe.DegreesToRadians(world.Lat);
            _center.Lon = Globe.DegreesToRadians(world.Lon);
            _offset.X = sim.X;
            _offset.Y = sim.Y;
            _projection = new AzimuthalEquidistant(_center.Lat, _center.Lon);
        }

        public struct SimulationPoint
        {
            public double X;
            public double Y;
        }

        public struct WorldPoint
        {
            public double Lon;
            public double Lat;
        }
    }
}