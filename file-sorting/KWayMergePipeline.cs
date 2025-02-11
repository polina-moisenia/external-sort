using FileSorting.IO;

namespace FileSorting;

public class KWayMergePipeline(FileSortingConfiguration config)
{
    private readonly FileSortingConfiguration _config = config;

    public async Task MergeAllAsync(CancellationToken cancellationToken = default)
    {
        var chunkFiles = Directory.GetFiles(_config.TempDirectory, "chunk_*.txt");

        if (chunkFiles.Length == 0)
            throw new Exception("No chunk files exist.");

        if (chunkFiles.Length == 1)
        {
            if (File.Exists(_config.OutputFile))
            {
                File.Delete(_config.OutputFile);
            }
            File.Move(chunkFiles[0], _config.OutputFile);
            return;
        }

        await MergeAsync(chunkFiles, cancellationToken);
    }

    private async Task MergeAsync(string[] inputFiles, CancellationToken cancellationToken = default)
    {
        await using var writer = new BufferedWriter(_config.OutputFile, _config.BufferSize);
        var priorityQueue = new PriorityQueue<(FileLineRecord Record, int EnumeratorIndex), FileLineRecord>(new FileLineRecordComparer());

        var enumerators = new List<IAsyncEnumerator<FileLineRecord>>();
        foreach (var file in inputFiles)
        {
            var reader = new BufferedReader(file, _config.BufferSize);
            enumerators.Add(reader.ReadRecordsAsync(cancellationToken).GetAsyncEnumerator(cancellationToken));
        }

        for (int i = 0; i < enumerators.Count; i++)
        {
            if (await enumerators[i].MoveNextAsync().ConfigureAwait(false))
                priorityQueue.Enqueue((enumerators[i].Current, i), enumerators[i].Current);
        }

        while (priorityQueue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (record, idx) = priorityQueue.Dequeue();
            await writer.WriteRecordAsync(record, cancellationToken).ConfigureAwait(false);

            if (await enumerators[idx].MoveNextAsync().ConfigureAwait(false))
                priorityQueue.Enqueue((enumerators[idx].Current, idx), enumerators[idx].Current);
        }

        foreach (var enumerator in enumerators)
            await enumerator.DisposeAsync().ConfigureAwait(false);
    }
}
