using BenchmarkDotNet.Attributes;
using FileSorting;

namespace FileSortingBenchmark;

[SimpleJob(warmupCount: 1, iterationCount: 5)]
[MemoryDiagnoser]
public class ChunkSorterPipelineBenchmark
{
    private FileSortingConfiguration _config;
    private ChunkSorterPipeline _pipeline;
    private string _tempDir;
    private string _inputFile;
    private string _sourceInputFile = @"C:\Proj\external-sort\data_1gb.txt";

    [GlobalSetup]
    public void GlobalSetup()
    {
        if (!File.Exists(_sourceInputFile))
        {
            throw new FileNotFoundException("Исходный тестовый файл не найден.", _sourceInputFile);
        }
    }

    [IterationSetup]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FileSortingBenchmark", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _inputFile = Path.Combine(_tempDir, "input.txt");
        File.Copy(_sourceInputFile, _inputFile, overwrite: true);

        _config = new FileSortingConfiguration(_inputFile, "tmp", 1_000_000, 1024 * 1024 * 32);

        _pipeline = new ChunkSorterPipeline(_config);
    }

    [IterationCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Benchmark]
    public async Task Benchmark_SplitAndSortChunksAsync()
    {
        await _pipeline.SplitAndSortChunksAsync();
    }
}