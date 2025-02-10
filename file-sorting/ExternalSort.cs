using System.Diagnostics;

namespace FileSorting;

public static class ExternalSort
{
    public static async Task Run(string inputFile, string outputFile)
    {
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: Input file '{inputFile}' does not exist.");
            return;
        }

        Stopwatch sw = Stopwatch.StartNew();
        FileSortingConfiguration config = null;
        try
        {
            config = new FileSortingConfiguration(inputFile, outputFile);
            var chunkSorter = new ChunkSorterPipeline(config);
            Console.WriteLine("Splitting and sorting chunks...");
            await chunkSorter.SplitAndSortChunksAsync();
            Console.WriteLine("Merging chunks...");
            var merger = new KWayMergePipeline(config);
            await merger.MergeAllAsync();
            Console.WriteLine($"Sorting completed in {sw.Elapsed.TotalSeconds:F2} seconds.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            if (config != null && Directory.Exists(config.TempDirectory))
            {
                try { Directory.Delete(config.TempDirectory, true); }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting temp directory: {ex.Message}");
                }
            }
        }
    }
}