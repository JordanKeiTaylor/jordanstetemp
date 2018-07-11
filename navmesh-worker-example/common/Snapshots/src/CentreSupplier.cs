namespace Snapshots {
    public class CentreSupplier {

        const double CENTRE_LAT = 51.5204732;
        const double CENTRE_LON = -0.1079289;

        EntityIdGenerator entityIdGenerator;

        public CentreSupplier(EntityIdGenerator entityIdGenerator) {
            this.entityIdGenerator = entityIdGenerator;
        }

        public void Generate(EntityOutputStream snapshotOutputStream) {
            snapshotOutputStream.WriteEntity(
                entityIdGenerator.getNextId(),
                EntityTemplates.CreateCentralEntity(CENTRE_LAT, CENTRE_LON)
            );
        }
    }
}
