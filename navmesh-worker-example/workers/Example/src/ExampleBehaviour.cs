using Improbable;
using Improbable.Extensions;
using Improbable.Navigation;
using Improbable.Navigation.Api;
using Improbable.Worker;
using World;

namespace Example {
    public class ExampleBehaviour {
        const string LoggerName = "ExampleBehaviour.cs";
        uint workerIndex;

        Environment environment;
        private IMeshNavigator _navigator = new DefaultMeshNavigator("./Tile_+007_+006_L21.obj.tiled.bin64");

        private Coordinates HalfExtents = new Coordinates(10.0, 10.0, 10.0);

        public ExampleBehaviour(Environment env) {
            workerIndex = (uint)(env.Connection.GetWorkerId().GetHashCode() & 0xff);
            environment = env;

            foreach (var path in System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory()))
            {
                env.Connection.SendLogMessage(LogLevel.Info, "Cool Stuff", path);
            }
            
            env.Dispatcher.OnAuthorityChange<Position>(OnAuthorityChange);
            env.Dispatcher.OnRemoveEntity(OnRemoveEntity);
            env.Dispatcher.OnAuthorityChange<Path>(OnAuthorityChange);
        }

        public void Log(LogLevel level, string msg) {
            environment.Connection.SendLogMessage(level, LoggerName, msg);
        }

        public void Tick(double dt)
        {
            //Log(LogLevel.Info, "Tick");
            var walkers = environment.Walkers;
            var positions = environment.Positions;

            //Log(LogLevel.Info, "Tick:foreach (" + walkers.Keys.Count + ")");
            foreach (var id in walkers.Keys) {
                if (!walkers.HasAuthority(id)) {
                    //Log(LogLevel.Info, "Tick:foreach:id [" + id.Id + "] walkers no authority");
                    continue;
                }
                if (!positions.HasAuthority(id)) {
                    //Log(LogLevel.Info, "Tick:foreach:id [" + id.Id + "] positions no authority");
                    continue;
                }
                //Log(LogLevel.Info, "Tick:foreach [" + id.Id + "] HasAuthority");

                if (walkers.TryGetValue(id, out var pathData) && positions.ContainsKey(id)) {
                    MoveWalker(id, pathData.Get().Value);
                }
            }
        }

        void MoveWalker(EntityId id, PathData pathData) {
            //Log(LogLevel.Info, "MoveWalker");
            if ((pathData.index >= pathData.path.Count) || (0 > pathData.index)) {
                //Log(LogLevel.Info, "MoveWalker:SetNewPath");
                SetNewPath(id);
            } else {
                //Log(LogLevel.Info, "MoveWalker:UpdatePosition");
                environment.UpdatePosition(id, ToCoordinates(pathData.path[pathData.index]));
                pathData.index++;
                //Log(LogLevel.Info, "MoveWalker:UpdatePath");
                environment.UpdatePath(id, new Path.Update { index = pathData.index } );
            }
        }

        void SetNewPath(EntityId id) {
            var positions = environment.Positions;
            if (positions.TryGetValue(id, out var position)) {
                var startPos = position.Get().Value.coords;
                var pointA = _navigator.GetNearestPoly(startPos, HalfExtents).Result;
                
                if (pointA == null)
                {
                    Log(LogLevel.Error, "Failed to get point A for [" + id.Id + "] for SetNewPath");
                    Log(LogLevel.Error, "Point A: " + startPos);
                    Log(LogLevel.Error, pointA.ToString());
                    return;
                }

                var pointB = _navigator.GetRandomPoint().Result;
                if (pointB == null) {
                    Log(LogLevel.Error, "Failed to get point B for [" + id.Id + "] for SetNewPath");
                    return;
                }

                var smoothPath = _navigator.GetMeshPath(pointA, pointB).Result;
                if (smoothPath.Status != PathStatus.Success) {
                    Log(LogLevel.Error, "Failed to find smooth path for [" + id.Id + "] for SetNewPath");
                    return;
                }

                var update = new Path.Update {
                    index = 0,
                    path = new Improbable.Collections.List<Vector3d>(),
                    smoothpathlen = smoothPath.Path.Count
                };
                
                for (int i = 0; i < smoothPath.Path.Count; i+=3) {
                    update.path.Value.Add(smoothPath.Path[i].Target.Coords.ToVector3d());
                }
                environment.UpdatePath(id, update);
            } else {
                Log(LogLevel.Error, "Could not find position for [" + id.Id + "] for SetNewPath");
            }
        }

        Coordinates ToCoordinates(Vector3d vector) {
            return new Coordinates(vector.x, vector.y, vector.z);
        }

        void OnRemoveEntity(RemoveEntityOp entity) {
        }

        void OnAuthorityChange(AuthorityChangeOp authChange) {
        }
    }
}
