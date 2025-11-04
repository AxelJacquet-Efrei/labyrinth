namespace Labyrinth.Crawl
{
    /// <summary>
    /// Defines a strategy for movement within the labyrinth.
    /// </summary>
    public interface IMovementStrategy
    {
        /// <summary>
        /// Execute a movement action on the crawler (turn and/or walk).
        /// </summary>
        /// <param name="crawler">The crawler to control.</param>
        void Execute(ICrawler crawler);
    }
}

