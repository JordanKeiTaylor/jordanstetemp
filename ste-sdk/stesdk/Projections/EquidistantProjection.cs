namespace Improbable.Projections
{
    public class EquidistantProjection
    {
        private IMapProjection _projection;

        private WorldPoint _center;
        private SimulationPoint _offset;

        public EquidistantProjection(double lat, double lon)
        {
            this._center.Lat = Globe.DegreesToRadians(lat);
            this._center.Lon = Globe.DegreesToRadians(lon);
            this._offset.X = 0;
            this._offset.Y = 0;

            // idea here is to be able switch projection types
            this._projection = new AzimuthalEquidistant(lat, lon);
        }

        public SimulationPoint Convert(WorldPoint input)
        {
            var p = this._projection.ToPlane(input.Lat, input.Lon);
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
            var p = this._projection.ToSphere(x, y);

            return new WorldPoint
            {
                Lat = p.X,
                Lon = p.Y,
            };
        }

        internal void ReSetCenter(WorldPoint world, SimulationPoint sim)
        {
            this._center.Lat = Globe.DegreesToRadians(world.Lat);
            this._center.Lon = Globe.DegreesToRadians(world.Lon);
            this._offset.X = sim.X;
            this._offset.Y = sim.Y;
            this._projection = new AzimuthalEquidistant(this._center.Lat, this._center.Lon);
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