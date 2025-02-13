namespace GameOfLife.Api.Utils
{
    /// <summary>
    /// Provides functionality to compute the next generation in Conway's Game of Life.
    /// </summary>
    public static class ConwayEngine
    {
        private static readonly FileLogger _logger = new FileLogger("log.txt", "ConwayEngine");

        /// <summary>
        /// Computes the next generation of the game board based on the current state.
        /// </summary>
        /// <param name="currentState">The current state of the game board as a 2D boolean array.</param>
        /// <returns>The next state of the game board as a 2D boolean array.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current state is null.</exception>
        /// <remarks>
        /// The method iterates through each cell in the current state and counts its live neighbors.
        /// Based on the number of live neighbors, the cell's state in the next generation is determined:
        /// - A live cell with 2 or 3 live neighbors stays alive; otherwise, it dies.
        /// - A dead cell with exactly 3 live neighbors becomes alive; otherwise, it stays dead.
        /// </remarks>
        public static bool[,] GetNextGeneration(bool[,] currentState)
        {
            if (currentState == null)
            {
                _logger.Log(LogLevel.Error, new EventId(), "GetNextGeneration called with null currentState", new ArgumentNullException(nameof(currentState)), (state, ex) => state.ToString());
                throw new ArgumentNullException(nameof(currentState));
            }

            int rows = currentState.GetLength(0);
            int cols = currentState.GetLength(1);
            bool[,] nextState = new bool[rows, cols];

            _logger.Log(LogLevel.Information, new EventId(), $"Computing next generation for a board of size {rows}x{cols}", null, (state, ex) => state.ToString());

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int liveNeighbors = CountLiveNeighbors(currentState, i, j);
                    if (currentState[i, j])
                    {
                        // Live cell: stays alive if 2 or 3 neighbors, otherwise dies
                        nextState[i, j] = liveNeighbors == 2 || liveNeighbors == 3;
                    }
                    else
                    {
                        // Dead cell: becomes alive if exactly 3 live neighbors
                        nextState[i, j] = liveNeighbors == 3;
                    }
                }
            }

            _logger.Log(LogLevel.Information, new EventId(), "Next generation computed successfully", null, (state, ex) => state.ToString());

            return nextState;
        }

        /// <summary>
        /// Counts the number of live neighbors for a given cell in the game board.
        /// </summary>
        /// <param name="board">The game board as a 2D boolean array.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="col">The column index of the cell.</param>
        /// <returns>The number of live neighbors.</returns>
        /// <remarks>
        /// The method checks the eight possible neighbors of the cell, ensuring that it does not go out of bounds.
        /// It skips the cell itself and counts only the live neighbors.
        /// </remarks>
        private static int CountLiveNeighbors(bool[,] board, int row, int col)
        {
            int count = 0;
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            for (int i = row - 1; i <= row + 1; i++)
            {
                for (int j = col - 1; j <= col + 1; j++)
                {
                    if (i == row && j == col)
                        continue; // Skip the cell itself

                    if (i >= 0 && i < rows && j >= 0 && j < cols && board[i, j])
                        count++;
                }
            }

            return count;
        }
    }
}
