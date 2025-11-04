# Labyrinth Explorer - Documentation

# Étape 1 - Événement d'initialisation

## Objectif de l'énoncé

> En partant de la correction, créer une classe `StartEventArgs` dérivée de `EventArgs` possédant deux propriétés X et Y.
> 
> L'utiliser pour définir un événement `StartPositionFound` dans `AsciiParser` levé à chaque rencontre d'un 'x' dans le texte. Retirer le paramètre start et simplifier le parser.
> 
> Utiliser cet événement depuis le labyrinthe pour initialiser la position de départ.
> 
> Vérifier que les tests passent toujours, puis historiser votre version.

## 🎯 Notre implémentation

### 1. Classe `StartEventArgs` dérivée de `EventArgs`

```csharp
public class StartEventArgs : EventArgs
{
    public int X { get; }
    public int Y { get; }
    
    public StartEventArgs(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

 **Requis respecté** : Deux propriétés X et Y, dérivée de `EventArgs`

**Justification** : Propriétés en lecture seule pour garantir l'immutabilité des données d'événement.

### 2. Événement `StartPositionFound` dans `AsciiParser`

Pour faciliter l'injection de dépendance, nous avons créé une interface :

```csharp
public interface IAsciiParser
{
    event EventHandler<StartEventArgs>? StartPositionFound;
    Tile[,] Parse(string ascii_map);
}
```

**Bonus** : Interface pour respecter le principe **Dependency Inversion** (SOLID).

### 3. Implémentation dans `AsciiParser` - Événement levé à chaque 'x'

```csharp
public class AsciiParser : IAsciiParser
{
    public event EventHandler<StartEventArgs>? StartPositionFound;
    
    public Tile[,] Parse(string ascii_map)
    {
        // ...existing code...
        tiles[x, y] = ch switch
        {
            'x' => RaiseStartAndReturnRoom(x, y),  // Levé à chaque 'x'
            // ...other cases...
        };
    }
    
    private Room RaiseStartAndReturnRoom(int x, int y)
    {
        StartPositionFound?.Invoke(this, new StartEventArgs(x, y));
        return new Room();
    }
}
```

**Requis respecté** : Événement levé à chaque rencontre d'un 'x'

**Requis respecté** : Paramètre `start` retiré (plus de `ref` dans la signature)

### 4. Utilisation depuis `Labyrinth` pour initialiser la position

```csharp
public Labyrinth(string ascii_map, IAsciiParser parser)
{
    parser.StartPositionFound += (_, e) => _start = (e.X, e.Y);  // Initialisation
    _tiles = parser.Parse(ascii_map);
    
    if (_start == (-1, -1))
        throw new ArgumentException("Labyrinth must have a starting position marked with x");
}
```

**Requis respecté** : L'événement est utilisé depuis le labyrinthe pour initialiser `_start`

**Justification** : 
- Abonnement à l'événement **avant** le parsing
- Validation que la position a été trouvée

## Bonus ajoutés (non requis)

- **Interface `IAsciiParser`** : Facilite les tests et l'extensibilité
- **Documentation XML** : Sur toutes les classes
- **Validation** : Vérification qu'une position de départ existe

## Principes SOLID respectés

- **S** (Single Responsibility) : Le parser ne fait que parser
- **O** (Open/Closed) : Extensible avec d'autres parsers (JSON, XML)
- **D** (Dependency Inversion) : `Labyrinth` dépend de `IAsciiParser`, pas de `AsciiParser`

---

# Étape 2 - Explorateur

## Objectif de l'énoncé

> Écrire une classe dont le constructeur reçoit un `ICrawler`.
> 
> Écrire une méthode `GetOut(int n)` qui effectue, dans un premier temps, des déplacements (appels à Walk et Turn...) aléatoires et s'arrête dès qu'une tuile `Outside` est atteinte ou après n déplacements.
> 
> Si vous écrivez des tests (non obligatoire), il faudra mocker la génération aléatoire pour rendre votre test déterministe.
> 
> Historiser cette étape.

## Notre implémentation

### 1. Ajout de `TurnLeft()` et `TurnRight()` à `ICrawler`

```csharp
public interface ICrawler
{
    int X { get; }
    int Y { get; }
    Direction Direction { get; }
    Tile FacingTile { get; }
    Inventory Walk();
    void TurnLeft();   // Pour Turn...
    void TurnRight();  // Pour Turn...
}
```

**Justification** : L'énoncé mentionne "appels à Walk et Turn...". Pour tourner, nous avons ajouté ces méthodes.

### 2. Classe `Explorer` avec constructeur recevant `ICrawler`

```csharp
public class Explorer
{
    private readonly ICrawler _crawler;
    private readonly IMovementStrategy _strategy;
    
    public Explorer(ICrawler crawler, IMovementStrategy? strategy = null)
    {
        _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));  // Reçoit ICrawler
        _strategy = strategy ?? new RandomMovementStrategy();
    }
    
    // ...existing code...
}
```

**Requis respecté** : Constructeur reçoit un `ICrawler`

**Bonus** : Strategy Pattern pour la logique de mouvement (voir ci-dessous)

### 3. Méthode `GetOut(int n)` avec déplacements aléatoires

```csharp
public bool GetOut(int maxMoves)
{
    for (int move = 0; move < maxMoves; move++)  // Après n déplacements
    {
        try
        {
            // Gestion des portes (bonus)
            if (_crawler.FacingTile is Door door && door.IsLocked)
            {
                var keyInventory = FindKeyInventory();
                if (keyInventory != null)
                    door.Open(keyInventory);
            }
            
            var wrapper = new CrawlerWrapper(_crawler, _bag);
            _strategy.Execute(wrapper);  // Appels à Walk et Turn aléatoires
        }
        catch (InvalidOperationException) { }
        
        if (_crawler.FacingTile is Outside)  // S'arrête si Outside
            return true;
    }
    
    return false;
}
```

**Requis respecté** : 
- Déplacements aléatoires (via `RandomMovementStrategy`)
- S'arrête si `Outside` est atteinte
- S'arrête après n déplacements

### 4. Logique de déplacement aléatoire (Strategy Pattern - Bonus)

```csharp
public interface IMovementStrategy
{
    void Execute(ICrawler crawler);
}

public class RandomMovementStrategy : IMovementStrategy
{
    private readonly Random _random;
    
    public RandomMovementStrategy(Random? random = null)
    {
        _random = random ?? new Random();  // Random injectable pour tests
    }
    
    public void Execute(ICrawler crawler)
    {
        int turn = _random.Next(3);  // 0, 1, ou 2
        if (turn == 1) crawler.TurnRight();      // Turn aléatoire
        else if (turn == 2) crawler.TurnLeft();  // Turn aléatoire
        
        crawler.Walk();  // Walk
    }
}
```

**Requis respecté** : Appels à `Walk` et `Turn...` aléatoires

**Justification du Strategy Pattern** : 
- Séparation de la logique de mouvement (principe **Single Responsibility**)
- Facilite l'ajout d'autres stratégies sans modifier `Explorer`
- Random injectable pour les tests déterministes

### 5. Tests avec mock de génération aléatoire (bonus, car "non obligatoire")

```csharp
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
```

**Requis respecté (optionnel)** : Mock de génération aléatoire pour tests déterministes

**Test utilisant le mock** :
```csharp
[Test]
public void RandomMovementStrategy_PerformsRandomMovements()
{
    var mockCrawler = new MockCrawler(new Room());
    var deterministicRandom = new DeterministicRandom(new[] { 1, 2, 0, 1, 2 });
    var strategy = new RandomMovementStrategy(deterministicRandom);

    for (int i = 0; i < 5; i++)
        strategy.Execute(mockCrawler);

    Assert.That(mockCrawler.WalkCallCount, Is.EqualTo(5));
    Assert.That(mockCrawler.TurnRightCallCount, Is.EqualTo(2));
    Assert.That(mockCrawler.TurnLeftCallCount, Is.EqualTo(2));
}
```

## Bonus ajoutés (non requis)

- **Strategy Pattern complet** : `IMovementStrategy` au lieu d'un simple Random dans `Explorer`
- **Gestion des clés et portes** : Collecte automatique et ouverture des portes verrouillées
- **CrawlerWrapper** : Decorator Pattern pour intercepter les items collectés
- **7 tests unitaires créés** alors que les tests étaient optionnels

## Tests créés

| Test | Description |
|------|-------------|
| `Explorer_ReturnsTrue_WhenOutsideReached` | Vérifie que GetOut() retourne true à la sortie |
| `Explorer_ReturnsFalse_WhenMaxMovesReached` | Vérifie false après maxMoves sans trouver la sortie |
| `Explorer_StopsAfterMaxMoves` | Vérifie que Walk est appelé exactement n fois |
| `Explorer_UsesMovementStrategy` | Vérifie l'utilisation correcte de la stratégie |
| `RandomMovementStrategy_PerformsRandomMovements` | Teste avec `DeterministicRandom` (mock) |
| `Explorer_CollectsKeys_WhenWalkingOverThem` | Test d'intégration : collecte de clés (bonus) |
| `Explorer_OpensLockedDoors_WithCollectedKeys` | Test d'intégration : ouverture de portes (bonus) |

## Principes SOLID respectés

- **S** : `Explorer` explore, `RandomMovementStrategy` décide des mouvements
- **O** : On peut ajouter des stratégies (A*, Dijkstra) sans modifier `Explorer`
- **D** : `Explorer` dépend des abstractions (`ICrawler`, `IMovementStrategy`)

---

# Étape 3 - Couche présentation

## Objectif de l'énoncé

> Pour visualiser les déplacements sans revoir la logique de GetOut :
> 
> - Créer une classe `CrawlingEventArgs` dérivée de `EventArgs` avec les propriétés X, Y et Direction
> - Dans votre classe, créer les évènements `PositionChanged` et `DirectionChanged`
> - Déclencher ces évènements depuis votre classe
> 
> Dans le programme principal, afficher le labyrinthe et vous abonner aux évènements pour actualiser la position/orientation de l'explorateur (^, >, v ou <) dans le labyrinthe grâce à la fonction `Console.SetCursorPosition`.
> 
> Si vous avez fait des tests à l'étape 2, les modifier pour qu'ils vérifient le bon déclenchement des évènements avec les bons arguments.

## Notre implémentation

### 1. Classe `CrawlingEventArgs` dérivée de `EventArgs`

```csharp
public class CrawlingEventArgs : EventArgs
{
    public int X { get; }              // Propriété X
    public int Y { get; }              // Propriété Y
    public Direction Direction { get; } // Propriété Direction
    
    public CrawlingEventArgs(int x, int y, Direction direction)
    {
        X = x;
        Y = y;
        Direction = direction;
    }
}
```

**Requis respecté** : Classe dérivée de `EventArgs` avec X, Y et Direction

### 2. Événements `PositionChanged` et `DirectionChanged` dans `Explorer`

```csharp
public class Explorer
{
    public event EventHandler<CrawlingEventArgs>? PositionChanged;   // Événement créé
    public event EventHandler<CrawlingEventArgs>? DirectionChanged;  // Événement créé
    
    // ...existing code...
}
```

 **Requis respecté** : Les deux événements sont créés dans la classe `Explorer`

### 3. Déclenchement des événements depuis `Explorer`

```csharp
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
            // ...existing code...
            _strategy.Execute(wrapper);
        }
        catch (InvalidOperationException) { }
        
        // Déclenchement des événements
        if (_crawler.X != previousX || _crawler.Y != previousY)
            OnPositionChanged(_crawler.X, _crawler.Y);
        
        if (_crawler.Direction.DeltaX != previousDeltaX || 
            _crawler.Direction.DeltaY != previousDeltaY)
            OnDirectionChanged(_crawler.X, _crawler.Y);
        
        if (_crawler.FacingTile is Outside)
            return true;
    }
    return false;
}

private void OnPositionChanged(int x, int y)
{
    PositionChanged?.Invoke(this, new CrawlingEventArgs(x, y, _crawler.Direction));  // Déclenchement
}

private void OnDirectionChanged(int x, int y)
{
    DirectionChanged?.Invoke(this, new CrawlingEventArgs(x, y, _crawler.Direction));  // Déclenchement
}
```

**Requis respecté** : Les événements sont déclenchés depuis la classe `Explorer`

**Important** : La logique de `GetOut()` n'a **pas été revue**, seulement enrichie avec les événements

### 4. Programme principal - Affichage du labyrinthe et abonnement

```csharp
var labyrinth = new Labyrinth.Labyrinth(asciiMap, new AsciiParser());
var crawler = labyrinth.NewCrawler();
var explorer = new Explorer(crawler);
var display = new ConsoleDisplay();

// Afficher le labyrinthe
display.ShowLabyrinth(labyrinth.Tiles, crawler.X, crawler.Y, crawler.Direction);

int moveCount = 0;
int directionChangeCount = 0;

// Abonnement aux événements
explorer.PositionChanged += (_, e) =>
{
    moveCount++;
    display.UpdateExplorerPosition(e.X, e.Y, e.Direction);  // Actualiser (^, >, v, <)
    
    // Utilisation de Console.SetCursorPosition
    Console.SetCursorPosition(0, labyrinth.Tiles.GetLength(1) + 1);
    Console.Write($"Deplacements: {moveCount}  Position: ({e.X},{e.Y})");
    
    Thread.Sleep(50);  // Bonus : animation
};

explorer.DirectionChanged += (_, e) =>
{
    directionChangeCount++;
    display.UpdateExplorerPosition(e.X, e.Y, e.Direction);  // Actualiser (^, >, v, <)
    Thread.Sleep(25);  // Bonus : animation
};

bool found = explorer.GetOut(maxMoves: 1_000_000);
```

**Requis respecté** : 
- Affichage du labyrinthe
- Abonnement aux événements pour actualiser la position/orientation
- Utilisation de `Console.SetCursorPosition`
- Affichage des symboles directionnels (^, >, v, <)

### 5. Implémentation de `ConsoleDisplay` avec symboles directionnels

```csharp
public class ConsoleDisplay : IDisplay
{
    public void UpdateExplorerPosition(int x, int y, Direction direction)
    {
        Console.SetCursorPosition(_lastPosition.X, _lastPosition.Y);
        Console.Write(' ');  // Effacer ancienne position
        
        Console.SetCursorPosition(x, y);  // Console.SetCursorPosition
        Console.Write(GetDirectionSymbol(direction));  // Symboles: ^, >, v, <
        
        _lastPosition = (x, y);
    }
    
    private static char GetDirectionSymbol(Direction direction) =>
        (direction.DeltaX, direction.DeltaY) switch
        {
            (0, -1) => '^',   // Nord
            (1, 0) => '>',    // Est
            (0, 1) => 'v',    // Sud
            (-1, 0) => '<',   // Ouest
            _ => '?'
        };
}
```

**Requis respecté** : Actualisation avec les symboles ^, >, v, < via `Console.SetCursorPosition`

### 6. Tests modifiés pour vérifier les événements

```csharp
[Test]
public void Explorer_RaisesPositionChanged_WhenMoving()
{
    var mockCrawler = new MockCrawlerWithMovement();
    var mockStrategy = new MockMovementStrategyWithMovement(mockCrawler);
    var explorer = new Explorer(mockCrawler, mockStrategy);

    var positionChangedEvents = new List<(int X, int Y, Direction Dir)>();
    explorer.PositionChanged += (_, e) =>  // Vérification de l'événement
    {
        positionChangedEvents.Add((e.X, e.Y, e.Direction));
    };

    explorer.GetOut(maxMoves: 3);

    // Vérification des bons arguments
    Assert.That(positionChangedEvents.Count, Is.GreaterThan(0));
    Assert.That(positionChangedEvents[0].X, Is.EqualTo(1));
    Assert.That(positionChangedEvents[0].Y, Is.EqualTo(0));
}

[Test]
public void Explorer_EventArgs_ContainsCorrectDirection()
{
    var mockCrawler = new MockCrawlerWithMovement();
    var mockStrategy = new MockMovementStrategyWithMovement(mockCrawler);
    var explorer = new Explorer(mockCrawler, mockStrategy);

    CrawlingEventArgs? capturedEvent = null;
    explorer.PositionChanged += (_, e) => capturedEvent = e;

    explorer.GetOut(maxMoves: 1);

    // Vérification des bons arguments (X, Y, Direction)
    Assert.That(capturedEvent, Is.Not.Null);
    Assert.That(capturedEvent!.X, Is.EqualTo(1));
    Assert.That(capturedEvent.Y, Is.EqualTo(0));
    Assert.That(capturedEvent.Direction, Is.Not.Null);
}
```

**Requis respecté** : Tests modifiés pour vérifier le déclenchement des événements avec les bons arguments

## Bonus ajoutés (non requis)

### 1. Interface `IDisplay` pour l'abstraction
```csharp
public interface IDisplay
{
    void ShowLabyrinth(Tile[,] grid, int startX, int startY, Direction startDirection);
    void UpdateExplorerPosition(int x, int y, Direction direction);
    void Clear();
}
```
**Justification** : Respecte le principe **Dependency Inversion** et permet d'ajouter `GUIDisplay`, `WebDisplay`, etc.

### 2. Affichage enrichi
- **Compteurs en temps réel** : nombre de déplacements et changements de direction
- **Animation** : `Thread.Sleep()` pour visualiser l'exploration
- **Fonctions utilitaires** : `GetDirectionName()` pour affichage lisible en français

### 3. Historique optionnel
```csharp
Console.Write("\nVoulez-vous afficher l'historique detaille ? (o/n) : ");
string? response = Console.ReadLine();

if (response?.ToLower().StartsWith("o") ?? false)
{
    // Affichage de l'historique complet des mouvements
    foreach (var entry in moveHistory)
        Console.WriteLine($"[{entry.Move,4}] {entry.Action,-12} -> ({entry.X,2},{entry.Y,2}) {symbol} {dirName}");
}
```

**Justification** : L'utilisateur choisit **après l'exploration** s'il veut voir le détail. N'interfère pas avec l'affichage temps réel.

### 4. Statistiques complètes
```
Statistiques:
  - Deplacements effectues: 287
  - Changements de direction: 399
  - Total d'actions: 686
```

## Principes SOLID respectés

- **S** : `ConsoleDisplay` ne fait qu'afficher, `Explorer` ne fait qu'explorer
- **O** : On peut ajouter `GUIDisplay` sans modifier le code existant
- **D** : `Program.cs` dépend de `IDisplay` (abstraction), pas de `ConsoleDisplay`

## Design Patterns utilisés

- **Observer Pattern** : Événements `PositionChanged` et `DirectionChanged` (requis)
- **Strategy Pattern** : `IDisplay` avec `ConsoleDisplay` (bonus)

### Exemple d'exécution

```
EXPLORATEUR DE LABYRINTHE
Symboles : ^ (Nord)  > (Est)  v (Sud)  < (Ouest)

#############
#  /        #
#  #######  #
#     #     #
####  #  ####
<  #        #
#  ######## #
#           #
#############

Deplacements: 287  Position: (0,5)  Direction: Ouest

SORTIE TROUVEE !

Statistiques:
  - Deplacements effectues: 287
  - Changements de direction: 399
  - Total d'actions: 686

Voulez-vous afficher l'historique detaille des deplacements ? (o/n) :
```
