using Labyrinth.Crawl;
using Labyrinth.Tiles;

namespace Labyrinth
{
    /// <summary>
    /// Explorer class that attempts to find the exit from a labyrinth using a movement strategy.
    /// </summary>
    public class Explorer
    {
        private readonly ICrawler _crawler;
        private readonly IMovementStrategy _strategy;

        /// <summary>
        /// Initialize an explorer with a crawler and movement strategy.
        /// </summary>
        /// <param name="crawler">The crawler to control for exploration.</param>
        /// <param name="strategy">Optional movement strategy. If null, uses RandomMovementStrategy.</param>
        public Explorer(ICrawler crawler, IMovementStrategy? strategy = null)
        {
            _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));
            _strategy = strategy ?? new RandomMovementStrategy();
        }

        /// <summary>
        /// Initialize an explorer with a crawler and optional random number generator for random movement.
        /// </summary>
        /// <param name="crawler">The crawler to control for exploration.</param>
        /// <param name="random">Optional random number generator for reproducible tests. If null, a new instance is created.</param>
        [Obsolete("Use Explorer(ICrawler, IMovementStrategy) instead for better extensibility.", false)]
        public Explorer(ICrawler crawler, Random? random = null)
            : this(crawler, new RandomMovementStrategy(random))
        {
        }

        /// <summary>
        /// Attempt to reach the exit with the configured movement strategy.
        /// </summary>
        /// <param name="maxMoves">Maximum number of moves before giving up.</param>
        /// <returns>True if the exit (Outside tile) is reached; false if maxMoves is exceeded.</returns>
        public bool GetOut(int maxMoves)
        {
            for (int move = 0; move < maxMoves; move++)
            {
                _strategy.Execute(_crawler);

                // Check if we reached the exit
                if (_crawler.FacingTile is Outside)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
