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

    public FileSortingConfiguration(string inputFile, string outputFile)
    {
        InputFile = inputFile;
        OutputFile = outputFile;
        string dir = Path.GetDirectoryName(outputFile) ?? ".";
        TempDirectory = Path.Combine(dir, "output_chunks");
        Directory.CreateDirectory(TempDirectory);

        FileInfo fi = new(inputFile);
        long fileSize = fi.Length;

        if(fileSize < 25_000_000_000) {
            ChunkSize = Math.Max((int)(fileSize / 1000), 1_000_000);
        } else {
            ChunkSize = (int)(fileSize / 5000);
        }

        BufferSize = 1024 * 1024 * 10;

        MaxParallelSort = Environment.ProcessorCount;
        MaxParallelWrite = Math.Max(6, Environment.ProcessorCount / 4);
    }
}
