using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class ChunkSorter
{
   public static void SplitAndSortChunks(string inputFile, string tempDirectory, int chunkSize, bool useSmartWrite)
    {
        var readQueue = new ConcurrentQueue<List<string>>();
        var writeQueue = new ConcurrentQueue<(int, List<string>)>();
        var cts = new CancellationTokenSource();
        int chunkIndex = 0;
        int sortingThreads = Environment.ProcessorCount;
        int writeThreads = WriteThreadManager.GetWriteThreads(useSmartWrite);

        var readerTask = Task.Run(() =>
        {
            using (var reader = new StreamReader(inputFile))
            {
                while (!reader.EndOfStream)
                {
                    var chunkLines = new List<string>(chunkSize);
                    while (chunkLines.Count < chunkSize && !reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (!Utils.IsValidFormat(line))
                            {
                                Console.WriteLine($"Error: Invalid file format detected in line: {line}");
                                cts.Cancel();
                                return;
                            }
                            chunkLines.Add(line);
                        }
                    }
                    readQueue.Enqueue(chunkLines);
                }
            }
        }, cts.Token);

        var sorterTasks = Enumerable.Range(0, sortingThreads).Select(_ => Task.Run(() =>
        {
            while (!readerTask.IsCompleted || !readQueue.IsEmpty)
            {
                if (cts.IsCancellationRequested) return;
                if (readQueue.TryDequeue(out var chunkLines))
                {
                    QuickSorter.Sort(chunkLines);
                    int currentIndex = Interlocked.Increment(ref chunkIndex) - 1;
                    writeQueue.Enqueue((currentIndex, chunkLines));
                }
            }
        }, cts.Token)).ToArray();

        var writerTasks = Enumerable.Range(0, writeThreads).Select(_ => Task.Run(() =>
        {
            while (!readerTask.IsCompleted || !writeQueue.IsEmpty || sorterTasks.Any(t => !t.IsCompleted))
            {
                if (cts.IsCancellationRequested) return;
                if (writeQueue.TryDequeue(out var chunkData))
                {
                    var (index, sortedLines) = chunkData;
                    var chunkFileName = Path.Combine(tempDirectory, $"chunk_{index}.txt");
                    File.WriteAllLines(chunkFileName, sortedLines);
                }
            }
        }, cts.Token)).ToArray();

        try
        {
            Task.WaitAll(sorterTasks);
            Task.WaitAll(writerTasks);
        }
        catch (AggregateException) { }
    }
}
