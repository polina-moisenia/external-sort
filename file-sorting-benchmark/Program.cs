using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FileSorting; 

namespace FileSortingBenchmark
{
    [SimpleJob(warmupCount: 1, iterationCount: 5)]
   [MemoryDiagnoser]
    public class ChunkSorterPipelineBenchmark
    {
        private FileSortingConfiguration _config;
        private ChunkSorterPipeline _pipeline;
        private string _tempDir;    // Временная папка для текущей итерации
        private string _inputFile;  // Путь к копии тестового файла для текущей итерации

        // Путь к уже существующему тестовому файлу (укажите свой путь)
        private string _sourceInputFile = @"C:\Proj\external-sort\data_1gb.txt"; 

        /// <summary>
        /// Глобальная настройка, выполняющаяся один раз перед всеми итерациями.
        /// Здесь проверяем, что исходный тестовый файл существует.
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            if (!File.Exists(_sourceInputFile))
            {
                throw new FileNotFoundException("Исходный тестовый файл не найден.", _sourceInputFile);
            }
        }

        /// <summary>
        /// Настройка перед каждой итерацией бенчмарка.
        /// Создаётся новая временная папка, и в неё копируется исходный тестовый файл.
        /// </summary>
        [IterationSetup]
        public void Setup()
        {
            // Создаем уникальную временную папку для текущей итерации
            _tempDir = Path.Combine(Path.GetTempPath(), "FileSortingBenchmark", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            // Копируем исходный тестовый файл в созданную папку
            _inputFile = Path.Combine(_tempDir, "input.txt");
            File.Copy(_sourceInputFile, _inputFile, overwrite: true);

//             ChunkSize: ~500 000 строк (вместо 2 000 000, чтобы снизить пик памяти)
// BufferSize: 32 MB (32 × 1024 × 1024 байт)
// FlushThreshold (BufferedWriter): 32 KB
// MaxParallelSort: Environment.ProcessorCount
// MaxParallelWrite: 4 (или можно экспериментировать, например, 6–8 для SSD)
// MergeDegree: оставить как есть (например, 32)

            _config = new FileSortingConfiguration(_inputFile, "tmp", 500_000, 1024 * 1024 * 32, 10_000, 32);

            _pipeline = new ChunkSorterPipeline(_config);
        }

        [IterationCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Benchmark]
        public async Task Benchmark_SplitAndSortChunksAsync()
        {
            await _pipeline.SplitAndSortChunksAsync();
        }

        // [Benchmark]
        // public async Task Benchmark_KWayMerge()
        // {
        //     await _pipeline.SplitAndSortChunksAsync();
        // }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<ChunkSorterPipelineBenchmark>();
        }
    }
}
