using System.Collections.Concurrent;

namespace GameOfLife.Api.Utils
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new ConcurrentDictionary<string, FileLogger>();

        public FileLoggerProvider(IConfiguration configuration)
        {
            _filePath = configuration.GetSection("Logging:File:Path").Value;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(_filePath));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
