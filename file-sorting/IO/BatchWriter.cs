using System.Text;

namespace FileSorting.IO;

public class BatchWriter : IAsyncDisposable
{
    private readonly StreamWriter _writer;

    public BatchWriter(string filePath, int bufferSize)
    {
        var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous);
        _writer = new StreamWriter(fileStream, Encoding.UTF8, bufferSize, leaveOpen: false);
    }

    public async Task WriteAsync<T>(IEnumerable<T> batch)
    {
        await _writer.WriteAsync(string.Join('\n', batch.Select(x => x.ToString())) + '\n');
    }

    public async ValueTask DisposeAsync()
    {
        await _writer.FlushAsync();
        await _writer.DisposeAsync();
    }
}