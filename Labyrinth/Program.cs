using Labyrinth;
using Labyrinth.Build;
using Labyrinth.Program;
using Labyrinth.Crawl;

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

/// <summary>
/// Converts direction deltas to a readable direction name.
/// </summary>
/// <param name="deltaX">Horizontal displacement (-1, 0, or 1).</param>
/// <param name="deltaY">Vertical displacement (-1, 0, or 1).</param>
/// <returns>The direction name (Nord, Est, Sud, Ouest) or "?" if unknown.</returns>
static string GetDirectionNameFromDeltas(int deltaX, int deltaY)
{
    if (deltaX == 0 && deltaY == -1) return "Nord";
    if (deltaX == 1 && deltaY == 0) return "Est";
    if (deltaX == 0 && deltaY == 1) return "Sud";
    if (deltaX == -1 && deltaY == 0) return "Ouest";
    return "?";
}

/// <summary>
/// Gets the direction name from a Direction object.
/// </summary>
/// <param name="direction">The Direction object containing DeltaX and DeltaY.</param>
/// <returns>The direction name (Nord, Est, Sud, Ouest).</returns>
static string GetDirectionName(Direction direction)
{
    return GetDirectionNameFromDeltas(direction.DeltaX, direction.DeltaY);
}

/// <summary>
/// Gets the visual symbol representing a direction.
/// </summary>
/// <param name="deltaX">Horizontal displacement (-1, 0, or 1).</param>
/// <param name="deltaY">Vertical displacement (-1, 0, or 1).</param>
/// <returns>The direction symbol (^, >, v, <) or '?' if unknown.</returns>
static char GetDirectionSymbol(int deltaX, int deltaY)
{
    if (deltaX == 0 && deltaY == -1) return '^';
    if (deltaX == 1 && deltaY == 0) return '>';
    if (deltaX == 0 && deltaY == 1) return 'v';
    if (deltaX == -1 && deltaY == 0) return '<';
    return '?';
}

Console.WriteLine("EXPLORATEUR DE LABYRINTHE");
Console.WriteLine("Symboles : ^ (Nord)  > (Est)  v (Sud)  < (Ouest)");
Console.WriteLine();

var labyrinth = new Labyrinth.Labyrinth(asciiMap, new AsciiParser());
var crawler = labyrinth.NewCrawler();
var explorer = new Explorer(crawler);
var display = new ConsoleDisplay();

display.ShowLabyrinth(labyrinth.Tiles, crawler.X, crawler.Y, crawler.Direction);

int moveCount = 0;
int directionChangeCount = 0;
var moveHistory = new List<(int Move, int X, int Y, int DeltaX, int DeltaY, string Action)>();

moveHistory.Add((0, crawler.X, crawler.Y, crawler.Direction.DeltaX, crawler.Direction.DeltaY, "Depart"));

explorer.PositionChanged += (_, e) =>
{
    moveCount++;
    display.UpdateExplorerPosition(e.X, e.Y, e.Direction);
    moveHistory.Add((moveCount, e.X, e.Y, e.Direction.DeltaX, e.Direction.DeltaY, "Deplacement"));
    
    // Afficher les stats en temps réel avec Console.SetCursorPosition
    Console.SetCursorPosition(0, labyrinth.Tiles.GetLength(1) + 1);
    Console.Write($"Deplacements: {moveCount}  Position: ({e.X},{e.Y})  Direction: {GetDirectionName(e.Direction)}     ");
    
    Thread.Sleep(50);
};

explorer.DirectionChanged += (_, e) =>
{
    directionChangeCount++;
    display.UpdateExplorerPosition(e.X, e.Y, e.Direction);
    moveHistory.Add((moveCount, e.X, e.Y, e.Direction.DeltaX, e.Direction.DeltaY, "Rotation"));
    
    Thread.Sleep(25);
};

int maxMoves = 1_000_000;
bool found = explorer.GetOut(maxMoves: maxMoves);

Console.SetCursorPosition(0, labyrinth.Tiles.GetLength(1) + 3);
Console.WriteLine();

if (found)
{
    Console.WriteLine("SORTIE TROUVEE !");
}
else
{
    Console.WriteLine($"Sortie non trouvee apres {maxMoves} deplacements");
}

Console.WriteLine($"\nStatistiques:");
Console.WriteLine($"  - Deplacements effectues: {moveCount}");
Console.WriteLine($"  - Changements de direction: {directionChangeCount}");
Console.WriteLine($"  - Total d'actions: {moveCount + directionChangeCount}");

Console.Write("\nVoulez-vous afficher l'historique detaille des deplacements ? (o/n) : ");
string? response = Console.ReadLine();
bool showHistory = response?.ToLower().StartsWith("o") ?? false;

if (showHistory)
{
    Console.WriteLine("\nHISTORIQUE COMPLET DES DEPLACEMENTS");
    Console.WriteLine("Format: [Mouvement#] Action -> Position (X,Y) Direction");
    Console.WriteLine(new string('-', 60));

    foreach (var entry in moveHistory)
    {
        var symbol = GetDirectionSymbol(entry.DeltaX, entry.DeltaY);
        var dirName = GetDirectionNameFromDeltas(entry.DeltaX, entry.DeltaY);
        Console.WriteLine($"[{entry.Move,4}] {entry.Action,-12} -> ({entry.X,2},{entry.Y,2}) {symbol} {dirName}");
    }

    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"Total: {moveHistory.Count} entrees dans l'historique");
}
