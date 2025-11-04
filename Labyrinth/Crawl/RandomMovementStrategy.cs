namespace Labyrinth.Crawl
{
    /// <summary>
    /// Movement strategy that performs random turns followed by a walk.
    /// </summary>
    public class RandomMovementStrategy : IMovementStrategy
    {
        private readonly Random _random;

        /// <summary>
        /// Initialize the random movement strategy with an optional random number generator.
        /// </summary>
        /// <param name="random">Optional random number generator for reproducible tests. If null, a new instance is created.</param>
        public RandomMovementStrategy(Random? random = null)
        {
            _random = random ?? new Random();
        }

        /// <summary>
        /// Execute a random movement: optionally turn, then walk.
        /// </summary>
        /// <param name="crawler">The crawler to control.</param>
        public void Execute(ICrawler crawler)
        {
            // Random turn: 0 = no turn, 1 = turn right, 2 = turn left
            int turn = _random.Next(3);
            switch (turn)
            {
                case 1:
                    crawler.TurnRight();
                    break;
                case 2:
                    crawler.TurnLeft();
                    break;
                // case 0: no turn
            }

            crawler.Walk();
        }
    }
}

