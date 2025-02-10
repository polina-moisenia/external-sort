# External Sort Project

This project consists of two console applications:
1. **TestFileGeneratorApp** – generates a test file with numbers.
2. **FileSortingApp** – sorts the generated file.

## Requirements

- .NET SDK 9
- Windows / Linux / MacOS with a command-line interface

## Build the Project

Open a terminal in the root directory and run:
```sh
 dotnet build
```

## Generate a Test File

To create a test file with numbers, use:
```sh
 dotnet run --project test-file-generator-app <filename> <size_in_bytes> --configuration Release
```
Example:
```sh
 dotnet run --project test-file-generator-app data.txt 1000000 --configuration Release
```
This will generate a file `data.txt` of approximately 1MB in size.

## Run File Sorting

After generating the file, you can sort it using:
```sh
 dotnet run --project file-sorting-app <input_file> <output_file> --configuration Release
```
Example:
```sh
 dotnet run --project file-sorting-app data.txt sorted_data.txt --configuration Release
```
As a result, `sorted_data.txt` will contain the sorted data.
