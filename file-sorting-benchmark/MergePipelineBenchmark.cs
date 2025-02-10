using BenchmarkDotNet.Attributes;
using FileSorting;

namespace FileSortingBenchmark;

[MemoryDiagnoser]
public class MergePipelineBenchmark
{
    private FileSortingConfiguration _config;
        private KWayMergePipeline _mergePipeline;
        private const string StableSourceDir = @"C:\Proj\external-sort\output_chunks";

        [GlobalSetup]
        public void GlobalSetup()
        {
            Directory.SetCurrentDirectory(@"C:\Proj\external-sort\benchmark_chunks");
            string inputFile = @"C:\Proj\external-sort\data_1gb.txt";
            string outputFile = "mergedOutput.txt";
            _config = FileSortingConfiguration.CreateConfiguration(inputFile, outputFile);
            Directory.CreateDirectory(_config.TempDirectory);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            foreach (var file in Directory.GetFiles(_config.TempDirectory))
            {
                File.Delete(file);
            }
            
            foreach (var file in Directory.GetFiles(StableSourceDir, "chunk_*.txt"))
            {
                string dest = Path.Combine(_config.TempDirectory, Path.GetFileName(file));
                File.Copy(file, dest, overwrite: true);
            }
            var chunkFiles = Directory.GetFiles(_config.TempDirectory, "chunk_*.txt");
            if (chunkFiles.Length == 0)
                throw new Exception("No chunk files exist in " + _config.TempDirectory);
            _mergePipeline = new KWayMergePipeline(_config);
        }

    [Benchmark]
    public async Task MergeAllAsyncBenchmark()
    {
        await _mergePipeline.MergeAllAsync(CancellationToken.None);
    }
}