namespace GameOfLifeAPI.Models
{
    public class Board
    {
        public Guid Id { get; set; }
        public bool[][] CurrentState { get; set; }

        public int Rows => CurrentState.Length;
        public int Columns => CurrentState[0].Length;

        public Board(bool[][] initialState)
        {
            Id = Guid.NewGuid();
            CurrentState = initialState;
        }
    }
}
