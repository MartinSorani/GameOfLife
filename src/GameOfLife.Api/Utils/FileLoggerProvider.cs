using System.Collections.Concurrent;

namespace GameOfLife.Api.Utils
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new ConcurrentDictionary<string, FileLogger>();

        public FileLoggerProvider(IConfiguration configuration)
        {
            var logFilePath = configuration.GetSection("Logging:File:Path")?.Value ?? "logs/GameOfLife.Logs.txt";
            // Create the logs directory in the solution root.
            var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            _filePath = Path.Combine(solutionRoot, logFilePath);
            EnsureDirectoryExists(_filePath);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(_filePath, categoryName));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        private void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
