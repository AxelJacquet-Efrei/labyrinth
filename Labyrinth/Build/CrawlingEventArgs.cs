using Labyrinth.Crawl;

namespace Labyrinth.Build
{
    /// <summary>
    /// Event arguments for crawler movement events (position and direction changes).
    /// </summary>
    public class CrawlingEventArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public Direction Direction { get; }

        /// <summary>
        /// Initialize crawling event arguments.
        /// </summary>
        /// <param name="x">The X coordinate of the crawler.</param>
        /// <param name="y">The Y coordinate of the crawler.</param>
        /// <param name="direction">The direction the crawler is facing.</param>
        public CrawlingEventArgs(int x, int y, Direction direction)
        {
            X = x;
            Y = y;
            Direction = direction;
        }
    }
}

