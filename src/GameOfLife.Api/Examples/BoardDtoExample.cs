using GameOfLife.Api.Models;
using Swashbuckle.AspNetCore.Filters;

namespace GameOfLife.Api.Examples
{
    public class BoardDtoExample : IExamplesProvider<BoardStateDto>
    {
        public BoardStateDto GetExamples()
        {
            return new BoardStateDto
            {
                Board = new bool[][]
                {
                    new bool[] { true, false, true },
                    new bool[] { false, true, false },
                    new bool[] { true, false, true },
                },
            };
        }
    }
}
