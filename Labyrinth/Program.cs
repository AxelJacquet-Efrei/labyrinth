using Labyrinth.Build;

Console.WriteLine(new Labyrinth.Labyrinth("""
    +--+--------+
    |  /        |
    |  +--+--+  |
    |     |k    |
    +--+  |  +--+
       |k  x    |
    +  +-------/|
    |           |
    +-----------+
    """, new AsciiParser()));
