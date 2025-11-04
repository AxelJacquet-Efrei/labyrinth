# Étape 1 - Événement d'initialisation

## Le problème

```csharp
// Couplage fort : le parser modifie directement _start
_tiles = AsciiParser.Parse(ascii_map, ref _start);
```

- Parser et labyrinthe trop liés
- Impossible de tester séparément
- Pas extensible

---

## Notre solution

### 1. `StartEventArgs` - Transporter les coordonnées

```csharp
public class StartEventArgs : EventArgs
{
    public int X { get; }  // Lecture seule = immutabilité
    public int Y { get; }
}
```

### 2. `IAsciiParser` - Interface pour l'injection

```csharp
public interface IAsciiParser
{
    event EventHandler<StartEventArgs>? StartPositionFound;
    Tile[,] Parse(string ascii_map);
}
```

**Pourquoi ?** Permet de mocker le parser et créer d'autres implémentations (JSON, XML...).

### 3. `AsciiParser` - Émetteur d'événements

```csharp
// Notification au lieu de modification
private Room RaiseStartAndReturnRoom(int x, int y)
{
    StartPositionFound?.Invoke(this, new StartEventArgs(x, y));
    return new Room();
}
```

### 4. `Labyrinth` - Injection de dépendance obligatoire

```csharp
public Labyrinth(string ascii_map, IAsciiParser parser)
{
    parser.StartPositionFound += (_, e) => _start = (e.X, e.Y);
    _tiles = parser.Parse(ascii_map);
}
```

**Choix d'implémentation :** Un seul constructeur avec injection obligatoire.
- Force l'utilisation explicite du parser (cohérence avec l'interface)
- Les tests et le code utilisent tous `new Labyrinth(map, new AsciiParser())`
- Rend visible la dépendance au parser (principe de transparence)

---

## Mise à jour des tests

Les tests ont été refactorés pour utiliser l'injection de dépendance :

**Avant :**
```csharp
private static ICrawler NewCrawlerFor(string ascii_map) =>
    new Labyrinth.Labyrinth(ascii_map).NewCrawler();
```

**Après :**
```csharp
private static ICrawler NewCrawlerFor(string ascii_map) =>
    new Labyrinth.Labyrinth(ascii_map, new AsciiParser()).NewCrawler();
```

**Impact :** Tous les appels à `new Labyrinth(map)` dans les tests et le code ont été mis à jour pour passer explicitement `new AsciiParser()`. Cette cohérence renforce l'architecture basée sur l'injection de dépendance.

---

# Étape 2 - Explorateur

## Le problème

Comment explorer un labyrinthe de manière automatisée ? Besoin d'une classe capable de :
- Contrôler un crawleur (ICrawler)
- Effectuer des déplacements aléatoires
- Déterminer quand la sortie est trouvée
- Être testable avec une génération aléatoire déterministe

---

## Notre solution

### 1. Enrichissement de `ICrawler` - Ajouter les rotations

```csharp
public interface ICrawler
{
    // ...existing code...
    void TurnRight();
    void TurnLeft();
}
```

**Pourquoi ?** L'interface n'exposait que `Walk()`. Pour explorer aléatoirement, il faut aussi tourner. Les rotations manipulent `Direction` qui appartient déjà au crawleur.

### 2. `Explorer` - Classe avec injection de dépendances

```csharp
public class Explorer
{
    private readonly ICrawler _crawler;
    private readonly Random _random;

    public Explorer(ICrawler crawler, Random? random = null)
    {
        _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));
        _random = random ?? new Random();
    }

    public bool GetOut(int maxMoves)
    {
        for (int move = 0; move < maxMoves; move++)
        {
            // Aléa : 0 = pas tourner, 1 = TurnRight, 2 = TurnLeft
            int turn = _random.Next(3);
            switch (turn)
            {
                case 1: _crawler.TurnRight(); break;
                case 2: _crawler.TurnLeft(); break;
            }

            _crawler.Walk();

            // Vérifier si on a atteint la sortie
            if (_crawler.FacingTile is Outside)
                return true;
        }

        return false;
    }
}
```

**Choix d'implémentation :**
- `Random` est **optionnel et injecté** (par défaut une nouvelle instance) permet les mocks déterministes en test
- Constructor guarding avec `ArgumentNullException` prévient les erreurs silencieuses
- `FacingTile is Outside` pattern matching pour une vérification directe et lisible

---

## Respect des principes SOLID

### Single Responsibility (S)
- `Explorer` = uniquement chargée de trouver la sortie par exploration aléatoire
- Pas de logique d'affichage, pas d'évènements (l'Étape 3 le fera)

### Open/Closed (O)
- Injection de `Random` permet d'étendre le comportement sans modifier la classe
- Injection de `ICrawler` permet d'utiliser n'importe quel crawleur

### Liskov Substitution (L)
- `Explorer` accepte `ICrawler` ; n'importe quelle implémentation fonctionne

### Interface Segregation (I)
- `ICrawler` a **exactement** les méthodes nécessaires (X, Y, Direction, FacingTile, Walk, TurnRight, TurnLeft)
- Pas de méthode inutile

### Dependency Inversion (D)
- `Explorer` dépend de l'abstraction `ICrawler`, pas d'une implémentation concrète
- `Random` est injecté → découplage de la génération aléatoire

---

## Tests - Au-delà des exigences

L'énoncé disait : **"Si vous écrivez des tests (non obligatoire), il faudra mocker la génération aléatoire"**

**Nous avons créé 4 tests :**

### Test 1 : `Explorer_ReturnsTrue_WhenOutsideReached`
Vérifie que `GetOut()` retourne `true` quand on atteint `Outside`.

### Test 2 : `Explorer_ReturnsFalse_WhenMaxMovesReachedWithoutExit`
Vérifie que `GetOut()` retourne `false` quand on atteint la limite sans trouver la sortie.

### Test 3 : `Explorer_StopsAfterMaxMoves`
Vérifie que `Walk()` est appelée **exactement** maxMoves fois (pas plus).
- Avec un mock qui compte les appels

### Test 4 : `Explorer_PerformsRandomMovements`
Vérifie que les rotations et déplacements sont effectués dans le bon ordre.
- Utilise `DeterministicRandom(new[] { 1, 2, 0, 1, 2 })` pour contrôler la séquence
- Teste que 2 TurnRight + 2 TurnLeft sont appelées à la bonne position

**Ajouts pour la testabilité :**
- `MockCrawler` : implémentation fake de `ICrawler` qui compte les appels
- `DeterministicRandom` : classe héritable de `Random` avec une séquence fixée

---

## Strategy Pattern - Implémentation pour extensibilité future

### Pourquoi mettre en place le Strategy Pattern dès maintenant ?

L'énoncé disait d'ajouter une simple logique aléatoire. Cependant, mettre en place le **Strategy Pattern** dès l'Étape 2 offre plusieurs avantages :

- **Extensibilité** : Ajouter de nouvelles stratégies (spirale, wall-following, etc.) sans modifier `Explorer`  
- **Testabilité** : Mocker la stratégie indépendamment du crawler  
- **Maintenabilité** : Séparation claire des responsabilités  
- **Respect de SOLID** : Open/Closed Principle appliqué

### Notre implémentation

#### 1. Interface `IMovementStrategy`

```csharp
public interface IMovementStrategy
{
    /// <summary>
    /// Execute a movement action on the crawler (turn and/or walk).
    /// </summary>
    void Execute(ICrawler crawler);
}
```

**Responsabilité unique** : Définir le contrat pour toute stratégie de mouvement.

#### 2. Implémentation `RandomMovementStrategy`

```csharp
public class RandomMovementStrategy : IMovementStrategy
{
    private readonly Random _random;

    public RandomMovementStrategy(Random? random = null)
    {
        _random = random ?? new Random();
    }

    public void Execute(ICrawler crawler)
    {
        int turn = _random.Next(3);
        switch (turn)
        {
            case 1: crawler.TurnRight(); break;
            case 2: crawler.TurnLeft(); break;
        }
        crawler.Walk();
    }
}
```

**Avantage** : La logique aléatoire est isolée dans sa propre classe.

#### 3. Refactorisation de `Explorer`

```csharp
public class Explorer
{
    private readonly ICrawler _crawler;
    private readonly IMovementStrategy _strategy;

    // Constructeur principal avec stratégie
    public Explorer(ICrawler crawler, IMovementStrategy? strategy = null)
    {
        _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));
        _strategy = strategy ?? new RandomMovementStrategy();
    }

    // Constructeur hérité pour compatibilité (marqué Obsolete)
    [Obsolete("Use Explorer(ICrawler, IMovementStrategy) instead for better extensibility.", false)]
    public Explorer(ICrawler crawler, Random? random = null)
        : this(crawler, new RandomMovementStrategy(random))
    {
    }

    public bool GetOut(int maxMoves)
    {
        for (int move = 0; move < maxMoves; move++)
        {
            _strategy.Execute(_crawler);
            if (_crawler.FacingTile is Outside) return true;
        }
        return false;
    }
}
```

**Bénéfices** :
- `Explorer` ne connaît pas les détails de la stratégie
- Injection de stratégie,  Dependency Inversion
- Constructeur obsolète maintient la compatibilité avec les tests existants
