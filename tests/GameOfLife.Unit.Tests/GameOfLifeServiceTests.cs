using GameOfLife.Api.Repositories;
using GameOfLife.Api.Services;

namespace GameOfLife.Unit.Tests
{
    public class GameOfLifeServiceTests
    {
        /// <summary>
        /// Helper method to create a new service instance with an in-memory repository.
        /// </summary>
        private IGameOfLifeService CreateService()
        {
            IBoardRepository repository = new InMemoryBoardRepository();
            return new GameOfLifeService(repository);
        }

        /// <summary>
        /// Helper method to create a simple 2D board representing a 2x2 block still life.
        /// </summary>
        private bool[,] CreateBlockBoard(int rows, int cols, int startRow, int startCol)
        {
            bool[,] board = new bool[rows, cols];
            // Place a 2x2 block (stable) at the given start position.
            board[startRow, startCol] = true;
            board[startRow, startCol + 1] = true;
            board[startRow + 1, startCol] = true;
            board[startRow + 1, startCol + 1] = true;
            return board;
        }

        /// <summary>
        /// Helper method to create a horizontal blinker pattern.
        /// </summary>
        private bool[,] CreateBlinkerBoard(int rows, int cols, int centerRow, int centerCol)
        {
            bool[,] board = new bool[rows, cols];
            // Place a horizontal blinker: three consecutive live cells in the center row.
            board[centerRow, centerCol - 1] = true;
            board[centerRow, centerCol] = true;
            board[centerRow, centerCol + 1] = true;
            return board;
        }

        // ============================================================
        // UploadBoard tests
        // ============================================================

        [Fact]
        public void UploadBoard_NullBoard_ThrowsArgumentNullException()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.UploadBoard(null!));
        }

        [Fact]
        public void UploadBoard_ValidBoard_ReturnsNonEmptyGuidAndStoresBoard()
        {
            // Arrange
            var service = CreateService();
            bool[,] board = new bool[3, 3]; // Simple empty board

            // Act
            Guid boardId = service.UploadBoard(board);

            // Assert
            Assert.NotEqual(Guid.Empty, boardId);
            // Retrieve the board via GetNextState (which fetches and updates the board)
            bool[,] retrievedState = service.GetNextState(boardId);
            Assert.NotNull(retrievedState);
        }

        // ============================================================
        // GetNextState tests
        // ============================================================

        [Fact]
        public void GetNextState_BoardNotFound_ThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            Guid nonExistentId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.GetNextState(nonExistentId));
        }

        [Fact]
        public void GetNextState_ReturnsCorrectNextState_ForBlockStillLife()
        {
            // Arrange: Use a 4x4 board with a block still life (should remain unchanged).
            var service = CreateService();
            bool[,] board = new bool[4, 4]
            {
                { false, false, false, false },
                { false, true,  true,  false },
                { false, true,  true,  false },
                { false, false, false, false }
            };
            Guid boardId = service.UploadBoard(board);

            // Act
            bool[,] nextState = service.GetNextState(boardId);

            // Assert: The block pattern is stable.
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Assert.Equal(board[i, j], nextState[i, j]);
                }
            }
        }

        // ============================================================
        // GetStateAfterSteps tests
        // ============================================================

        [Fact]
        public void GetStateAfterSteps_NegativeSteps_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var service = CreateService();
            bool[,] board = new bool[3, 3];
            Guid boardId = service.UploadBoard(board);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => service.GetStateAfterSteps(boardId, -1));
        }

        [Fact]
        public void GetStateAfterSteps_BoardNotFound_ThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            Guid nonExistentId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.GetStateAfterSteps(nonExistentId, 3));
        }

        [Fact]
        public void GetStateAfterSteps_ReturnsCorrectStateAfterMultipleGenerations()
        {
            // Arrange: Use a horizontal blinker pattern in a 5x5 board.
            var service = CreateService();
            // Create a blinker centered at (2,2) in a 5x5 board.
            bool[,] initialBoard = CreateBlinkerBoard(5, 5, 2, 2);
            // The horizontal blinker has live cells at (2,1), (2,2), (2,3).
            Guid boardId = service.UploadBoard(initialBoard);

            // Act:
            // After 1 generation, the blinker should become vertical (cells at (1,2), (2,2), (3,2)).
            bool[,] stateAfterOne = service.GetStateAfterSteps(boardId, 1);
            // Then, after 1 more generation (2 total), it should revert to horizontal.
            bool[,] stateAfterTwo = service.GetStateAfterSteps(boardId, 1);

            // Assert:
            // Expect horizontal blinker: live cells at (2,1), (2,2), (2,3)
            bool[,] expectedHorizontal = new bool[5, 5];
            expectedHorizontal[2, 1] = true;
            expectedHorizontal[2, 2] = true;
            expectedHorizontal[2, 3] = true;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Assert.Equal(expectedHorizontal[i, j], stateAfterTwo[i, j]);
                }
            }
        }

        // ============================================================
        // GetFinalState tests
        // ============================================================

        [Fact]
        public void GetFinalState_MaxIterationsLessThanOrEqualZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var service = CreateService();
            bool[,] board = new bool[3, 3];
            Guid boardId = service.UploadBoard(board);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => service.GetFinalState(boardId, 0));
        }

        [Fact]
        public void GetFinalState_BoardNotFound_ThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            Guid nonExistentId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.GetFinalState(nonExistentId, 10));
        }

        [Fact]
        public void GetFinalState_StableStateReached_ReturnsFinalState()
        {
            // Arrange: Use a block still life pattern (stable) on a 7x7 board.
            var service = CreateService();
            bool[,] blockBoard = CreateBlockBoard(7, 7, 3, 3); // 2x2 block at position (3,3)
            Guid boardId = service.UploadBoard(blockBoard);

            // Act: A block is stable so its final state should be the same.
            bool[,] finalState = service.GetFinalState(boardId, 10);

            // Assert: final state equals the original block board.
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Assert.Equal(blockBoard[i, j], finalState[i, j]);
                }
            }
        }

        [Fact]
        public void GetFinalState_OscillatoryPattern_ThrowsInvalidOperationException()
        {
            // Arrange: Use a blinker pattern (oscillatory) in a 5x5 board.
            var service = CreateService();
            bool[,] blinkerBoard = CreateBlinkerBoard(5, 5, 2, 2);  // Horizontal blinker
            Guid boardId = service.UploadBoard(blinkerBoard);

            // Act & Assert:
            // Our GetFinalState method checks for a stable state (i.e. successive generations equal each other).
            // An oscillatory pattern like the blinker will never become "stable" by that definition,
            // so we expect an InvalidOperationException.
            Assert.Throws<InvalidOperationException>(() => service.GetFinalState(boardId, 10));
        }
    }
}
