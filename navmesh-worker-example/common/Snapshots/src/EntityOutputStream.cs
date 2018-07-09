using System;
using Improbable.Worker;
using Improbable;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Snapshots {
    public class EntityOutputStream : IDisposable {
        private SnapshotOutputStream snapshotOutputStream;
        private Dictionary<string, uint> entityTypeCounts;

        private const string ROW_FORMAT = " {0,-30} {1,-10}";

        public EntityOutputStream(String fname) {
            snapshotOutputStream = new SnapshotOutputStream(fname);

            entityTypeCounts = new Dictionary<string, uint>();
        }

        public void Dispose() {
            snapshotOutputStream.Dispose();
        }

        public void PrintSummary() {
            Console.WriteLine(ROW_FORMAT, "Entity type", "Count");
            foreach (KeyValuePair<string, uint> entityPair in entityTypeCounts) {
                Console.WriteLine(ROW_FORMAT, entityPair.Key, entityPair.Value);
            }

            Console.WriteLine(ROW_FORMAT, "Total", entityTypeCounts.Values.Sum(val => val));
        }

        public void WriteEntity(EntityId entityId, Entity entity) {
            var result = snapshotOutputStream.WriteEntity(entityId, entity);

            if (result.HasValue) {
                throw new InvalidOperationException($"Exception while writing entity with id {entityId}: {result.Value}");
            }

            var entityType = entity.Get<Metadata>().Value.Get().Value.entityType;
            entityTypeCounts[entityType] = (entityTypeCounts.ContainsKey(entityType) ? entityTypeCounts[entityType] : 0) + 1;
        }
    }
}
