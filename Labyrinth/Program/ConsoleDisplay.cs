using Labyrinth.Tiles;
using Labyrinth.Crawl;

namespace Labyrinth.Program
{
    /// <summary>
    /// Console-based display implementation for the labyrinth and explorer.
    /// </summary>
    public class ConsoleDisplay : IDisplay
    {
        private Tile[,]? _grid;
        private (int X, int Y) _lastPosition;
        private char _lastSymbol = ' ';

        /// <summary>
        /// Display the labyrinth grid and initial explorer position.
        /// </summary>
        public void ShowLabyrinth(Tile[,] grid, int startX, int startY, Direction startDirection)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Console.Clear();

            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    Console.Write(GetTileChar(grid[x, y]));
                }
                Console.WriteLine();
            }

            _lastPosition = (startX, startY);
            _lastSymbol = GetDirectionSymbol(startDirection);
            Console.SetCursorPosition(startX, startY);
            Console.Write(_lastSymbol);
            Console.SetCursorPosition(0, grid.GetLength(1)); // Move cursor below grid
        }

        /// <summary>
        /// Get the character representation of a tile.
        /// </summary>
        private static char GetTileChar(Tile tile)
        {
            return tile switch
            {
                Room room => room.HasItem ? 'k' : ' ',
                Wall => '#',
                Door => '/',
                Outside => ' ',
                _ => '?'
            };
        }

        /// <summary>
        /// Update the explorer's position and direction on the display.
        /// </summary>
        public void UpdateExplorerPosition(int x, int y, Direction direction)
        {
            if (_grid == null)
                return;

            Console.SetCursorPosition(_lastPosition.X, _lastPosition.Y);
            Console.Write(' ');

            char symbol = GetDirectionSymbol(direction);
            Console.SetCursorPosition(x, y);
            Console.Write(symbol);

            _lastPosition = (x, y);
            _lastSymbol = symbol;

            Console.SetCursorPosition(0, _grid.GetLength(1));
        }

        /// <summary>
        /// Clear the console display.
        /// </summary>
        public void Clear()
        {
            Console.Clear();
        }

        /// <summary>
        /// Convert a Direction to its visual representation.
        /// </summary>
        private static char GetDirectionSymbol(Direction direction)
        {
            if (direction.DeltaX == 0 && direction.DeltaY == -1) return '^';
            if (direction.DeltaX == 1 && direction.DeltaY == 0) return '>';
            if (direction.DeltaX == 0 && direction.DeltaY == 1) return 'v';
            if (direction.DeltaX == -1 && direction.DeltaY == 0) return '<';

            return '?';
        }
    }
}
