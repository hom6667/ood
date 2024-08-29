using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the GameFactory.Please choose your game:");
            Console.WriteLine("'1'- Gomoku");
            Console.WriteLine("'2'- Notakto");
            

            while (true)
            {
                int gameSelect = Convert.ToInt32(Console.ReadLine());
                if (gameSelect == 1)// choosing gomomlu
                {
                    GameFactory gomoku = new GomokuFactory();
                    Console.WriteLine("Choose the game mode, by pressing the relevant number:");
                    Console.WriteLine("'1' - Player vs Player");
                    Console.WriteLine("'2' - Player vs Computer");
                    Console.WriteLine("Type 'help' to see the commands.");

                    string input1 = Console.ReadLine();
                    int mode1 = 0;

                    if (input1.ToLower() == "help")
                    {
                        HelpSystem.DisplayHelp();
                        return;
                    }
                    else if (int.TryParse(input1, out mode1) && (mode1 == 1 || mode1 == 2))
                    {
                        Console.Clear();
                        GameBluePrint game = new Gomoku(15, mode1 == 2); // Initialize a 15x15 board, checking if the mode is against computer
                        game.Start();
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please restart the game.");
                    }
                    break;
                }
                else if (gameSelect == 2)// chossing Notakto 
                {
                    GameFactory notakto = new NotaktoFactory();
                    Console.WriteLine("Choose the game mode, by pressing the relevant number:");
                    Console.WriteLine("'1' - Player vs Player");
                    Console.WriteLine("'2' - Player vs Computer");
                    Console.WriteLine("Type 'help' to see the commands.");

                    string input1 = Console.ReadLine();
                    int mode1 = 0;

                    if (input1.ToLower() == "help")
                    {
                        HelpSystem.DisplayHelp();
                        return;
                    }
                    else if (int.TryParse(input1, out mode1) && (mode1 == 1 || mode1 == 2))
                    {
                        Console.Clear();
                        GameBluePrint game = new Notakto(3, mode1 == 2); // Initialize a 15x15 board, checking if the mode is against computer
                        game.Start();
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please restart the game.");
                    }
                    break;
                }
                    
                
                Console.WriteLine("Number is invalid.Please select number 1 or 2");
                  
            }
            Console.Clear();
        }
    }


    // implementing factory method
    public abstract class GameFactory
    {
        public abstract GameBluePrint InitializeGame(bool mode); 
    }
    public class GomokuFactory : GameFactory
    {
        public override GameBluePrint InitializeGame(bool mode)
        {
            return new Gomoku(15,mode);
        }
    }
    public class NotaktoFactory: GameFactory
    {
        public override GameBluePrint InitializeGame(bool mode)
        {
            return new Notakto(3, mode);

        }
    }
// end of game factory method




   // abstract class for both games 
    public abstract class GameBluePrint
    {
        protected Board board;
        protected Player player1;
        protected Player player2;
        protected Player currentPlayer;
        protected ComputerPlayer computerPlayer;
        protected bool mode; // True if playing against computer
        protected bool gameOver;

        //methods
        protected abstract void GetPlayers();
        protected abstract void ManagePlayerTurns();
        protected abstract bool IsFinishedGame();
        protected abstract void DisplayWinner();

        protected void DisplayBoard()
        {
            Console.Clear();
            board.Display();
        }
        protected void SwitchPlayer()
        {
            currentPlayer = currentPlayer == player1 ? player2 : player1;
        }
        protected void FinishGame()
        {
            Console.WriteLine("Game Over");
            Console.ReadKey();
        }


        protected GameBluePrint(int boardSize)
        {
            board = new Board(boardSize);
            gameOver = false;
            
        }
        //template method patter
        public void Start()
        {
            GetPlayers();
            while (!gameOver)
            {
                DisplayBoard();
                ManagePlayerTurns();
                if (IsFinishedGame())
                {
                    gameOver = true;
                    DisplayWinner();
                }
                else
                {
                    SwitchPlayer();
                }
            }
            FinishGame();
        }
        
    }
    //end of abstract class and template method


    //Notakto Game

    public class Notakto:GameBluePrint
    {
        private Board[] boards;
        private int noPlayBoardNumb;
        public Notakto(int boardSize, bool mode):base(boardSize)
        {
            int numbOfBoards = 3;
            boards = new Board[numbOfBoards];
            for(int i = 0; i < boards.Length; i++)
            {
                boards[i] = new Board(boardSize);
            }
            this.mode = mode;
            noPlayBoardNumb = 0;
            GetPlayers();
            currentPlayer = player1;
        }
        protected override void GetPlayers()
        {
            player1 = new HumanPlayer('X', "Player 1");
            player2 = mode ? (Player)new ComputerPlayer('X', "Computer") : new HumanPlayer('X', "Player 2");
            currentPlayer = player1;
        }
        
        protected override void ManagePlayerTurns()
        {
            while (!gameOver)
            {
                Console.Clear();
                foreach (Board board in boards)
                {
                    board.Display();
                }
                Console.WriteLine($"Player {currentPlayer.PlayerName}'s turn.");

                int x;
                int y;
                int boardNumb = 0;
                if (mode && currentPlayer == player2 && player2 is ComputerPlayer computerPlayer)//playing with computer 
                {
                    boardNumb = GetPlayableBoards();

                    if (boardNumb == -1)
                    {
                        Console.WriteLine("No more playable boards left. The game is over");
                        gameOver = true;
                        return;
                    }
                    (x, y) = computerPlayer.GetMove(boards[boardNumb]);
                }
                else
                {
                    while (true)
                    {
                        (boardNumb, x, y) = GetPlayerInput(currentPlayer);
                        if (boards[boardNumb].IsOn)
                            break;
                        Console.WriteLine("This board is not playable anymore, choose another board");
                    }
                }
                boards[boardNumb].PlacePiece(x, y, currentPlayer.Symbol);
                if (CheckWin(boardNumb, x, y, currentPlayer.Symbol))
                {

                    Console.Clear();
                    boards[boardNumb].Display();
                    boards[boardNumb].NoPlayBoard();

                    noPlayBoardNumb++;
                    if (noPlayBoardNumb == boards.Length)
                    {
                        Console.WriteLine($"Player {currentPlayer.PlayerName} lost the game");
                        gameOver = true;
                    }
                    else
                    {
                        SwitchPlayer();
                    }

                }
                else if (boards[boardNumb].IsFull())
                {
                    Console.Clear();
                    boards[boardNumb].Display();
                    Console.WriteLine("The game is a draw!");
                    gameOver = true;
                }
                else
                {
                    SwitchPlayer();
                }
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();


        }

        private (int boardNumb,int x, int y) GetPlayerInput(Player player)
        {
            int x;
            int y;
            int boardNumb;
            while (true)
            {
                try
                {
                    Console.WriteLine("Choose the board from 1 to 3");
                    boardNumb = int.Parse(Console.ReadLine()) - 1;

                    while (true)
                    {
                        if (!boards[boardNumb].IsOn || (boardNumb < 0 || boardNumb > 3))
                        {
                            Console.WriteLine("This board is not playable, or incorrect number have been choosen the other boards. Choose another board");
                            boardNumb = int.Parse(Console.ReadLine()) - 1;
                        }
                        else
                        {
                            break;
                        }
                    }


                    Console.Write("Enter row (1 to {0}): ", boards[boardNumb].Size);
                    x = int.Parse(Console.ReadLine()) - 1;
                    Console.Write("Enter column (1 to {0}): ", boards[boardNumb].Size);
                    y = int.Parse(Console.ReadLine()) - 1;

                    if (boards[boardNumb].IsValidMove(x, y))
                    {
                        return (boardNumb, x, y);
                    }
                    else
                    {
                        Console.WriteLine("Invalid move. The cell is already occupied or out of bounds.");
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input. Please enter numeric values.");
                }
                




            }
        }
        
        private bool  CheckWin(int x, int y, char symbol)
        {
            return CheckDirection(x, y, symbol, 1, 0) || // Horizontal
                   CheckDirection(x, y, symbol, 0, 1) || // Vertical
                   CheckDirection(x, y, symbol, 1, 1) || // Diagonal \
                   CheckDirection(x, y, symbol, 1, -1);  // Diagonal /
        }

        private bool CheckWin(int boardNumb, int x, int y, char symbol)
        {
            return CheckDirection(boardNumb, x, y, symbol, 1, 0) || // Horizontal
                   CheckDirection(boardNumb, x, y, symbol, 0, 1) || // Vertical
                   CheckDirection(boardNumb, x, y, symbol, 1, 1) || // Diagonal \
                   CheckDirection(boardNumb, x, y, symbol, 1, -1);  // Diagonal /
        }


        protected override bool IsFinishedGame()
        {
            int x = 0;
            int y = 0;

            for (int i = 0; i< board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    if((board.GetCell(i, j) == currentPlayer.Symbol)&& CheckWin(i,j,currentPlayer.Symbol))
                    {
                        return true;
                    }
                }
            }
            return board.IsFull();
            
        }
        
            

        private bool CheckDirection(int x, int y, char symbol, int dx, int dy)
        {
            int count = 1;

            count += CountPieces(x, y, symbol, dx, dy);
            count += CountPieces(x, y, symbol, -dx, -dy);

            return count >= 3;
        }
        private bool CheckDirection(int boardNumb, int x, int y, char symbol, int dx, int dy)
        {
            int count = 1;

            count += CountPieces(boardNumb, x, y, symbol, dx, dy);
            count += CountPieces(boardNumb, x, y, symbol, -dx, -dy);

            return count >= 3;
        }


        private int CountPieces(int x, int y, char symbol, int dx, int dy)
        {
            int count = 0;
            int i = x + dx;
            int j = y + dy;

            while (board.IsInsideBoard(i, j) && board.GetCell(i, j) == symbol)
            {
                count++;
                i += dx;
                j += dy;
            }

            return count;
        }
        private int CountPieces(int boardNumb, int x, int y, char symbol, int dx, int dy)
        {
            int count = 0;
            int i = x + dx;
            int j = y + dy;

            while (boards[boardNumb].IsInsideBoard(i, j) && boards[boardNumb].GetCell(i, j) == symbol)
            {
                count++;
                i += dx;
                j += dy;
            }

            return count;
        }


        private void SwitchPlayer()
        {
            currentPlayer = currentPlayer == player1 ? player2 : player1;
        }
        protected override void DisplayWinner()
        {
            Console.Clear();
            board.Display();
            if (CheckWin(0,0, currentPlayer.Symbol))
            {
                Console.WriteLine($"Player {currentPlayer.Symbol} loses!");
            }
            else
            {
                Console.WriteLine("The game is a draw");
            }
        }
        private int GetPlayableBoards()
        {
            for (int i = 0; i < boards.Length; i++)
            {
                if (boards[i].IsOn)
                    return i;
            }
            return -1;
        }


    }

    public class Gomoku : GameBluePrint
    {
        public Gomoku(int boardSize, bool mode) : base(boardSize)
        {
            this.mode = mode;

        }
        protected override void GetPlayers()
        {
            player1 = new HumanPlayer('X', "Player 1");
            player2 = mode ? (Player)new ComputerPlayer('O', "Computer") : new HumanPlayer('O', "Player 2");
            currentPlayer = player1;
        }

        protected override void ManagePlayerTurns()
        {
            Console.WriteLine($"Player {currentPlayer.Symbol}'s turn.");
            Console.WriteLine("Type 'help' for commands, 'save' to save, 'undo' to undo, 'redo' to redo.");

            int x;
            int y;
            if (mode && currentPlayer == player2)
            {
                (x, y) = player2.GetMove(board);
            }
            else
            {
                (x, y) = GetPlayerInput(currentPlayer);
            }

            if (x == -1 && y == -1)
            {
                return;
            }
            board.PlacePiece(x, y, currentPlayer.Symbol);
            /*history.StoreMove(x, y, currentPlayer.Symbol);*/

        }
        private (int x, int y) GetPlayerInput(Player player)
        {
            while (true)
            {
                Console.Write("Enter row (1 to 15) or a command: ");
                string input = Console.ReadLine();

                if (input.ToLower() == "help")
                {
                    HelpSystem.DisplayHelp();
                    return (-1, -1);
                }
                else if (input.ToLower() == "undo")
                {
                    /*history.UndoMove(board);*/
                    return (-1, -1);
                }
                else if (input.ToLower() == "redo")
                {
                    /*history.RedoMove(board);*/
                    return (-1, -1);
                }
                else if (input.ToLower() == "save")
                {
                    /*SaveFile.SaveGame(board, history);*/
                    Console.WriteLine("Game saved!");
                    return (-1, -1);
                }

                try
                {
                    int x = int.Parse(input) - 1;
                    Console.Write("Enter column (1 to 15): ");
                    int y = int.Parse(Console.ReadLine()) - 1;

                    if (board.IsValidMove(x, y))
                    {
                        return (x, y);
                    }
                    else
                    {
                        Console.WriteLine("Invalid move. The cell is already occupied or out of bounds.");
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input. Please enter numeric values.");
                }
            }
        }

        private bool CheckWin(int x, int y, char symbol)
        {
            return CheckDirection(x, y, symbol, 1, 0) || // Horizontal
                   CheckDirection(x, y, symbol, 0, 1) || // Vertical
                   CheckDirection(x, y, symbol, 1, 1) || // Diagonal \
                   CheckDirection(x, y, symbol, 1, -1);  // Diagonal /
        }
        protected override bool IsFinishedGame()
        {
            int x = 0;
            int y = 0;

            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    if ((board.GetCell(i, j) == currentPlayer.Symbol) && CheckWin(i, j, currentPlayer.Symbol))
                    {
                        return true;
                    }
                }
            }
            return board.IsFull();

        }



        private bool CheckDirection(int x, int y, char symbol, int dx, int dy)
        {
            int count = 1;

            count += CountPieces(x, y, symbol, dx, dy);
            count += CountPieces(x, y, symbol, -dx, -dy);

            return count >= 5;
        }

        private int CountPieces(int x, int y, char symbol, int dx, int dy)
        {
            int count = 0;
            int i = x + dx;
            int j = y + dy;

            while (board.IsInsideBoard(i, j) && board.GetCell(i, j) == symbol)
            {
                count++;
                i += dx;
                j += dy;
            }

            return count;
        }

        private void SwitchPlayer()
        {
            currentPlayer = currentPlayer == player1 ? player2 : player1;
        }
        protected override void DisplayWinner()
        {
            Console.Clear();
            board.Display();
            if (CheckWin(0, 0, currentPlayer.Symbol))
            {
                Console.WriteLine($"Player {currentPlayer.Symbol} wins!");
            }
            else
            {
                Console.WriteLine("The game is a draw");
            }
        }
    }




    public class Board
    {
        private char[,] grid;
        public int Size { get; private set; }

        public bool IsOn { get; private set; }
        public Board(int size)
        {
            Size = size;
            grid = new char[size, size];
            IsOn = true;
            Initialize();
        }

        private void Initialize()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    grid[i, j] = ' ';
                }
            }
        }

        public void Display()
        {
            Console.Write("   ");
            for (int i = 1; i <= Size; i++)
            {
                Console.Write($" {i,2} ");
            }
            Console.WriteLine();

            for (int i = 0; i < Size; i++)
            {
                Console.Write($"{i + 1,2} ");
                for (int j = 0; j < Size; j++)
                {
                    Console.Write($" {grid[i, j]} ");
                    if (j < Size - 1)
                    {
                        Console.Write("|");
                    }
                }
                Console.WriteLine();

                if (i < Size - 1)
                {
                    Console.Write("   ");
                    for (int j = 0; j < Size; j++)
                    {
                        Console.Write("---");
                        if (j < Size - 1)
                        {
                            Console.Write("+");
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        public bool IsValidMove(int x, int y)
        {
            return IsInsideBoard(x, y) && grid[x, y] == ' ';
        }

        public bool IsInsideBoard(int x, int y)
        {
            return x >= 0 && x < Size && y >= 0 && y < Size;
        }

        public void PlacePiece(int x, int y, char symbol)
        {
            grid[x, y] = symbol;
        }

        public char GetCell(int x, int y)
        {
            return grid[x, y];
        }

        public bool IsFull()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (grid[i, j] == ' ')
                        return false;
                }
            }
            return true;
        }
        public void NoPlayBoard()
        {
            IsOn = false;
        }

    }

    

    public abstract class Player
    {
        public char Symbol { get; protected set; }
        public string PlayerName { get; protected set; }

        public Player(char symbol, string name)
        {
            Symbol = symbol;
            PlayerName = name;
        }

        public abstract (int x, int y) GetMove(Board board);
    }

    public class HumanPlayer : Player
    {
        public HumanPlayer(char symbol, string name) : base(symbol, name) { }

        public override (int x, int y) GetMove(Board board)
        {
            return (0, 0); // This will be handled by the Game class's GetPlayerInput
        }
    }

    public class ComputerPlayer : Player
    {
        private Random randomMove;

        public ComputerPlayer(char symbol, string name) : base(symbol, name)
        {
            randomMove = new Random();
        }

        public override (int x, int y) GetMove(Board board)
        {
            int x = 0;
            int y = 0;
            bool isTrue = false;
            while (!isTrue)
            {
                x = randomMove.Next(0, board.Size);
                y = randomMove.Next(0, board.Size);

                if (board.IsValidMove(x, y))
                {
                    isTrue = true;
                }
            }
            return (x, y);
        }
    }

    public class HistoryOfMoves
    {
        private Stack<(int x, int y, char symbol)> moves;
        private Stack<(int x, int y, char symbol)> undoneMoves;

        public HistoryOfMoves()
        {
            moves = new Stack<(int x, int y, char symbol)>();
            undoneMoves = new Stack<(int x, int y, char symbol)>();
        }

        public void StoreMove(int x, int y, char symbol)
        {
            moves.Push((x, y, symbol));
            undoneMoves.Clear();
        }

        public void UndoMove(Board board)
        {
            if (moves.Count > 0)
            {
                var move = moves.Pop();
                board.PlacePiece(move.x, move.y, ' ');
                undoneMoves.Push(move);
            }
            else
            {
                Console.WriteLine("No more moves to undo.");
            }
        }

        public void RedoMove(Board board)
        {
            if (undoneMoves.Count > 0)
            {
                var move = undoneMoves.Pop();
                board.PlacePiece(move.x, move.y, move.symbol);
                moves.Push(move);
            }
            else
            {
                Console.WriteLine("No more moves to redo.");
            }
        }
    }

    public static class HelpSystem
    {
        public static void DisplayHelp()
        {
            Console.WriteLine("Available Commands:");
            Console.WriteLine("'help' - Show this help message");
            Console.WriteLine("'save' - Save the game state");
            Console.WriteLine("'undo' - Undo the last move");
            Console.WriteLine("'redo' - Redo the last undone move");
            Console.WriteLine("'exit' - Exit the game");
            Console.WriteLine("Press any key to return to the game...");
            Console.ReadKey();
        }
    }

    public static class SaveFile
    {
        public static void SaveGame(Board board, HistoryOfMoves history)
        {
            // Save game logic here
        }
    }
}
