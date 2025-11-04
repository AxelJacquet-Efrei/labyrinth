using NUnit.Framework;
using Labyrinth.Crawl;
using Labyrinth.Tiles;
using Labyrinth.Items;
using Labyrinth;

namespace LabyrinthTest.Crawl
{
    [TestFixture]
    public class ExplorerTest
    {
        /// <summary>
        /// Test that GetOut returns true when the exit (Outside tile) is reachable.
        /// </summary>
        [Test]
        public void Explorer_ReturnsTrue_WhenOutsideReached()
        {
            // Arrange
            var mockCrawler = new MockCrawler(Outside.Singleton);
            var mockStrategy = new MockMovementStrategy();
            var explorer = new Explorer(mockCrawler, mockStrategy);

            // Act
            bool result = explorer.GetOut(maxMoves: 10);

            // Assert
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test that GetOut returns false when max moves are exhausted without finding the exit.
        /// </summary>
        [Test]
        public void Explorer_ReturnsFalse_WhenMaxMovesReachedWithoutExit()
        {
            // Arrange
            var mockCrawler = new MockCrawler(new Room()); // Room, not Outside
            var mockStrategy = new MockMovementStrategy();
            var explorer = new Explorer(mockCrawler, mockStrategy);

            // Act
            bool result = explorer.GetOut(maxMoves: 5);

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test that GetOut respects the maxMoves limit.
        /// </summary>
        [Test]
        public void Explorer_StopsAfterMaxMoves()
        {
            // Arrange
            var mockCrawler = new MockCrawler(new Room());
            var mockStrategy = new MockMovementStrategy();
            var explorer = new Explorer(mockCrawler, mockStrategy);

            // Act
            bool result = explorer.GetOut(maxMoves: 3);

            // Assert
            Assert.That(mockStrategy.ExecuteCallCount, Is.EqualTo(3), "Strategy.Execute should be called exactly maxMoves times");
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test that explorer uses the movement strategy correctly.
        /// </summary>
        [Test]
        public void Explorer_UsesMovementStrategy()
        {
            // Arrange
            var mockCrawler = new MockCrawler(new Room());
            var mockStrategy = new MockMovementStrategy();
            var explorer = new Explorer(mockCrawler, mockStrategy);

            // Act
            explorer.GetOut(maxMoves: 5);

            // Assert
            Assert.That(mockStrategy.ExecuteCallCount, Is.EqualTo(5), "Strategy.Execute should be called 5 times");
        }

        /// <summary>
        /// Test that RandomMovementStrategy calls Turn and Walk on the crawler.
        /// </summary>
        [Test]
        public void RandomMovementStrategy_PerformsRandomMovements()
        {
            // Arrange
            var mockCrawler = new MockCrawler(new Room());
            var deterministicRandom = new DeterministicRandom(new[] { 1, 2, 0, 1, 2 }); // Fixed sequence
            var strategy = new RandomMovementStrategy(deterministicRandom);

            // Act
            for (int i = 0; i < 5; i++)
            {
                strategy.Execute(mockCrawler);
            }

            // Assert
            Assert.That(mockCrawler.WalkCallCount, Is.EqualTo(5), "Walk should be called 5 times");
            Assert.That(mockCrawler.TurnRightCallCount, Is.EqualTo(2), "TurnRight called on indices 0 and 3");
            Assert.That(mockCrawler.TurnLeftCallCount, Is.EqualTo(2), "TurnLeft called on indices 1 and 4");
        }
    }

    /// <summary>
    /// Mock crawler for testing Explorer behavior without a real labyrinth.
    /// </summary>
    public class MockCrawler : ICrawler
    {
        private readonly Tile _facingTile;

        public int X => 0;
        public int Y => 0;
        public Direction Direction => Direction.North;
        public Tile FacingTile => _facingTile;

        public int WalkCallCount { get; private set; }
        public int TurnRightCallCount { get; private set; }
        public int TurnLeftCallCount { get; private set; }

        public MockCrawler(Tile facingTile)
        {
            _facingTile = facingTile;
        }

        public Inventory Walk()
        {
            WalkCallCount++;
            return new MyInventory(null);
        }

        public void TurnRight()
        {
            TurnRightCallCount++;
        }

        public void TurnLeft()
        {
            TurnLeftCallCount++;
        }
    }

    /// <summary>
    /// Mock movement strategy for testing Explorer's strategy usage.
    /// </summary>
    public class MockMovementStrategy : IMovementStrategy
    {
        public int ExecuteCallCount { get; private set; }

        public void Execute(ICrawler crawler)
        {
            ExecuteCallCount++;
            crawler.Walk();
        }
    }

    /// <summary>
    /// Deterministic random for reproducible test sequences.
    /// </summary>
    public class DeterministicRandom : Random
    {
        private readonly int[] _sequence;
        private int _index;

        public DeterministicRandom(int[] sequence)
        {
            _sequence = sequence;
            _index = 0;
        }

        public override int Next(int maxValue)
        {
            if (_index >= _sequence.Length)
                throw new InvalidOperationException("Deterministic random sequence exhausted");

            return _sequence[_index++] % maxValue;
        }
    }
}
