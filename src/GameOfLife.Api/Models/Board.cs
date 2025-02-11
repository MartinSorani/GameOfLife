namespace GameOfLife.Api.Models
{
    public class Board
    {
        public Guid Id { get; set; }
        public bool[,] CurrentState { get; set; }

        public int Rows => CurrentState.GetLength(0);
        public int Columns => CurrentState.GetLength(1);

        public Board(bool[,] initialState)
        {
            Id = Guid.NewGuid();
            CurrentState = initialState;
        }
    }
}
