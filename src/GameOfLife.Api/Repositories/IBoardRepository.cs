using GameOfLife.Api.Models;

namespace GameOfLife.Api.Repositories
{
    public interface IBoardRepository
    {
        void AddBoard(Board board);
        Board GetBoard(Guid boardId);
        void UpdateBoard(Board board);
    }
}
