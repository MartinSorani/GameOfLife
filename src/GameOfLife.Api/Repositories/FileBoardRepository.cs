using GameOfLife.Api.Models;
using Newtonsoft.Json;
using Serilog;

namespace GameOfLife.Api.Repositories
{
    public class FileBoardRepository : IBoardRepository
    {
        private readonly string _filePath;
        private Dictionary<Guid, Board> _boards;
        private readonly object _lock = new();

        public FileBoardRepository(string filePath = "boards.json")
        {
            _filePath = filePath;
            if (File.Exists(_filePath))
            {
                // Read and deserialize existing boards from file.
                string json = File.ReadAllText(_filePath);
                _boards = JsonConvert.DeserializeObject<Dictionary<Guid, Board>>(json)
                          ?? new Dictionary<Guid, Board>();
                Log.Information("Loaded boards from {FilePath}", _filePath);
            }
            else
            {
                _boards = new Dictionary<Guid, Board>();
                Log.Information("No existing boards found, starting with an empty collection.");
            }
        }

        public void AddBoard(Board board)
        {
            lock (_lock)
            {
                _boards[board.Id] = board;
                Save();
                Log.Information("Added board with ID {BoardId}", board.Id);
            }
        }

        public Board GetBoard(Guid boardId)
        {
            lock (_lock)
            {
                if (_boards.TryGetValue(boardId, out var board))
                {
                    Log.Information("Retrieved board with ID {BoardId}", boardId);
                    return board;
                }
                else
                {
                    Log.Warning("Board with ID {BoardId} not found", boardId);
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
                Log.Information("Updated board with ID {BoardId}", board.Id);
            }
        }

        private void Save()
        {
            // Serialize the dictionary and write it to the file.
            string json = JsonConvert.SerializeObject(_boards, Formatting.Indented);
            File.WriteAllText(_filePath, json);
            Log.Information("Saved boards to {FilePath}", _filePath);
        }
    }
}
