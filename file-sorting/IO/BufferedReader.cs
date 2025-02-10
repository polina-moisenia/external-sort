namespace FileSorting.IO;

public class BufferedReader : StreamReader
{
    public BufferedReader(string filePath, int bufferSize)
        : base(new BufferedStream(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous), bufferSize))
    { }
}

