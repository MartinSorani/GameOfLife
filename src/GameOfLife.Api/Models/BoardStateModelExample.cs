using Swashbuckle.AspNetCore.Filters;

namespace GameOfLife.Api.Models
{
    public class BoardStateModelExample : IExamplesProvider<BoardStateModel>
    {
        public BoardStateModel GetExamples()
        {
            return BoardStateModel.Example;
        }
    }
}
