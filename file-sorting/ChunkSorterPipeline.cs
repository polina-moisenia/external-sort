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

    public async Task SplitAndSortChunksAsync()
    {
        var sortBlock = new TransformBlock<(int, List<string>), (int, List<FileLineRecord>)>(chunk =>
        {

            var records = new List<FileLineRecord>(chunk.Item2.Count);
            foreach (var line in chunk.Item2)
            {
                if (FileLineRecord.TryParse(line, out var record))
                    records.Add(record);
            }
            records.Sort();
            return (chunk.Item1, records);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _config.MaxParallelSort });

        var writeBlock = new ActionBlock<(int, List<FileLineRecord>)>(async chunk =>
        {
            string chunkFile = Path.Combine(_config.TempDirectory, $"chunk_{chunk.Item1}.txt");
            await using var writer = new BufferedWriter(chunkFile, _config.BufferSize);
            await writer.WriteLinesAsync(chunk.Item2).ConfigureAwait(false);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _config.MaxParallelWrite });

        sortBlock.LinkTo(writeBlock, new DataflowLinkOptions { PropagateCompletion = true });

        using var reader = new BufferedReader(_config.InputFile, _config.BufferSize);
        int chunkIndex = 0;
        var chunk = new List<string>(_config.ChunkSize);
        string? line;

        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            chunk.Add(line.TrimEnd('\r'));
            if (chunk.Count >= _config.ChunkSize)
            {
                sortBlock.Post((chunkIndex++, chunk));
                chunk = new List<string>(_config.ChunkSize);
            }
        }
        if (chunk.Count > 0) sortBlock.Post((chunkIndex++, chunk));

        sortBlock.Complete();
        await writeBlock.Completion.ConfigureAwait(false);
    }
}