using GameOfLife.Api.Utils;

namespace GameOfLife.UnitTests
{
    public class ConwayEngineTests
    {
        // Helper method to compare two 2D boolean arrays.
        private void AssertBoardsEqual(bool[,] expected, bool[,] actual)
        {
            int expectedRows = expected.GetLength(0);
            int expectedCols = expected.GetLength(1);
            int actualRows = actual.GetLength(0);
            int actualCols = actual.GetLength(1);

            Assert.Equal(expectedRows, actualRows);
            Assert.Equal(expectedCols, actualCols);

            for (int i = 0; i < expectedRows; i++)
            {
                for (int j = 0; j < expectedCols; j++)
                {
                    Assert.Equal(expected[i, j], actual[i, j]);
                }
            }
        }

        [Fact]
        public void GetNextGeneration_EmptyBoard_RemainsEmpty()
        {
            // Arrange: an empty board (all cells dead)
            bool[,] emptyBoard = new bool[3, 3];

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(emptyBoard);

            // Assert: Expect the board to remain empty
            bool[,] expectedBoard = new bool[3, 3];
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        [Fact]
        public void GetNextGeneration_BlockStillLife_RemainsUnchanged()
        {
            // Arrange: a block still life pattern (2x2 block in a 4x4 grid)
            bool[,] initialBoard = new bool[4, 4]
            {
                { false, false, false, false },
                { false, true,  true,  false },
                { false, true,  true,  false },
                { false, false, false, false }
            };

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);

            // Assert: The block should remain unchanged
            AssertBoardsEqual(initialBoard, nextBoard);
        }

        [Fact]
        public void GetNextGeneration_BlinkerOscillator_TogglesCorrectly()
        {
            // Arrange: a horizontal blinker in a 5x5 grid
            bool[,] initialBoard = new bool[5, 5]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, true,  true,  true,  false },
                { false, false, false, false, false },
                { false, false, false, false, false }
            };

            // Expected: The blinker should flip to a vertical orientation
            bool[,] expectedBoard = new bool[5, 5]
            {
                { false, false, false, false, false },
                { false, false, true,  false, false },
                { false, false, true,  false, false },
                { false, false, true,  false, false },
                { false, false, false, false, false }
            };

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);

            // Assert
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        [Fact]
        public void GetNextGeneration_SingleLiveCell_Dies()
        {
            // Arrange: a board with a single live cell
            bool[,] initialBoard = new bool[3, 3]
            {
                { false, false, false },
                { false, true,  false },
                { false, false, false }
            };

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);

            // Assert: A lone cell should die due to underpopulation
            bool[,] expectedBoard = new bool[3, 3]; // all cells false
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        [Fact]
        public void GetNextGeneration_FullBoard_ProducesCorrectPattern()
        {
            // Arrange: a 3x3 full board (all cells alive)
            bool[,] initialBoard = new bool[3, 3]
            {
                { true, true, true },
                { true, true, true },
                { true, true, true }
            };

            // Calculation for a 3x3 full board:
            // Corners (e.g., (0,0)): 3 neighbors  => lives.
            // Edges (e.g., (0,1)): 5 neighbors     => dies.
            // Center (1,1)): 8 neighbors           => dies.
            bool[,] expectedBoard = new bool[3, 3]
            {
                { true,  false, true },
                { false, false, false },
                { true,  false, true }
            };

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);

            // Assert
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        [Fact]
        public void GetNextGeneration_OneRowBoard_WorksCorrectly()
        {
            // Arrange: a board with a single row
            bool[,] initialBoard = new bool[1, 5]
            {
                { false, true, true, true, false }
            };

            // In a one-row board, only horizontal neighbors count.
            // For index 1: live with one live neighbor (index 2) -> dies.
            // For index 2: live with two neighbors -> survives.
            // For index 3: live with one neighbor -> dies.
            bool[,] expectedBoard = new bool[1, 5]
            {
                { false, false, true, false, false }
            };

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);

            // Assert
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        [Fact]
        public void GetNextGeneration_OneColumnBoard_WorksCorrectly()
        {
            // Arrange: a board with a single column
            bool[,] initialBoard = new bool[5, 1]
            {
                { false },
                { true },
                { true },
                { true },
                { false }
            };

            // Expected behavior is similar to the one-row case.
            bool[,] expectedBoard = new bool[5, 1]
            {
                { false },
                { false },
                { true },
                { false },
                { false }
            };

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);

            // Assert
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        [Fact]
        public void GetNextGeneration_NullBoard_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert: Passing a null board should throw an exception.
            Assert.Throws<ArgumentNullException>(() => ConwayEngine.GetNextGeneration(null!));
        }

        [Fact]
        public void GetNextGeneration_GliderPattern_EvolvesCorrectly()
        {
            // Arrange:
            // Create a 7x7 board and place a glider pattern with an offset.
            bool[,] initialBoard = new bool[7, 7];
            int offset = 1; // offset to place the glider away from edges

            // Place initial glider pattern.
            initialBoard[offset + 0, offset + 1] = true;
            initialBoard[offset + 1, offset + 2] = true;
            initialBoard[offset + 2, offset + 0] = true;
            initialBoard[offset + 2, offset + 1] = true;
            initialBoard[offset + 2, offset + 2] = true;

            // Build expected board for next generation.
            bool[,] expectedBoard = new bool[7, 7];
            expectedBoard[offset + 1, offset + 0] = true;
            expectedBoard[offset + 1, offset + 2] = true;
            expectedBoard[offset + 2, offset + 1] = true;
            expectedBoard[offset + 2, offset + 2] = true;
            expectedBoard[offset + 3, offset + 1] = true; // (4,2)

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);

            // Assert
            AssertBoardsEqual(expectedBoard, nextBoard);
        }


        [Fact]
        public void GetNextGeneration_7x7BlockStillLife_RemainsUnchanged()
        {
            // Arrange:
            // Create a 7x7 board with a 2x2 block (stable pattern) in the center.
            // A 2x2 block should remain unchanged from generation to generation.
            bool[,] initialBoard = new bool[7, 7];
            initialBoard[3, 3] = true;
            initialBoard[3, 4] = true;
            initialBoard[4, 3] = true;
            initialBoard[4, 4] = true;

            // The expected board is identical to the initial board.
            bool[,] expectedBoard = new bool[7, 7];
            expectedBoard[3, 3] = true;
            expectedBoard[3, 4] = true;
            expectedBoard[4, 3] = true;
            expectedBoard[4, 4] = true;

            // Act
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);

            // Assert
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        [Fact]
        public void GetNextGeneration_7x7GliderEvolvesCorrectlyOverMultipleGenerations()
        {
            // Arrange:
            // Create a 7x7 board with a glider pattern placed with an offset.
            // After 4 generations, a glider shifts one cell diagonally.
            bool[,] board = new bool[7, 7];
            int offset = 1;

            // Place initial glider pattern.
            board[offset + 0, offset + 1] = true;
            board[offset + 1, offset + 2] = true;
            board[offset + 2, offset + 0] = true;
            board[offset + 2, offset + 1] = true;
            board[offset + 2, offset + 2] = true;

            // Act: Evolve the board for 4 generations.
            bool[,] current = board;
            for (int gen = 0; gen < 4; gen++)
            {
                current = ConwayEngine.GetNextGeneration(current);
            }

            // Expected board after 4 generations (shift by (1,1) relative to initial pattern):
            bool[,] expectedBoard = new bool[7, 7];
            expectedBoard[offset + 0 + 1, offset + 1 + 1] = true;
            expectedBoard[offset + 1 + 1, offset + 2 + 1] = true;
            expectedBoard[offset + 2 + 1, offset + 0 + 1] = true;
            expectedBoard[offset + 2 + 1, offset + 1 + 1] = true;
            expectedBoard[offset + 2 + 1, offset + 2 + 1] = true;

            // Act and assert:
            AssertBoardsEqual(expectedBoard, current);
        }

    }
}
