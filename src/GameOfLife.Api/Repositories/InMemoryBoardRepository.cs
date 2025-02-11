using GameOfLifeAPI.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace GameOfLifeAPI.Repositories
{
    public class InMemoryBoardRepository : IBoardRepository
    {
        private readonly ConcurrentDictionary<Guid, Board> _boards;
        private const string DataFile = "boards.json";
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public InMemoryBoardRepository()
        {
            _boards = LoadBoards();
        }

        public Task<Guid> AddBoardAsync(bool[][] initialState)
        {
            var board = new Board(initialState);
            _boards[board.Id] = board;
            return Task.FromResult(board.Id);
        }

        public async Task<Board> GetBoardAsync(Guid boardId)
        {
            _boards.TryGetValue(boardId, out var board);
            return await Task.FromResult(board ?? throw new KeyNotFoundException($"Board with ID {boardId} not found."));
        }

        public Task UpdateBoardAsync(Board board)
        {
            _boards[board.Id] = board;
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_boards, options);
                await File.WriteAllTextAsync(DataFile, json);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private ConcurrentDictionary<Guid, Board> LoadBoards()
        {
            if (!File.Exists(DataFile))
            {
                return new ConcurrentDictionary<Guid, Board>();
            }

            var json = File.ReadAllText(DataFile);
            var boards = JsonSerializer.Deserialize<ConcurrentDictionary<Guid, Board>>(json);
            return boards ?? new ConcurrentDictionary<Guid, Board>();
        }
    }
}