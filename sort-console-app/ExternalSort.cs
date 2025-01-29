using System;
using System.Diagnostics;
using System.IO;

public static class ExternalSort
{
    public static void Run(string inputFile, string outputFile, int chunkSize, bool useSmartWrite)
    {
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: Input file '{inputFile}' does not exist.");
            return;
        }

        string tempDirectory = Path.Combine(Path.GetTempPath(), "ExternalSort");
        Directory.CreateDirectory(tempDirectory);

        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Starting external sort...");

        try
        {
            ChunkSorter.SplitAndSortChunks(inputFile, tempDirectory, chunkSize, useSmartWrite);
            MergeSorter.MergeChunks(tempDirectory, outputFile);
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine($"Total execution time: {stopwatch.Elapsed}");
            Utils.LogMemoryUsage("Final");

            Directory.Delete(tempDirectory, true);
        }

        Console.WriteLine($"Sorted file created: {outputFile}");
        Console.WriteLine($"Output file path: {Path.GetFullPath(outputFile)}");
    }
}