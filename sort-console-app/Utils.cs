using System;

public static class Utils
{
    public const int ChunkSize = 2_000_000;
    public static void LogMemoryUsage(string phase)
    {
        long memoryUsed = GC.GetTotalMemory(false);
        Console.WriteLine($"{phase}: Memory used: {memoryUsed / (1024 * 1024)} MB");
    }

    public static bool IsValidFormat(string line) => line.Contains(". ");

    public static int CompareLines(string line1, string line2)
    {
        var splitA = line1.Split(new[] { ". " }, 2, StringSplitOptions.None);
        var splitB = line2.Split(new[] { ". " }, 2, StringSplitOptions.None);

        int numberA = int.TryParse(splitA[0], out int numA) ? numA : 0;
        int numberB = int.TryParse(splitB[0], out int numB) ? numB : 0;

        string textA = splitA.Length > 1 ? splitA[1] : "";
        string textB = splitB.Length > 1 ? splitB[1] : "";

        int cmp = string.Compare(textA, textB, StringComparison.Ordinal);
        return cmp != 0 ? cmp : numberA.CompareTo(numberB);
    }
}
