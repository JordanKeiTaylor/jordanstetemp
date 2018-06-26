using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using Improbable;
using Improbable.sandbox.Navigation;
using Improbable.sandbox.Navigation.Api;
using NUnit.Framework;

namespace Tests.Navigation
{
    [TestFixture]
    public class DefaultGraphNavigatorTest
    {
        private const int PathsToTest = 1000;
        private const int RandSeed = 1928;
        private Random _rand;
        
        private const string PointsCsv = "./../../resources/graph/sanfran-micro.points.csv";
        private const string GraphCsv = "./../../resources/graph/sanfran-micro.graph.csv";        
        private IGraphNavigator _navigator;
        private Dictionary<EntityId, PathNode> _nodes;
        private Dictionary<PathNode, List<PathEdge>> _outbounds;
        private PathNode[] _nodesArray;

        [SetUp]
        public void PathfindingSetup()
        {
            _rand = new Random(RandSeed); // Re-seed Random generator before each test.
            
            if (_navigator != null)
            {
                return;
            }
            
            _nodes = new Dictionary<EntityId, PathNode>();
            var edges = new List<PathEdge>();

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            SnapshotParsingUtils.SetGraphNodes(Path.Combine(dir, PointsCsv), _nodes);
            SnapshotParsingUtils.SetGraphEdges(Path.Combine(dir, GraphCsv), _nodes, edges);

            _nodesArray = _nodes.Values.ToArray();
            _outbounds = new Dictionary<PathNode, List<PathEdge>>();
            foreach (var edge in edges)
            {
                if (_outbounds.ContainsKey(edge.Source) == false)
                {
                    _outbounds.Add(edge.Source, new List<PathEdge>());
                }
                _outbounds[edge.Source].Add(edge);
            }

            _navigator = new DefaultPathfinder(_nodes, edges);
        }

        [Test]
        public void ValidNavGraphTraversal()
        {
            var stopwatch = new Stopwatch();
            var averageLength = 0;
            var averageCompleteionTime = 0L;
            
            for (var i = 0; i < PathsToTest; i++)
            {
                var tuple = GenerateValidStartStop();
                
                stopwatch.Start();
                var pathTask = _navigator.GetGraphPath(tuple.Item1, tuple.Item2);
                pathTask.Wait();
                stopwatch.Stop();

                if (pathTask.IsCanceled || pathTask.IsFaulted)
                {
                    throw new Exception("Pathfinder failed to find path for valid start and stop node pair" +
                                        $"({tuple.Item1.EntityId.Id}, {tuple.Item2.EntityId.Id})");
                }

                Assert.True(pathTask.Result != null && pathTask.Result.GetPath().Count > 0);
                averageLength += pathTask.Result.GetPath().Count;
                averageCompleteionTime += stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
            }

            averageLength /= PathsToTest;
            averageCompleteionTime /= PathsToTest;
            
            Console.WriteLine($"ValidNavGraphTraversal: Completed successfully. Tested {PathsToTest} routes with an " +
                              $"average length of {averageLength} and average completion time of " +
                              $"{averageCompleteionTime} ms.");
        }

        [Test]
        public void InvalidNavGraphTraversal()
        {
            var stopwatch = new Stopwatch();
            var successfullyCompleted = 0;
            var averageFailedPathfindTime = 0L;

            for (var i = 0; i < PathsToTest; i++)
            {
                var tuple = GenerateRandomStartStop();
                
                stopwatch.Start();
                var pathTask = _navigator.GetGraphPath(tuple.Item1, tuple.Item2);
                pathTask.Wait();
                stopwatch.Stop();
                
                if (pathTask.IsCanceled || pathTask.IsFaulted)
                {
                    throw new Exception("Pathfinder failed to find path for random start and stop node pair" +
                                        $"({tuple.Item1.EntityId.Id}, {tuple.Item2.EntityId.Id})");
                }

                if (pathTask.Result == null || pathTask.Result.GetPath().Count == 0)
                {
                    Assert.False(PathIsValid(tuple.Item1, tuple.Item2));
                    averageFailedPathfindTime += stopwatch.ElapsedMilliseconds;
                }
                else
                {
                    successfullyCompleted++;
                }
                
                stopwatch.Reset();
            }

            averageFailedPathfindTime /= Math.Max((PathsToTest - successfullyCompleted), 1);
            
            Console.WriteLine($"InvalidNavGraphTraversal: Completed successfully. Found {successfullyCompleted} " +
                              $"valid paths. Failed path average calculation time {(averageFailedPathfindTime)}");
        }

        private Tuple<PathNode, PathNode> GenerateValidStartStop()
        {
            const int attempts = 20;
            var count = 0;
            PathNode start = null;
            PathNode stop = null;

            while (start == null || stop == null)
            {
                start = _nodesArray[_rand.Next(0, _nodesArray.Length)];
                stop = start;

                for (var i = 0; i < _rand.Next(100); i++)
                {
                    if (!_outbounds.ContainsKey(stop) || _outbounds[stop].Count <= 0)
                    {
                        break;
                    }
                    
                    var outbounds = _outbounds[stop];
                    stop = outbounds[_rand.Next(0, outbounds.Count)].Target;
                }

                if (start == stop)
                {
                    start = null;
                    stop = null;
                }

                Assert.True(count < attempts);
                
                count++;
            }
            
            return new Tuple<PathNode, PathNode>(start, stop);
        }

        private Tuple<PathNode, PathNode> GenerateRandomStartStop()
        {
            var start = _nodesArray[_rand.Next(0, _nodesArray.Length)];
            var stop = _nodesArray[_rand.Next(0, _nodesArray.Length)];
            
            return new Tuple<PathNode, PathNode>(start, stop);
        }

        private bool PathIsValid(PathNode start, PathNode stop)
        {
            var search = new Queue<PathNode>();
            var visited = new HashSet<PathNode>();
            
            search.Enqueue(start);

            while (!search.IsNullOrEmpty())
            {
                var curr = search.Dequeue();
                
                if (curr == stop)
                {
                    return true;
                }
                
                if (_outbounds.ContainsKey(curr))
                {
                    var targets = _outbounds[curr].Select(edge => edge.Target);

                    visited.Add(curr);
                    foreach (var target in targets)
                    {
                        if (visited.Contains(target) == false)
                        {
                            search.Enqueue(target);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return false;
        }
    }
}