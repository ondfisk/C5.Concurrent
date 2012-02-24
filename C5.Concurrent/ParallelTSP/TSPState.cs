using System;
using System.Collections.Generic;

namespace ParallelTSP
{
    /// <summary>
    /// This class represents a node of the binary search tree that represents
    /// our Traveling Salesman Problem. Each node contains a matrix representing the
    /// current options for travel from this node, the optimal cost of this node, 
    /// best next move possible as calculated by a heuristic, and the path so far.
    /// @author: Danny Iland
    /// @author: Robert Clark
    /// </summary>
    public class TSPState
    {
        private readonly int[] _nextBest;
        private readonly TSPState _parent;
        private readonly int[] _columnMap;
        private readonly int[] _rowMap;
        private readonly int[] _theBest = new int[2];
        private readonly int _size;
        private readonly long _optimalCost;
        private Dictionary<int, int> _path;

        public long[][] Matrix { get; private set; }

        /// <summary>
        /// Initializes a node from a given matrix. At creation, the optimalCost
        /// row mappings and next best path are updated.
        /// </summary>
        /// <param name="weightMatrix">The matrix from which to construct a TSPState</param>
        /// <param name="parent">The parent of this state in the tree</param>
        public TSPState(long[][] weightMatrix, TSPState parent)
        {
            Matrix = weightMatrix;
            _size = weightMatrix.Length;
            _parent = parent;
            _columnMap = new int[weightMatrix.Length];
            _rowMap = new int[weightMatrix.Length];
            for (var x = 0; x < weightMatrix.Length; x++)
            {
                _columnMap[x] = x;
                _rowMap[x] = x;
            }
            if (parent != null)
            {
                long reduction = Reduce();
                _optimalCost = parent._optimalCost + reduction;
                if (_optimalCost < parent._optimalCost)
                    _optimalCost = long.MaxValue;
            }
            else
            {
                _optimalCost = Reduce();
            }
            _nextBest = BestCoord();

        }

        /// <summary>
        /// Constructs a node from the given input data. After creation, performs
        /// reduction and determines the optimal cost and best path from here.
        /// </summary>
        /// <param name="weightMatrix">The matrix representing the graph</param>
        /// <param name="parent">The parent of this node</param>
        /// <param name="columnMap">An array mapping weightMatrix to the original matrix</param>
        /// <param name="rowMap">An array mapping weightMatrix to the original matrix</param>
        /// <param name="size">The number of unspecified paths remaining</param>
        public TSPState(long[][] weightMatrix, TSPState parent, int[] columnMap, int[] rowMap, int size)
        {
            Matrix = weightMatrix;
            _parent = parent;
            _rowMap = rowMap;
            _columnMap = columnMap;
            _size = size;
            if (parent != null)
            {
                var reduction = Reduce();
                _optimalCost = parent._optimalCost + reduction;
                if (_optimalCost < parent._optimalCost)
                {
                    _optimalCost = long.MaxValue;
                }
            }
            else
            {
                _optimalCost = Reduce();
            }
            _nextBest = BestCoord();

        }
        /// <summary>
        /// Returns the lowest possible path score for any child of this node
        /// </summary>
        /// <returns></returns>
        public long GetLowerBound()
        {
            return _optimalCost;
        }

        /// <summary>
        /// Check if this array represents a readonly state,
        /// a state that cannot be divided anymore, i.e. has only one path remaining
        /// </summary>
        /// <returns></returns>
        public bool IsFinalState()
        {
            return (Matrix.Length < 1 || Matrix[0].Length < 1);
        }

        /// <summary>
        /// Called on terminal nodes
        /// </summary>
        /// <returns>The path through the matrix that ends on this node</returns>
        public Dictionary<int, int> GetPath()
        {
            if (_path == null)
            {
                if (_parent == null)
                {
                    _path = new Dictionary<int, int>(_size);
                }
                else
                {
                    _path = new Dictionary<int, int>(_parent.GetPath());
                    _path[_parent._theBest[0]] = _parent._theBest[1];
                }
            }
            return _path;
        }

        /// <summary>
        /// Prints the cycle, beginning with 0, that represents the solution
        /// to the traveling salesman problem held in path.
        /// The path Dictionary holds routes from key city to value city, exactly one
        /// route per source and exactly one per destination.
        /// </summary>
        /// <param name="path"></param>
        public static void PrintPath(Dictionary<int, int> path)
        {
            Console.WriteLine("Printing best route:");
            int index = 0;
            int counter = path.Count;
            do
            {
                Console.Write(index + " , ");
                if (!path.ContainsKey(index))
                {
                    return;
                }
                index = path[index];
                counter--;
            } while (index != 0 && counter > 0);
            Console.WriteLine(0);
        }

        /**
     * Reduces the values by first subtracting the minimum of every
     * column from each value, then subtracting the minimum of every
     * row from each value. This normalizes the data.
     * 
     * Returns the minimum possible value for a complete loop.
     */
        public long Reduce()
        {
            var lowerBound = 0L;
            var length = Matrix.Length;
            // For each row
            for (var x = 0; x < length; x++)
            {
                // discover this row's minimum
                var min = Matrix[x][0];
                for (var y = 1; y < length; y++)
                {
                    if (Matrix[x][y] < min)
                    {
                        min = Matrix[x][y];
                    }
                }
                // The subtract it from each value
                for (int y = 0; y < length; y++)
                {
                    Matrix[x][y] = Matrix[x][y] - min;
                }
                // And add it to the lower bound.
                lowerBound = lowerBound + min;
            }
            // For each column
            for (var y = 0; y < length; y++)
            {
                // discover this column's minimum
                var min = Matrix[0][y];
                for (var x = 1; x < length; x++)
                {
                    if (Matrix[x][y] < min)
                    {
                        min = Matrix[x][y];
                    }
                }

                // The subtract it from each value
                for (var x = 0; x < length; x++)
                {
                    Matrix[x][y] = Matrix[x][y] - min;
                }
                // And add it to the lower bound.
                lowerBound = lowerBound + min;
            }
            //printMatrix();
            return lowerBound;
        }

        /// <summary>
        /// Calculate the resulting weighted graph after left split at the nextBest
        /// </summary>
        /// <returns></returns>
        public TSPState LeftSplit()
        {
            var x = _nextBest[0];
            var y = _nextBest[1];
            var newmatrix = new long[Matrix.Length - 1][/*Matrix.Length-1*/];
            var offset = 0;
            for (var c = 0; c < Matrix.Length; c++)
            {
                // skip x
                if (c == x)
                {
                    offset = 1;
                    continue;
                }
                Array.Copy(Matrix[c], 0, newmatrix[c - offset], 0, y);
                if (Matrix.Length != y)
                {
                    Array.Copy(Matrix[c], y + 1, newmatrix[c - offset], y, Matrix[c].Length - y - 1);
                }
            }

            var newCol = new int[Matrix.Length - 1];
            var newRow = new int[Matrix.Length - 1];
            // Map the old rows and columns into new rows and columns
            Array.Copy(_columnMap, 0, newCol, 0, y);
            Array.Copy(_columnMap, y + 1, newCol, y, (Matrix.Length - 1) - y);
            Array.Copy(_rowMap, 0, newRow, 0, x);
            Array.Copy(_rowMap, x + 1, newRow, x, (Matrix.Length - 1) - x);

            var columnExists = -1;
            var rowExists = -1;

            for (var i = 0; i < newCol.Length; i++)
            {
                if (newRow[i] == y)
                {
                    rowExists = i;
                }
                if (newCol[i] == x)
                {
                    columnExists = i;
                }

            }
            if (columnExists >= 0 && rowExists >= 0)
            {
                newmatrix[rowExists][columnExists] = long.MaxValue;
            }
            return new TSPState(newmatrix, this, newCol, newRow, _size);
        }

        /// <summary>
        /// Calculate the resulting weighted graph after right split at nextBest
        /// </summary>
        /// <returns></returns>
        public TSPState RightSplit()
        {
            var x = _nextBest[0];
            var y = _nextBest[1];
            var newmatrix = new long[Matrix.Length][/*Matrix.Length*/];
            for (var c = 0; c < Matrix.Length; c++)
            {
                Array.Copy(Matrix[c], 0, newmatrix[c], 0, Matrix[c].Length);
            }
            if (newmatrix[x][y] == long.MaxValue)
            {
                return null;
            }
            newmatrix[x][y] = long.MaxValue;

            FixNextBest();
            var newState = new TSPState(newmatrix, this, _columnMap, _rowMap, _size);
            return newState;
        }

        /// <summary>
        /// Updates theBest to represent the correct nodes after modifying 
        /// the backing matrix
        /// </summary>
        public void FixNextBest()
        {
            _theBest[0] = _rowMap[_nextBest[0]];
            _theBest[1] = _columnMap[_nextBest[1]];
        }

        /// <summary>
        /// Find the element that, when ignored, results in the largest
        /// minimum element of row + minimum element of column. This is a
        /// heurustic for splitting most effectively, from Reingold'sCombinatorial Algorithms: Theory and Practice
        /// </summary>
        /// <returns></returns>
        public int[] BestCoord()
        {
            if (_nextBest != null) return _nextBest;
            long largestSoFar = 0;
            var retVal = new int[2]; // x][y
            // For each element of the array
            for (var x = 0; x < Matrix.Length; x++)
            {
                for (var y = 0; y < Matrix.Length; y++)
                {
                    // If this element is zero
                    if (Matrix[x][y] == 0)
                    {
                        // add the next lowest row value and the next lowest column value.
                        var reduction = GetNextLowestRowValue(Matrix[x], y) + GetNextLowestColumnValue(Matrix, y, x);

                        if (reduction >= largestSoFar)
                        {
                            retVal[0] = x;
                            retVal[1] = y;
                            largestSoFar = reduction;
                        }
                    }
                }
            }
            // Return the lowest sum
            return retVal;
        }


        /// <summary>
        /// A helper for bestCoord
        /// </summary>
        /// <param name="row"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private long GetNextLowestRowValue(long[] row, int index)
        {
            var lowest = long.MaxValue;
            for (var i = 0; i < row.Length; i++)
            {
                if (row[i] < lowest && i != index)
                {
                    lowest = row[i];
                }
            }
            return lowest;
        }
        /// <summary>
        /// A helper for bestCoord
        /// </summary>
        /// <param name="array"></param>
        /// <param name="columnIndex"></param>
        /// <param name="elementIndex"></param>
        /// <returns></returns>
        private long GetNextLowestColumnValue(long[][] array, int columnIndex, int elementIndex)
        {
            long lowest = long.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i][columnIndex] < lowest && i != elementIndex)
                {
                    lowest = array[i][columnIndex];
                }
            }
            return lowest;
        }

        /// <summary>
        /// Print the state with labels reflecting Original Values.
        /// Useful during debugging.
        /// </summary>
        public void PrintMatrix()
        {
            Console.WriteLine("Adjacency matrix of graph weights:\n");
            Console.Write("\t");
            for (var x = 0; x < Matrix.Length; x++)
                Console.Write(_columnMap[x] + "\t");

            Console.WriteLine("\n");
            for (var x = 0; x < Matrix.Length; x++)
            {
                Console.Write(_rowMap[x] + "\t");
                for (var y = 0; y < Matrix[x].Length; y++)
                {
                    if (Matrix[x][y] > long.MaxValue - 100000)
                    {
                        Console.Write("Inf\t");
                    }
                    else
                    {
                        Console.Write(Matrix[x][y] + "\t");
                    }
                }
                Console.WriteLine("\n");
            }
        }
    }
}
