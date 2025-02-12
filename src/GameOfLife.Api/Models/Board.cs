namespace GameOfLife.Api.Models
{
    public class Board
    {
        public Guid Id { get; set; }
        public required bool[,] CurrentState { get; set; }
    }
}
