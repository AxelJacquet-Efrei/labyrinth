using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Tiles;
using Labyrinth.Build;

namespace LabyrinthTest.Crawl;

[TestFixture(Description = "Integration test for the crawler implementation in the labyrinth")]
public class LabyrinthCrawlerTest
{
    private static ICrawler NewCrawlerFor(string asciiMap) =>
        new Labyrinth.Labyrinth(asciiMap, new AsciiParser()).NewCrawler();

    #region Initialization
    [Test]
    public void InitWithCenteredX()
    {
        // Arrange
        string asciiMap = """
            +--+
            | x|
            +--+
            """;

        // Act
        var test = NewCrawlerFor(asciiMap);

        // Assert
        Assert.That(test.X, Is.EqualTo(2));
        Assert.That(test.Y, Is.EqualTo(1));
        Assert.That(test.Direction, Is.EqualTo(Direction.North));
        Assert.That(test.FacingTile, Is.TypeOf<Wall>());
    }

    [Test]
    public void InitWithMultipleXUsesLastOne()
    {
        // Arrange
        string asciiMap = """
            +--+
            | x|
            |x |
            +--+
            """;

        // Act
        var test = NewCrawlerFor(asciiMap);

        // Assert
        Assert.That(test.X, Is.EqualTo(1));
        Assert.That(test.Y, Is.EqualTo(2));
        Assert.That(test.Direction, Is.EqualTo(Direction.North));
        Assert.That(test.FacingTile, Is.TypeOf<Room>());
    }

    [Test]
    public void InitWithNoXThrowsArgumentException()
    {
        // Arrange
        string asciiMap = """
            +--+
            |  |
            +--+
            """;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Labyrinth.Labyrinth(asciiMap, new AsciiParser())
        );
    }
    #endregion

    #region Labyrinth borders
    [Test]
    public void FacingNorthOnUpperTileReturnsOutside()
    {
        // Arrange
        string asciiMap = """
            +x+
            | |
            +-+
            """;

        // Act
        var test = NewCrawlerFor(asciiMap);

        // Assert
        Assert.That(test.X, Is.EqualTo(1));
        Assert.That(test.Y, Is.EqualTo(0));
        Assert.That(test.Direction, Is.EqualTo(Direction.North));
        Assert.That(test.FacingTile, Is.TypeOf<Outside>());
    }

    [Test]
    public void FacingWestOnFarLeftTileReturnsOutside()
    {
        // Arrange
        string asciiMap = """
            +-+
            x |
            +-+
            """;
        var test = NewCrawlerFor(asciiMap);

        // Act
        test.Direction.TurnLeft();

        // Assert
        Assert.That(test.X, Is.EqualTo(0));
        Assert.That(test.Y, Is.EqualTo(1));
        Assert.That(test.Direction, Is.EqualTo(Direction.West));
        Assert.That(test.FacingTile, Is.TypeOf<Outside>());
    }

    [Test]
    public void FacingEastOnFarRightTileReturnsOutside()
    {
        // Arrange
        string asciiMap = """
            +-+
            | x
            +-+
            """;
        var test = NewCrawlerFor(asciiMap);

        // Act
        test.Direction.TurnRight();

        // Assert
        Assert.That(test.X, Is.EqualTo(2));
        Assert.That(test.Y, Is.EqualTo(1));
        Assert.That(test.Direction, Is.EqualTo(Direction.East));
        Assert.That(test.FacingTile, Is.TypeOf<Outside>());
    }

    [Test]
    public void FacingSouthOnBottomTileReturnsOutside()
    {
        // Arrange
        string asciiMap = """
            +-+
            | |
            +x+
            """;
        var test = NewCrawlerFor(asciiMap);

        // Act
        test.Direction.TurnLeft();
        test.Direction.TurnLeft();

        // Assert
        Assert.That(test.X, Is.EqualTo(1));
        Assert.That(test.Y, Is.EqualTo(2));
        Assert.That(test.Direction, Is.EqualTo(Direction.South));
        Assert.That(test.FacingTile, Is.TypeOf<Outside>());
    }
    #endregion

    #region Moves
    [Test]
    public void TurnLeftFacesWestTile()
    {
        // Arrange
        string asciiMap = """
            +---+
            |/xk|
            +---+
            """;
        var test = NewCrawlerFor(asciiMap);

        // Act
        test.Direction.TurnLeft();

        // Assert
        Assert.That(test.X, Is.EqualTo(2));
        Assert.That(test.Y, Is.EqualTo(1));
        Assert.That(test.Direction, Is.EqualTo(Direction.West));
        Assert.That(test.FacingTile, Is.TypeOf<Door>());
    }

    [Test]
    public void WalkReturnsInventoryAndChangesPositionAndFacingTile()
    {
        // Arrange
        string asciiMap = """
            +/-+
            |  |
            |xk|
            +--+
            """;
        var test = NewCrawlerFor(asciiMap);

        // Act
        var inventory = test.Walk();

        // Assert
        Assert.That(inventory.HasItem, Is.False);
        Assert.That(test.X, Is.EqualTo(1));
        Assert.That(test.Y, Is.EqualTo(1));
        Assert.That(test.Direction, Is.EqualTo(Direction.North));
        Assert.That(test.FacingTile, Is.TypeOf<Door>());
    }

    [Test]
    public void TurnAndWalkReturnsInventoryChangesPositionAndFacingTile()
    {
        // Arrange
        string asciiMap = """
            +--+
            |x |
            +--+
            """;
        var test = NewCrawlerFor(asciiMap);
        test.Direction.TurnRight();

        // Act
        var inventory = test.Walk();

        // Assert
        Assert.That(inventory.HasItem, Is.False);
        Assert.That(test.X, Is.EqualTo(2));
        Assert.That(test.Y, Is.EqualTo(1));
        Assert.That(test.Direction, Is.EqualTo(Direction.East));
        Assert.That(test.FacingTile, Is.TypeOf<Wall>());
    }

    [Test]
    public void WalkOnNonTraversableTileThrowsInvalidOperationExceptionAndDontMove()
    {
        // Arrange
        string asciiMap = """
            +--+
            |/-+
            |xk|
            +--+
            """;
        var test = NewCrawlerFor(asciiMap);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => test.Walk());
        Assert.That(test.X, Is.EqualTo(1));
        Assert.That(test.Y, Is.EqualTo(2));
        Assert.That(test.Direction, Is.EqualTo(Direction.North));
        Assert.That(test.FacingTile, Is.TypeOf<Door>());
    }

    [Test]
    public void WalkOutsideThrowsInvalidOperationExceptionAndDontMove()
    {
        // Arrange
        string asciiMap = """
            |x|
            | |
            +-+
            """;
        var test = NewCrawlerFor(asciiMap);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => test.Walk());
        Assert.That(test.X, Is.EqualTo(1));
        Assert.That(test.Y, Is.EqualTo(0));
        Assert.That(test.Direction, Is.EqualTo(Direction.North));
        Assert.That(test.FacingTile, Is.TypeOf<Outside>());
    }
    #endregion

    #region Items and doors
    [Test]
    public void WalkInARoomWithAnItem()
    {
        // Arrange
        string asciiMap = """
        +---+
        |  k|
        |/ x|
        +---+
        """;
        var test = NewCrawlerFor(asciiMap);

        // Act
        var inventory = test.Walk();

        // Assert
        Assert.That(inventory.HasItem, Is.True);
        Assert.That(inventory.ItemType, Is.EqualTo(typeof(Key)));
    }

    [Test]
    public void WalkUseAWrongKeyToOpenADoor()
    {
        // Arrange
        string asciiMap = """
            +---+
            |/ k|
            |k  |
            |x /|
            +---+
            """;
        var test = NewCrawlerFor(asciiMap);

        // Act
        var inventory = test.Walk();
        var door = (Door)test.FacingTile;
        bool doorOpened = door.Open(inventory);

        // Assert
        Assert.That(doorOpened, Is.False);
        Assert.That(door.IsLocked, Is.True);
        Assert.That(door.IsTraversable, Is.False);
        Assert.That(inventory.HasItem, Is.True);
    }

    [Test]
    public void WalkUseKeyToOpenADoorAndPass()
    {
        // Arrange
        string asciiMap = """
            +--+
            |xk|
            +-/|
            """;
        var laby = new Labyrinth.Labyrinth(asciiMap, new AsciiParser());
        var test = laby.NewCrawler();
        test.Direction.TurnRight();

        // Act
        var inventory = test.Walk();
        test.Direction.TurnRight();
        ((Door)test.FacingTile).Open(inventory);
        test.Walk();

        // Assert
        Assert.That(test.X, Is.EqualTo(2));
        Assert.That(test.Y, Is.EqualTo(2));
        Assert.That(test.Direction, Is.EqualTo(Direction.South));
        Assert.That(test.FacingTile, Is.TypeOf<Outside>());
    }
    #endregion
}
