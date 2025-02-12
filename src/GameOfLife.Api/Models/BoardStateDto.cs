using System.ComponentModel.DataAnnotations;

namespace GameOfLife.Api.Models
{
    /// <summary>
    /// Request DTO for uploading a new board state.
    /// Each inner array represents a row of the board.
    /// </summary>
    public class BoardStateDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Board must have at least one row.")]
        public bool[][] Board { get; set; } = new bool[][]
        {
            new bool[] { true, false, true },
            new bool[] { false, true, false },
            new bool[] { true, false, true }
        };
    }
}
