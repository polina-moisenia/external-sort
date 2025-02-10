namespace FileSorting;

public class FileSortingConfiguration
{
    public string InputFile { get; }
    public string OutputFile { get; }
    public string TempDirectory { get; }
    public int ChunkSize { get; }
    public int BufferSize { get; }
    public int OutputBufferSize { get; }
    public int MergeDegree { get; }
    public int BatchLineCount { get; }
    public int PrefetchBufferSize { get; }
    public int MaxParallelSort { get; }
    public int MaxParallelWrite { get; }

    public FileSortingConfiguration(string inputFile, string outputFile,
                      int chunkSize = 1_000_000,
                      int bufferSize = 1024 * 1024 * 10,
                      int outputBufferSize = 10_000,
                      int mergeDegree = 32)
    {
        InputFile = inputFile;
        OutputFile = outputFile;
        TempDirectory = Path.Combine(Path.GetDirectoryName(outputFile), "output_chunks");
        Directory.CreateDirectory(TempDirectory);

        ChunkSize = chunkSize;
        BufferSize = bufferSize;
        OutputBufferSize = outputBufferSize;
        MergeDegree = mergeDegree;
        BatchLineCount = Math.Min(10_000, chunkSize);
        PrefetchBufferSize = BatchLineCount;

        MaxParallelSort = Environment.ProcessorCount;
        MaxParallelWrite = 4;
    }
}
