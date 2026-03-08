using System;

// ============================================================
//  LABYRINTHE ASCII - C# Console
//  ✅ Optimisé : seules les cellules modifiées sont redessinées
//               via Console.SetCursorPosition()
// ============================================================

// Constants
const int WIDTH = 50;
const int HEIGHT = 20;
const int OFFSET_X = 0;
const int OFFSET_Y = 3;
const int CELL_WIDTH = WIDTH / 2;
const int CELL_HEIGHT = HEIGHT / 2;

const ConsoleColor COLOR_WALL = ConsoleColor.DarkGray;
const ConsoleColor COLOR_PLAYER = ConsoleColor.Yellow;
const ConsoleColor COLOR_EXIT = ConsoleColor.Green;
const ConsoleColor COLOR_CORRIDOR = ConsoleColor.DarkBlue;
const ConsoleColor COLOR_TITLE = ConsoleColor.Cyan;
const ConsoleColor COLOR_INSTRUCTIONS = ConsoleColor.DarkCyan;
const ConsoleColor COLOR_WIN = ConsoleColor.Green;
const ConsoleColor COLOR_QUIT = ConsoleColor.Red;

const string MSG_TITLE = """
    ╔══════════════════════════════════════════════════╗
    ║          🏃 LABYRINTHE ASCII  C#  🏃             ║
    ╚══════════════════════════════════════════════════╝
    """;

const string MSG_INSTRUCTIONS = "  [Z/↑] Haut   [S/↓] Bas   [Q/←] Gauche   [D/→] Droite   [Échap] Quitter";

const string MSG_WIN = """
      ╔════════════════════════════════╗
      ║   🎉  FÉLICITATIONS !  🎉      ║
      ║   Vous avez trouvé la sortie ! ║
      ╚════════════════════════════════╝
    """;

const string MSG_QUIT = "\n  Partie abandonnée. À bientôt !";
const string MSG_EXIT = "  Appuyez sur une key pour quitter...";

var grid = new CellType[WIDTH, HEIGHT];

// ── Génération du labyrinthe par « recursive backtracker » ──
for (var y = 0; y < HEIGHT; y++)
    for (var x = 0; x < WIDTH; x++)
        grid[x, y] = CellType.Wall;

var stackX = new int[CELL_WIDTH * CELL_HEIGHT];
var stackY = new int[CELL_WIDTH * CELL_HEIGHT];
var stackTop = 0;

var visited = new bool[CELL_WIDTH, CELL_HEIGHT];

var dx = new[] { 0, 1, 0, -1 };
var dy = new[] { -1, 0, 1, 0 };

var rng = new Random();

var startCX = 0; var startCY = 0;
visited[startCX, startCY] = true;
grid[startCX * 2, startCY * 2] = CellType.Corridor;

stackX[stackTop] = startCX;
stackY[stackTop] = startCY;
stackTop++;

while (stackTop > 0)
{
    var cx = stackX[stackTop - 1];
    var cy = stackY[stackTop - 1];

    var directions = new[] { 0, 1, 2, 3 };
    rng.Shuffle(directions);

    var found = false;
    foreach (var dir in directions)
    {
        var nx = cx + dx[dir];
        var ny = cy + dy[dir];
        if (nx >= 0 && nx < CELL_WIDTH && ny >= 0 && ny < CELL_HEIGHT && !visited[nx, ny])
        {
            grid[cx * 2 + dx[dir], cy * 2 + dy[dir]] = CellType.Corridor;
            grid[nx * 2, ny * 2] = CellType.Corridor;
            visited[nx, ny] = true;
            stackX[stackTop] = nx;
            stackY[stackTop] = ny;
            stackTop++;
            found = true;
            break;
        }
    }
    if (!found) stackTop--;
}

// ── Position player et exit ──
var playerX = 0; var playerY = 0;
var exitX = (CELL_WIDTH - 1) * 2;
var exitY = (CELL_HEIGHT - 1) * 2;

grid[playerX, playerY] = CellType.Player;
grid[exitX, exitY] = CellType.Exit;

// ── Dessin initial complet (une seule fois) ──
Console.Clear();
Console.CursorVisible = false;

Console.SetCursorPosition(0, 0);
Console.ForegroundColor = COLOR_TITLE;
Console.Write(MSG_TITLE);
Console.ResetColor();

for (var y = 0; y < HEIGHT; y++)
{
    for (var x = 0; x < WIDTH; x++)
    {
        Console.SetCursorPosition(OFFSET_X + x, OFFSET_Y + y);
        var cell = grid[x, y];
        if (cell == CellType.Wall) { Console.ForegroundColor = COLOR_WALL; Console.Write("█"); }
        else if (cell == CellType.Player) { Console.ForegroundColor = COLOR_PLAYER; Console.Write("@"); }
        else if (cell == CellType.Exit) { Console.ForegroundColor = COLOR_EXIT; Console.Write("★"); }
        else { Console.ForegroundColor = COLOR_CORRIDOR; Console.Write(" "); }
    }
}

Console.SetCursorPosition(0, OFFSET_Y + HEIGHT + 1);
Console.ForegroundColor = COLOR_INSTRUCTIONS;
Console.Write(MSG_INSTRUCTIONS);
Console.ResetColor();

// ── Action locale : redessiner UNE seule cellule via SetCursorPosition ──
void DrawCell(int cx, int cy)
{
    Console.SetCursorPosition(OFFSET_X + cx, OFFSET_Y + cy);
    var cell = grid[cx, cy];
    if (cell == CellType.Wall) { Console.ForegroundColor = COLOR_WALL; Console.Write("█"); }
    else if (cell == CellType.Player) { Console.ForegroundColor = COLOR_PLAYER; Console.Write("@"); }
    else if (cell == CellType.Exit) { Console.ForegroundColor = COLOR_EXIT; Console.Write("★"); }
    else { Console.ForegroundColor = COLOR_CORRIDOR; Console.Write(" "); }
    Console.ResetColor();
}

// ── Boucle de jeu ──
var won = false;

while (!won)
{
    ConsoleKey key = Console.ReadKey(true).Key;

    var nx2 = playerX;
    var ny2 = playerY;

    if (key == ConsoleKey.Z || key == ConsoleKey.UpArrow) ny2--;
    else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow) ny2++;
    else if (key == ConsoleKey.Q || key == ConsoleKey.LeftArrow) nx2--;
    else if (key == ConsoleKey.D || key == ConsoleKey.RightArrow) nx2++;
    else if (key == ConsoleKey.Escape) break;

    if (nx2 >= 0 && nx2 < WIDTH && ny2 >= 0 && ny2 < HEIGHT && grid[nx2, ny2] != CellType.Wall)
    {
        if (grid[nx2, ny2] == CellType.Exit) won = true;

        grid[playerX, playerY] = CellType.Corridor;
        DrawCell(playerX, playerY);

        playerX = nx2;
        playerY = ny2;
        grid[playerX, playerY] = CellType.Player;
        DrawCell(playerX, playerY);
    }
}

// ── Écran de victoire ──
Console.SetCursorPosition(0, OFFSET_Y + HEIGHT + 3);
if (won)
{
    Console.ForegroundColor = COLOR_WIN;
    Console.Write(MSG_WIN);
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = COLOR_QUIT;
    Console.Write(MSG_QUIT);
    Console.ResetColor();
}

Console.SetCursorPosition(0, OFFSET_Y + HEIGHT + 8);
Console.WriteLine(MSG_EXIT);
Console.CursorVisible = true;
Console.ReadKey(true);

// ── Enum CellType ──
enum CellType
{
    Corridor = 0,
    Wall = 1,
    Player = 2,
    Exit = 3
}
