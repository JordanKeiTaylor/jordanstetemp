using Improbable;
using Improbable.Context;
using Improbable.Log;
using Improbable.Worker;
using World;

namespace Example {
    public class Environment {
        public readonly IConnection Connection;

        public readonly IDispatcher Dispatcher;

        public readonly ComponentMap<Path> Walkers;
        public readonly ComponentMap<Position> Positions;

        const string LoggerName = "Environment.cs";

        private static readonly NamedLogger Logger = Improbable.Log.Logger.DefaultWithName(LoggerName);

        public Environment(IConnection conn, IDispatcher dispatch) {
            Connection = conn;
            Dispatcher = dispatch;
            Walkers = new ComponentMap<Path>(dispatch);
            Positions = new ComponentMap<Position>(dispatch);
        }

        public static void Exit(Connection conn, int status) {
            Logger.Warn("Exiting: " + status);
        }

        public void UpdatePath(EntityId id, Path.Update update) {
            if (Walkers.HasAuthority(id) && Walkers.TryGetValue(id, out var path)) {
                Connection.SendComponentUpdate(id, update);
                update.ApplyTo(path);
            }
        }

        public void UpdatePosition(EntityId id, Coordinates coord) {
            if (Positions.HasAuthority(id)) {
                if (Positions.TryGetValue(id, out var position)) {
                    var update = new Position.Update() {
                        coords = coord
                    };
                    Connection.SendComponentUpdate(id, update);
                    update.ApplyTo(position);
                } else {
                    Logger.Error("Unable to get position for [" + id.Id + "]");
                }
            } else {
                Logger.Error("Worker does not have authority over position of [" + id.Id + "]");
            }
        }
    }
}
