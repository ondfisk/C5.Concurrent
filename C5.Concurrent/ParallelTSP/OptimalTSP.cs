using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ParallelTSP
{
    /// <summary>
    /// This class implements a sequential brute force search for 
    /// the traveling salesman problem using branch and bound.
    /// 
    /// @author: Robert Clark
    /// @author: Danny Iland
    /// </summary>
    public class OptimalTSP
    {
        private readonly long[][] _staticMatrix;
        private readonly long[][] _weightMatrix;
        private Dictionary<int, int> _optimalPath = new Dictionary<int, int>();
        private long _optimalCost = long.MaxValue;
        private readonly SortedDictionary<long, TSPState> _rightMap;
        private readonly SortedDictionary<long, TSPState> _leftMap;

        /// <summary>
        /// Constructs an OptimalTSP solver object containing the Graph 
        /// object produced from the input file. Solves the TSP, then reports 
        /// runtime. 
        /// </summary>
        /// <param name="args">the Graph file to read in from</param>
        public static void MainZ(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            //Comm.init(args);
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: OptimalTSP graphFile");
                return;
            }

            var graph = new Graph();

            try
            {
                graph.LoadMatrix(args[0]);
            }
            catch
            {
                Console.WriteLine("Unable to load matrix");
                return;
            }

            var solver = new OptimalTSP(graph);
            Console.WriteLine("Starting Solver");

            solver.Start();

            stopwatch.Stop();

            Console.WriteLine("Runtime for optimal TSP: {0} milliseconds.", stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Constructs an OptimalTSP object representing the provided
        /// graph. Stores a copy of the initial matrix for later use
        /// by getCost. 
        /// </summary>
        /// <param name="inputGraph">The graph object representing the complete graph.</param>
        private OptimalTSP(Graph inputGraph)
        {
            _weightMatrix = inputGraph.GetMatrix();
            var length = _weightMatrix.Length;

            _staticMatrix = new long[length][/*length*/];
            inputGraph.PrintMatrix();
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    _staticMatrix[i][j] = _weightMatrix[i][j];
                }
            }
            _rightMap = new SortedDictionary<long, TSPState>();
            _leftMap = new SortedDictionary<long, TSPState>();
        }

        /// <summary>
        /// Initializes the solving routine. Splits the graph's root node, beginning
        /// the partitioning into left and right maps.
        /// </summary>
        public void Start()
        {
            var startMatrix = new long[_weightMatrix.Length][/*_weightMatrix.Length*/];
            Array.Copy(_weightMatrix, 0, startMatrix, 0, _weightMatrix.Length);
            var startState = new TSPState(startMatrix, null);
            var left = startState.LeftSplit();
            _leftMap[left.GetLowerBound()] = left;
            var right = startState.RightSplit();
            _rightMap[right.GetLowerBound()] = right;
            Run();
        }


        /// <summary>
        /// Solves the traveling salesman problem. Traverses down the left
        /// path until it reaches a readonly state, putting right children into a map
        /// along the way. Then it checks if this readonly state is the best so far,
        /// and stores it if that is the case. At each step, if this node's children
        /// cannot be better than the current best node, the node is pruned. After
        /// reaching the end of a left branch, or upon pruning, the right map
        /// is polled for the next state to begin the search from.
        /// </summary>
        public void Run()
        {
            while (_leftMap.Count > 0 || _rightMap.Count > 0)
            {
                TSPState state;
                if (_leftMap.Count > 0)
                {
                    var first = _leftMap.Keys.First();
                    state = _leftMap[first];
                    _leftMap.Remove(first);
                }
                else
                {
                    if (_rightMap.Count > 0)
                    {
                        var first = _rightMap.Keys.First();
                        state = _rightMap[first];
                        _rightMap.Remove(first);
                    }
                    else
                    {
                        state = null;
                    }
                }
                if (state != null && state.IsFinalState())
                {
                    var thisPath = state.GetPath();
                    var thisCost = GetCost(thisPath);
                    if ((thisPath.Count >= _staticMatrix.Length) && (thisCost < _optimalCost))
                    {
                        _optimalCost = thisCost;
                        _optimalPath = thisPath;
                    }
                }
                else if (state != null)
                {
                    if (state.GetLowerBound() < _optimalCost)
                    {
                        var left = state.LeftSplit();
                        var lowerBound = left.GetLowerBound();
                        while (_leftMap.ContainsKey(lowerBound))
                        {
                            lowerBound++;
                        }
                        _leftMap[lowerBound] = left;
                        var right = state.RightSplit();
                        if (right != null)
                        {
                            lowerBound = right.GetLowerBound();
                            while (_rightMap.ContainsKey(lowerBound))
                            {
                                lowerBound++;
                            }
                            _rightMap[lowerBound] = right;
                        }
                    }
                }
            }
            Console.WriteLine("The shortest cycle is of distance " + _optimalCost);
            TSPState.PrintPath(_optimalPath);
        }

        /// <summary>
        /// Prints the matrix with column and row headings.
        /// </summary>
        /// <param name="matrix">The matrix to print.</param>

        public static void PrintMatrix(long[][] matrix)
        {
            Console.WriteLine("Adjacency matrix of graph weights:\n");
            Console.Write("\t");
            for (var x = 0; x < matrix.Length; x++)
            {
                Console.Write(x + "\t");
            }
            Console.WriteLine("\n");
            for (var x = 0; x < matrix.Length; x++)
            {
                Console.Write(x + "\t");
                for (int y = 0; y < matrix[x].Length; y++)
                {
                    if (matrix[x][y] > long.MaxValue - 10000)
                    {
                        Console.Write("Inf\t");
                    }
                    else
                    {
                        Console.Write(matrix[x][y] + "\t");
                    }
                }
                Console.WriteLine("\n");
            }
        }

        /// <summary>
        /// Returns the length to complete a cycle in the order specified.
        /// </summary>
        /// <param name="path"> The path to measure</param>
        /// <returns>The total cost to travel the path</returns>
        public long GetCost(Dictionary<int, int> path)
        {
            var distance = 0L;
            var start = 0;
            var count = 0;
            do
            {
                if (!path.ContainsKey(start))
                {
                    return long.MaxValue;
                }
                var end = path[start];
                distance = distance + _staticMatrix[start][end];
                start = end;
                count++;
            } while (start != 0);

            return count < path.Count ? long.MaxValue : distance;
        }
    }
}
