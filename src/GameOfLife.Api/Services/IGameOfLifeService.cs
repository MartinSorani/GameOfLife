namespace GameOfLife.Api.Services
{
    public interface IGameOfLifeService
    {
        /// <summary>
        /// Uploads a new board and returns its unique identifier.
        /// </summary>
        Guid UploadBoard(bool[,] board);

        /// <summary>
        /// Computes and returns the next state for the board.
        /// </summary>
        bool[,] GetNextState(Guid boardId);

        /// <summary>
        /// Computes and returns the state after a specified number of generations.
        /// </summary>
        bool[,] GetStateAfterSteps(Guid boardId, int steps);

        /// <summary>
        /// Computes and returns the final state of the board.
        /// Throws an exception if a stable state is not reached within maxIterations.
        /// </summary>
        bool[,] GetFinalState(Guid boardId, int maxIterations);
    }
}
