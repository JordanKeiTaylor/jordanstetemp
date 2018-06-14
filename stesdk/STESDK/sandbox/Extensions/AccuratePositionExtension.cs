using System;
using stesdk.sandbox.Log;

namespace stesdk.sandbox.Extensions
{
    public static class AccuratePositionExtension
    {
        private const double DefaultCoarseGridSize = 100.0;
        private const double DefaultFineGridSize = 0.01;

        public static AccuratePositionConstants ReadFromFlags(IConnection connection, IDispatcher dispatcher)
        {
            var coarse = new DynamicFlag<double>(connection, dispatcher, "coarse_grid_size_m", Convert.ToDouble, DefaultCoarseGridSize);
            var fine = new DynamicFlag<double>(connection, dispatcher, "fine_grid_size_m", Convert.ToDouble, DefaultFineGridSize);
            var namedLogger = Logger.DefaultWithName("AccuratePosition");

            if (coarse.Value >= fine.Value * short.MaxValue)
            {
                namedLogger.Error("Worker flags specifying incompatible fine and coarse grid sizes. Expect the unexpected.");
            }

            return new AccuratePositionConstants(coarse, fine);
        }

        public static AccuratePositionConstants ReadFromFlags(IConnectionManager connectionManager, IDispatcher dispatcher)
        {
            var coarse = new DynamicFlag<double>(dispatcher, "coarse_grid_size_m", Convert.ToDouble, DefaultCoarseGridSize);
            var fine = new DynamicFlag<double>(dispatcher, "fine_grid_size_m", Convert.ToDouble, DefaultFineGridSize);
            var namedLogger = Logger.DefaultWithName("AccuratePosition");

            connectionManager.AddConnectionReceiver(coarse);
            connectionManager.AddConnectionReceiver(fine);

            if (coarse.Value >= fine.Value * short.MaxValue)
            {
                namedLogger.Error("Worker flags specifying incompatible fine and coarse grid sizes. Expect the unexpected.");
            }

            return new AccuratePositionConstants(coarse, fine);
        }

        /// <summary>
        /// Convert to a Coordinates
        /// </summary>
        /// <returns>Coordinates that this AccuratePosition represents.</returns>
        /// <param name="p">P.</param>
        public static Coordinates ToCoordinate(this AccuratePosition.Data p, AccuratePositionConstants constants)
        {
            var value = p.Get().Value;

            var x = value.centerX * constants.CoarseGridSize.Value + value.offsetX * constants.FineGridSize.Value;
            var z = value.centerZ * constants.CoarseGridSize.Value + value.offsetZ * constants.FineGridSize.Value;

            return new Coordinates(x, 0.0, z);
        }

        /// <summary>
        /// Updates the position on an AccuratePosition to the supplied Coordinates
        /// </summary>
        /// <param name="original">The original AccuratePosition.Data</param>
        /// <param name="update">The update, this will be modified to update the position, minimising the change</param>
        /// <param name="newCoordinates">The coordinate to write/store</param>
        public static void UpdatePosition(this AccuratePosition.Data original, AccuratePosition.Update update, Coordinates newCoordinates, AccuratePositionConstants constants)
        {
            var originalData = original.Get().Value;

            int positionX = (int)Math.Round(newCoordinates.x / constants.CoarseGridSize.Value);
            int positionZ = (int)Math.Round(newCoordinates.z / constants.CoarseGridSize.Value);

            int deltaX = (int)Math.Round((newCoordinates.x - positionX * constants.CoarseGridSize.Value) / constants.FineGridSize.Value);
            int deltaZ = (int)Math.Round((newCoordinates.z - positionZ * constants.CoarseGridSize.Value) / constants.FineGridSize.Value);

            if (positionX != originalData.centerX)
            {
                update.SetCenterX(positionX);
            }

            if (positionZ != originalData.centerZ)
            {
                update.SetCenterZ(positionZ);
            }

            update.SetOffsetX(deltaX).SetOffsetZ(deltaZ);
        }
    }
}
