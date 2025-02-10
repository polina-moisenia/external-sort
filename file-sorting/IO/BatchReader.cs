using System.Text;
using System.Threading.Channels;

namespace FileSorting.IO;

public class BatchReader : IAsyncDisposable
{
    private readonly string _filePath;
    private readonly int _bufferSize;
    private readonly int _batchLineCount;
    private readonly Channel<FileLineRecord> _recordChannel;
    private Task _readingTask;

    public BatchReader(string filePath, int bufferSize, int batchLineCount, int prefetchBufferSize)
    {
        _filePath = filePath;
        _bufferSize = bufferSize;
        _batchLineCount = batchLineCount;
        _recordChannel = Channel.CreateBounded<FileLineRecord>(new BoundedChannelOptions(prefetchBufferSize)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public ChannelReader<FileLineRecord> Reader => _recordChannel.Reader;

    public void StartReading() => _readingTask = ReadFileAsync();

    private async Task ReadFileAsync()
    {
        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize, useAsync: true);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: _bufferSize);

        var batch = new List<string>(_batchLineCount);
        while (!reader.EndOfStream)
        {
            batch.Clear();
            for (int i = 0; i < _batchLineCount && !reader.EndOfStream; i++)
            {
                string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line is not null)
                    batch.Add(line);
            }
            if (batch.Count > 0)
                await ProcessBatchAsync(batch).ConfigureAwait(false);
        }

        _recordChannel.Writer.Complete();
    }

    private async Task ProcessBatchAsync(List<string> batch)
    {
        var results = new FileLineRecord[batch.Count];

        Parallel.For(0, batch.Count, i =>
        {
            string line = batch[i];
            if (!string.IsNullOrWhiteSpace(line) && FileLineRecord.TryParse(line, out FileLineRecord record))
                results[i] = record;
        });

        foreach (var record in results)
        {
            await _recordChannel.Writer.WriteAsync(record).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_readingTask is not null)
            await _readingTask.ConfigureAwait(false);
    }
}
