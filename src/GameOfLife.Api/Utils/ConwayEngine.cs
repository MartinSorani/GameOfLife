namespace GameOfLife.Api.Utils
{
    public static class ConwayEngine
    {
        public static bool[,] GetNextGeneration(bool[,] currentState)
        {
            if (currentState == null)
            {
                throw new ArgumentNullException(nameof(currentState));
            }

            int rows = currentState.GetLength(0);
            int cols = currentState.GetLength(1);
            bool[,] nextState = new bool[rows, cols];

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

            return nextState;
        }

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
