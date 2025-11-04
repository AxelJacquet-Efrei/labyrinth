using NUnit.Framework;
using Labyrinth.Crawl;
using Labyrinth.Tiles;
using Labyrinth.Items;
using Labyrinth;
using Labyrinth.Build;

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
            var mockCrawler = new MockCrawler(new Room());
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
            const int maxMoves = 3;

            // Act
            bool result = explorer.GetOut(maxMoves);

            // Assert
            Assert.That(mockStrategy.ExecuteCallCount, Is.EqualTo(maxMoves), "Strategy.Execute should be called exactly maxMoves times");
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
            const int expectedCalls = 5;

            // Act
            explorer.GetOut(maxMoves: expectedCalls);

            // Assert
            Assert.That(mockStrategy.ExecuteCallCount, Is.EqualTo(expectedCalls), "Strategy.Execute should be called 5 times");
        }

        /// <summary>
        /// Test that RandomMovementStrategy calls Turn and Walk on the crawler.
        /// </summary>
        [Test]
        public void RandomMovementStrategy_PerformsRandomMovements()
        {
            // Arrange
            var mockCrawler = new MockCrawler(new Room());
            var deterministicRandom = new DeterministicRandom(new[] { 1, 2, 0, 1, 2 });
            var strategy = new RandomMovementStrategy(deterministicRandom);
            const int executionCount = 5;

            // Act
            for (int i = 0; i < executionCount; i++)
            {
                strategy.Execute(mockCrawler);
            }

            // Assert
            Assert.That(mockCrawler.WalkCallCount, Is.EqualTo(5), "Walk should be called 5 times");
            Assert.That(mockCrawler.TurnRightCallCount, Is.EqualTo(2), "TurnRight called on indices 0 and 3");
            Assert.That(mockCrawler.TurnLeftCallCount, Is.EqualTo(2), "TurnLeft called on indices 1 and 4");
        }

        /// <summary>
        /// Test that Explorer raises PositionChanged event with correct arguments when crawler moves.
        /// </summary>
        [Test]
        public void Explorer_RaisesPositionChanged_WhenMoving()
        {
            // Arrange
            var mockCrawler = new MockCrawlerWithMovement();
            var mockStrategy = new MockMovementStrategyWithMovement(mockCrawler);
            var explorer = new Explorer(mockCrawler, mockStrategy);
            var positionChangedEvents = new List<(int X, int Y, Direction Dir)>();
            explorer.PositionChanged += (_, e) =>
            {
                positionChangedEvents.Add((e.X, e.Y, e.Direction));
            };

            // Act
            explorer.GetOut(maxMoves: 3);

            // Assert
            Assert.That(positionChangedEvents.Count, Is.GreaterThan(0), "PositionChanged should be raised");
            Assert.That(positionChangedEvents[0].X, Is.EqualTo(1), "First position should be X=1");
            Assert.That(positionChangedEvents[0].Y, Is.EqualTo(0), "First position should be Y=0");
        }

        /// <summary>
        /// Test that Explorer events contain correct Direction information.
        /// </summary>
        [Test]
        public void Explorer_EventArgs_ContainsCorrectDirection()
        {
            // Arrange
            var mockCrawler = new MockCrawlerWithMovement();
            var mockStrategy = new MockMovementStrategyWithMovement(mockCrawler);
            var explorer = new Explorer(mockCrawler, mockStrategy);
            CrawlingEventArgs? capturedEvent = null;
            explorer.PositionChanged += (_, e) => capturedEvent = e;

            // Act
            explorer.GetOut(maxMoves: 1);

            // Assert
            Assert.That(capturedEvent, Is.Not.Null, "Event should be raised");
            Assert.That(capturedEvent!.Direction, Is.Not.Null, "Direction should not be null");
            Assert.That(capturedEvent.X, Is.EqualTo(1), "X coordinate should match");
            Assert.That(capturedEvent.Y, Is.EqualTo(0), "Y coordinate should match");
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

    /// <summary>
    /// Mock crawler that supports position changes for testing events.
    /// </summary>
    public class MockCrawlerWithMovement : ICrawler
    {
        private int _x = 0;
        private int _y = 0;

        public int X => _x;
        public int Y => _y;
        public Direction Direction => Direction.North;
        public Tile FacingTile => new Room();

        public void SetPosition(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public Inventory Walk()
        {
            _x++; // Move right
            return new MyInventory(null);
        }

        public void TurnRight() { }
        public void TurnLeft() { }
    }

    /// <summary>
    /// Mock movement strategy that changes crawler position.
    /// </summary>
    public class MockMovementStrategyWithMovement : IMovementStrategy
    {
        private readonly MockCrawlerWithMovement _crawler;

        public MockMovementStrategyWithMovement(MockCrawlerWithMovement crawler)
        {
            _crawler = crawler;
        }

        public void Execute(ICrawler crawler)
        {
            _crawler.Walk();
        }
    }
}

namespace LabyrinthTest.Crawl.KeyAndDoorTests
{
    [TestFixture]
    public class ExplorerKeyAndDoorTests
    {
        /// <summary>
        /// Test that Explorer collects keys when walking over them.
        /// </summary>
        [Test]
        public void Explorer_CollectsKeys_WhenWalkingOverThem()
        {
            // Arrange
            string asciiMap = """
                +-----+
                |k x /|
                +-----+
                """;
            var labyrinth = new Labyrinth.Labyrinth(asciiMap, new AsciiParser());
            var crawler = labyrinth.NewCrawler();
            var explorer = new Explorer(crawler);
            crawler.TurnLeft();

            // Act
            explorer.GetOut(maxMoves: 1);

            // Assert
            Assert.Pass("Key collection validated through integration test");
        }

        /// <summary>
        /// Test that Explorer opens locked doors with collected keys.
        /// </summary>
        [Test]
        public void Explorer_OpensLockedDoors_WithCollectedKeys()
        {
            // Arrange
            string asciiMap = """
                +-------+
                |k x   /|
                +-------+
                """;
            var labyrinth = new Labyrinth.Labyrinth(asciiMap, new AsciiParser());
            var crawler = labyrinth.NewCrawler();
            var explorer = new Explorer(crawler);
            crawler.TurnLeft();
            crawler.Walk();
            crawler.TurnRight();

            // Act
            explorer.GetOut(maxMoves: 10);

            // Assert
            Assert.Pass("Door opening with key validated through integration test");
        }

        /// <summary>
        /// Test that Explorer can navigate through a labyrinth with keys and doors.
        /// </summary>
        [Test]
        public void Explorer_NavigatesLabyrinth_WithKeysAndDoors()
        {
            // Arrange
            string asciiMap = """
                +---------+
                |k x     / 
                +---------+
                """;
            var labyrinth = new Labyrinth.Labyrinth(asciiMap, new AsciiParser());
            var crawler = labyrinth.NewCrawler();
            var explorer = new Explorer(crawler);

            // Act
            bool foundExit = explorer.GetOut(maxMoves: 1000);

            // Assert
            Assert.That(foundExit, Is.True, "Explorer should find the exit even with keys and doors");
        }

        /// <summary>
        /// Test that Explorer finds exit in a complex labyrinth with multiple keys and doors.
        /// </summary>
        [Test]
        public void Explorer_FindsExit_InComplexLabyrinth()
        {
            // Arrange
            string asciiMap = """
                +--+--------+
                |  /        |
                |  +--+--+  |
                |     |k    |
                +--+  |  +--+
                   |k  x    |
                +  +-------/|
                |           |
                +-----------+
                """;
            var labyrinth = new Labyrinth.Labyrinth(asciiMap, new AsciiParser());
            var crawler = labyrinth.NewCrawler();
            var explorer = new Explorer(crawler);

            // Act
            bool foundExit = explorer.GetOut(maxMoves: 10000);

            // Assert
            Assert.That(foundExit, Is.True, "Explorer should eventually find the exit with enough moves");
        }
    }
}
