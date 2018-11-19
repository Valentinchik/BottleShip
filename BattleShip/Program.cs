using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

//сделать отсчет времени между ударами компьютера
namespace BattleShip
{
    public enum Direction
    {
        Up,
        Left,
        None
    }

    class Program
    {
        const string FilePath = @"C:\Users\Valentine\Desktop\ShipWorld.txt";
        const int SizeField = 10;
        const int MissValue = 6;
        const int HitValue = 8;
        const int CountHitForWin = 20;

        static void Main(string[] args)
        {
            int[,] playerField = FieldArrayFilling(ReadFile(FilePath));
            int[,] enemyField = FieldEnemy();
            int[,] enemyFieldForPlayer = FieldEnemy();
            Data data = new Data();
            enemyField = RandomShipsEnemy(enemyField);
            FieldOutput(playerField, enemyFieldForPlayer);
            DebugField(enemyField);
            data.FieldForEnemy = CreatingListcoordinatesForEnemyAttack();
            StepPlayer(enemyFieldForPlayer, enemyField, playerField, data);

            Console.ReadLine();
        }

        static List<Coordenate> CreatingListcoordinatesForEnemyAttack()
        {
            List<Coordenate> tempList = new List<Coordenate>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    tempList.Add(new Coordenate(i, j));
                }
            }
            return tempList;
        }

        static void StepPlayer(int[,] enemyFieldForPlayer, int[,] enemyField, int[,] playerField, Data data)
        {
            if (CheckWiner(data))
            {
                return;
            }

            int y;
            int x;
            Console.WriteLine();
            Console.Write("Enter X coordinate for the attack (1-SizeField): ");
            string tmpy = Console.ReadLine();
            Console.Write("Enter Y coordinate for the attack (1-SizeField): ");
            string tmpx = Console.ReadLine();
            if (!int.TryParse(tmpy, out y) || !int.TryParse(tmpx, out x) || y <= 0 || y > 10 || x <= 0 || x > 10)
            {
                FieldOutput(playerField, enemyFieldForPlayer);
                Console.WriteLine("You did not enter the coordinate correctly, try again!");
                StepPlayer(enemyFieldForPlayer, enemyField, playerField, data);
                return;
            }

            x -= 1;
            y -= 1;

            if (enemyFieldForPlayer[x, y] != 0)
            {
                FieldOutput(playerField, enemyFieldForPlayer);
                Console.WriteLine("You have already attacked at these coordinates, try again!");
                StepPlayer(enemyFieldForPlayer, enemyField, playerField, data);
                return;
            }

            if (enemyField[x, y] == 0)
            {
                enemyFieldForPlayer[x, y] = MissValue;
                FieldOutput(playerField, enemyFieldForPlayer);
                Console.WriteLine("Sorry, but you missed...");
                StepEnemy(enemyFieldForPlayer, enemyField, playerField, data);
            }
            else
            {
                enemyFieldForPlayer[x, y] = enemyField[x, y];
                List<Coordenate> coordsDecks = KillCheckForPlayer(new Coordenate(x, y), enemyFieldForPlayer);
                if (coordsDecks.Count == enemyField[x, y])
                {
                    coordsDecks = StartEndShip(FindDirection(coordsDecks), coordsDecks);
                    enemyFieldForPlayer = MarkBorderFoeEnemyField(enemyFieldForPlayer, coordsDecks);
                }
                FieldOutput(playerField, enemyFieldForPlayer);
                ResultConsoleOutput(coordsDecks, enemyFieldForPlayer);
                data.CountPlayer++;
                StepPlayer(enemyFieldForPlayer, enemyField, playerField, data);
            }
        }

        static int[,] MarkBorderFoeEnemyField(int[,] field, List<Coordenate> coordDecks)
        {
            if (coordDecks.Count == 0)
            {
                return field;
            }
            int count = 0;
            for (int i = coordDecks[0].X - 1; i <= coordDecks[coordDecks.Count - 1].X + 1; i++)
            {
                if (i >= 0 && i < 10)
                {
                    for (int j = coordDecks[0].Y - 1; j <= coordDecks[coordDecks.Count - 1].Y + 1; j++)
                    {
                        if (j >= 0 && j < 10 && field[i, j] == 0)
                        {
                            field[i, j] = MissValue;
                            count++;
                        }
                    }
                }
            }

            return field;
        }

        static void ResultConsoleOutput(List<Coordenate> killDecks, int[,] fieldEnemy)
        {
            Console.WriteLine();
            if (killDecks.Count > 0 && fieldEnemy[killDecks[0].X, killDecks[0].Y] == killDecks.Count)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("You killed {0} deck ship!", killDecks.Count);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Fine! You hit!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        static List<Coordenate> KillCheckForPlayer(Coordenate coord, int[,] field)
        {
            int countDeck = field[coord.X, coord.Y];
            Direction dir = Direction.None;
            if (coord.X + 1 < 10 && field[coord.X, coord.Y] == field[coord.X + 1, coord.Y])
            {
                dir = Direction.Up;
            }
            else if (coord.X - 1 >= 0 && field[coord.X, coord.Y] == field[coord.X - 1, coord.Y])
            {
                dir = Direction.Up;
            }
            else if (coord.Y + 1 < 10 && field[coord.X, coord.Y] == field[coord.X, coord.Y + 1])
            {
                dir = Direction.Left;
            }
            else if (coord.Y - 1 >= 0 && field[coord.X, coord.Y] == field[coord.X, coord.Y - 1])
            {
                dir = Direction.Left;
            }

            return DecksCheck(field, countDeck, coord, dir);
        }

        static List<Coordenate> DecksCheck(int[,] field, int countDeck, Coordenate coord, Direction dir)
        {
            List<Coordenate> coordenatesDecks = new List<Coordenate>();

            switch (dir)
            {
                case Direction.Up:
                    for (int i = coord.X - countDeck - 1; i <= coord.X + countDeck - 1; i++)
                    {
                        if ((i >= 0 && i < 10) && field[i, coord.Y] == countDeck)
                        {
                            coordenatesDecks.Add(new Coordenate(i, coord.Y));
                        }
                    }
                    break;
                case Direction.Left:
                    for (int i = coord.Y - countDeck - 1; i <= coord.Y + countDeck - 1; i++)
                    {
                        if ((i >= 0 && i < 10) && field[coord.X, i] == countDeck)
                        {
                            coordenatesDecks.Add(new Coordenate(coord.X, i));
                        }
                    }
                    break;
            }

            return coordenatesDecks;
        }

        static void StepEnemy(int[,] enemyFieldForPlayer, int[,] enemyField, int[,] playerField, Data data)
        {
            if (CheckWiner(data))
            {
                return;
            }

            Coordenate coord = new Coordenate(0, 0);
            Random rand = new Random();

            if (data.Touch)
            {
                data.LikelyCoordinatesForEnemy = SearchNeighboring(data);

                if (data.LikelyCoordinatesForEnemy.Count == 0)
                {
                    data.Touch = false;
                }

                coord = data.LikelyCoordinatesForEnemy[rand.Next(0, data.LikelyCoordinatesForEnemy.Count)];
                data.LikelyCoordinatesForEnemy.Remove(coord);
                data.FieldForEnemy.Remove(coord);
            }
            else
            {
                coord = data.FieldForEnemy[rand.Next(0, data.FieldForEnemy.Count)];
                data.FieldForEnemy.Remove(coord);
            }

            AttackEnemy(enemyFieldForPlayer, enemyField, playerField, data, coord);
        }

        static void AttackEnemy(int[,] enemyFieldForPlayer, int[,] enemyField, int[,] playerField, Data data, Coordenate hitCoord)
        {
            if (playerField[hitCoord.X, hitCoord.Y] == 0)
            {
                playerField[hitCoord.X, hitCoord.Y] = MissValue;
                FieldOutput(playerField, enemyFieldForPlayer);
                Console.WriteLine("Fortunately, the enemy missed!");
                StepPlayer(enemyFieldForPlayer, enemyField, playerField, data);
            }
            else
            {
                int countDecks = 0;
                if (data.Touch)
                {
                    data.TouchCount++;
                    if (data.TouchCount == playerField[hitCoord.X, hitCoord.Y])
                    {
                        data.Touch = false;
                        data.Dir = data.Dir == Direction.None ? FindDirection(data.DecksShip) : data.Dir;
                        data.DecksShip.Add(hitCoord);
                        data.DecksShip = StartEndShip(data.Dir, data.DecksShip);
                        countDecks = data.DecksShip.Count;
                        playerField = MarkBorders(playerField, data.DecksShip[0], data.DecksShip[data.DecksShip.Count - 1], data);
                    }
                    else
                    {
                        data.Dir = data.Dir == Direction.None ? FindDirection(data.DecksShip) : data.Dir;
                        data.DecksShip.Add(hitCoord);
                        data.DecksShip = StartEndShip(data.Dir, data.DecksShip);
                    }
                }
                else if (IntegrityCheck(playerField, hitCoord.X, hitCoord.Y))
                {
                    data.Touch = true;
                    data.TouchCount = 1;
                    data.DecksShip = new List<Coordenate>();
                    data.DecksShip.Add(hitCoord);
                    data.Dir = Direction.None;
                }
                else
                {
                    countDecks = 1;
                    playerField = MarkBorders(playerField, hitCoord, hitCoord, data);
                }

                playerField[hitCoord.X, hitCoord.Y] = HitValue;
                FieldOutput(playerField, enemyFieldForPlayer);

                if (countDecks != 0)
                {
                    WriteHitEnemyPLeyersShip(countDecks);
                }
                else
                {
                    Console.WriteLine("You hit!");
                }

                data.CountEnemy++;
                StepEnemy(enemyFieldForPlayer, enemyField, playerField, data);
            }
        }

        static void WriteHitEnemyPLeyersShip(int countDecks)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Your {0} decker ship is destroyed!", countDecks);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadLine();
        }

        static List<Coordenate> SearchNeighboring(Data data)
        {
            Coordenate temp = data.DecksShip[0];
            List<Coordenate> tempList = new List<Coordenate>();
            foreach (Coordenate c in data.FieldForEnemy)
            {
                if (data.Dir == Direction.None && CheckNeighborhoodWithPoint(temp, c) && !tempList.Contains(c))
                {
                    tempList.Add(c);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                foreach (Coordenate c in data.FieldForEnemy)
                {
                    if (CheckExtremePoint(temp, c, data.Dir) && !tempList.Contains(c))
                    {
                        tempList.Add(c);
                    }
                }
                temp = data.DecksShip[data.DecksShip.Count - 1];
            }

            return tempList;
        }

        static bool CheckNeighborhoodWithPoint(Coordenate mainPoint, Coordenate checkPoint)
        {
            return (checkPoint.X == mainPoint.X && (checkPoint.Y == mainPoint.Y + 1 || checkPoint.Y == mainPoint.Y - 1))
                || (checkPoint.Y == mainPoint.Y && (checkPoint.X == mainPoint.X + 1 || checkPoint.X == mainPoint.X - 1));
        }

        static bool CheckExtremePoint(Coordenate mainPoint, Coordenate checkPoint, Direction dir)
        {
            if (dir == Direction.Up && (checkPoint.Y == mainPoint.Y && (checkPoint.X == mainPoint.X + 1 || checkPoint.X == mainPoint.Y - 1)))
            {
                return true;
            }
            if (dir == Direction.Left && (checkPoint.X == mainPoint.X && (checkPoint.Y == mainPoint.Y + 1 || checkPoint.Y == mainPoint.Y - 1)))
            {
                return true;
            }

            return false;
        }

        static Direction FindDirection(List<Coordenate> coords)
        {
            Direction dir = Direction.None;
            if (coords.Count <= 1)
            {
                return dir;
            }

            if (coords[0].X - coords[1].X > 0 || coords[0].X - coords[1].X < 0)
            {
                dir = Direction.Up;
            }
            else if (coords[0].Y - coords[1].Y > 0 || coords[0].Y - coords[1].Y < 0)
            {
                dir = Direction.Left;
            }

            return dir;
        }

        static bool IntegrityCheck(int[,] field, int x, int y)
        {
            return field[x, y] > 1 && field[x, y] != 0;
        }

        static int[,] MarkBorders(int[,] field, Coordenate start, Coordenate end, Data data)
        {
            for (int i = data.FieldForEnemy.Count - 1; i > 0; i--)
            {
                if ((data.FieldForEnemy[i].X >= start.X - 1 && data.FieldForEnemy[i].X <= end.X + 1)
                    && (data.FieldForEnemy[i].Y >= start.Y - 1 && data.FieldForEnemy[i].Y <= end.Y + 1))
                {
                    field[data.FieldForEnemy[i].X, data.FieldForEnemy[i].Y] = MissValue;
                    data.FieldForEnemy.Remove(data.FieldForEnemy[i]);
                }
            }

            return field;
        }

        static List<Coordenate> StartEndShip(Direction dir, List<Coordenate> coordDecks)
        {
            if (coordDecks.Count <= 1)
            {
                return coordDecks;
            }

            Coordenate temp;
            if (dir == Direction.Up)
            {
                for (int i = 0; i < coordDecks.Count; i++)
                {
                    for (int j = i + 1; j < coordDecks.Count; j++)
                    {
                        if (coordDecks[j].X < coordDecks[i].X)
                        {
                            temp = coordDecks[i];
                            coordDecks[i] = coordDecks[j];
                            coordDecks[j] = temp;
                        }
                    }
                }
            }
            else if (dir == Direction.Left)
            {
                for (int i = 0; i < coordDecks.Count; i++)
                {
                    for (int j = i + 1; j < coordDecks.Count; j++)
                    {
                        if (coordDecks[j].Y < coordDecks[i].Y)
                        {
                            temp = coordDecks[i];
                            coordDecks[i] = coordDecks[j];
                            coordDecks[j] = temp;
                        }
                    }
                }
            }

            return coordDecks;
        }

        static bool CheckWiner(Data data)
        {
            if (data.CountPlayer == CountHitForWin)
            {
                Console.WriteLine("The game is over! You won!");
                return true;
            }
            if (data.CountEnemy == CountHitForWin)
            {
                Console.WriteLine("The game is over! You lose!");
                return true;
            }

            return false;
        }

        static string[] ReadFile(string puth)
        {
            StreamReader sr = new StreamReader(puth);

            string[] temp = new string[SizeField];

            for (int i = 0; i < SizeField; i++)
            {
                temp[i] = sr.ReadLine();
            }

            return temp;
        }

        static int[,] FieldArrayFilling(string[] lines)
        {
            int[,] coordinates = new int[SizeField, SizeField];
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    coordinates[i, j] = int.Parse(lines[i][j].ToString());
                }
            }

            return coordinates;
        }

        static int[,] FieldEnemy()
        {
            string[] field = new string[SizeField];
            StringBuilder sb = new StringBuilder("");

            for (int i = 0; i < SizeField; i++)
            {
                sb.Clear();
                for (int j = 0; j < SizeField; j++)
                {
                    sb.Append("0");
                }
                field[i] = sb.ToString();
            }

            return FieldArrayFilling(field);
        }

        static int[,] RandomShipsEnemy(int[,] enemyField)
        {
            int numberEmpty = 7;

            enemyField = SpawnShips(enemyField, 4, 1, numberEmpty);
            enemyField = SpawnShips(enemyField, 3, 2, numberEmpty);
            enemyField = SpawnShips(enemyField, 2, 3, numberEmpty);
            enemyField = SpawnShips(enemyField, 1, 4, numberEmpty);

            for (int i = 0; i < SizeField; i++)
            {
                for (int j = 0; j < SizeField; j++)
                {
                    if (enemyField[i, j] == numberEmpty)
                    {
                        enemyField[i, j] = 0;
                    }
                }
            }

            return enemyField;
        }

        static int[,] SpawnShips(int[,] enemyField, int countDeck, int countShips, int numberEmpty)
        {
            Random rand = new Random();
            int[,] temp = null;
            for (int i = 0; i < countShips; i++)
            {
                while (temp == null)
                {
                    temp = SetDeckShip(enemyField, rand.Next(0, SizeField), rand.Next(0, SizeField), countDeck, HorizontalAndVertical(), numberEmpty);
                }
                enemyField = temp;
                temp = null;
            }

            return enemyField;
        }

        static int HorizontalAndVertical()
        {
            Random rand = new Random();

            return rand.Next(0, 1000) > 500 ? 1 : 0;
        }

        static int[,] SetDeckShip(int[,] enemyField, int x, int y, int numberDeck, int horVer, int numberEmpty)
        {
            if (enemyField[x, y] != 0)
            {
                return null;
            }
            else
            {
                if (horVer == 1)
                {
                    enemyField = HorizontalSpawn(enemyField, numberDeck, numberEmpty, x, y);
                }
                else
                {
                    enemyField = VerticalSpawn(enemyField, numberDeck, numberEmpty, x, y);
                }
            }

            return enemyField;
        }

        static int[,] HorizontalSpawn(int[,] field, int countDeck, int numberEmpty, int x, int y)
        {
            bool stop = false;
            int count = 0;
            int start = y;

            for (int i = 0; i < countDeck; i++)
            {
                if (y + i >= SizeField || field[x, y + i] == numberEmpty)
                {
                    stop = true;
                    break;
                }
                count++;
            }

            if (stop)
            {
                for (int i = 0; i < countDeck - count; i++)
                {
                    if (y - i <= 0 || field[x, y - i] == numberEmpty)
                    {
                        return null;
                    }
                    start -= 1;
                }
            }

            for (int i = start; i < start + countDeck; i++)
            {
                field[x, i] = countDeck;
            }

            for (int i = start - 1; i < start + countDeck + 1; i++)
            {
                for (int j = x - 1; j <= x + 1; j++)
                {
                    if (i >= 0 && i < SizeField && j >= 0 && j < SizeField && field[j, i] != countDeck)
                    {
                        field[j, i] = numberEmpty;
                    }
                }
            }

            return field;
        }

        static int[,] VerticalSpawn(int[,] field, int countDeck, int numberEmpty, int x, int y)
        {
            bool stop = false;
            int count = 0;
            int start = x;

            for (int i = 0; i < countDeck; i++)
            {
                if (x + i >= SizeField || field[x + i, y] == numberEmpty)
                {
                    stop = true;
                    break;
                }
                count++;
            }

            if (stop)
            {
                for (int i = 0; i < countDeck - count; i++)
                {
                    if (x - i <= 0 || field[x - i, y] == numberEmpty)
                    {
                        return null;
                    }
                    start -= 1;
                }
            }

            for (int i = start; i < start + countDeck; i++)
            {
                field[i, y] = countDeck;
            }

            for (int i = start - 1; i < start + countDeck + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (i >= 0 && i < SizeField && j >= 0 && j < SizeField && field[i, j] != countDeck)
                    {
                        field[i, j] = numberEmpty;
                    }
                }
            }

            return field;
        }

        static int ParameterSetting(int count, int coord)
        {
            return coord + count;
        }


        static void FieldOutput(int[,] playerField, int[,] enemyField)
        {
            Console.Clear();
            WriteCoordenates();
            WriteCoordenates();
            Console.WriteLine();
            Console.WriteLine();

            for (int i = 0; i < SizeField; i++)
            {
                if (i < 9)
                {
                    Console.Write(i + 1 + "   ");
                }
                else
                {
                    Console.Write(i + 1 + "  ");
                }

                WriteLineField(i, playerField, ConsoleColor.DarkBlue, ConsoleColor.DarkRed, ConsoleColor.Yellow, true);
                Console.Write("     ");
                WriteLineField(i, enemyField, ConsoleColor.DarkBlue, ConsoleColor.DarkRed, ConsoleColor.DarkGreen, false);

                Console.Write("  ");
                Console.Write(i + 1);
                Console.WriteLine();
            }
        }

        static void WriteLineField(int x, int[,] field, ConsoleColor colorFone, ConsoleColor colorMiss, ConsoleColor colorHit, bool playerField)
        {
            Console.BackgroundColor = colorFone;
            for (int j = 0; j < SizeField; j++)
            {
                if (field[x, j] == MissValue)
                {
                    WriteCoordenate(colorFone, colorMiss, x, j, field);
                }
                else if ((playerField && field[x, j] == HitValue) || (!playerField && field[x, j] != 0))
                {
                    WriteCoordenate(colorFone, colorHit, x, j, field);
                }
                else
                {
                    WriteCoordenate(colorFone, colorFone, x, j, field);
                }
            }
            Console.BackgroundColor = ConsoleColor.Black;
        }

        static void WriteCoordenate(ConsoleColor colorFone, ConsoleColor colorCoord, int x, int y, int[,] field)
        {
            Console.BackgroundColor = colorCoord;
            Console.Write(field[x, y] + " ");
            Console.BackgroundColor = colorFone;
        }

        static void WriteCoordenates()
        {
            Console.Write("    ");
            for (int j = 0; j < SizeField; j++)
            {
                Console.Write(j + 1 + " ");
            }
        }

        //The field debug method, in order to find out how the enemy has placed the ships.
        static void DebugField(int[,] field = null)
        {
            for (int i = 0; i < SizeField; i++)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                for (int j = 0; j < SizeField; j++)
                {
                    if (field[i, j] == MissValue)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.Write(field[i, j] + " ");
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                    }
                    else if (field[i, j] != 0)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.Write(field[i, j] + " ");
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                    }
                    else
                    {
                        Console.Write(field[i, j] + " ");
                    }
                }
                Console.BackgroundColor = ConsoleColor.Black;

            }
            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
