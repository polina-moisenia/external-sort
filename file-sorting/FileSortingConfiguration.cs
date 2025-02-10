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
                      int chunkSize,
                      int bufferSize,
                      int outputBufferSize,
                      int mergeDegree)
    {
        InputFile = inputFile;
        OutputFile = outputFile;
        string dir = Path.GetDirectoryName(outputFile) ?? ".";
        TempDirectory = Path.Combine(dir, "output_chunks");
        Directory.CreateDirectory(TempDirectory);

        ChunkSize = chunkSize;
        BufferSize = bufferSize;
        OutputBufferSize = outputBufferSize;
        MergeDegree = mergeDegree;

        BatchLineCount = Math.Min(10_000, chunkSize);
        PrefetchBufferSize = BatchLineCount;

        MaxParallelSort = Environment.ProcessorCount;
        MaxParallelWrite = 6;
    }

    public static FileSortingConfiguration CreateConfiguration(string inputFile, string outputFile)
    {
        FileInfo fi = new FileInfo(inputFile);
        long fileSize = fi.Length; // в байтах


        return new FileSortingConfiguration(inputFile, outputFile, 500_000, 1024 * 1024 * 32, 10_000, 32);
    }
}
