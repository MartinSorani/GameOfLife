using System.ComponentModel.DataAnnotations;

namespace GameOfLifeAPI.Models
{
    public class BoardStateModel
    {
        [Required]
        public required bool[][] State { get; set; }

        public static BoardStateModel Example => new BoardStateModel
        {
            State = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { true, false, false },
                new bool[] { false, true, false }
            }
        };
    }
}