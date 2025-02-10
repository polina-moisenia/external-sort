using System;
using System.IO;
using System.Text;

namespace TestFileGeneratorLib;

public class TestFileGenerator
{
    public void Generate(string fileName, long targetSizeBytes)
    {
        const int batchThreshold = 1024 * 1024;

        using FileStream stream = new FileStream(
            fileName,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 8192);
        using StreamWriter writer = new StreamWriter(stream, Encoding.ASCII);

        writer.NewLine = "\n";
        var stringsGenerator = new StringsGenerator(targetSizeBytes);

        long currentSize = 0;
        var batchBuilder = new StringBuilder(batchThreshold);

        while (currentSize < targetSizeBytes)
        {
            string line = stringsGenerator.GenerateLine();
            batchBuilder.AppendLine(line);

            if (batchBuilder.Length >= batchThreshold)
            {
                writer.Write(batchBuilder.ToString());
                currentSize += batchBuilder.Length;
                batchBuilder.Clear();
            }
        }

        if (batchBuilder.Length > 0)
        {
            writer.Write(batchBuilder.ToString());
            currentSize += batchBuilder.Length;
            batchBuilder.Clear();
        }
    }
}