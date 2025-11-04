using Labyrinth.Tiles;

namespace Labyrinth.Build
{
    /// <summary>
    /// Interface for parsing ASCII maps into tile grids.
    /// </summary>
    public interface IAsciiParser
    {
        /// <summary>
        /// Event raised when a start position ('x') is found in the map.
        /// </summary>
        event EventHandler<StartEventArgs>? StartPositionFound;

        /// <summary>
        /// Parses an ASCII map into a 2D tile array.
        /// </summary>
        /// <param name="ascii_map">The ASCII representation of the map.</param>
        /// <returns>A 2D array of tiles.</returns>
        Tile[,] Parse(string ascii_map);
    }
}

