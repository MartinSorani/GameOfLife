namespace GameOfLife.Api.Models
{
    /// <summary>
    /// Request DTO for uploading a new board state.
    /// Each inner array represents a row of the board.
    /// </summary>
    public class BoardStateDto
    {
        public bool[][] Board { get; set; } = new bool[][]
        {
            new bool[] { true, false, true },
            new bool[] { false, true, false },
            new bool[] { true, false, true }
        };
    }
}
