using System;

namespace ParallelTSP
{
    /// <summary>
    /// This class is simply a complete graph random
    /// generator. Mainly used as a helper class to
    /// create matrices of varying size, and maximum costs.
    /// 
    /// Author: Robert Clark
    /// </summary>
    public class CompleteGenerator
    {
        public static void MainZ(string[] args)
        {
            if (args.Length == 3)
            {
                var size = int.Parse(args[0]);
                var maxLen = int.Parse(args[1]);
                var outputName = args[2];

                var graph = new Graph(size);
                graph.Randomize(maxLen);
                graph.PrintMatrix();
                try
                {
                    graph.SaveMatrix(outputName);
                }
                catch
                {
                    Console.WriteLine("Unable to save matrix");
                }
            }
            else
            {
                Console.WriteLine("Usage: CompleteGenerator size max_length output");
            }
        }
    }
}
