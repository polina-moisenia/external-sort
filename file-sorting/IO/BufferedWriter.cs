using System.Text;

namespace FileSorting.IO;

public sealed class BufferedWriter : IAsyncDisposable
    {
        private readonly StreamWriter _writer;
        private readonly StringBuilder _sb;
        private readonly int _flushThreshold; 

        public BufferedWriter(string filePath, int bufferSize, int flushThreshold = 32 * 1024)
        {
            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous);
            _writer = new StreamWriter(new BufferedStream(fileStream, bufferSize), Encoding.UTF8, bufferSize);
            _sb = new StringBuilder();
            _flushThreshold = flushThreshold;
        }

        public async Task WriteLineAsync(string line)
        {
            _sb.AppendLine(line);
            if (_sb.Length >= _flushThreshold)
            {
                await FlushAsync().ConfigureAwait(false);
            }
        }

        public async Task FlushAsync()
        {
            if (_sb.Length > 0)
            {
                await _writer.WriteAsync(_sb.ToString()).ConfigureAwait(false);
                _sb.Clear();
            }
            await _writer.FlushAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await FlushAsync().ConfigureAwait(false);
            await _writer.DisposeAsync().ConfigureAwait(false);
        }
}
