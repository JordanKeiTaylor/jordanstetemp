using Improbable;
using Improbable.Collections;

namespace Snapshots {
    public static class Acls {
        public static readonly WorkerAttributeSet ExampleAttributeSet = new WorkerAttributeSet(new List<string> { "Example" });
        public static readonly WorkerRequirementSet ExampleRequirementSet = new WorkerRequirementSet(new List<WorkerAttributeSet> { ExampleAttributeSet });
    }
}
