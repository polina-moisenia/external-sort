using System.Text;

namespace TestFileGeneratorLib;

public class StringsGenerator
{
    private readonly Random _random = new();
    private static readonly string _wordsCorpus =
        "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua Ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur Excepteur sint occaecat cupidatat non proident sunt in culpa qui officia deserunt mollit anim id est laborum";
    private readonly string[] _corpusWords;
    private readonly string[] _sampleStrings;
    private readonly long _sampleStringsCount;
    private const int MaxSampleStringLength = 1024;
    private const int MinSampleStringLength = 10;

    public StringsGenerator(long fileSize)
    {
        _corpusWords = _wordsCorpus.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        _sampleStringsCount = fileSize > 100000 ? fileSize / 100000 : fileSize / 100;
        _sampleStrings = new string[_sampleStringsCount];

        for (int i = 0; i < _sampleStringsCount; i++)
        {
            _sampleStrings[i] = GenerateRandomSampleString();
        }
    }

    public string GenerateLine()
    {
        int number = _random.Next(1, 100000);
        string sample = _sampleStrings[_random.Next(_sampleStrings.Length)];
        return $"{number}. {sample}";
    }

    private string GenerateRandomSampleString()
    {
        var sb = new StringBuilder();
        bool first = true;
        while (true)
        {
            string word = _corpusWords[_random.Next(_corpusWords.Length)];
            int additionalLength = first ? word.Length : (1 + word.Length);

            if (sb.Length + additionalLength > MaxSampleStringLength)
            {
                if (first && word.Length > MaxSampleStringLength)
                {
                    sb.Append(word.Substring(0, MaxSampleStringLength));
                }
                break;
            }

            if (!first)
            {
                sb.Append(' ');
            }
            sb.Append(word);
            first = false;

            if (sb.Length >= MinSampleStringLength && _random.NextDouble() < 0.3)
            {
                break;
            }
        }
        return sb.ToString();
    }
}
