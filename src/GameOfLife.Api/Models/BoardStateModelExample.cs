using Swashbuckle.AspNetCore.Filters;

namespace GameOfLifeAPI.Models
{
    public class BoardStateModelExample : IExamplesProvider<BoardStateModel>
    {
        public BoardStateModel GetExamples()
        {
            return BoardStateModel.Example;
        }
    }
}
