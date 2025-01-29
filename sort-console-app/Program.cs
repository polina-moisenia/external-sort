using System;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ExternalSortApp <input_file> <output_file>");
            return;
        }

        string inputFile = args[0];
        string outputFile = args[1];

        ExternalSort.Run(inputFile, outputFile, Utils.ChunkSize, true);
    }
}
