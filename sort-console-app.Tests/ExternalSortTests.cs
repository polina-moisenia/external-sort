using System;
using System.IO;
using Xunit;

public class ExternalSortTests
{
    private static readonly string TestFilesDirectory = @"C:\Proj\external-sort\TestFiles";

    static ExternalSortTests()
    {
        Directory.CreateDirectory(TestFilesDirectory);
    }

    [Fact]
    public void TestRunExternalSort_InputFileDoesNotExist()
    {
        string inputFile = Path.Combine(TestFilesDirectory, "nonexistent.txt");
        string outputFile = Path.Combine(TestFilesDirectory, "output.txt");

        using (var sw = new StringWriter())
        {
            Console.SetOut(sw);

            ExternalSort.Run(inputFile, outputFile, 1000, true);

            var result = sw.ToString().Trim();
            Assert.Equal($"Error: Input file '{inputFile}' does not exist.", result);
        }
    }

    [Fact]
    public void TestRunExternalSort_ValidInputFile()
    {
        string inputFile = Path.Combine(TestFilesDirectory, "input.txt");
        string outputFile = Path.Combine(TestFilesDirectory, "output.txt");

        File.WriteAllLines(inputFile, new[] { "3. Simple", "1. Line", "2. Of code" });

        var originalOut = Console.Out;
        try
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                Console.WriteLine($"Input file path: {Path.GetFullPath(inputFile)}");
                Console.WriteLine($"Output file path: {Path.GetFullPath(outputFile)}");

                ExternalSort.Run(inputFile, outputFile, 1000, true);

                bool outputFileExists = File.Exists(outputFile);
                Console.WriteLine($"Output file exists: {outputFileExists}");
                Assert.True(outputFileExists, "Output file was not created.");

                var outputLines = File.ReadAllLines(outputFile);
                Assert.Equal(new[] { "1. Line", "2. Of code", "3. Simple" }, outputLines);

                var result = sw.ToString().Trim();
                Assert.Contains($"Sorted file created: {outputFile}", result);
            }
        }
        finally
        {
            Console.SetOut(originalOut);
            // Clean up
            if (File.Exists(inputFile)) File.Delete(inputFile);
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }

    [Fact]
    public void TestRunExternalSort_1GBFile()
    {
        string inputFile = Path.Combine(TestFilesDirectory, "input.txt");
        string outputFile = Path.Combine(TestFilesDirectory, "output.txt");

        using (var writer = new StreamWriter(inputFile))
        {
            Random random = new Random();
            for (int i = 0; i < 5000000; i++)
            {
                writer.WriteLine($"{random.Next(10000)}. {RandomStringGenerator.Generate(100)}");
            }
        }

        var originalOut = Console.Out;
        try
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                Console.WriteLine($"Input file path: {Path.GetFullPath(inputFile)}");
                Console.WriteLine($"Output file path: {Path.GetFullPath(outputFile)}");

                ExternalSort.Run(inputFile, outputFile, 1000000, true);

                bool outputFileExists = File.Exists(outputFile);
                Console.WriteLine($"Output file exists: {outputFileExists}");
                Assert.True(outputFileExists, "Output file was not created.");

                var result = sw.ToString().Trim();
                Assert.Contains($"Sorted file created: {outputFile}", result);
            }
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}