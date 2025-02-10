using FileSorting.IO;

namespace FileSorting;

public class KWayMergePipeline
{
    private readonly FileSortingConfiguration _config;

    public KWayMergePipeline(FileSortingConfiguration config)
    {
        _config = config;
    }

    public async Task MergeAllAsync(CancellationToken cancellationToken = default)
    {
        var chunkFiles = Directory.GetFiles(_config.TempDirectory, "chunk_*.txt");

        if (chunkFiles.Length == 0)
            throw new Exception("No chunk files exist.");

        await MergeAsync(chunkFiles, _config.OutputFile, cancellationToken);
    }

    private async Task MergeAsync(string[] inputFiles, string outputFile, CancellationToken cancellationToken = default)
    {
        await using var writer = new BufferedWriter(outputFile, _config.BufferSize);
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
