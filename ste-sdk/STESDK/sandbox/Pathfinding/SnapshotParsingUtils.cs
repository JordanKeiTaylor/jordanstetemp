using System.Collections.Generic;
using System.IO;
using Improbable.Sandbox.Extensions;
using Improbable.Sandbox.Pathfinding.Api;

namespace Improbable.Sandbox.Pathfinding
{
    public static class SnapshotParsingUtils
    {
        public static void SetGraphNodes(string pointsCsv, Dictionary<EntityId, PathNode> nodes)
        {
            using (var stream = new FileStream(pointsCsv, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                var line = reader.ReadLine(); // Skip the header
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.Contains("RoutingNode") || line.Trim().Length == 0)
                    {
                        continue;
                    }

                    var row = line.Trim().Split(',');
                    nodes.Add(
                        new EntityId(long.Parse(row[0])),
                        new PathNode
                        {
                            EntityId = new EntityId(long.Parse(row[0])),
                            Coords = new Coordinates(double.Parse(row[2]), double.Parse(row[3]), double.Parse(row[4])),
                        });
                }
            }
        }

        public static void SetGraphEdges(string graphCsv, Dictionary<EntityId, PathNode> nodes, List<PathEdge> edges)
        {
            using (var stream = new FileStream(graphCsv, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var row = line.Split(',');

                    if (row.Length < 1)
                    {
                        continue;
                    }

                    var connectionIds = row[1].Split(' ');
                    foreach (var id in connectionIds)
                    {
                        if (id.Trim().Equals(string.Empty))
                        {
                            continue;
                        }

                        var source = nodes[new EntityId(long.Parse(row[0]))];
                        var target = nodes[new EntityId(long.Parse(id))];
                        var weight = source.Coords.DistanceTo(target.Coords);
                        edges.Add(new PathEdge { Source = source, Target = target, Weight = weight, });
                    }
                }
            }
        }
    }
}