using GameOfLife.Api.Models;
using GameOfLife.Api.Utils;
using Newtonsoft.Json;

namespace GameOfLife.Api.Repositories
{
    public class FileBoardRepository : IBoardRepository
    {
        private readonly string _filePath;
        private Dictionary<Guid, Board> _boards;
        private readonly object _lock = new();
        private readonly ILogger _logger;

        public FileBoardRepository(string filePath = "boards.json", ILogger? logger = null)
        {
            _filePath = filePath;
            _logger = logger ?? new FileLogger("FileBoardRepository", "log.txt");
            if (File.Exists(_filePath))
            {
                // Read and deserialize existing boards from file.
                string json = File.ReadAllText(_filePath);
                _boards = JsonConvert.DeserializeObject<Dictionary<Guid, Board>>(json)
                          ?? new Dictionary<Guid, Board>();
                _logger.Log(LogLevel.Information, new EventId(), $"Loaded boards from {_filePath}", null, (state, ex) => state.ToString());
            }
            else
            {
                _boards = new Dictionary<Guid, Board>();
                _logger.Log(LogLevel.Information, new EventId(), "No existing boards found, starting with an empty collection.", null, (state, ex) => state.ToString());
            }
        }

        public void AddBoard(Board board)
        {
            lock (_lock)
            {
                _boards[board.Id] = board;
                Save();
                _logger.Log(LogLevel.Information, new EventId(), $"Added board with ID {board.Id}", null, (state, ex) => state.ToString());
            }
        }

        public Board GetBoard(Guid boardId)
        {
            lock (_lock)
            {
                if (_boards.TryGetValue(boardId, out var board))
                {
                    _logger.Log(LogLevel.Information, new EventId(), $"Retrieved board with ID {boardId}", null, (state, ex) => state.ToString());
                    return board;
                }
                else
                {
                    _logger.Log(LogLevel.Warning, new EventId(), $"Board with ID {boardId} not found", null, (state, ex) => state.ToString());
                    throw new KeyNotFoundException($"Board with ID {boardId} not found.");
                }
            }
        }

        public void UpdateBoard(Board board)
        {
            lock (_lock)
            {
                _boards[board.Id] = board;
                Save();
                _logger.Log(LogLevel.Information, new EventId(), $"Updated board with ID {board.Id}", null, (state, ex) => state.ToString());
            }
        }

        private void Save()
        {
            // Serialize the dictionary and write it to the file.
            string json = JsonConvert.SerializeObject(_boards, Formatting.Indented);
            File.WriteAllText(_filePath, json);
            _logger.Log(LogLevel.Information, new EventId(), $"Saved boards to {_filePath}", null, (state, ex) => state.ToString());
        }
    }
}
