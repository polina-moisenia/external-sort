using System.Threading.Channels;
using FileSorting.IO;

namespace FileSorting;

public class KWayMergePipeline
{
    private readonly FileSortingConfiguration _config;

    public KWayMergePipeline(FileSortingConfiguration config)
    {
        _config = config;
    }

    public async Task MergeAllAsync()
    {
        var chunkFiles = Directory.GetFiles(_config.TempDirectory, "chunk_*.txt").ToList();
        if (chunkFiles.Count == 0)
            throw new Exception("Нет чанков для слияния.");

        while (chunkFiles.Count > _config.MergeDegree)
        {
            var intermediateFiles = new List<string>();
            var groups = chunkFiles
                .Select((file, index) => new { file, index })
                .GroupBy(x => x.index / _config.MergeDegree)
                .Select(g => g.Select(x => x.file).ToList())
                .ToList();

            var mergeTasks = groups.Select(async group =>
            {
                string intermFile = Path.Combine(_config.TempDirectory, $"merged_{Guid.NewGuid()}.txt");
                intermediateFiles.Add(intermFile);
                await MergeAsync(group, intermFile);
            }).ToArray();

            await Task.WhenAll(mergeTasks);
            foreach (var file in chunkFiles)
            {
                try { File.Delete(file); } catch { }
            }
            chunkFiles = intermediateFiles;
        }

        await MergeAsync(chunkFiles, _config.OutputFile);
        foreach (var file in chunkFiles)
        {
            if (!string.Equals(file, _config.OutputFile, StringComparison.OrdinalIgnoreCase))
                try { File.Delete(file); } catch { }
        }
    }

    public async Task MergeAsync(List<string> inputFiles, string outputFile)
    {
        var outputChannel = Channel.CreateUnbounded<FileLineRecord>();
        Task mergeTask = MergeFilesAsync(inputFiles, outputChannel);
        Task writeTask = WriteOutputAsync(outputChannel, outputFile);
        await Task.WhenAll(mergeTask, writeTask);
    }

    private async Task MergeFilesAsync(List<string> inputFiles, Channel<FileLineRecord> outputChannel)
    {
        int fileCount = inputFiles.Count;
        var batchReaders = new BatchReader[fileCount];
        for (int i = 0; i < fileCount; i++)
        {
            batchReaders[i] = new BatchReader(inputFiles[i], _config.BufferSize, _config.BatchLineCount, _config.PrefetchBufferSize);
            batchReaders[i].StartReading();
        }

        var priorityQueue = new PriorityQueue<(FileLineRecord Record, int ReaderIndex), FileLineRecord>(new FileLineRecordComparer());
        for (int i = 0; i < fileCount; i++)
        {
            if (await batchReaders[i].Reader.WaitToReadAsync())
            {
                if (batchReaders[i].Reader.TryRead(out var record))
                    priorityQueue.Enqueue((record, i), record);
            }
        }

        while (priorityQueue.Count > 0)
        {
            var (record, readerIndex) = priorityQueue.Dequeue();
            await outputChannel.Writer.WriteAsync(record);
            if (await batchReaders[readerIndex].Reader.WaitToReadAsync())
            {
                if (batchReaders[readerIndex].Reader.TryRead(out var nextRecord))
                    priorityQueue.Enqueue((nextRecord, readerIndex), nextRecord);
            }
        }

        foreach (var br in batchReaders)
            await br.DisposeAsync();

        outputChannel.Writer.Complete();
    }

    private async Task WriteOutputAsync(Channel<FileLineRecord> outputChannel, string outputFile)
    {
        await using var writer = new BatchWriter(outputFile, _config.BufferSize);
        var outputBuffer = new List<FileLineRecord>(_config.OutputBufferSize);
        await foreach (var record in outputChannel.Reader.ReadAllAsync())
        {
            outputBuffer.Add(record);
            if (outputBuffer.Count >= _config.OutputBufferSize)
            {
                await writer.WriteAsync(outputBuffer);
                outputBuffer.Clear();
            }
        }
        if (outputBuffer.Count > 0)
            await writer.WriteAsync(outputBuffer);
    }
}