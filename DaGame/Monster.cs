using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DaGame
{
     
    public abstract class Monster
    {
        int _hp;
        public int HP 
        {
            get { return _hp; } 
            set
            {
                if (value < 0)
                    _hp = 0;
                else _hp = value;
            }
        }
        public int EP { get; set; }
        public Coords Position { get; set; }
        public int Damage { get; set; }
        public List<char> Loot { get; set; }
        public bool Food { get; set; }
        public string InfoText;
        public string SpecialText;
        public void Attack(Hero sam, bool shackled)
        {
            int dmg = Damage;
            int rand = DataBase.RNG.Next(0, 10);
            if (rand > 8)
            {
                dmg++;
            }
            else if (rand < 3)
                dmg--;
            double multiplier = 1;
            if (sam.LeftHand == 'b')
                multiplier *= 0.75;
            if (sam.RightHand == 'b')
                multiplier *= 0.75;
            if (shackled)
                multiplier *= 0.5;
            sam.HP -= (int)Math.Round(dmg * multiplier);
        }
        public abstract void Wander(Hero sam, Labyrinth dungeon);
        public abstract void SpecialAttack(Hero sam, bool shackled);
    }

     
    public class Mimic : Monster
    {
        public Mimic(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 6;
            EP = 1;
            Damage = 2;
            Food = true;
            Loot = new List<char>() { 's', 'p', 'r', 't', 'b' };
            InfoText = "A mimic!";
            SpecialText = "Mimic doesn't have a special attack.";
            Position = new Coords(0, 0);
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
        }
    }

     
    public class Tentacle : Monster
    {
        public Tentacle(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 5;
            EP = 1;
            Damage = 0;
            Food = false;
            Loot = new List<char>();
            InfoText = "A tentacle!";
            SpecialText = "The tentacle drags you into the deeps. Only cold death awaits you there.";
            Position = new Coords(0, 0);
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {

        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            sam.HP -= 100;
        }
    }

     
    public class Bat : Monster
    {
        public Bat(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 6;
            EP = 1;
            Damage = 1;
            Food = true;
            Loot = new List<char>();
            InfoText = "A bat!";
            SpecialText = "A vampiric bite, which heals the bat.";
            Position = new Coords(0, 0);
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {

        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
            if (sam.LeftHand != 'b' && sam.RightHand != 'b')
            {
                HP += 2;
                if (HP > 5)
                    HP = 5;
            }
        }
    }

     
    public class Ghost : Monster
    {
        public Ghost(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 4;
            EP = 1;
            Damage = 1;
            Food = false;
            Loot = new List<char>() { 's', 'p', 'r', 't', 'b' };
            InfoText = "A wandering ghost. It seems, that this creature doesn't want to attack you, but it's still dangerous to come close.";
            SpecialText = "A vicious attack, that chills you to your bones. Deals damage both to your health and stamina.";
            while (true)
            {
                int coordY = DataBase.RNG.Next(41);
                int coordX = DataBase.RNG.Next(41);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == '0' || dungeon.Map[coordY, coordX].ID == 'e' || dungeon.Map[coordY, coordX].ID == 'b' || dungeon.Map[coordY, coordX].ID == 'w')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            Coords[] newCoords = new Coords[4];
            if (Position.Y > 0)
                newCoords[0] = new Coords(Position.Y - 1, Position.X);
            if (Position.X < 40)
                newCoords[1] = new Coords(Position.Y, Position.X + 1);
            if (Position.Y < 40)
                newCoords[2] = new Coords(Position.Y + 1, Position.X);
            if (Position.X > 0)
                newCoords[3] = new Coords(Position.Y, Position.X - 1);
            int index = DataBase.RNG.Next(4);
            for (int i = 0; i < 4; i++)
            {
                Coords move = newCoords[(index + i) % 4];
                if (move == null)
                    continue;
                if (dungeon.Map[move.Y, move.X].ID == '0' || dungeon.Map[move.Y, move.X].ID == 'e' || dungeon.Map[move.Y, move.X].ID == 'b' || dungeon.Map[move.Y, move.X].ID == 'w')
                {
                    Position = move;
                    break;
                }
            }
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
            if (sam.LeftHand != 'b' && sam.RightHand != 'b')
                sam.EP--;
        }
    }

     
    public class Statue : Monster
    {
        public Statue(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 100;
            EP = 1;
            Damage = 0;
            Food = false;
            Loot = new List<char>();
            InfoText = "An ancient statue. Seems to be safe, but when you look at it, chills run down your spine. It's better not to touch this thing.";
            SpecialText = "???";
            while (true)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            HP = 100;
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            if (HP < 100)
                sam.HP -= 100;
        }
    }

     
    public class Goblin : Monster
    {
        public Goblin(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 4;
            EP = 1;
            Damage = 1;
            Food = true;
            Loot = new List<char>();
            InfoText = "That's greedy goblin! Who knows, what treasures he has in his bag?";
            SpecialText = "The goblin pushes you and runs away!";
            while (true)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            Coords[] newCoords = new Coords[4];
            if (Position.Y > 0)
                newCoords[0] = new Coords(Position.Y - 1, Position.X);
            if (Position.X < 40)
                newCoords[1] = new Coords(Position.Y, Position.X + 1);
            if (Position.Y < 40)
                newCoords[2] = new Coords(Position.Y + 1, Position.X);
            if (Position.X > 0)
                newCoords[3] = new Coords(Position.Y, Position.X - 1);
            int index = DataBase.RNG.Next(4);
            for (int i = 0; i < 4; i++)
            {
                Coords move = newCoords[(index + i) % 4];
                if (move == null)
                    continue;
                if (dungeon.Map[move.Y, move.X].ID == 'e' && !sam.Position.Equals(move))
                {
                    Position = move;
                    break;
                }
            }
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
        }
    }

     
    public class Rat : Monster
    {
        public Rat(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 5;
            EP = 1;
            Damage = 2;
            Food = true;
            Loot = new List<char>() { 'r' };
            InfoText = "A giant rat. It seems to be rather aggressive.";
            SpecialText = "The rat knocks you down and rummages in your bags for something edible. Hopefully, you have something in there.";
            while (true)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            Coords[] newCoords = new Coords[4];
            if (Position.Y > 0)
                newCoords[0] = new Coords(Position.Y - 1, Position.X);
            if (Position.X < 40)
                newCoords[1] = new Coords(Position.Y, Position.X + 1);
            if (Position.Y < 40)
                newCoords[2] = new Coords(Position.Y + 1, Position.X);
            if (Position.X > 0)
                newCoords[3] = new Coords(Position.Y, Position.X - 1);
            int index = DataBase.RNG.Next(4);
            int chosen = -1;
            for (int i = 0; i < 4; i++)
            {
                Coords move = newCoords[(index + i) % 4];
                if (move == null)
                    continue;
                if (sam.Position.Equals(move))
                {
                    Position = move;
                    chosen = -1;
                    break;
                }
                if (dungeon.Map[move.Y, move.X].ID == 'e')
                {
                    chosen = (index + i) % 4;
                }
            }
            if (chosen != -1)
                Position = newCoords[chosen];
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
            if (sam.LeftHand != 'b' && sam.RightHand != 'b')
            {
                if (sam.Food > 0)
                    sam.Food--;
                else Attack(sam, shackled);
            }
        }
    }

     
    public class Spider : Monster
    {
        public Spider(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 6;
            EP = 1;
            Damage = 2;
            Food = true;
            Loot = new List<char>() { 's', 'p', 'r', 't', 'b' };
            InfoText = "A giant spider. Fortuntely, it won't go out of it's lair even to attack you.";
            SpecialText = "You get stuck in sticky web. Can't run away anymore.";
            while (true)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
        }
    }

     
    public class Ooze : Monster
    {
        public Ooze(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 7;
            EP = 1;
            Damage = 1;
            Food = false;
            Loot = new List<char>() { 's', 'p', 'r', 'b' };
            InfoText = "A giant slime. Try not to get in it's way.";
            SpecialText = "The ooze covers your weapons in it's goo. They will be useless in this fight.";
            while (true)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            if (DataBase.RNG.Next(2) > 0)
            {
                Coords[] newCoords = new Coords[4];
                if (Position.Y > 0)
                    newCoords[0] = new Coords(Position.Y - 1, Position.X);
                if (Position.X < 40)
                    newCoords[1] = new Coords(Position.Y, Position.X + 1);
                if (Position.Y < 40)
                    newCoords[2] = new Coords(Position.Y + 1, Position.X);
                if (Position.X > 0)
                    newCoords[3] = new Coords(Position.Y, Position.X - 1);
                int index = DataBase.RNG.Next(4);
                for (int i = 0; i < 4; i++)
                {
                    Coords move = newCoords[(index + i) % 4];
                    if (move == null)
                        continue;
                    if (dungeon.Map[move.Y, move.X].ID == 'e')
                    {
                        Position = move;
                        break;
                    }
                }
            }
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
        }
    }

     
    public class Zombie : Monster
    {
        public Zombie(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 7;
            EP = 1;
            Damage = 1;
            Food = false;
            Loot = new List<char>() { 's', 'p', 'r', 't', 'b' };
            InfoText = "A terrifying creature stands in front of you. It's a walking dead, which hungers for flesh. Your flesh.";
            SpecialText = "The zombie gets you in a deadly grip. You get poisoned by it's rotten bite.";
            while (true)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            Coords[] newCoords = new Coords[4];
            if (Position.Y > 0)
                newCoords[0] = new Coords(Position.Y - 1, Position.X);
            if (Position.X < 40)
                newCoords[1] = new Coords(Position.Y, Position.X + 1);
            if (Position.Y < 40)
                newCoords[2] = new Coords(Position.Y + 1, Position.X);
            if (Position.X > 0)
                newCoords[3] = new Coords(Position.Y, Position.X - 1);
            int index = DataBase.RNG.Next(4);
            int chosen = -1;
            for (int i = 0; i < 4; i++)
            {
                Coords move = newCoords[(index + i) % 4];
                if (move == null)
                    continue;
                if (sam.Position.Equals(move))
                {
                    Position = move;
                    chosen = -1;
                    break;
                }
                if (dungeon.Map[move.Y, move.X].ID == 'e')
                {
                    chosen = (index + i) % 4;
                }
            }
            if (chosen != -1)
                Position = newCoords[chosen];
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
        }
    }

     
    public class Skeleton : Monster
    {
        public Skeleton(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 5;
            EP = 1;
            Damage = 3;
            Food = false;
            Loot = new List<char>() { 's' };
            InfoText = "A living skeleton stands in your way. Beware of it's sharp sword!";
            SpecialText = "The skeleton inflicts a deadly blow, dealing massive damage.";
            while (true)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            Coords[] newCoords = new Coords[4];
            if (Position.Y > 0)
                newCoords[0] = new Coords(Position.Y - 1, Position.X);
            if (Position.X < 40)
                newCoords[1] = new Coords(Position.Y, Position.X + 1);
            if (Position.Y < 40)
                newCoords[2] = new Coords(Position.Y + 1, Position.X);
            if (Position.X > 0)
                newCoords[3] = new Coords(Position.Y, Position.X - 1);
            int index = DataBase.RNG.Next(4);
            int chosen = -1;
            for (int i = 0; i < 4; i++)
            {
                Coords move = newCoords[(index + i) % 4];
                if (move == null)
                    continue;
                if (sam.Position.Equals(move))
                {
                    Position = move;
                    chosen = -1;
                    break;
                }
                if (dungeon.Map[move.Y, move.X].ID == 'e')
                {
                    chosen = (index + i) % 4;
                }
            }
            if (chosen != -1)
                Position = newCoords[chosen];
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
            if (sam.LeftHand != 'b' && sam.RightHand != 'b')
                Attack(sam, shackled);
        }
    }

     
    public class Shadow : Monster
    {
        public Shadow(Labyrinth dungeon, Hero sam, Cell[,] table)
        {
            HP = 7;
            EP = 1;
            Damage = 3;
            if (sam.LeftHand == 't' || sam.RightHand == 't')
                Damage--;
            Food = false;
            Loot = new List<char>();
            InfoText = "You stare into the swirling darkness and see something watching back. The shadow of this very dungeon is here. Perhaps, it's unstable energy can transport you somewhere...";
            SpecialText = "The shadow becomes ethereal, making this fight even harder. Your strikes can barely damage it then.";
            while (true)
            {
                int coordY = DataBase.RNG.Next(1, 40);
                int coordX = DataBase.RNG.Next(1, 40);
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (tableX > -1 && tableX < 3 && tableY > -1 && tableY < 3 && table[tableY, tableX].Visible == true)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void Wander(Hero sam, Labyrinth dungeon)
        {
            while (true)
            {
                int coordY = DataBase.RNG.Next(Math.Max(1, sam.Position.Y - 5), Math.Min(6, sam.Position.Y + 5));
                int coordX = DataBase.RNG.Next(Math.Max(1, sam.Position.X - 5), Math.Min(6, sam.Position.X + 5));
                int tableY = sam.Position.Y - coordY + 2;
                int tableX = sam.Position.X - coordX + 2;
                if (sam.Position.Y == coordY && sam.Position.X == coordX)
                    continue;
                if (dungeon.Map[coordY, coordX].ID == 'e')
                {
                    Position = new Coords(coordY, coordX);
                    break;
                }
            }
        }
        public override void SpecialAttack(Hero sam, bool shackled)
        {
            Attack(sam, shackled);
        }
    }
}
