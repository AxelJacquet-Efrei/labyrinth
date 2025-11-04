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


