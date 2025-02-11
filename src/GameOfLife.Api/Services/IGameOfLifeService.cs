namespace GameOfLifeAPI.Services
{
    public interface IGameOfLifeService
    {
        Task<Guid> CreateBoardAsync(bool[][] initialState);
        Task<bool[][]> GetNextStateAsync(Guid boardId);
        Task<bool[][]> GetFutureStateAsync(Guid boardId, int generations);
        Task<bool[][]> GetFinalStateAsync(Guid boardId, int maxAttempts);
    }
}