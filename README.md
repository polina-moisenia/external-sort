# Sort Console App

This is a minimal .NET 6 console application.

## Getting Started

To build and run this application, follow these steps:

1. **Clone the repository** (if applicable):
   ```sh
   git clone <repository-url>
   cd sort-console-app
   ```

2. **Build the application**:
   ```sh
   dotnet build
   ```

3. **Run the application**:
   ```sh
   dotnet run -- <input_file> <output_file>
   ```

   Replace `<input_file>` with the name of the file you want to provide as an input parameter and `<output_file>` with the name of the file where you want to save the sorted output.

## Project Structure

- `Program.cs`: Entry point of the application.
- `sort-console-app.csproj`: Project configuration file.
- `QuickSorter.cs`: Implements the quick sort algorithm.
- `FileProcessor.cs`: Handles reading and writing files.
- `Utils.cs`: Contains utility functions.
- `MergeSorter.cs`: Merges sorted chunks into a single sorted file.
- `ChunkSorter.cs`: Splits the input file into chunks and sorts them.