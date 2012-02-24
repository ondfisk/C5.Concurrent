using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ParallelTSP
{
    /// <summary>
    /// This class implements a brute force search 
    /// for the traveling salesman problem, with pruning
    /// via the branch and bound method.
    ///
    /// @author: Robert Clark
    /// @author: Danny Iland
    /// </summary>
    public class OptimalTSPSMP
    {
        private readonly long[][] _staticMatrix;
        private readonly long[][] _weightMatrix;

        private Dictionary<int, int> _optimalPath = new Dictionary<int, int>(); // SharedObject<Dictionary<int, int>> optimalPath = new SharedObject<Dictionary<int, int>>();
        private long _optimalCost = long.MaxValue; // SharedLong optimalCost = new SharedLong(long.MaxValue);
        private Stack<TSPState> _rightStack;
        private Stack<TSPState> _leftStack;
        private SortedDictionary<long, TSPState> sharedStack;
        private int _needMoreStates; // SharedInteger needMoreStates = new SharedInteger(0);

        /// <summary>
        /// Constructs an OptimalTSPSMP solver object containing the Graph
        /// object produced from the input file. Solves the TSP, then reports
        /// runtime. 
        /// </summary>
        /// <param name="args">The graph file produced by CompleteGenerator</param>
        public static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            // Comm.init(args);
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: OptimalTSPSMP graphFile");
                return;
            }

            var theGraph = new Graph();
            try
            {
                theGraph.LoadMatrix(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to load matrix");
                return;
            }

            var solver = new OptimalTSPSMP(theGraph);
            Console.WriteLine("Starting Solver");
            solver.Start();

            stopwatch.Stop();

            Console.WriteLine("Runtime for optimal TSP: {0} milliseconds", stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Constructor for a parallel solver object. Stores a copy of the initial
        /// matrix for later use by getCost. 
        /// </summary>
        /// <param name="inputGraph">The Graph object representing the complete graph.</param>
        private OptimalTSPSMP(Graph inputGraph)
        {
            _weightMatrix = inputGraph.GetMatrix();
            var length = _weightMatrix.Length;
            _staticMatrix = new long[length][ /*length*/];
            inputGraph.PrintMatrix();

            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    _staticMatrix[i][j] = _weightMatrix[i][j];
                }
            }
            //sharedStack = Collections.synchronizedSortedMap(new Dictionary<long, TSPState>()); 
        }

        /**
     * Sequential initialization routine before solving in parallel. Traverses
     * down the left branch, splitting and populating sharedStack until
     * enough nodes exist for parallel execution.
     * 
     * @throws Exception
     */

        public void Start()
        {
            var startMatrix = new long[_weightMatrix.Length][ /*_weightMatrix.Length*/];
            Array.Copy(_weightMatrix, 0, startMatrix, 0, _weightMatrix.Length);
            var startState = new TSPState(startMatrix, null);

            long lowerBound;
            for (var i = 0; i <= 0 /*Comm.world().size()*/; i++)
            {
                if (startState != null && !startState.IsFinalState())
                {
                    var left = startState.LeftSplit();
                    var right = startState.RightSplit();

                    lowerBound = right.GetLowerBound();
                    while (sharedStack.ContainsKey(lowerBound))
                    {
                        lowerBound++;
                    }
                    sharedStack[lowerBound] = right;
                    startState = left;
                }
                else
                {
                    break;
                }
            }
            lowerBound = startState.GetLowerBound();
            while (sharedStack.ContainsKey(lowerBound))
            {
                lowerBound++;
            }
            sharedStack[lowerBound] = startState;
            Run();
        }

        /// <summary>
        /// Parallel Solver class. Since the problem is represented as
        /// a binary tree, a thread can execute the same algorithm used
        /// for solving the entire tree on any subtree, without conflict
        /// or overlapping. Uses a shared map of right states sorted by lower bound
        /// and a sharedLong for current lowest cost path found.
        /// </summary>
        public void Run()
        {
            //new ParallelTeam().execute (new ParallelRegion() {

            //     public void Run() {
            //         Dictionary<long, TSPState> leftStack = new Dictionary<long, TSPState>();
            //         Dictionary<long, TSPState> rightStack = new Dictionary<long, TSPState>();
            //         TSPState state = null;

            //         synchronized(sharedStack) {
            //             if(!sharedStack.isEmpty()) {
            //                 state = sharedStack.remove(sharedStack.firstKey());
            //                 leftStack.put(state.GetLowerBound(), state);
            //             }
            //         }

            //         while(!leftStack.isEmpty() || !rightStack.isEmpty() ) {
            //             if(!leftStack.isEmpty()) {
            //                 state = leftStack.remove(leftStack.firstKey());
            //             } else if (!rightStack.isEmpty()) {
            //                 state = rightStack.remove(rightStack.firstKey());
            //             }

            //             if( state != null && state.IsFinalState() ) {
            //                 Dictionary<int, int> thisPath = state.GetPath();
            //                 long thisCost = getCost(thisPath);
            //                 if( ( thisPath.size() >= staticMatrix.Length ) && ( thisCost < optimalCost.get() ) ) {
            //                     optimalCost.set(thisCost);
            //                     optimalPath.set(thisPath);
            //                 }
            //             } else if (state != null ){
            //                 if ( state.GetLowerBound() < optimalCost.get() ) {
            //                     TSPState left = state.LeftSplit();
            //                     leftStack.put(left.GetLowerBound(), left);
            //                     TSPState right = state.RightSplit();
            //                     if(right != null) {
            //                         if(needMoreStates.get() > 0 ) {
            //                             sharedStack.put(right.GetLowerBound(), right);
            //                             needMoreStates.decrementAndGet();
            //                         } else {
            //                             long lowerBound = right.GetLowerBound();
            //                             while(rightStack.containsKey(lowerBound)) {
            //                                 lowerBound++;
            //                             }
            //                             rightStack.put(lowerBound, right);
            //                         }
            //                     }
            //                 }
            //             }
            //             if(leftStack.isEmpty() && rightStack.isEmpty()) {
            //                 boolean done = false;
            //                 int waiting = needMoreStates.incrementAndGet();
            //                 while(needMoreStates.get() > waiting) {
            //                     if(needMoreStates.get() == Comm.world().size() ) {
            //                         done = true;
            //                     }

            //                 }
            //                 if(!done) {
            //                     synchronized(sharedStack) {
            //                         if(!sharedStack.isEmpty()) {
            //                             state = sharedStack.remove(
            //                                     sharedStack.firstKey());
            //                         } else {
            //                             state = null;
            //                         }
            //                         if( state != null ) {
            //                             leftStack.put( state.GetLowerBound(), state);
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // });

            // Console.WriteLine("The shortest cycle is of distance " + optimalCost);
            // TSPState.printPath(optimalPath.get());
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
        /// <param name="path">The path to measure</param>
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
