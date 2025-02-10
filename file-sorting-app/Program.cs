using FileSorting;

namespace FileSortingApp;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: file-sorting-app <input_file> <output_file>");
            return;
        }

        string inputFile = args[0];
        string outputFile = args[1];

        await ExternalSort.Run(inputFile, outputFile);
    }
}
