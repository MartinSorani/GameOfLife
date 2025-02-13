namespace GameOfLife.Api.Utils
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();

        public FileLogger(string filePath)
        {
            _filePath = filePath;

            // Ensure the file is overwritten when the logger is initialized.
            lock (_lock)
            {
                File.WriteAllText(_filePath, string.Empty);
            }
        }

        IDisposable? ILogger.BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message} {exception?.StackTrace}";
            lock (_lock)
            {
                File.AppendAllText(_filePath, logRecord + Environment.NewLine);
            }
        }
    }
}
