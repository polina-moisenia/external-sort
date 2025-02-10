using System.Text;

namespace FileSorting.IO;

public class BufferedWriter : IAsyncDisposable
{
    private readonly StreamWriter _writer;

    public BufferedWriter(string filePath, int bufferSize)
    {
        var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous);
        _writer = new StreamWriter(stream, Encoding.UTF8, bufferSize);
    }

    public async Task WriteRecordAsync(FileLineRecord record, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _writer.WriteLineAsync(record.ToString()).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await _writer.FlushAsync().ConfigureAwait(false);
        await _writer.DisposeAsync().ConfigureAwait(false);
    }
}