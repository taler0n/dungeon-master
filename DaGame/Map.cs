using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DaGame
{
    
    public class Coords : IEquatable<Coords>
    {
        int _x;
        int _y;
        public int X
        {
            get { return _x; }
            set
            {
                if (value < 0 || value > 40)
                    throw new ArgumentOutOfRangeException();
                _x = value;
            }
        }
        public int Y
        {
            get { return _y; }
            set
            {
                if (value < 0 || value > 40)
                    throw new ArgumentOutOfRangeException();
                _y = value;
            }
        }
        public Coords(int y, int x)
        {
            Y = y;
            X = x;
        }
        public override int GetHashCode()
        {
            string str = Y.ToString() + " " + X.ToString();
            return str.GetHashCode();
        }
        public bool Equals(Coords other)
        {
            if (Y == other.Y && X == other.X)
                return true;
            return false;
        }
    }

    public class Tile
    {
        //e - empty tile
        //w - water
        //b - brick
        //h - hole
        //c - chest
        //t - treasure
        //s - start
        public string InfoText { get; set; }
        public char ID { get; set; }
        public bool ActionMade { get; set; }
        public Tile(char type)
        {
            ID = type;
            ActionMade = false;
            switch (ID)
            {
                case 'e':
                    {
                        InfoText = "Nothing there.";
                        break;
                    }
                case 's':
                    {
                        InfoText = "The exit is here. You need to get the treasure and return to this point.";
                        break;
                    }
                case 'w':
                    {
                        InfoText = "An underground lake. You need to have an eye on your energy, so you don't drown to your death.";
                        break;
                    }
                case 'b':
                    {
                        InfoText = "This wall looks different. With a closer look you see, that it is built from big stone bricks. Maybe it's not so solid as the others.";
                        break;
                    }
                case 'h':
                    {
                        InfoText = "You stand in front of the dark pit. You are not sure how deep it is. It's too dangerous to go down without some insurance.";
                        break;
                    }
                case 'c':
                    {
                        InfoText = "You found a chest! Someone has hidden their loot in this, so you wonder what is lying there, waiting for you to come.";
                        break;
                    }
                case 't':
                    {
                        InfoText = "The treasure is here! Grab it and make your way to the entrance as fast as possible. There is no need to be in this dark caverns anymore.";
                        break;
                    }
                case '0':
                    {
                        InfoText = "A solid wall. You doubt, if you could even scratch those.";
                        break;
                    }
            }
        }
    }
    
    public class Labyrinth
    {
        public Coords Start { get; private set; }
        public Tile[,] Map { get; set; }
        public List<Monster> Monsters { get; set; }
        public Dictionary<Coords, char> Chests { get; set; }
        public List<Coords> Holes { get; set; }
        public int MonsterTier { get; set; }
        public Labyrinth()
        {
            //Генерация изначальной сетки будущего лабиринта
            Map = new Tile[41, 41];
            bool[,] marked = new bool[20, 20];
            Stack<Coords> stack = new Stack<Coords>();
            marked[0, 0] = true;
            for (int i = 0; i < 41; i += 2)
            {
                for (int j = 0; j < 41; j++)
                    Map[i, j] = new Tile('0');
            }
            for (int i = 1; i < 41; i += 2)
            {
                int j = 0;
                for (j = 0; j < 41; j += 2)
                    Map[i, j] = new Tile('0');
                for (j = 39; j > 0; j -= 2)
                    Map[i, j] = new Tile('e');
            }
            //Пробивание стенок, создание "идеального лабиринта" без циклов и комнат
            int count = 0;
            List<Coords> deadends = new List<Coords>();
            bool building = true;
            Coords current = new Coords(0, 0);
            while (count < 400)
            {
                List<Coords> possibleMoves = new List<Coords>();
                if (current.Y > 0 && !marked[current.Y - 1, current.X])
                    possibleMoves.Add(new Coords(current.Y - 1, current.X));
                if (current.Y < 19 && !marked[current.Y + 1, current.X])
                    possibleMoves.Add(new Coords(current.Y + 1, current.X));
                if (current.X > 0 && !marked[current.Y, current.X - 1])
                    possibleMoves.Add(new Coords(current.Y, current.X - 1));
                if (current.X < 19 && !marked[current.Y, current.X + 1])
                    possibleMoves.Add(new Coords(current.Y, current.X + 1));
                if (possibleMoves.Count > 0)
                {
                    marked[current.Y, current.X] = true;
                    count++;
                    building = true;
                    int move = DataBase.RNG.Next(possibleMoves.Count);
                    stack.Push(current);
                    Map[current.Y + possibleMoves[move].Y + 1, current.X + possibleMoves[move].X + 1] = new Tile('e');
                    current = possibleMoves[move];
                }
                else
                {
                    if (building)
                    {
                        marked[current.Y, current.X] = true;
                        count++;
                        deadends.Add(current);
                    }
                    building = false;
                    current = stack.Pop();
                }
            }
            Start = new Coords(1, 1);
            Chests = new Dictionary<Coords, char>();
            Holes = new List<Coords>();
            Monsters = new List<Monster>();
            MonsterTier = 1;
            Map[1, 1] = new Tile('s');
            //Размещение сокровища в удаленном тупике, либо в люблом тупике, если все близко
            bool noTreasure = true;
            foreach (var tile in deadends)
            {
                if (tile.X > 10 || tile.Y > 10)
                {
                    noTreasure = false;
                    Map[(tile.Y * 2) + 1, (tile.X * 2) + 1] = new Tile('t');
                    deadends.Remove(tile);
                    break;
                }
            }
            if (noTreasure)
            {
                Map[(deadends[deadends.Count - 1].Y * 2) + 1, (deadends[deadends.Count - 1].X * 2) + 1] = new Tile('t');
                deadends.RemoveAt(deadends.Count - 1);
            }
            //Размещение дыр в полу в тупиках
            if (deadends.Count > 5)
            {
                for (int i = 0; i < 5; i++)
                {
                    int pos = DataBase.RNG.Next(deadends.Count);
                    var hole = new Coords((deadends[pos].Y * 2) + 1, (deadends[pos].X * 2) + 1);
                    Holes.Add(hole);
                    Map[hole.Y, hole.X] = new Tile('h');
                    deadends.RemoveAt(pos);
                }
            }
            //Все остальные тупики - под сундуки (или мимиков)
            foreach (var tile in deadends)
            {
                var chest = new Coords((tile.Y * 2) + 1, (tile.X * 2) + 1);
                Map[chest.Y, chest.X] = new Tile('c');
                if (DataBase.RNG.Next(10) > 1)
                {
                    Chests.Add(chest, DataBase.Weapons.ElementAt(DataBase.RNG.Next(1, DataBase.Weapons.Count - 1)).Key);
                }
                else Chests.Add(chest, 'm');
            }
            //Уничтожение случайных стен, чтобы были циклы
            count = DataBase.RNG.Next(15, 20);
            while (count > 0)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int step = DataBase.RNG.Next(-1, 2);
                if (step == 0)
                    step++;
                while (coordX < 40 && coordX > 0)
                {
                    if (Map[coordY, coordX].ID == '0')
                    {
                        bool wallNumber = true;
                        if (Map[coordY - 1, coordX].ID == '0')
                        {
                            if (Map[coordY, coordX - 1].ID == '0' || Map[coordY, coordX + 1].ID == '0')
                                wallNumber = false;
                        }
                        if (Map[coordY + 1, coordX].ID == '0')
                        {
                            if (Map[coordY, coordX - 1].ID == '0' || Map[coordY, coordX + 1].ID == '0')
                                wallNumber = false;
                        }
                        if (wallNumber)
                        {
                            Map[coordY, coordX] = new Tile('e');
                            count--;
                            break;
                        }
                    }
                    coordX += step;
                }
            }
            //Замена случайных стен на кирпичи
            count = DataBase.RNG.Next(20);
            while (count > 0)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int step = DataBase.RNG.Next(-1, 2);
                if (step == 0)
                    step++;
                while (coordX < 40 && coordX > 0)
                {
                    if (Map[coordY, coordX].ID == '0')
                    {
                        bool wallNumber = true;
                        if (Map[coordY - 1, coordX].ID == '0')
                        {
                            if (Map[coordY, coordX - 1].ID == '0' || Map[coordY, coordX + 1].ID == '0')
                                wallNumber = false;
                        }
                        if (Map[coordY + 1, coordX].ID == '0')
                        {
                            if (Map[coordY, coordX - 1].ID == '0' || Map[coordY, coordX + 1].ID == '0')
                                wallNumber = false;
                        }
                        if (wallNumber)
                        {
                            Map[coordY, coordX] = new Tile('b');
                            count--;
                            break;
                        }
                    }
                    coordX += step;
                }
            }
            //Размещение луж воды
            count = DataBase.RNG.Next(20);
            while (count > 0)
            {
                int coordY = DataBase.RNG.Next(2, 39);
                int coordX = DataBase.RNG.Next(2, 39);
                int stepX = DataBase.RNG.Next(-1, 2);
                int stepY = DataBase.RNG.Next(-1, 2);
                int sum = Math.Abs(stepX) + Math.Abs(stepY);
                if (sum > 1)
                    stepX = 0;
                else if (sum == 0)
                    stepX = 1;
                while (coordX < 39 && coordX > 2)
                {
                    if (Map[coordY, coordX].ID == '0' || Map[coordY, coordX].ID == 'e')
                    {
                        Map[coordY, coordX] = new Tile('w');
                        if (DataBase.RNG.Next(2) > 0)
                        {
                            Map[coordY + stepY, coordX + stepX] = new Tile('w');
                            if (DataBase.RNG.Next(4) > 2 && coordY + stepY + stepY > 0 && coordY + stepY + stepY < 40 && coordX + stepX + stepX > 0 && coordX + stepX + stepX < 40)
                            {
                                Map[coordY + stepY + stepY, coordX + stepX + stepX] = new Tile('w');
                            }
                        }
                        count--;
                        break;
                    }
                    coordY += stepY;
                    coordX += stepX;
                }
            }
        }
        //Генерация случайного монстра в зависимости от стадии игры
        public void GenerateMonster(Hero sam, Cell[,] table)
        {
            if (Monsters.Count < 10 * MonsterTier)
            {
                int index = DataBase.RNG.Next(3 * MonsterTier);
                Type[] gameTypes = new Type[] { typeof(Labyrinth), typeof(Hero), typeof(Cell[,])};
                object[] gameValues = new object[] { this, sam, table };
                var o = DataBase.MonsterTypes[index].GetConstructor(gameTypes);
                Monster m = o.Invoke(gameValues) as Monster;
                Monsters.Add(m);

            }
        }
        public Monster FindMonster(Coords position)
        {
            foreach (var beast in Monsters)
                if (beast.Position.Equals(position))
                    return beast;
            return null;
        }
    }
}
