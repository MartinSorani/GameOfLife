using GameOfLife.Api.Utils;

namespace GameOfLife.UnitTests
{
    [Trait("Category", "Unit")]
    public class ConwayEngineTests
    {
        /// <summary>
        /// Helper method to compare two 2D boolean arrays.
        /// </summary>
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

        /// <summary>
        /// Tests that an empty board remains empty in the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_EmptyBoard_RemainsEmpty()
        {
            bool[,] emptyBoard = new bool[3, 3];
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(emptyBoard);
            bool[,] expectedBoard = new bool[3, 3];
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        /// <summary>
        /// Tests that a block still life pattern remains unchanged in the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_BlockStillLife_RemainsUnchanged()
        {
            bool[,] initialBoard = new bool[4, 4]
            {
                        { false, false, false, false },
                        { false, true,  true,  false },
                        { false, true,  true,  false },
                        { false, false, false, false }
            };
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);
            AssertBoardsEqual(initialBoard, nextBoard);
        }

        /// <summary>
        /// Tests that a blinker oscillator toggles correctly between horizontal and vertical states in the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_BlinkerOscillator_TogglesCorrectly()
        {
            bool[,] initialBoard = new bool[5, 5]
            {
                        { false, false, false, false, false },
                        { false, false, false, false, false },
                        { false, true,  true,  true,  false },
                        { false, false, false, false, false },
                        { false, false, false, false, false }
            };
            bool[,] expectedBoard = new bool[5, 5]
            {
                        { false, false, false, false, false },
                        { false, false, true,  false, false },
                        { false, false, true,  false, false },
                        { false, false, true,  false, false },
                        { false, false, false, false, false }
            };
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        /// <summary>
        /// Tests that a single live cell dies due to underpopulation in the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_SingleLiveCell_Dies()
        {
            bool[,] initialBoard = new bool[3, 3]
            {
                        { false, false, false },
                        { false, true,  false },
                        { false, false, false }
            };
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);
            bool[,] expectedBoard = new bool[3, 3];
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        /// <summary>
        /// Tests that a full board produces the correct pattern in the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_FullBoard_ProducesCorrectPattern()
        {
            bool[,] initialBoard = new bool[3, 3]
            {
                        { true, true, true },
                        { true, true, true },
                        { true, true, true }
            };
            bool[,] expectedBoard = new bool[3, 3]
            {
                        { true,  false, true },
                        { false, false, false },
                        { true,  false, true }
            };
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        /// <summary>
        /// Tests that a single row board evolves correctly in the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_OneRowBoard_WorksCorrectly()
        {
            bool[,] initialBoard = new bool[1, 5]
            {
                        { false, true, true, true, false }
            };
            bool[,] expectedBoard = new bool[1, 5]
            {
                        { false, false, true, false, false }
            };
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        /// <summary>
        /// Tests that a single column board evolves correctly in the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_OneColumnBoard_WorksCorrectly()
        {
            bool[,] initialBoard = new bool[5, 1]
            {
                        { false },
                        { true },
                        { true },
                        { true },
                        { false }
            };
            bool[,] expectedBoard = new bool[5, 1]
            {
                        { false },
                        { false },
                        { true },
                        { false },
                        { false }
            };
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        /// <summary>
        /// Tests that passing a null board throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void GetNextGeneration_NullBoard_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ConwayEngine.GetNextGeneration(null!));
        }

        /// <summary>
        /// Tests that a glider pattern evolves correctly to the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_GliderPattern_EvolvesCorrectly()
        {
            bool[,] initialBoard = new bool[7, 7];
            int offset = 1;
            initialBoard[offset + 0, offset + 1] = true;
            initialBoard[offset + 1, offset + 2] = true;
            initialBoard[offset + 2, offset + 0] = true;
            initialBoard[offset + 2, offset + 1] = true;
            initialBoard[offset + 2, offset + 2] = true;
            bool[,] expectedBoard = new bool[7, 7];
            expectedBoard[offset + 1, offset + 0] = true;
            expectedBoard[offset + 1, offset + 2] = true;
            expectedBoard[offset + 2, offset + 1] = true;
            expectedBoard[offset + 2, offset + 2] = true;
            expectedBoard[offset + 3, offset + 1] = true;
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        /// <summary>
        /// Tests that a 2x2 block still life pattern in a 7x7 board remains unchanged in the next generation.
        /// </summary>
        [Fact]
        public void GetNextGeneration_7x7BlockStillLife_RemainsUnchanged()
        {
            bool[,] initialBoard = new bool[7, 7];
            initialBoard[3, 3] = true;
            initialBoard[3, 4] = true;
            initialBoard[4, 3] = true;
            initialBoard[4, 4] = true;
            bool[,] expectedBoard = new bool[7, 7];
            expectedBoard[3, 3] = true;
            expectedBoard[3, 4] = true;
            expectedBoard[4, 3] = true;
            expectedBoard[4, 4] = true;
            bool[,] nextBoard = ConwayEngine.GetNextGeneration(initialBoard);
            AssertBoardsEqual(expectedBoard, nextBoard);
        }

        /// <summary>
        /// Tests that a glider pattern in a 7x7 board evolves correctly over multiple generations.
        /// </summary>
        [Fact]
        public void GetNextGeneration_7x7GliderEvolvesCorrectlyOverMultipleGenerations()
        {
            bool[,] board = new bool[7, 7];
            int offset = 1;
            board[offset + 0, offset + 1] = true;
            board[offset + 1, offset + 2] = true;
            board[offset + 2, offset + 0] = true;
            board[offset + 2, offset + 1] = true;
            board[offset + 2, offset + 2] = true;
            bool[,] current = board;
            for (int gen = 0; gen < 4; gen++)
            {
                current = ConwayEngine.GetNextGeneration(current);
            }
            bool[,] expectedBoard = new bool[7, 7];
            expectedBoard[offset + 0 + 1, offset + 1 + 1] = true;
            expectedBoard[offset + 1 + 1, offset + 2 + 1] = true;
            expectedBoard[offset + 2 + 1, offset + 0 + 1] = true;
            expectedBoard[offset + 2 + 1, offset + 1 + 1] = true;
            expectedBoard[offset + 2 + 1, offset + 2 + 1] = true;
            AssertBoardsEqual(expectedBoard, current);
        }
    }
}
