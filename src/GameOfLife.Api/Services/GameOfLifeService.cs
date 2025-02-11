using GameOfLifeAPI.Repositories;
using GameOfLifeAPI.Services;
using GameOfLifeAPI.Utils;

namespace GameOfLife.Api.Services
{
    public class GameOfLifeService : IGameOfLifeService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly ILogger<GameOfLifeService> _logger;

        public GameOfLifeService(IBoardRepository boardRepository, ILogger<GameOfLifeService> logger)
        {
            _boardRepository = boardRepository;
            _logger = logger;
        }

        public async Task<Guid> CreateBoardAsync(bool[][] initialState)
        {
            _logger.LogInformation("Creating a new board.");

            // Validate minimum board size
            if (initialState.Length < 3 || initialState.Any(row => row.Length < 3))
            {
                _logger.LogWarning("Board size is too small. Minimum size is 3x3.");
                throw new ArgumentException("Board size is too small. Minimum size is 3x3.");
            }

            var boardId = await _boardRepository.AddBoardAsync(initialState);
            await _boardRepository.SaveAsync();
            _logger.LogInformation($"Board {boardId} created.");
            return boardId;
        }

        public async Task<bool[][]> GetNextStateAsync(Guid boardId)
        {
            _logger.LogInformation($"Getting next state for board {boardId}.");
            var board = await _boardRepository.GetBoardAsync(boardId);
            if (board == null)
            {
                _logger.LogWarning("Board {BoardId} not found.", boardId);
                throw new Exception("Board not found.");
            }

            var nextState = ConwayEngine.GetNextGeneration(board.CurrentState);
            board.CurrentState = nextState;
            await _boardRepository.UpdateBoardAsync(board);
            await _boardRepository.SaveAsync();
            _logger.LogInformation($"Next state for board {boardId} calculated and saved.");

            return nextState;
        }

        public async Task<bool[][]> GetFutureStateAsync(Guid boardId, int generations)
        {
            _logger.LogInformation($"Getting future state for board {boardId} for {generations} generations.");
            var board = await _boardRepository.GetBoardAsync(boardId);
            if (board == null)
            {
                _logger.LogWarning("Board {BoardId} not found.", boardId);
                throw new Exception("Board not found.");
            }

            var currentState = board.CurrentState;
            for (int i = 0; i < generations; i++)
            {
                currentState = ConwayEngine.GetNextGeneration(currentState);
            }
            board.CurrentState = currentState;
            await _boardRepository.UpdateBoardAsync(board);
            await _boardRepository.SaveAsync();
            _logger.LogInformation($"Future state for board {boardId} calculated and saved.");

            return currentState;
        }

        public async Task<bool[][]> GetFinalStateAsync(Guid boardId, int maxAttempts)
        {
            _logger.LogInformation($"Getting final state for board {boardId} with a maximum of {maxAttempts} attempts.");
            var board = await _boardRepository.GetBoardAsync(boardId);
            if (board == null)
            {
                _logger.LogWarning("Board {BoardId} not found.", boardId);
                throw new Exception("Board not found.");
            }

            var previousState = board.CurrentState;
            for (int i = 0; i < maxAttempts; i++)
            {
                var nextState = ConwayEngine.GetNextGeneration(previousState);
                if (IsStatesEqual(previousState, nextState))
                {
                    board.CurrentState = nextState;
                    await _boardRepository.UpdateBoardAsync(board);
                    await _boardRepository.SaveAsync();
                    _logger.LogInformation($"Final state for board {boardId} reached and saved.");

                    return nextState;
                }
                previousState = nextState;
            }
            _logger.LogWarning($"Board {boardId} did not reach a stable state within the maximum number of attempts.");
            throw new Exception("Board did not reach a stable state within the maximum number of attempts.");
        }

        private bool IsStatesEqual(bool[][] state1, bool[][] state2)
        {
            for (int i = 0; i < state1.Length; i++)
            {
                for (int j = 0; j < state1[i].Length; j++)
                {
                    if (state1[i][j] != state2[i][j])
                        return false;
                }
            }
            return true;
        }
    }
}
