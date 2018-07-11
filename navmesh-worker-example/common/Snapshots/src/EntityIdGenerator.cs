using Improbable;

namespace Snapshots {
    public class EntityIdGenerator {
        private long nextId = 100;

        public EntityId getNextId() {
            return new EntityId(nextId++);
        }
    }
}
