using System;
using Improbable.Navigation;
using Improbable.Navigation.Api;

namespace Snapshots {
    public class WalkerSupplier {
        const int NUMBER_OF_ENTITIES = 500;

        EntityIdGenerator entityIdGenerator;

        private IMeshNavigator _meshNavigator = new DefaultMeshNavigator("./Tile_+007_+006_L21.obj.tiled.bin64");

        public WalkerSupplier(EntityIdGenerator entityIdGenerator) {
            this.entityIdGenerator = entityIdGenerator;
        }

        public void Generate(EntityOutputStream snapshotOutputStream) {
            for (int i = 0; i < NUMBER_OF_ENTITIES; i++) {
                double[] point = GetRandomPoint();
                if (null != point) {
                    snapshotOutputStream.WriteEntity(entityIdGenerator.getNextId(),
                                                     EntityTemplates.CreateWalkerEntity(point[0], point[1], point[2]));
                }
            }
        }

        double[] GetRandomPoint() {
            var result = _meshNavigator.GetRandomPoint().Result;
            return new[] { result.Coords.x, result.Coords.y, result.Coords.z };
        }
    }
}
