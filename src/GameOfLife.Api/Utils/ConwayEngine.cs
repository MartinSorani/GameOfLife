namespace GameOfLifeAPI.Utils
{
    /// <summary>
    /// Provides functionality to compute the next generation in Conway's Game of Life.
    /// </summary>
    public static class ConwayEngine
    {
        /// <summary>
        /// Computes the next generation of the game board.
        /// </summary>
        /// <param name="currentState">The current state of the game board.</param>
        /// <param name="logger">An optional logger for tracing computations.</param>
        /// <returns>The next state of the game board.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the currentState is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the board is not rectangular.</exception>
        public static bool[][] GetNextGeneration(bool[][] currentState, ILogger? logger = null)
        {
            if (currentState == null)
                throw new ArgumentNullException(
                    nameof(currentState),
                    "The current state cannot be null."
                );

            if (currentState.Length == 0)
            {
                logger?.LogWarning("The board is empty. Returning the empty board as is.");
                return currentState; // Return empty board as is.
            }

            int rows = currentState.Length;
            int cols = currentState[0]?.Length ?? 0;

            if (cols == 0)
            {
                logger?.LogWarning("The board has no columns. Returning the board as is.");
                return currentState; // Return empty board as is.
            }

            // Ensure the board is rectangular
            for (int i = 1; i < rows; i++)
            {
                if (currentState[i].Length != cols)
                    throw new InvalidOperationException(
                        $"Row {i} length ({currentState[i].Length}) does not match the expected column count ({cols}). Board must be rectangular."
                    );
            }

            var nextState = new bool[rows][];
            for (int i = 0; i < rows; i++)
            {
                nextState[i] = new bool[cols];
                for (int j = 0; j < cols; j++)
                {
                    int aliveNeighbors = CountAliveNeighbors(currentState, i, j, rows, cols);
                    bool isAlive = currentState[i][j];

                    // Log current cell status
                    logger?.LogDebug(
                        $"Processing cell ({i},{j}): IsAlive={isAlive}, AliveNeighbors={aliveNeighbors}"
                    );

                    if (isAlive)
                    {
                        // Any live cell with two or three live neighbours survives.
                        nextState[i][j] = aliveNeighbors == 2 || aliveNeighbors == 3;
                        logger?.LogDebug(
                            $"Cell ({i},{j}) is alive with {aliveNeighbors} neighbors: {(nextState[i][j] ? "survives" : "dies")}."
                        );
                    }
                    else
                    {
                        // Any dead cell with exactly three live neighbours becomes a live cell.
                        nextState[i][j] = aliveNeighbors == 3;
                        logger?.LogDebug(
                            $"Cell ({i},{j}) is dead with {aliveNeighbors} neighbors: {(nextState[i][j] ? "becomes alive" : "stays dead")}."
                        );
                    }

                    // Log next cell status
                    logger?.LogDebug(
                        $"Cell ({i},{j}) will be {(nextState[i][j] ? "alive" : "dead")} in next generation."
                    );
                }
            }

            return nextState;
        }

        private static int CountAliveNeighbors(bool[][] board, int x, int y, int rows, int cols)
        {
            int count = 0;

            for (int i = x - 1; i <= x + 1; i++)
            {
                if (i < 0 || i >= rows)
                    continue;

                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (j < 0 || j >= cols)
                        continue;

                    if (i == x && j == y)
                        continue;

                    if (board[i][j])
                        count++;
                }
            }

            return count;
        }
    }
}
