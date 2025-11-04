using Labyrinth.Tiles;
using Labyrinth.Crawl;

namespace Labyrinth.Program
{
    /// <summary>
    /// Interface for displaying the labyrinth and explorer state.
    /// Allows different display implementations (Console, GUI, Web...).
    /// </summary>
    public interface IDisplay
    {
        /// <summary>
        /// Display the labyrinth grid.
        /// </summary>
        /// <param name="grid">The tile grid to display.</param>
        /// <param name="startX">Starting X position of the explorer.</param>
        /// <param name="startY">Starting Y position of the explorer.</param>
        /// <param name="startDirection">Starting direction of the explorer.</param>
        void ShowLabyrinth(Tile[,] grid, int startX, int startY, Direction startDirection);

        /// <summary>
        /// Update the explorer's position and direction on display.
        /// </summary>
        /// <param name="x">New X coordinate.</param>
        /// <param name="y">New Y coordinate.</param>
        /// <param name="direction">New direction.</param>
        void UpdateExplorerPosition(int x, int y, Direction direction);

        /// <summary>
        /// Clear the display.
        /// </summary>
        void Clear();
    }
}

