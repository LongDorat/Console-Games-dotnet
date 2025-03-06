using System.Diagnostics;
using System.Text;

namespace Minesweeper
{
    struct Cell(bool isMine, bool isOpened, bool isFlagged, int adjacentMines)
    {
        public bool IsMine = isMine;
        public bool IsOpened = isOpened;
        public bool IsFlagged = isFlagged;
        public int AdjacentMines = adjacentMines;
    }

    struct Cursor(int row, int column)
    {
        public int Row = row;
        public int Column = column;
    }
    class Program
    {
        const int MAX_HEIGHT = 50;
        const int MAX_WIDTH = 50;
        const int MIN_HEIGHT = 10;
        const int MIN_WIDTH = 10;
        static void Main(string[] args)
        {
            //? Get the dimensions and create the board
            int selectedHeight = GetDimensionInput($"Enter the height of the board ({MIN_HEIGHT}-{MAX_HEIGHT}): ", MIN_HEIGHT, MAX_HEIGHT);
            int selectedWidth = GetDimensionInput($"Enter the width of the board ({MIN_WIDTH}-{MAX_WIDTH}): ", MIN_WIDTH, MAX_WIDTH);
            var board = new Cell[selectedHeight, selectedWidth];
            for (int i = 0; i < selectedHeight; i++)
            {
                for (int j = 0; j < selectedWidth; j++)
                {
                    board[i, j] = new Cell(false, false, false, 0);
                }
            }

            //?Get the mine ratio and mine count
            double mineRatio = GetMineRatioInput("Enter the mine ratio (0-1): ", 0.01, 0.99);
            int mineCount = (int)(selectedHeight * selectedWidth * mineRatio);

            //? Set cursor position to the middle of the board
            var cursor = new Cursor(selectedHeight / 2, selectedWidth / 2);

            Render.Board(board, cursor);

            //? Main game loop
            bool isMineGenerated = false;
            while (true)
            {
                //? Check if the player won
                int openedCellCount = 0;
                for (int i = 0; i < board.GetLength(0); i++)
                {
                    for (int j = 0; j < board.GetLength(1); j++)
                    {
                        if (board[i, j].IsOpened && !board[i, j].IsMine)
                        {
                            openedCellCount++;
                        }
                    }
                }
                if (openedCellCount == selectedHeight * selectedWidth - mineCount)
                {
                    Console.Clear();
                    Render.Board(board, cursor);
                    Console.WriteLine("You won!");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                //? Handle player input
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Spacebar:
                        if (!isMineGenerated)
                        {
                            board = Mine.Generate(board, mineCount, cursor);
                            isMineGenerated = true;
                        }
                        board = Control.Spacebar(board, cursor);
                        break;
                    case ConsoleKey.F:
                        board = Control.Flag(board, cursor);
                        break;
                    case ConsoleKey.Escape:
                        Console.Clear();
                        Console.Write("Minesweeper is closed.");
                        Thread.Sleep(2000);
                        Environment.Exit(0);
                        return;
                    default:
                        cursor = Control.Move(cursor, selectedHeight, selectedWidth, key.Key);
                        break;
                }
                Render.Board(board, cursor);
            }
        }

        static int GetDimensionInput(string message, int min, int max)
        {
            int dimension;
            do
            {
                Console.Write(message);
                if (!int.TryParse(Console.ReadLine(), out dimension))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }
                if (dimension < min || dimension > max)
                {
                    Console.WriteLine($"Invalid input. Please enter a number between {min} and {max}.");
                    continue;
                }
                break;
            } while (true);
            return dimension;
        }

        static double GetMineRatioInput(string message, double min, double max)
        {
            double ratio;
            do
            {
                Console.Write(message);
                if (!double.TryParse(Console.ReadLine(), out ratio))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }
                if (ratio < min || ratio > max)
                {
                    Console.WriteLine($"Invalid input. Please enter a number between {min} and {max}.");
                    continue;
                }
                break;
            } while (true);
            return ratio;
        }
    }

    static class Control
    {
        public static Cursor Move(Cursor cursor, int height, int width, ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    cursor.Row = Math.Max(0, cursor.Row - 1);
                    break;
                case ConsoleKey.DownArrow:
                    cursor.Row = Math.Min(height - 1, cursor.Row + 1);
                    break;
                case ConsoleKey.LeftArrow:
                    cursor.Column = Math.Max(0, cursor.Column - 1);
                    break;
                case ConsoleKey.RightArrow:
                    cursor.Column = Math.Min(width - 1, cursor.Column + 1);
                    break;
            }
            return cursor;
        }

        public static Cell[,] Spacebar(Cell[,] board, Cursor cursor)
        {
            if (board[cursor.Row, cursor.Column].IsMine && !board[cursor.Row, cursor.Column].IsFlagged)
            {
                for (int i = 0; i < board.GetLength(0); i++)
                {
                    for (int j = 0; j < board.GetLength(1); j++)
                    {
                        board[i, j].IsOpened = true;
                        if (board[i, j].IsFlagged)
                        {
                            board[i, j].IsFlagged = false;
                        }
                    }
                }
                cursor.Row = -1;
                cursor.Column = -1;

                Console.Clear();
                Render.Board(board, cursor);
                Console.WriteLine("Game Over!");
                Console.ReadKey();
                Environment.Exit(0);
            }
            if (!board[cursor.Row, cursor.Column].IsFlagged)
            {
                board[cursor.Row, cursor.Column].IsOpened = true;
                if (board[cursor.Row, cursor.Column].AdjacentMines == 0)
                {
                    board = RevealAdjacentTile(board, cursor);
                }
            }
            return board;
        }

        private static Cell[,] RevealAdjacentTile(Cell[,] board, Cursor cursor)
        {
            for (int i = cursor.Row - 1; i <= cursor.Row + 1; i++)
            {
                for (int j = cursor.Column - 1; j <= cursor.Column + 1; j++)
                {
                    if (i < 0 || i >= board.GetLength(0) || j < 0 || j >= board.GetLength(1))
                    {
                        continue;
                    }
                    if (!board[i, j].IsOpened && !board[i, j].IsMine)
                    {
                        board[i, j].IsOpened = true;
                        if (board[i, j].AdjacentMines == 0)
                        {
                            board = RevealAdjacentTile(board, new Cursor(i, j));
                        }
                    }
                }
            }

            return board;
        }

        public static Cell[,] Flag(Cell[,] board, Cursor cursor)
        {
            if (!board[cursor.Row, cursor.Column].IsOpened)
            {
                board[cursor.Row, cursor.Column].IsFlagged = !board[cursor.Row, cursor.Column].IsFlagged;
            }
            return board;
        }
    }

    static class Mine
    {
        public static Cell[,] Generate(Cell[,] board, int mineCount, Cursor cursor)
        {
            var random = new Random();
            int placedMines = 0;
            while (placedMines < mineCount)
            {
                int row = random.Next(board.GetLength(0));
                int column = random.Next(board.GetLength(1));
                if (!board[row, column].IsMine && !(cursor.Row == row && cursor.Column == column))
                {
                    board[row, column].IsMine = true;
                    placedMines++;
                }
            }
            return SetAdjacentMines(board);
        }

        private static Cell[,] SetAdjacentMines(Cell[,] board)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j].IsMine)
                    {
                        continue;
                    }
                    for (int k = i - 1; k <= i + 1; k++)
                    {
                        for (int l = j - 1; l <= j + 1; l++)
                        {
                            if (k < 0 || k >= board.GetLength(0) || l < 0 || l >= board.GetLength(1))
                            {
                                continue;
                            }
                            if (board[k, l].IsMine)
                            {
                                board[i, j].AdjacentMines++;
                            }
                        }
                    }
                }
            }
            return board;
        }
    }

    static class Render
    {
        public static void Board(Cell[,] board, Cursor cursor)
        {
            var output = new StringBuilder();
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (cursor.Row == i && cursor.Column == j)
                    {
                        output.Append("© ");
                    }
                    else if (board[i, j].IsFlagged)
                    {
                        output.Append("F ");
                    }
                    else if (board[i, j].IsOpened)
                    {
                        if (board[i, j].IsMine)
                        {
                            output.Append("X ");
                        }
                        else
                        {
                            output.Append($"{board[i, j].AdjacentMines} ");
                        }
                    }
                    else
                    {
                        output.Append("# ");
                    }
                }
                output.AppendLine();
            }
            Console.Clear();
            Console.Write(output.ToString());
        }
    }
}