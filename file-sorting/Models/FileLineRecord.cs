namespace FileSorting.IO;

public struct FileLineRecord : IComparable<FileLineRecord>
{
    public int? Number { get; }
    public string? Text { get; }

    public FileLineRecord(int? number, string? text)
    {
        Number = number;
        Text = text;
    }

    public int CompareTo(FileLineRecord other)
    {
        int cmp = CompareNullableStrings(Text, other.Text);
        if (cmp != 0) return cmp;
        return CompareNullableInts(Number, other.Number);
    }

    private static int CompareNullableStrings(string? a, string? b)
    {
        if (a is null && b is null) return 0;
        if (a is null) return -1;
        if (b is null) return 1;
        return string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);
    }

    private static int CompareNullableInts(int? a, int? b)
    {
        if (a is null && b is null) return 0;
        if (a is null) return -1;
        if (b is null) return 1;
        return a.Value.CompareTo(b.Value);
    }

    public override string ToString() => $"{Number?.ToString() ?? ""}. {Text ?? ""}";

    public static bool TryParse(string line, out FileLineRecord record)
    {
        record = default;
        if (line is null) return false;
        
        int dotIndex = line.IndexOf('.');
        if (dotIndex < 0)
            return false;

        string numberPart = line[..dotIndex].Trim();
        string textPart = line[(dotIndex + 1)..].Trim();

        int? number = null;
        if (!string.IsNullOrEmpty(numberPart))
        {
            if (int.TryParse(numberPart, out int n))
                number = n;
            else
                return false;
        }

        string? text = string.IsNullOrEmpty(textPart) ? null : textPart;

        record = new FileLineRecord(number, text);

        return true;
    }
}

public class FileLineRecordComparer : IComparer<FileLineRecord>
{
    public int Compare(FileLineRecord x, FileLineRecord y) => x.CompareTo(y);
}
