using System.Globalization;

namespace FileSorting.IO;

public readonly struct FileLineRecord : IComparable<FileLineRecord>
{
    public readonly int Number;
    public readonly string OriginalLine;
    public readonly int TextOffset;
    public readonly int TextLength;

    public FileLineRecord(int number, string originalLine, int textOffset, int textLength)
    {
        Number = number;
        OriginalLine = originalLine;
        TextOffset = textOffset;
        TextLength = textLength;
    }

    public int CompareTo(FileLineRecord other)
    {
        ReadOnlySpan<char> thisText = OriginalLine.AsSpan(TextOffset, TextLength);
        ReadOnlySpan<char> otherText = other.OriginalLine.AsSpan(other.TextOffset, other.TextLength);
        int cmp = thisText.CompareTo(otherText, StringComparison.OrdinalIgnoreCase);
        if (cmp != 0) return cmp;
        return Number.CompareTo(other.Number);
    }

    public override string ToString() => OriginalLine;

    public static bool TryParse(string line, out FileLineRecord record)
    {
        record = default;
        if (string.IsNullOrEmpty(line))
            return false;

        ReadOnlySpan<char> span = line.AsSpan();
        int dotIndex = span.IndexOf('.');
        if (dotIndex < 0)
            return false;

        ReadOnlySpan<char> numberSpan = span.Slice(0, dotIndex).Trim();
        if (!int.TryParse(numberSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
            return false;

        int start = dotIndex + 1;
        while (start < line.Length && char.IsWhiteSpace(line[start]))
            start++;
        int textLength = line.Length - start;

        if (textLength <= 0)
            return false;

        record = new FileLineRecord(number, line, start, textLength);
        return true;
    }
}

public class FileLineRecordComparer : IComparer<FileLineRecord>
{
    public int Compare(FileLineRecord x, FileLineRecord y)
    {
        return x.CompareTo(y);
    }
}