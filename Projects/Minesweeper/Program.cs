namespace Minesweeper
{
    struct Cell(bool isMine, bool isOpened, bool isFlagged, int adjacentMines)
    {
        public bool IsMine = isMine;
        public bool IsOpened = isOpened;
        public bool IsFlagged = isFlagged;
        public int AdjacentMines = adjacentMines;
    }

    struct Cursor (int row, int column)
    {
        public int Row = row;
        public int Column = column;
    }
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: Get the dimensions and create the board

            //TODO: Get the mine ratio and mine count

            //TODO: Set cursor position to the middle of the board

            //TODO: Draw the board

            //TODO: Start the game loop
        }
    }

    static class Control
    {
        //TODO: Implement the movement methods

        //TODO: Implement the opening cell method

        //TODO: Implement the flagging cell method
    }

    static class Mine
    {
        //TODO: Generate mines

        //TODO: Set adjacent mine count
    }

    static class Render
    {
        //TODO: Implement the board rendering method
    }
}