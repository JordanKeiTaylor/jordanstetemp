using Improbable;
using Improbable.Collections;
using Improbable.Worker;
using World;

namespace Snapshots {
    public struct RouteNode {
        public EntityId entityId;
        public Coordinates coordinates;
    }

    public static class EntityTemplates {

        static Entity CreateBaseEntity(Coordinates coord, string name, WorkerRequirementSet readAcl, Map<uint, WorkerRequirementSet> writeAcl) {
            var entity = new Entity();
            entity.Add(new Position.Data(coord));
            entity.Add(new Persistence.Data());
            entity.Add(new Metadata.Data(name));

            var aclData = new EntityAclData {
                readAcl = readAcl,
                componentWriteAcl = writeAcl
            };
            entity.Add(new EntityAcl.Data(aclData));

            return entity;
        }

        public static Entity CreateCentralEntity(double lat, double lon) {
            var writeAcl = new Map<uint, WorkerRequirementSet>
            {
                { Centre.ComponentId, Acls.ExampleRequirementSet }
            };

            var entity = CreateBaseEntity(new Coordinates(0, 0, 0), "Centre", Acls.ExampleRequirementSet, writeAcl);

            var centreData = new Centre.Data(new CentreData(lat, lon));

            entity.Add(centreData);

            return entity;
        }

        public static Entity CreateWalkerEntity(double x, double y, double z) {
            var writeAcl = new Map<uint, WorkerRequirementSet>
            {
                { Position.ComponentId, Acls.ExampleRequirementSet },
                { Path.ComponentId, Acls.ExampleRequirementSet }
            };

            var entity = CreateBaseEntity(new Coordinates(x, y, z), "Walker", Acls.ExampleRequirementSet, writeAcl);
            entity.Add(new Path.Data(new Improbable.Collections.List<Vector3d>(), 0, 0, 0, 0));
            return entity;
        }
    }
}
