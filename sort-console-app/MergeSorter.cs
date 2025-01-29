using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class MergeSorter
{
    public static void MergeChunks(string tempDirectory, string outputFile)
    {
        var chunkFiles = Directory.GetFiles(tempDirectory, "*.txt");
        if (chunkFiles.Length == 0)
        {
            Console.WriteLine("Error: No sorted chunks found in temp directory.");
            return;
        }

        var readers = chunkFiles.Select(file => new StreamReader(file, Encoding.UTF8, true)).ToList();
        var minHeap = new SortedDictionary<string, Queue<int>>();

        try
        {
            for (int i = 0; i < readers.Count; i++)
            {
                if (!readers[i].EndOfStream)
                {
                    string line = readers[i].ReadLine()?.TrimEnd('\r');
                    if (line != null)
                    {
                        if (!minHeap.ContainsKey(line))
                        {
                            minHeap[line] = new Queue<int>();
                        }
                        minHeap[line].Enqueue(i);
                    }
                }
            }

            string outputDirectory = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using var writer = new StreamWriter(outputFile, false, Encoding.UTF8);
            while (minHeap.Count > 0)
            {
                var minEntry = minHeap.First();
                string minValue = minEntry.Key;
                int readerIndex = minEntry.Value.Dequeue();

                if (minEntry.Value.Count == 0)
                {
                    minHeap.Remove(minValue);
                }

                writer.WriteLine(minValue);

                if (!readers[readerIndex].EndOfStream)
                {
                    string line = readers[readerIndex].ReadLine()?.TrimEnd('\r');
                    if (line != null)
                    {
                        if (!minHeap.ContainsKey(line))
                        {
                            minHeap[line] = new Queue<int>();
                        }
                        minHeap[line].Enqueue(readerIndex);
                    }
                }
            }

            Console.WriteLine($"Merged file created: {outputFile}");
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader.Dispose();
            }
        }
    }
}