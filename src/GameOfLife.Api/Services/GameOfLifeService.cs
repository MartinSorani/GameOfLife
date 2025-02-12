using GameOfLife.Api.Models;
using GameOfLife.Api.Repositories;
using GameOfLife.Api.Utils;

namespace GameOfLife.Api.Services
{
    /// <summary>
    /// Provides services for managing and simulating the Game of Life.
    /// </summary>
    public class GameOfLifeService : IGameOfLifeService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly Serilog.ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameOfLifeService"/> class.
        /// </summary>
        /// <param name="boardRepository">The repository for managing game boards.</param>
        /// <param name="logger">The logger instance.</param>
        public GameOfLifeService(IBoardRepository boardRepository, Serilog.ILogger logger)
        {
            _boardRepository = boardRepository;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a new board and returns its unique identifier.
        /// </summary>
        /// <param name="board">The initial state of the game board.</param>
        /// <returns>The unique identifier of the created board.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the board is null.</exception>
        public Guid UploadBoard(bool[,] board)
        {
            if (board == null)
            {
                _logger.Error("UploadBoard: Board is null");
                throw new ArgumentNullException(nameof(board));
            }

            var newBoard = new Board { Id = Guid.NewGuid(), CurrentState = board };
            _boardRepository.AddBoard(newBoard);
            _logger.Information("UploadBoard: Board uploaded with ID {BoardId}", newBoard.Id);
            return newBoard.Id;
        }

        /// <summary>
        /// Computes and returns the next state for the board.
        /// </summary>
        /// <param name="boardId">The unique identifier of the board.</param>
        /// <returns>The next state of the board.</returns>
        /// <exception cref="ArgumentException">Thrown when the board is not found.</exception>
        public bool[,] GetNextState(Guid boardId)
        {
            Board board;
            try
            {
                board = _boardRepository.GetBoard(boardId);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Warning(ex, "GetNextState: Board with ID {BoardId} not found", boardId);
                throw new ArgumentException("Board not found", nameof(boardId));
            }

            var nextState = ConwayEngine.GetNextGeneration(board.CurrentState);
            board.CurrentState = nextState;
            _boardRepository.UpdateBoard(board);
            _logger.Information("GetNextState: Next state computed for board ID {BoardId}", boardId);
            return nextState;
        }

        /// <summary>
        /// Computes and returns the state after a specified number of generations.
        /// </summary>
        /// <param name="boardId">The unique identifier of the board.</param>
        /// <param name="steps">The number of generations to simulate.</param>
        /// <returns>The state of the board after the specified number of generations.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when steps is negative.</exception>
        /// <exception cref="ArgumentException">Thrown when the board is not found.</exception>
        public bool[,] GetStateAfterSteps(Guid boardId, int steps)
        {
            if (steps < 0)
            {
                _logger.Error("GetStateAfterSteps: Steps must be non-negative");
                throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be non-negative");
            }

            Board board;
            try
            {
                board = _boardRepository.GetBoard(boardId);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Warning(ex, "GetStateAfterSteps: Board with ID {BoardId} not found", boardId);
                throw new ArgumentException("Board not found", nameof(boardId));
            }

            bool[,] state = board.CurrentState;
            for (int i = 0; i < steps; i++)
            {
                state = ConwayEngine.GetNextGeneration(state);
            }

            board.CurrentState = state;
            _boardRepository.UpdateBoard(board);
            _logger.Information("GetStateAfterSteps: State after {Steps} steps computed for board ID {BoardId}", steps, boardId);
            return state;
        }

        /// <summary>
        /// Computes and returns the final state of the board.
        /// Throws an exception if a stable state is not reached within maxIterations.
        /// </summary>
        /// <param name="boardId">The unique identifier of the board.</param>
        /// <param name="maxIterations">The maximum number of iterations to simulate.</param>
        /// <returns>The final state of the board if stable.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when maxIterations is not positive.</exception>
        /// <exception cref="ArgumentException">Thrown when the board is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a stable state is not reached within the maximum iterations.</exception>
        public bool[,] GetFinalState(Guid boardId, int maxIterations)
        {
            if (maxIterations <= 0)
            {
                _logger.Error("GetFinalState: Max iterations must be positive");
                throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive");
            }

            Board board;
            try
            {
                board = _boardRepository.GetBoard(boardId);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Warning(ex, "GetFinalState: Board with ID {BoardId} not found", boardId);
                throw new ArgumentException("Board not found", nameof(boardId));
            }

            bool[,] currentState = board.CurrentState;
            int iterations = 0;

            while (iterations < maxIterations)
            {
                bool[,] nextState = ConwayEngine.GetNextGeneration(currentState);
                if (AreBoardsEqual(currentState, nextState))
                {
                    board.CurrentState = nextState;
                    _boardRepository.UpdateBoard(board);
                    _logger.Information("GetFinalState: Final state reached for board ID {BoardId} after {Iterations} iterations", boardId, iterations);
                    return nextState;
                }
                currentState = nextState;
                iterations++;
            }

            _logger.Error("GetFinalState: Final state not reached within {MaxIterations} iterations for board ID {BoardId}", maxIterations, boardId);
            throw new InvalidOperationException("Final state not reached within maximum iterations");
        }

        /// <summary>
        /// Helper method to compare two boards for equality.
        /// </summary>
        /// <param name="board1">The first board to compare.</param>
        /// <param name="board2">The second board to compare.</param>
        /// <returns>True if the boards are equal; otherwise, false.</returns>
        private bool AreBoardsEqual(bool[,] board1, bool[,] board2)
        {
            int rows = board1.GetLength(0);
            int cols = board1.GetLength(1);
            if (board2.GetLength(0) != rows || board2.GetLength(1) != cols)
                return false;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (board1[i, j] != board2[i, j])
                        return false;
                }
            }
            return true;
        }
    }
}
