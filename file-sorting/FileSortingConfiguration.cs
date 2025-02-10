namespace FileSorting;

public class FileSortingConfiguration
{
    public string InputFile { get; }
    public string OutputFile { get; }
    public string TempDirectory { get; }
    public int ChunkSize { get; }
    public int BufferSize { get; }
    public int MaxParallelSort { get; }
    public int MaxParallelWrite { get; }

    public FileSortingConfiguration(string inputFile, string outputFile, int chunkSize, int bufferSize)
    {
        InputFile = inputFile;
        OutputFile = outputFile;
        string dir = Path.GetDirectoryName(outputFile) ?? ".";
        TempDirectory = Path.Combine(dir, "output_chunks");
        Directory.CreateDirectory(TempDirectory);

        ChunkSize = chunkSize;
        BufferSize = bufferSize;

        MaxParallelSort = Environment.ProcessorCount;
        MaxParallelWrite = Math.Max(6, Environment.ProcessorCount / 4);
    }

    public static FileSortingConfiguration CreateConfiguration(string inputFile, string outputFile)
    {
        FileInfo fi = new FileInfo(inputFile);
        long fileSize = fi.Length; // bytes
        //TODO

        return new FileSortingConfiguration(inputFile, outputFile, 10_000_000, 1024 * 1024 * 32);
    }
}
