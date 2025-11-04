namespace Labyrinth.Build
{
    /// <summary>
    /// Event arguments for start position found event.
    /// </summary>
    public class StartEventArgs : EventArgs
    {
        /// <summary>
        /// X coordinate of the start position.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y coordinate of the start position.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Initializes a new instance of StartEventArgs.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public StartEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}

