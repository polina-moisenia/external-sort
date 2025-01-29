using System;
using System.Linq;

class RandomStringGenerator
{
    private static readonly Random _random = new Random();
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string Generate(int length = 16)
    {
        return new string(Enumerable.Range(0, length)
            .Select(_ => Chars[_random.Next(Chars.Length)]).ToArray());
    }
}