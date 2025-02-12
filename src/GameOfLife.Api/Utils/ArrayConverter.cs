using Serilog;

namespace GameOfLife.Api.Utils
{
    public static class ArrayConverter
    {
        /// <summary>
        /// Converts a jagged bool array to a 2D bool array.
        /// Throws an exception if the jagged array is irregular.
        /// </summary>
        /// <param name="jaggedArray">The jagged array to convert.</param>
        /// <returns>A 2D bool array.</returns>
        public static bool[,] ConvertTo2DArray(bool[][] jaggedArray)
        {
            if (jaggedArray.Length == 0)
            {
                Log.Warning("The provided jagged array is empty.");
                return new bool[0, 0];
            }

            int rows = jaggedArray.Length;
            int cols = jaggedArray[0].Length;
            var result = new bool[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                if (jaggedArray[i].Length != cols)
                {
                    Log.Error("Jagged array is irregular at row {Row}.", i);
                    throw new ArgumentException("All rows must have the same number of columns.");
                }
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = jaggedArray[i][j];
                }
            }

            Log.Information("Successfully converted jagged array to 2D array.");
            return result;
        }

        /// <summary>
        /// Converts a 2D array into a jagged array.
        /// </summary>
        public static bool[][] ConvertToJaggedArray(bool[,] twoDArray)
        {
            int rows = twoDArray.GetLength(0);
            int cols = twoDArray.GetLength(1);
            bool[][] jagged = new bool[rows][];

            for (int i = 0; i < rows; i++)
            {
                jagged[i] = new bool[cols];
                for (int j = 0; j < cols; j++)
                {
                    jagged[i][j] = twoDArray[i, j];
                }
            }

            Log.Information("Successfully converted 2D array to jagged array.");
            return jagged;
        }
    }
}
