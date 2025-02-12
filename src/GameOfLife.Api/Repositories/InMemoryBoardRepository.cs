using GameOfLife.Api.Models;
using System.Collections.Concurrent;

namespace GameOfLife.Api.Repositories
{
    public class InMemoryBoardRepository : IBoardRepository
    {
        // Use a thread-safe dictionary for simple in-memory storage.
        private readonly ConcurrentDictionary<Guid, Board> _boards = new();

        public void AddBoard(Board board)
        {
            _boards[board.Id] = board;
        }

        public Board GetBoard(Guid boardId)
        {
            _boards.TryGetValue(boardId, out var board);
            return board ?? throw new KeyNotFoundException($"Board with ID {boardId} not found.");
        }

        public void UpdateBoard(Board board)
        {
            _boards[board.Id] = board;
        }
    }
}
