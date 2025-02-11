using GameOfLifeAPI.Utils;

namespace GameOfLifeAPI.UnitTests
{
    public class ConwayEngineTests
    {
        private bool StatesAreEqual(bool[][] state1, bool[][] state2)
        {
            if (state1.Length != state2.Length)
                return false;

            for (int i = 0; i < state1.Length; i++)
            {
                if (state1[i].Length != state2[i].Length)
                    return false;

                for (int j = 0; j < state1[i].Length; j++)
                {
                    if (state1[i][j] != state2[i][j])
                        return false;
                }
            }
            return true;
        }

        [Fact]
        public void GetNextGeneration_ShouldReturnSameState_ForStillLifeBlock()
        {
            // Arrange: a 4x4 board with a 2x2 block in the center (still life)
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, false, false, false },
                new bool[] { false, true,  true,  false },
                new bool[] { false, true,  true,  false },
                new bool[] { false, false, false, false }
            };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: the block remains unchanged
            Assert.True(StatesAreEqual(initialState, nextState));
        }

        [Fact]
        public void GetNextGeneration_ShouldOscillate_ForBlinker()
        {
            // Arrange: a horizontal blinker
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, false, false },
                new bool[] { true,  true,  true },
                new bool[] { false, false, false }
            };

            // Expected vertical blinker
            bool[][] expectedState = new bool[][]
            {
                new bool[] { false, true,  false },
                new bool[] { false, true,  false },
                new bool[] { false, true,  false }
            };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: the blinker has oscillated
            Assert.True(StatesAreEqual(expectedState, nextState));
        }

        [Fact]
        public void GetNextGeneration_ShouldOscillate_ForToad()
        {
            // Arrange: a toad oscillator
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, false, false, false, false, false },
                new bool[] { false, false, false, false, false, false },
                new bool[] { false, false, true,  true,  true,  false },
                new bool[] { false, true,  true,  true,  false, false },
                new bool[] { false, false, false, false, false, false },
                new bool[] { false, false, false, false, false, false }
            };

            // Expected next state
            bool[][] expectedState = new bool[][]
            {
                new bool[] { false, false, false, false, false, false },
                new bool[] { false, false, false, true,  false, false },
                new bool[] { false, true,  false, false, true,  false },
                new bool[] { false, true,  false, false, true,  false },
                new bool[] { false, false, true,  false, false, false },
                new bool[] { false, false, false, false, false, false }
            };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: the toad has oscillated
            Assert.True(StatesAreEqual(expectedState, nextState));
        }

        [Fact]
        public void GetNextGeneration_ShouldMove_ForGlider()
        {
            // Arrange: a glider pattern
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, false, true,  false, false },
                new bool[] { true,  false, true,  false, false },
                new bool[] { false, true,  true,  false, false },
                new bool[] { false, false, false, false, false },
                new bool[] { false, false, false, false, false }
            };

            // Expected next state
            bool[][] expectedState = new bool[][]
            {
                new bool[] { false, false, false, false, false },
                new bool[] { false, false, true,  false, false },
                new bool[] { true,  false, true,  false, false },
                new bool[] { false, true,  true,  false, false },
                new bool[] { false, false, false, false, false }
            };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: the glider has moved
            Assert.True(StatesAreEqual(expectedState, nextState));
        }

        [Fact]
        public void GetNextGeneration_ShouldReturnEmpty_ForAllDeadCells()
        {
            // Arrange: an empty board
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, false, false },
                new bool[] { false, false, false },
                new bool[] { false, false, false }
            };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: the board remains empty
            Assert.True(StatesAreEqual(initialState, nextState));
        }

        [Fact]
        public void GetNextGeneration_ShouldHandle_EdgeCellsWrapping()
        {
            // Arrange: cells at the edges
            bool[][] initialState = new bool[][]
            {
                new bool[] { true,  false, true },
                new bool[] { false, false, false },
                new bool[] { true,  false, true }
            };

            // Expected next state without wrapping (cells die due to underpopulation)
            bool[][] expectedState = new bool[][]
            {
                new bool[] { false, false, false },
                new bool[] { false, false, false },
                new bool[] { false, false, false }
            };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: edge cells handled correctly
            Assert.True(StatesAreEqual(expectedState, nextState));
        }

        [Fact]
        public void GetNextGeneration_ShouldReturnSameState_ForEmptyBoard()
        {
            // Arrange: an empty board (no rows)
            bool[][] initialState = new bool[][] { };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: the board remains unchanged
            Assert.True(StatesAreEqual(initialState, nextState));
        }

        [Fact]
        public void GetNextGeneration_ShouldThrowException_ForNullInput()
        {
            // Arrange: null board
            bool[][] initialState = null;

            // Act & Assert: expect ArgumentNullException
            Assert.Throws<ArgumentNullException>(() => ConwayEngine.GetNextGeneration(initialState));
        }

        [Fact]
        public void GetNextGeneration_ShouldHandle_NonRectangularBoard()
        {
            // Arrange: a non-rectangular (jagged) board
            bool[][] initialState = new bool[][]
            {
                new bool[] { true, false },
                new bool[] { false, true, true },
                new bool[] { true }
            };

            // Act & Assert: expect an exception due to inconsistent row lengths
            Assert.Throws<InvalidOperationException>(() => ConwayEngine.GetNextGeneration(initialState));
        }

        [Fact]
        public void GetNextGeneration_ShouldHandle_SingleCell()
        {
            // Arrange: a board with a single live cell
            bool[][] initialState = new bool[][]
            {
                new bool[] { true }
            };

            // Expected next state (cell dies due to underpopulation)
            bool[][] expectedState = new bool[][]
            {
                new bool[] { false }
            };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: the single cell dies
            Assert.True(StatesAreEqual(expectedState, nextState));
        }

        [Fact]
        public void GetNextGeneration_ShouldReviveCell_WithThreeNeighbors()
        {
            // Arrange: a cell with exactly three neighbors becomes alive
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, true,  false },
                new bool[] { true,  false, true },
                new bool[] { false, false, false }
            };

            // Expected next state
            bool[][] expectedState = new bool[][]
            {
                new bool[] { false, false, false },
                new bool[] { false, true,  false },
                new bool[] { false, false, false }
            };

            // Act: compute the next generation
            bool[][] nextState = ConwayEngine.GetNextGeneration(initialState);

            // Assert: the center cell becomes alive
            Assert.True(StatesAreEqual(expectedState, nextState));
        }
    }
}
