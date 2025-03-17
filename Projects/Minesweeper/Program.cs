namespace Minesweeper;

struct Cell(bool isMine, int adjacentMines, bool isRevealed, bool isFlagged)
{
    public bool IsMine = isMine;
    public int AdjacentMines = adjacentMines;
    public bool IsRevealed = isRevealed;
    public bool IsFlagged = isFlagged;
}

struct Cursor(int row, int column)
{
    public int Row = row;
    public int Column = column;
}

class Program
{
    const int MAX_BOARD_HEIGHT = 50;
    const int MAX_BOARD_WIDTH = 50;
    const int MIN_BOARD_HEIGHT = 5;
    const int MIN_BOARD_WIDTH = 5;
    const double MAX_MINE_DENSITY = 0.99;
    const double MIN_MINE_DENSITY = 0.01;

    static void Main()
    {
        int selectedHeight = GetUserInput($"Enter the height of the board ({MIN_BOARD_HEIGHT} - {MAX_BOARD_HEIGHT}): ", MIN_BOARD_HEIGHT, MAX_BOARD_HEIGHT);
        int selectedWidth = GetUserInput($"Enter the width of the board ({MIN_BOARD_WIDTH} - {MAX_BOARD_WIDTH}): ", MIN_BOARD_WIDTH, MAX_BOARD_WIDTH);
        Cell[,] board = GenerateBoard(selectedHeight, selectedWidth);

        double selectedDensity = GetUserInput($"Enter the mine density ({MIN_MINE_DENSITY} - {MAX_MINE_DENSITY}): ", MIN_MINE_DENSITY, MAX_MINE_DENSITY);
        int mineCount = (int)(selectedDensity * selectedHeight * selectedWidth);

        var cursor = new Cursor(selectedHeight / 2, selectedWidth / 2);

        Game.Start(board, cursor, mineCount);
    }

    static T GetUserInput<T>(string message, T min, T max) where T : IComparable<T>, IConvertible
    {
        do
        {
            Console.Write(message);
            string? input = Console.ReadLine();
            if (input == null)
            {
                Console.WriteLine($"Invalid input. Please enter a value between {min} and {max}.");
                continue;
            }

            try
            {
                T result = (T)Convert.ChangeType(input, typeof(T));
                if (result.CompareTo(min) >= 0 && result.CompareTo(max) <= 0)
                {
                    return result;
                }
            }
            catch (Exception)
            {
                // Ignore this exception and continue to the prompt
            }

            Console.WriteLine($"Invalid input. Please enter a value between {min} and {max}.");
        } while (true);
    }

    static Cell[,] GenerateBoard(int height, int width)
    {
        var board = new Cell[height, width];

        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                board[row, column] = new Cell(false, 0, false, false);
            }
        }

        return board;
    }
}

static class Game
{
    public static void Start(Cell[,] board, Cursor cursor, int mineCount)
    {
        Render.Board(board, cursor);

        bool isMineGenerated = false;
        while (true)
        {
            if (IsWin(board, mineCount))
            {
                EndGame("You win!", board);
            }

            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    cursor.Row = Math.Max(0, cursor.Row - 1);
                    break;
                case ConsoleKey.DownArrow:
                    cursor.Row = Math.Min(board.GetLength(0) - 1, cursor.Row + 1);
                    break;
                case ConsoleKey.LeftArrow:
                    cursor.Column = Math.Max(0, cursor.Column - 1);
                    break;
                case ConsoleKey.RightArrow:
                    cursor.Column = Math.Min(board.GetLength(1) - 1, cursor.Column + 1);
                    break;
                case ConsoleKey.Spacebar:
                    if (!isMineGenerated)
                    {
                        board = PlaceMine(board, cursor, mineCount);
                        isMineGenerated = true;
                    }

                    if (board[cursor.Row, cursor.Column].IsMine)
                    {
                        EndGame("Game over!", board);
                    }

                    if (board[cursor.Row, cursor.Column].AdjacentMines == 0)
                    {
                        board = RevealAdjacentCells(board, cursor);
                    }
                    else
                    {
                        board[cursor.Row, cursor.Column].IsRevealed = true;
                    }
                    break;
                case ConsoleKey.F:
                    board[cursor.Row, cursor.Column].IsFlagged = !board[cursor.Row, cursor.Column].IsFlagged;
                    break;
                case ConsoleKey.Escape:
                    Console.Clear();
                    Console.WriteLine("MineSweeper has been closed.");
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                    break;
            }

            Render.Board(board, cursor);
        }
    }

    private static Cell[,] PlaceMine(Cell[,] board, Cursor cursor, int mineCount)
    {
        var random = new Random();
        int minesPlaced = 0;

        while (minesPlaced < mineCount)
        {
            int row = random.Next(board.GetLength(0));
            int column = random.Next(board.GetLength(1));

            if (board[row, column].IsMine || (row == cursor.Row && column == cursor.Column))
            {
                continue;
            }

            board[row, column].IsMine = true;
            minesPlaced++;
        }

        return SetAdjacentMines(board);
    }

    private static Cell[,] SetAdjacentMines(Cell[,] board)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int column = 0; column < board.GetLength(1); column++)
            {
                if (board[row, column].IsMine)
                {
                    continue;
                }

                int adjacentMines = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (row + i < 0 || row + i >= board.GetLength(0) || column + j < 0 || column + j >= board.GetLength(1))
                        {
                            continue;
                        }

                        if (board[row + i, column + j].IsMine)
                        {
                            adjacentMines++;
                        }
                    }
                }

                board[row, column].AdjacentMines = adjacentMines;
            }
        }

        return board;
    }

    private static Cell[,] RevealAdjacentCells(Cell[,] board, Cursor cursor)
    {
        var queue = new Queue<Cursor>();
        queue.Enqueue(cursor);

        while (queue.Count > 0)
        {
            Cursor current = queue.Dequeue();
            board[current.Row, current.Column].IsRevealed = true;

            if (board[current.Row, current.Column].AdjacentMines == 0)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (current.Row + i < 0 || current.Row + i >= board.GetLength(0) || current.Column + j < 0 || current.Column + j >= board.GetLength(1))
                        {
                            continue;
                        }

                        if (!board[current.Row + i, current.Column + j].IsRevealed)
                        {
                            queue.Enqueue(new Cursor(current.Row + i, current.Column + j));
                        }
                    }
                }
            }
        }

        return board;
    }

    private static bool IsWin(Cell[,] board, int mineCount)
    {
        int revealedCells = 0;
        int flaggedMines = 0;

        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int column = 0; column < board.GetLength(1); column++)
            {
                if (board[row, column].IsRevealed && !board[row, column].IsMine)
                {
                    revealedCells++;
                }
                if (board[row, column].IsMine && board[row, column].IsFlagged)
                {
                    flaggedMines++;
                }
            }
        }

        return revealedCells == board.GetLength(0) * board.GetLength(1) - mineCount && flaggedMines == mineCount;
    }

    private static void EndGame(string message, Cell[,] board)
    {
        Render.Board(board, new Cursor(0, 0));
        Console.SetCursorPosition(0, board.GetLength(0) + 1);
        Console.WriteLine(message);

        Console.ReadKey();
        Environment.Exit(0);
    }
}

static class Render
{
    public static void Board(Cell[,] board, Cursor cursor)
    {
        Console.Clear();

        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int column = 0; column < board.GetLength(1); column++)
            {
                if (board[row, column].IsFlagged)
                {
                    Console.Write("F ");
                }
                else if (board[row, column].IsRevealed)
                {
                    if (board[row, column].IsMine)
                    {
                        Console.Write("X ");
                    }
                    else
                    {
                        Console.Write($"{board[row, column].AdjacentMines} ");
                    }
                }
                else
                {
                    Console.Write("# ");
                }
            }
            Console.WriteLine();
        }
        Console.SetCursorPosition(cursor.Column * 2, cursor.Row);
    }
}