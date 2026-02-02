using System;
using System.Threading;

namespace Task4_Tetris {

    // Класс для представления одной фигуры
    class TetrisShape {
        public int[][] Shape { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        public TetrisShape(int[][] shape, int startX = 0, int startY = 0) {
            Shape = CloneShape(shape);
            X = startX;
            Y = startY;
        }

        // Копия зубчатого массива
        private static int[][] CloneShape(int[][] original) {
            int[][] clone = new int[original.Length][];
            for (int i = 0; i < original.Length; i++) {
                clone[i] = (int[])original[i].Clone();
            }
            return clone;
        }

        public void Rotate() {
            int rows = Shape.Length;
            int cols = Shape[0].Length;
            int[][] rotated = new int[cols][];

            for (int i = 0; i < cols; i++) {
                rotated[i] = new int[rows];
                for (int j = 0; j < rows; j++) {
                    rotated[i][j] = Shape[rows - 1 - j][i];
                }
            }

            Shape = rotated;
        }

        public int Width => Shape.Length > 0 ? Shape[0].Length : 0;
        public int Height => Shape.Length;
    }

    // Класс игрового поля
    class GameBoard {
        public const int Width = 10;
        public const int Height = 20;
        private int[,] board = new int[Height, Width];

        public bool CanPlace(TetrisShape piece) {
            for (int y = 0; y < piece.Height; y++) {
                for (int x = 0; x < piece.Width; x++) {
                    if (piece.Shape[y][x] == 1) {
                        int boardX = piece.X + x;
                        int boardY = piece.Y + y;

                        if (boardX < 0 || boardX >= Width || boardY >= Height)
                            return false;

                        if (boardY >= 0 && board[boardY, boardX] == 1)
                            return false;
                    }
                }
            }
            return true;
        }

        public void Place(TetrisShape piece) {
            for (int y = 0; y < piece.Height; y++) {
                for (int x = 0; x < piece.Width; x++) {
                    if (piece.Shape[y][x] == 1) {
                        int boardY = piece.Y + y;
                        int boardX = piece.X + x;
                        if (boardY >= 0)
                            board[boardY, boardX] = 1;
                    }
                }
            }
            ClearLines();
        }

        private void ClearLines() {
            for (int y = Height - 1; y >= 0; y--) {
                bool isFull = true;
                for (int x = 0; x < Width; x++) {
                    if (board[y, x] == 0) {
                        isFull = false;
                        break;
                    }
                }
                if (isFull) {
                    // Сдвигаем строки вниз
                    for (int yy = y; yy > 0; yy--)
                        for (int x = 0; x < Width; x++)
                            board[yy, x] = board[yy - 1, x];
                    // Очищаеем верхнюю строку
                    for (int x = 0; x < Width; x++)
                        board[0, x] = 0;
                    y++; // Проверяем ту же строку опять
                }
            }
        }

        public void Draw(TetrisShape currentPiece) {
            // Создаём временную копию доски
            int[,] display = new int[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    display[y, x] = board[y, x];

            // Добавляем текущую фигуру
            for (int y = 0; y < currentPiece.Height; y++) {
                for (int x = 0; x < currentPiece.Width; x++) {
                    if (currentPiece.Shape[y][x] == 1) {
                        int boardY = currentPiece.Y + y;
                        int boardX = currentPiece.X + x;
                        if (boardY >= 0 && boardY < Height && boardX >= 0 && boardX < Width)
                            display[boardY, boardX] = 2; // 2 = текущая фигура
                    }
                }
            }

            Console.Clear();
            Console.WriteLine("=== TETRIS ===");
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    Console.Write(display[y, x] == 0 ? "." : "█");
                }
                Console.WriteLine();
            }
            Console.WriteLine("Управление: A/D – влево/вправо, S – вниз, W – поворот, Q – выход");
        }

        public bool IsGameOver => CheckTopRow();

        private bool CheckTopRow() {
            for (int x = 0; x < Width; x++)
                if (board[0, x] == 1)
                    return true;
            return false;
        }
    }

    // Класс программы
    class Program {
        // Фигуры тетриса
        static readonly int[][][] Shapes = {
            new int[][] { new int[] {1,1,1,1} },
            new int[][] { new int[] {1,1}, new int[] {1,1} },
            new int[][] { new int[] {0,1,0}, new int[] {1,1,1} },
            new int[][] { new int[] {1,0}, new int[] {1,0}, new int[] {1,1} },
            new int[][] { new int[] {0,1}, new int[] {0,1}, new int[] {1,1} },
            new int[][] { new int[] {0,1,1}, new int[] {1,1,0} },
            new int[][] { new int[] {1,1,0}, new int[] {0,1,1} }
        };

        static Random rand = new Random();

        static TetrisShape CreateRandomPiece() {
            int index = rand.Next(Shapes.Length);
            return new TetrisShape(Shapes[index], startX: GameBoard.Width / 2 - 1, startY: 0);
        }

        static void Main(string[] args) {
        StartGame:
            var board = new GameBoard();
            var currentPiece = CreateRandomPiece();
            bool gameOver = false;
            int dropCounter = 0;
            const int dropInterval = 500; // 500 милисекунд

            while (!gameOver) {

                // Обработка ввода
                if (Console.KeyAvailable) {
                    var key = Console.ReadKey(true).Key;
                    var backup = new TetrisShape(currentPiece.Shape, currentPiece.X, currentPiece.Y);

                    switch (key) {
                        case ConsoleKey.A:
                            currentPiece.X--;
                            if (!board.CanPlace(currentPiece)) currentPiece.X++;
                            break;
                        case ConsoleKey.D:
                            currentPiece.X++;
                            if (!board.CanPlace(currentPiece)) currentPiece.X--;
                            break;
                        case ConsoleKey.S:
                            currentPiece.Y++;
                            if (!board.CanPlace(currentPiece)) {
                                currentPiece.Y--;
                                board.Place(currentPiece);
                                currentPiece = CreateRandomPiece();
                                if (!board.CanPlace(currentPiece))
                                    gameOver = true;
                            }
                            break;
                        case ConsoleKey.W:
                            currentPiece.Rotate();
                            if (!board.CanPlace(currentPiece)) currentPiece = backup;
                            break;
                        case ConsoleKey.Q:
                            return;
                    }
                }

                // Автоматическое падение
                dropCounter += 100;
                if (dropCounter >= dropInterval) {
                    dropCounter = 0;
                    var backup = new TetrisShape(currentPiece.Shape, currentPiece.X, currentPiece.Y);
                    currentPiece.Y++;
                    if (!board.CanPlace(currentPiece)) {
                        currentPiece.Y--;
                        board.Place(currentPiece);
                        currentPiece = CreateRandomPiece();
                        if (!board.CanPlace(currentPiece))
                            gameOver = true;
                    }
                }

                board.Draw(currentPiece);
                Thread.Sleep(100);
            }

            Console.WriteLine("\nИгра окончена! Нажмите R для повторной игры или любую другую клавишу для выхода.");
            var restartKey = Console.ReadKey().Key;
            if (restartKey == ConsoleKey.R)
                goto StartGame;
        }
    }
}