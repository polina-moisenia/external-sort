using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FileSorting;

namespace FileSortingBenchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // BenchmarkRunner.Run<ChunkSorterPipelineBenchmark>();
            BenchmarkRunner.Run<MergePipelineBenchmark>();
        }
    }
}
