using System;
using System.IO;
using System.Text;

namespace ParallelTSP
{
    /// <summary>
    /// This is the graph glass for the traveling salesman problem
    /// 
    /// @author: Robert Clark
    /// @author: Danny Iland
    /// </summary>
    public class Graph
    {
        private long[][] _weights;
        private readonly int _graphSize;

        public Graph()
        {
            _graphSize = 0;
            _weights = null;
        }

        public Graph(int size)
        {
            _graphSize = size;
            _weights = new long[size][/*size*/];
        }

        /// <summary>
        /// This simply generates a randomized graph
        /// for testability, this will probably need
        /// to accept a seed at some point
        /// </summary>
        /// <param name="max"></param>
        public void Randomize(int max)
        {
            var random = new Random();
            for (var x = 0; x < _graphSize; x++)
            {
                for (var y = x; y < _graphSize; y++)
                {
                    if (x == y)
                    {
                        _weights[x][y] = long.MaxValue;
                    }
                    else
                    {
                        // random.Next(max-1) generates a random int between 0 and n - 1. 
                        // Adding one guarantees next will be positive.
                        long next = random.Next(max - 1) + 1;
                        _weights[x][y] = next;
                        _weights[y][x] = next;
                    }
                }
            }
        }

        public long[][] GetMatrix()
        {
            return _weights;
        }

        public void PrintMatrix()
        {
            Console.WriteLine("Adjacency matrix of graph weights:\n");
            Console.Write("\t");
            for (var x = 0; x < _graphSize; x++)
            {
                Console.Write(x + "\t");
            }

            Console.WriteLine("\n");
            for (var x = 0; x < _graphSize; x++)
            {
                Console.Write(x + "\t");
                for (var y = 0; y < _graphSize; y++)
                {
                    if (_weights[x][y] == long.MaxValue)
                    {
                        Console.Write("Inf\t");
                    }
                    else
                    {
                        Console.Write(_weights[x][y] + "\t");
                    }
                }
                Console.WriteLine("\n");
            }
        }

        public void SaveMatrix(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
            {
                streamWriter.WriteLine(_graphSize);
                for (var x = 0; x < _graphSize; x++)
                {
                    for (var y = 0; y < _graphSize; y++)
                    {
                        streamWriter.Write(_weights[x][y] + " ");
                    }
                }
            }
        }

        public void LoadMatrix(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                var graphSize = int.Parse(streamReader.ReadLine());
                _weights = new long[graphSize][/*graphSize*/];
                for (var x = 0; x < graphSize; x++)
                {
                    for (var y = 0; y < graphSize; y++)
                    {
                        _weights[x][y] = streamReader.Read();
                    }
                }
            }
        }
    }
}
