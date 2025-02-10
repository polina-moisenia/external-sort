using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using FileSorting.IO;

namespace FileSorting;

public class ChunkSorterPipeline
{
    private readonly FileSortingConfiguration _config;

    public ChunkSorterPipeline(FileSortingConfiguration config)
    {
        _config = config;
    }

    public async Task SplitAndSortChunksAsync(CancellationToken cancellationToken = default)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var sortBlock = new TransformBlock<(int, List<FileLineRecord>), (int, List<FileLineRecord>)>(chunk =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            chunk.Item2.Sort();
            return chunk;
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _config.MaxParallelSort });

        var writeBlock = new ActionBlock<(int, List<FileLineRecord>)>(async chunk =>
        {
            string chunkFile = Path.Combine(_config.TempDirectory, $"chunk_{chunk.Item1}.txt");
            await using var writer = new BufferedWriter(chunkFile, _config.BufferSize);
            foreach (var record in chunk.Item2)
            {
                await writer.WriteRecordAsync(record, cancellationToken).ConfigureAwait(false);
            }
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _config.MaxParallelWrite });

        sortBlock.LinkTo(writeBlock, new DataflowLinkOptions { PropagateCompletion = true });

        var reader = new BufferedReader(_config.InputFile, _config.BufferSize);
        int chunkIndex = 0;
        var chunk = new List<FileLineRecord>(_config.ChunkSize);

        await foreach (var record in reader.ReadRecordsAsync(cancellationToken).ConfigureAwait(false))
        {
            chunk.Add(record);
            if (chunk.Count == _config.ChunkSize)
            {
                sortBlock.Post((chunkIndex++, chunk));
                chunk = new List<FileLineRecord>(_config.ChunkSize);
            }
        }
        if (chunk.Count > 0) sortBlock.Post((chunkIndex++, chunk));

        sortBlock.Complete();
        Console.WriteLine($"File was read in {sw.Elapsed.TotalSeconds:F2} seconds.");

        await writeBlock.Completion.ConfigureAwait(false);
        Console.WriteLine($"Chunks were created in {sw.Elapsed.TotalSeconds:F2} seconds.");
    }
}