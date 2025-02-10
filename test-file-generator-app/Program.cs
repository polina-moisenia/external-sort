using TestFileGeneratorLib;

namespace TestFileGeneratorApp;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: test-file-generator-app <filename> <size_in_bytes>");
            return;
        }

        string fileName = args[0];

        if (!long.TryParse(args[1], out long targetSize))
        {
            Console.WriteLine("Invalid size provided. Please provide a numeric value for size in bytes.");
            return;
        }

        try
        {
            string fullPath = Path.GetFullPath(fileName);
            string driveRoot = Path.GetPathRoot(fullPath);
            DriveInfo driveInfo = new DriveInfo(driveRoot);
            if (driveInfo.AvailableFreeSpace < targetSize)
            {
                Console.WriteLine($"Not enough free space on drive {driveRoot}.\nAvailable: {driveInfo.AvailableFreeSpace} bytes, required: {targetSize} bytes.");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error checking disk space: " + ex.Message);
            return;
        }

        try
        {
            var generator = new TestFileGenerator();
            Console.WriteLine($"Generating file '{fileName}' of approximately {targetSize} bytes...");
            generator.Generate(fileName, targetSize);
            Console.WriteLine("File generation completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while generating the file: " + ex.Message);
        }
    }
}