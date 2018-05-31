using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
namespace DaGame
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public class Cell
    {
        public bool Visible { get; set; }
        public Label Lbl { get; private set; }
        public Cell(Label link)
        {
            Visible = true;
            Lbl = link;
        }
    }
    public partial class MainWindow : Window
    {
        Cell[,] Table { get; set; }
        bool MovingEnabled = true;
        bool InBattle = false;
        List<Monster> Killed;
        public Hero Sam;
        public Labyrinth Dungeon;
        public static RoutedEvent NoEnergyEvent = EventManager.RegisterRoutedEvent("NoEnergy", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainWindow));
        public event RoutedEventHandler NoEnergy
        {
            add { AddHandler(NoEnergyEvent, value); }
            remove { RemoveHandler(NoEnergyEvent, value); }
        }
        public static RoutedEvent NoHealthEvent = EventManager.RegisterRoutedEvent("NoHealth", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainWindow));
        public event RoutedEventHandler NoHealth
        {
            add { AddHandler(NoHealthEvent, value); }
            remove { RemoveHandler(NoHealthEvent, value); }
        }
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            Dungeon = new Labyrinth();
            Sam = new Hero(Dungeon.Start);
            Killed = new List<Monster>();
            NoEnergy += Fatigue;
            NoHealth += Death;
            Table = new Cell[5, 5];
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    string name = "cell" + i + j;
                    Table[i, j] = new Cell((Label)Grid2.FindName(name));
                }
            Table[0, 0].Visible = false;
            Table[0, 4].Visible = false;
            Table[4, 0].Visible = false;
            Table[4, 4].Visible = false;
            LeftHand.Content = DataBase.WeaponImages[Sam.LeftHand];
            RightHand.Content = DataBase.WeaponImages[Sam.RightHand];
            Redraw();
        }
        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        { 
            
            if (MovingEnabled)
            {
                MovingEnabled = false;
                switch (e.Key)
                {
                    case Key.Up:
                        {
                            Move(-1, 0);
                            break;
                        }
                    case Key.Right:
                        {
                            Move(0, 1);
                            break;
                        }
                    case Key.Down:
                        {
                            Move(1, 0);
                            break;
                        }
                    case Key.Left:
                        {
                            Move(0, -1);
                            break;
                        }
                }
                MovingEnabled = true;
            }
        }
        void Move(int vertical, int horizontal)
        {
            try
            {
                int coordY = Sam.Position.Y + vertical;
                int coordX = Sam.Position.X + horizontal;
                if (!(Dungeon.Map[coordY, coordX].ID == '0'))
                {
                    if (Dungeon.Map[coordY, coordX].ID == 'b')
                    {
                        if (Sam.LeftHand == 'p' || Sam.RightHand == 'p')
                        {
                            if (Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID != 'w')
                            {
                                EnergySpendWindow esw = new EnergySpendWindow();
                                esw.ShowDialog();
                                if (esw.OK)
                                {
                                    Sam.Position.Y = coordY;
                                    Sam.Position.X = coordX;
                                    Dungeon.Map[coordY, coordX] = new Tile('e');
                                    Redraw();
                                    EnergyChange(-1);
                                    WanderingMonsters();
                                    RemoveMonsters();
                                    Redraw();
                                }
                            }
                        }
                    }
                    else if (Dungeon.Map[coordY, coordX].ID == 'h')
                    {
                        if (Sam.LeftHand == 'r' || Sam.RightHand == 'r')
                        {
                            EnergySpendWindow esw = new EnergySpendWindow();
                            esw.ShowDialog();
                            if (esw.OK)
                            {
                                int energySpent = 1;
                                if (Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID == 'w')
                                    energySpent++;
                                if (Sam.EP >= energySpent)
                                {
                                    if (DataBase.RNG.Next(3) > 0)
                                    {
                                        StartBattle(new Bat(Dungeon, Sam, Table), false);
                                    }
                                    if (DataBase.RNG.Next(3) > 1)
                                    {
                                        var wcw = new WeaponChooseWindow(Sam, LeftHand, RightHand);
                                        Visibility = Visibility.Hidden;
                                        wcw.ShowDialog();
                                        Visibility = Visibility.Visible;
                                        Dungeon.MonsterTier = 2;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Lurking in the dark, you found some glowing shrooms. They are strange, but quite tasty.");
                                        FoodChange(1);
                                    }
                                    int nextHole = DataBase.RNG.Next(Dungeon.Holes.Count);
                                    Coords jump = Dungeon.Holes[nextHole];
                                    if (jump.Equals(new Coords(coordY, coordX)))
                                        jump = Dungeon.Holes[(nextHole + 1) % Dungeon.Holes.Count];
                                    Sam.Position.X = jump.X;
                                    Sam.Position.Y = jump.Y;
                                    EnergyChange(-energySpent);
                                    Redraw();
                                    WanderingMonsters();
                                    RemoveMonsters();
                                    Redraw();
                                    Keyboard.Focus(this);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID == 'w')
                        {
                            Sam.Position.Y = coordY;
                            Sam.Position.X = coordX;
                            Redraw();
                            EnergyChange(-1);
                        }
                        else
                        {
                            Sam.Position.Y = coordY;
                            Sam.Position.X = coordX;
                            Redraw();
                        }
                        Interaction(-vertical, -horizontal);
                        RemoveMonsters();
                        Redraw();
                    }
                    int healChance = DataBase.RNG.Next(5);
                    if (Sam.EP > healChance)
                        ChangeHealth(1);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("You naughty cheater!");
            }
        }
        void Redraw()
        {
            VisibilityCheck();
            for (int i = 1; i < 4; i++)
            {
                DrawTile(0, i);
                DrawTile(4, i);
                for (int j = 0; j < 5; j++)
                    DrawTile(i, j);
            }
            Table[2, 2].Lbl.Content = DataBase.HeroImage;

        }
        void DrawTile(int y, int x)
        {
            int coordY = Sam.Position.Y + y - 2;
            int coordX = Sam.Position.X + x - 2;
            if (coordY >= 0 && coordY < 41 && coordX >= 0 && coordX < 41)
            {
                Tile tmp = Dungeon.Map[coordY, coordX];
                if (Table[y, x].Visible)
                {
                    Table[y, x].Lbl.Background = DataBase.TileImages[Dungeon.Map[coordY, coordX].ID];
                    var beast = Dungeon.FindMonster(new Coords(coordY, coordX));
                    if (beast != null)
                    {
                        var image = new Image();
                        image.Source = DataBase.MonsterImages[beast.GetType().Name];
                        image.Stretch = Stretch.UniformToFill;
                        Table[y, x].Lbl.Content = image;
                    }
                    else Table[y, x].Lbl.Content = null;
                }
                else
                {
                    Table[y, x].Lbl.Background = Brushes.Black;
                    Table[y, x].Lbl.Content = null;
                }
            }
            else
            {
                Table[y, x].Lbl.Background = Brushes.Black;
                Table[y, x].Lbl.Content = null;
            }
        }
        void VisibilityCheck()
        {
            //terrible code below
            if (Sam.LeftHand == 't' || Sam.RightHand == 't')
            {
                for (int i = 1; i < 4; i++)
                {
                    Table[0, i].Visible = true;
                    Table[4, i].Visible = true;
                    Table[i, 0].Visible = true;
                    Table[i, 4].Visible = true;
                }
                if (Dungeon.Map[Sam.Position.Y - 1, Sam.Position.X].ID == '0' || Dungeon.Map[Sam.Position.Y - 1, Sam.Position.X].ID == 'b')
                {
                    Table[0, 1].Visible = false;
                    Table[0, 2].Visible = false;
                    Table[0, 3].Visible = false;
                }
                if (Dungeon.Map[Sam.Position.Y + 1, Sam.Position.X].ID == '0' || Dungeon.Map[Sam.Position.Y + 1, Sam.Position.X].ID == 'b')
                {
                    Table[4, 1].Visible = false;
                    Table[4, 2].Visible = false;
                    Table[4, 3].Visible = false;
                }
                if (Dungeon.Map[Sam.Position.Y, Sam.Position.X + 1].ID == '0' || Dungeon.Map[Sam.Position.Y, Sam.Position.X + 1].ID == 'b')
                {
                    Table[1, 4].Visible = false;
                    Table[2, 4].Visible = false;
                    Table[3, 4].Visible = false;
                }
                if (Dungeon.Map[Sam.Position.Y, Sam.Position.X - 1].ID == '0' || Dungeon.Map[Sam.Position.Y, Sam.Position.X - 1].ID == 'b')
                {
                    Table[1, 0].Visible = false;
                    Table[2, 0].Visible = false;
                    Table[3, 0].Visible = false;
                }
                if (Dungeon.Map[Sam.Position.Y - 1, Sam.Position.X - 1].ID == '0' || Dungeon.Map[Sam.Position.Y - 1, Sam.Position.X - 1].ID == 'b')
                {
                    Table[1, 0].Visible = false;
                    Table[0, 1].Visible = false;
                }
                if (Dungeon.Map[Sam.Position.Y - 1, Sam.Position.X + 1].ID == '0' || Dungeon.Map[Sam.Position.Y - 1, Sam.Position.X + 1].ID == 'b')
                {
                    Table[1, 4].Visible = false;
                    Table[0, 3].Visible = false;
                }
                if (Dungeon.Map[Sam.Position.Y + 1, Sam.Position.X - 1].ID == '0' || Dungeon.Map[Sam.Position.Y + 1, Sam.Position.X - 1].ID == 'b')
                {
                    Table[3, 0].Visible = false;
                    Table[4, 1].Visible = false;
                }
                if (Dungeon.Map[Sam.Position.Y + 1, Sam.Position.X + 1].ID == '0' || Dungeon.Map[Sam.Position.Y + 1, Sam.Position.X + 1].ID == 'b')
                {
                    Table[3, 4].Visible = false;
                    Table[4, 3].Visible = false;
                }
            }
            else
                for (int i = 1; i < 4; i++)
                {
                    Table[0, i].Visible = false;
                    Table[4, i].Visible = false;
                    Table[i, 0].Visible = false;
                    Table[i, 4].Visible = false;
                }
        }
        void Interaction(int backY, int backX)
        {
            bool fleed = false;
            switch (Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID)
            {
                case 's':
                    {
                        if (Sam.RightHand == 'g' || Sam.LeftHand == 'g')
                        {
                            WinWindow endWin = new WinWindow();
                            endWin.Owner = this;
                            Visibility = Visibility.Hidden;
                            endWin.ShowDialog();
                        }
                        break;
                    }
                case 't':
                    {
                        if (!Dungeon.Map[Sam.Position.Y, Sam.Position.X].ActionMade)
                        {
                            WeaponChangeWindow wcw = new WeaponChangeWindow('g', Sam, LeftHand, RightHand);
                            wcw.ShowDialog();
                            if (Sam.LeftHand == 'g' || Sam.RightHand == 'g')
                                Dungeon.MonsterTier = 3;
                        }
                        break;
                    }
                case 'w':
                    {
                        if (DataBase.RNG.Next(10) == 0)
                        {
                            StartBattle(new Tentacle(Dungeon, Sam, Table), false);
                        }
                        break;
                    }
                case 'c':
                    {
                        if (DataBase.RNG.Next(10) > 1)
                        {
                            if (!Dungeon.Map[Sam.Position.Y, Sam.Position.X].ActionMade)
                            {
                                Dungeon.MonsterTier = 2;
                                WeaponChangeWindow wcw = new WeaponChangeWindow(Dungeon.Chests[Sam.Position], Sam, LeftHand, RightHand);
                                wcw.ShowDialog();
                                if (wcw.ChestExplored)
                                    Dungeon.Map[Sam.Position.Y, Sam.Position.X].ActionMade = true;
                            }
                        }
                        else
                        {
                            StartBattle(new Mimic(Dungeon, Sam, Table), false);
                        }
                        break;
                    }
                case 'e':
                    {
                        foreach (var beast in Dungeon.Monsters)
                        {
                            if (beast.Position.Equals(Sam.Position))
                            {
                                fleed = StartBattle(beast, true);
                                if (fleed)
                                {
                                    Sam.Position.Y += backY;
                                    Sam.Position.X += backX;
                                }
                            }
                        }
                        break;
                    }
            }
            if (!fleed)
            {
                RemoveMonsters();
                WanderingMonsters();
            }
        }
        void WanderingMonsters()
        {
            foreach (var beast in Dungeon.Monsters)
            {
                beast.Wander(Sam, Dungeon);
                if (beast.Position.Equals(Sam.Position))
                {
                    StartBattle(beast, false);
                }
            }
            Dungeon.GenerateMonster(Sam, Table);
        }
        bool StartBattle(Monster beast, bool initiative)
        {
            bool fleed = false;
            InBattle = true;
            var battleground = new BattleWindow(Sam, beast, initiative);
            battleground.Owner = this;
            Visibility = Visibility.Hidden;
            battleground.ShowDialog();
            fleed = battleground.Fleed;
            if (!IsLoaded)
                return false;
            Visibility = Visibility.Visible;
            if (beast.HP == 0)
            {
                if (beast is Goblin)
                {
                    WeaponChooseWindow weapon = new WeaponChooseWindow(Sam, LeftHand, RightHand);
                    weapon.ShowDialog();
                    weapon.Focus();
                }
                else if (beast is Mimic)
                {
                    Dungeon.Chests.Remove(Sam.Position);
                    Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID = 'e';
                }
                else if (beast is Shadow)
                {
                    if (Dungeon.Map[Dungeon.Start.Y + 1, Dungeon.Start.X].ID == '0' || Dungeon.Map[Dungeon.Start.Y + 1, Dungeon.Start.X].ID == 'b')
                        Sam.Position = new Coords(Dungeon.Start.Y, Dungeon.Start.X + 1);
                    else Sam.Position = new Coords(Dungeon.Start.Y + 1, Dungeon.Start.X);
                }
                else if (beast.Loot.Count > 0)
                {
                    WeaponChangeWindow wcw = new WeaponChangeWindow(beast.Loot[DataBase.RNG.Next(beast.Loot.Count)], Sam, LeftHand, RightHand);
                    wcw.ShowDialog();
                }
                if (beast.Food && DataBase.RNG.Next(3) == 0)
                {
                    MessageBox.Show("You also found some food.");
                    FoodChange(1);
                }
                Killed.Add(beast);
            }
            Keyboard.Focus(this);
            InBattle = false;
            ChangeHealth(0);
            EnergyChange(0);
            FoodChange(0);
            return fleed;
        }
        void RemoveMonsters()
        {
            foreach (var beast in Killed)
                Dungeon.Monsters.Remove(beast);
            Killed.Clear();
        }
        void Death(object sender, RoutedEventArgs e)
        {
            var endLose = new DeathWindow();
            endLose.Owner = this;
            Visibility = Visibility.Hidden;
            endLose.ShowDialog();
        }
        void ChangeHealth(int delta)
        {
            int count = Sam.HP + delta;
            for (int i = 0; i < 10; i++)
            {
                string name = "HP" + i;
                if (count > 0)
                {
                    ((Label)FindName(name)).Visibility = Visibility.Visible;
                    count--;
                }
                else ((Label)FindName(name)).Visibility = Visibility.Hidden;
            }
            Sam.HP += delta;
        }
        void EnergyChange(int delta)
        {
            int count = Sam.EP + delta;
            for (int i = 0; i < 5; i++)
            {
                string name = "EP" + i;
                if (count > 0)
                {
                    ((Label)FindName(name)).Visibility = Visibility.Visible;
                    count--;
                }
                else ((Label)FindName(name)).Visibility = Visibility.Hidden;
            }
            Sam.EP += delta;
        }
        void Fatigue(object sender, RoutedEventArgs e)
        {
            if (!InBattle)
            {
                if (Sam.Food > 0)
                {
                    MessageBox.Show("You feel tired. Fortunately, you still have some food. It helps you to recover quickly.");
                    FoodChange(-1);
                    EnergyChange(5);
                }
                else if (Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID == 'w')
                {
                    MessageBox.Show("You try not to drown as hard as you can. But soon your last strength disappeared. These cold deeps welcome you now.");
                    Sam.HP = 0;
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                        for (int j = 0; j < 5; j++)
                            Table[i, j].Lbl.Background = Brushes.Black;
                    MessageBox.Show("You are completely tired. Suddenly, you feel, that you're falling asleep. Hopefully, nothing will happen while you rest.");
                    while (Dungeon.Monsters.Count < 10 * Dungeon.MonsterTier)
                        Dungeon.GenerateMonster(Sam, Table);
                    EnergyChange(5);
                }
            }
        }
        void FoodChange(int delta)
        {
            int count = Sam.Food + delta;
            for (int i = 0; i < 3; i++)
            {
                string name = "F" + i;
                if (count > 0)
                {
                    ((Label)FindName(name)).Visibility = Visibility.Visible;
                    count--;
                }
                else ((Label)FindName(name)).Visibility = Visibility.Hidden;
            }
            Sam.Food += delta;
        }

        private void MainWindow_Closing(object sender, EventArgs e)
        {
            Owner.Visibility = Visibility.Visible;
            Application.Current.MainWindow = Owner;
        }

        private void Tile_ShowInfo(object sender, MouseEventArgs e)
        {
            Label tile = (Label)sender;
            int y = int.Parse(tile.Name[4].ToString());
            int x = int.Parse(tile.Name[5].ToString());
            if (Table[y, x].Visible)
            {
                bool empty = true;
                foreach (var beast in Dungeon.Monsters)
                    if (beast.Position.Y == Sam.Position.Y + y - 2 && beast.Position.X == Sam.Position.X + x - 2)
                    {
                        empty = false;
                        Info.Text = beast.InfoText;
                        break;
                    }
                if (empty)
                    Info.Text = Dungeon.Map[Sam.Position.Y + y - 2, Sam.Position.X + x - 2].InfoText;
            }
            else Info.Text = "It's too dark to see anything.";
        }

        private void MainWindow_HideInfo(object sender, MouseEventArgs e)
        {
            Info.Text = "Hover your mouse over something to get some info...";
        }

        private void HP_ShowInfo(object sender, MouseEventArgs e)
        {
            Info.Text = "This bar shows your current health state. If it reaches zero - you die.";
        }

        private void EP_ShowInfo(object sender, MouseEventArgs e)
        {
            Info.Text = "You need energy to perform hard physical actions. When you have no energy, you can fill it up by eating food or having rest. But be careful - the more you rest the more dangerous this trip becomes.";
        }

        private void Food_ShowInfo(object sender, MouseEventArgs e)
        {
            Info.Text = "Food is basically your energy supply. When you are out of food the only way to refill energy is sleeping.";
        }

        private void LeftHand_ShowInfo(object sender, MouseEventArgs e)
        {
            Info.Text = DataBase.Weapons[Sam.LeftHand].InfoText;
        }

        private void RightHand_ShowInfo(object sender, MouseEventArgs e)
        {
            Info.Text = DataBase.Weapons[Sam.RightHand].InfoText;
        }

        private void Journal_ShowInfo(object sender, MouseEventArgs e)
        {
            Info.Text = "That is your journal. Here you can see some info about things around.";
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
