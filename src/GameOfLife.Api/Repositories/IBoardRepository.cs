using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameOfLifeAPI.Models;

namespace GameOfLifeAPI.Repositories
{
    public interface IBoardRepository
    {
        Task<Guid> AddBoardAsync(bool[][] initialState);
        Task<Board> GetBoardAsync(Guid boardId);
        Task UpdateBoardAsync(Board board);
        Task SaveAsync();
    }
}
