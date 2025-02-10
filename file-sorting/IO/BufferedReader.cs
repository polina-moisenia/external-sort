using System.Runtime.CompilerServices;
using System.Text;

namespace FileSorting.IO;

public class BufferedReader
{
    private readonly string _filePath;
    private readonly int _bufferSize;

    public BufferedReader(string filePath, int bufferSize)
    {
        _filePath = filePath;
        _bufferSize = bufferSize;
    }

    public async IAsyncEnumerable<FileLineRecord> ReadRecordsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize, FileOptions.Asynchronous);
        using var reader = new StreamReader(stream, Encoding.UTF8, true, _bufferSize);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
        {
            if (FileLineRecord.TryParse(line, out var record))
                yield return record;
        }
    }
}
