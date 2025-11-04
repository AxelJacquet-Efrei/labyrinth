using Labyrinth.Crawl;

namespace LabyrinthTest.Crawl;

[TestFixture(Description = "Direction unit test class")]
public class DirectionTest
{
    [Test]
    public void TestInitNorth()
    {
        // Arrange
        var direction = Direction.North;

        // Act
        var deltaX = direction.DeltaX;
        var deltaY = direction.DeltaY;

        // Assert
        Assert.That(deltaX, Is.EqualTo(0));
        Assert.That(deltaY, Is.EqualTo(-1));
    }

    [Test]
    public void TestInitSouth()
    {
        // Arrange
        var direction = Direction.South;

        // Act
        var deltaX = direction.DeltaX;
        var deltaY = direction.DeltaY;

        // Assert
        Assert.That(deltaX, Is.EqualTo(0));
        Assert.That(deltaY, Is.EqualTo(1));
    }

    [Test]
    public void TestInitEast()
    {
        // Arrange
        var direction = Direction.East;

        // Act
        var deltaX = direction.DeltaX;
        var deltaY = direction.DeltaY;

        // Assert
        Assert.That(deltaX, Is.EqualTo(1));
        Assert.That(deltaY, Is.EqualTo(0));
    }

    [Test]
    public void TestInitWest()
    {
        // Arrange
        var direction = Direction.West;

        // Act
        var deltaX = direction.DeltaX;
        var deltaY = direction.DeltaY;

        // Assert
        Assert.That(deltaX, Is.EqualTo(-1));
        Assert.That(deltaY, Is.EqualTo(0));
    }

    [Test]
    public void TestTurnRightFromNorthGoesEast()
    {
        // Arrange
        var test = Direction.North;

        // Act
        test.TurnRight();

        // Assert
        Assert.That(test, Is.EqualTo(Direction.East));
    }

    [Test]
    public void TestTurnRightThenLeftStillTheSame()
    {
        // Arrange
        var test = Direction.East;

        // Act
        test.TurnRight();
        test.TurnLeft();

        // Assert
        Assert.That(test, Is.EqualTo(Direction.East));
    }

    [Test]
    public void TestTurnLeftFromNorthGoesWest()
    {
        // Arrange
        var test = Direction.North;

        // Act
        test.TurnLeft();

        // Assert
        Assert.That(test, Is.EqualTo(Direction.West));
    }

    [Test]
    public void TestTurnLeftTwiceFromWestGoesEast()
    {
        // Arrange
        var test = Direction.West;

        // Act
        test.TurnLeft();
        test.TurnLeft();

        // Assert
        Assert.That(test, Is.EqualTo(Direction.East));
    }

    [Test]
    public void TestTurnRightFourTimesStillTheSame()
    {
        // Arrange
        var test = Direction.East;

        // Act
        test.TurnRight();
        test.TurnRight();
        test.TurnRight();
        test.TurnRight();

        // Assert
        Assert.That(test, Is.EqualTo(Direction.East));
    }

}
