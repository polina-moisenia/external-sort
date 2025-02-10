namespace FileSorting.IO;

public sealed class BufferedWriter : StreamWriter, IAsyncDisposable
{
    public BufferedWriter(string filePath, int bufferSize)
        : base(new BufferedStream(
                   new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous),
                   bufferSize))
    {
    }

    public async Task WriteLinesAsync<T>(IEnumerable<T> lines)
    {
        foreach (var line in lines)
        {
            await WriteLineAsync(line.ToString()).ConfigureAwait(false);
        }
        await FlushAsync().ConfigureAwait(false);
    }

    public new async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
