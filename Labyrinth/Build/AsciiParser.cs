using Labyrinth.Tiles;

namespace Labyrinth.Build
{
    public class AsciiParser : IAsciiParser
    {
        public event EventHandler<StartEventArgs>? StartPositionFound;

        public Tile[,] Parse(string ascii_map)
        {
            var lines = ascii_map.Split("\n,\r\n".Split(','), StringSplitOptions.None);
            var width = lines[0].Length;
            var tiles = new Tile[width, lines.Length];

            using var km = new Keymaster();

            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (lines[y].Length != width)
                    throw new ArgumentException("Invalid map: all lines must have the same length.");

                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    char ch = lines[y][x];
                    tiles[x, y] = ch switch
                    {
                        'x' => RaiseStartAndReturnRoom(x, y),
                        ' ' => new Room(),
                        '+' or '-' or '|' => Wall.Singleton,
                        '/' => km.NewDoor(),
                        'k' => km.NewKeyRoom(),
                        _ => throw new ArgumentException(
                            $"Invalid map: unknown character '{ch}' at line {y}, col {x}.")
                    };
                }
            }
            return tiles;
        }

        private Room RaiseStartAndReturnRoom(int x, int y)
        {
            StartPositionFound?.Invoke(this, new StartEventArgs(x, y));
            return new Room();
        }
    }
}