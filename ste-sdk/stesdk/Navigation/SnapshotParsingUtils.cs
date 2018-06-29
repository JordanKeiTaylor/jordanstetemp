using System.Collections.Generic;
using System.IO;
using Improbable.Extensions;
using Improbable.Navigation.Api;

namespace Improbable.Navigation
{
    public static class SnapshotParsingUtils
    {
        /// <summary>
        /// Parse a pointsCSV file into a dictionary of nodes.
        /// 
        /// This file is expected to have a header.
        /// 
        /// Expected CSV Structure:
        /// id,entityType,x,y,z
        /// </summary>
        /// <param name="pointsCsv">Filepath to pointsCSV file</param>
        /// <param name="nodes">Nodes are written to this dictionary</param>
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
                            Id = long.Parse(row[0]),
                            Coords = new Coordinates(double.Parse(row[2]), double.Parse(row[3]), double.Parse(row[4])),
                        });
                }
            }
        }

        /// <summary>
        /// Parse a graphCSV file into a list of edges.
        /// 
        /// This files is expected to not contain a header.
        /// 
        /// Expected CSV Structure:
        /// id (source id),id (target id)
        /// </summary>
        /// <param name="graphCsv"></param>
        /// <param name="nodes"></param>
        /// <param name="edges"></param>
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