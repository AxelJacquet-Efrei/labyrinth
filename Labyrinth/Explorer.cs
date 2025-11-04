using Labyrinth.Crawl;
using Labyrinth.Tiles;
using Labyrinth.Build;
using Labyrinth.Items;

namespace Labyrinth
{
    /// <summary>
    /// Explorer class that attempts to find the exit from a labyrinth using a movement strategy.
    /// </summary>
    public class Explorer
    {
        private readonly ICrawler _crawler;
        private readonly IMovementStrategy _strategy;
        private readonly List<Inventory> _bag = new();

        /// <summary>
        /// Event raised when the explorer's position changes.
        /// </summary>
        public event EventHandler<CrawlingEventArgs>? PositionChanged;

        /// <summary>
        /// Event raised when the explorer's direction changes.
        /// </summary>
        public event EventHandler<CrawlingEventArgs>? DirectionChanged;

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
        /// Attempt to reach the exit with the configured movement strategy.
        /// </summary>
        /// <param name="maxMoves">Maximum number of moves before giving up.</param>
        /// <returns>True if the exit (Outside tile) is reached; false if maxMoves is exceeded.</returns>
        public bool GetOut(int maxMoves)
        {
            for (int move = 0; move < maxMoves; move++)
            {
                int previousX = _crawler.X;
                int previousY = _crawler.Y;
                int previousDeltaX = _crawler.Direction.DeltaX;
                int previousDeltaY = _crawler.Direction.DeltaY;

                try
                {
                    if (_crawler.FacingTile is Door door && door.IsLocked)
                    {
                        var keyInventory = FindKeyInventory();
                        if (keyInventory != null)
                        {
                            if (door.Open(keyInventory) && !keyInventory.HasItem)
                            {
                                _bag.Remove(keyInventory);
                            }
                        }
                    }

                    var wrapper = new CrawlerWrapper(_crawler, _bag);
                    _strategy.Execute(wrapper);
                }
                catch (InvalidOperationException)
                {
                    // A walk attempt failed (e.g. wall). Ignore and continue exploring.
                }

                if (_crawler.X != previousX || _crawler.Y != previousY)
                {
                    OnPositionChanged(_crawler.X, _crawler.Y);
                }

                if (_crawler.Direction.DeltaX != previousDeltaX || _crawler.Direction.DeltaY != previousDeltaY)
                {
                    OnDirectionChanged(_crawler.X, _crawler.Y);
                }

                if (_crawler.FacingTile is Outside)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find a key inventory in the bag.
        /// </summary>
        private Inventory? FindKeyInventory()
        {
            foreach (var inventory in _bag)
            {
                if (inventory.HasItem && inventory.ItemType == typeof(Key))
                {
                    return inventory;
                }
            }
            return null;
        }

        /// <summary>
        /// Raise the PositionChanged event.
        /// </summary>
        private void OnPositionChanged(int x, int y)
        {
            PositionChanged?.Invoke(this, new CrawlingEventArgs(x, y, _crawler.Direction));
        }

        /// <summary>
        /// Raise the DirectionChanged event.
        /// </summary>
        private void OnDirectionChanged(int x, int y)
        {
            DirectionChanged?.Invoke(this, new CrawlingEventArgs(x, y, _crawler.Direction));
        }

        /// <summary>
        /// Wrapper around ICrawler to capture inventory from Walk().
        /// </summary>
        private class CrawlerWrapper : ICrawler
        {
            private readonly ICrawler _crawler;
            private readonly List<Inventory> _bag;

            public CrawlerWrapper(ICrawler crawler, List<Inventory> bag)
            {
                _crawler = crawler;
                _bag = bag;
            }

            public int X => _crawler.X;
            public int Y => _crawler.Y;
            public Direction Direction => _crawler.Direction;
            public Tile FacingTile => _crawler.FacingTile;

            public Inventory Walk()
            {
                var inventory = _crawler.Walk();
                if (inventory.HasItem)
                {
                    var slot = new MyInventory();
                    slot.MoveItemFrom(inventory);
                    _bag.Add(slot);
                }
                return inventory;
            }

            public void TurnLeft() => _crawler.TurnLeft();
            public void TurnRight() => _crawler.TurnRight();
        }
    }
}
