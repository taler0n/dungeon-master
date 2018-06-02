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
    //Клетка игрового поля
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
        //Создаем события для смерти и усталости
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
            //Рисуем начальное положение и вставляем руки
            Table[0, 0].Visible = false;
            Table[0, 4].Visible = false;
            Table[4, 0].Visible = false;
            Table[4, 4].Visible = false;
            var ImageL = new Image();
            ImageL.Source = DataBase.WeaponImages[Sam.LeftHand];
            LeftHand.Content = ImageL;
            var ImageR = new Image();
            ImageR.Source = DataBase.WeaponImages[Sam.RightHand];
            RightHand.Content = ImageR;
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
                //Пытаемся сделать шаг не в стену
                if (!(Dungeon.Map[coordY, coordX].ID == '0'))
                {
                    if (Dungeon.Map[coordY, coordX].ID == 'b')
                    {
                        //Если кирпичи, то нужна кирка. Из воды кирпичи ломать нельзя, потому что нельзя
                        if (Sam.LeftHand == 'p' || Sam.RightHand == 'p')
                        {
                            if (Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID != 'w')
                            {
                                //Предупреждаем о трате энергии
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
                        //Пытаемся спуститься в дыру на веревке. Опять же, тратим энергию. Тратим еще энергию, если выходим из воды.
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
                                    //Возможно деремся, возможно что-то находим
                                    if (DataBase.RNG.Next(3) > 0)
                                    {
                                        StartBattle(new Bat(Dungeon, Sam, Table), false);
                                    }
                                    if (DataBase.RNG.Next(3) > 1)
                                    {
                                        var wcw = new WeaponChooseWindow(Sam, LeftHand, RightHand);
                                        wcw.Owner = this;
                                        wcw.ShowDialog();
                                        Dungeon.MonsterTier = 2;
                                    }
                                    else
                                    {
                                        var food = new FoodWindow();
                                        food.Owner = this;
                                        food.ShowDialog();
                                        FoodChange(1);
                                    }
                                    //Вылезаем из другой случайной дыры
                                    int nextHole = DataBase.RNG.Next(Dungeon.Holes.Count);
                                    Coords jump = Dungeon.Holes[nextHole];
                                    if (jump.Equals(new Coords(coordY, coordX)))
                                        jump = Dungeon.Holes[(nextHole + 1) % Dungeon.Holes.Count];
                                    Sam.Position.X = jump.X;
                                    Sam.Position.Y = jump.Y;
                                    EnergyChange(-energySpent);
                                    Redraw();
                                    WanderingMonsters();
                                    Redraw();
                                    Keyboard.Focus(this);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Тратим энергию при выходе из воды
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
                        //Происходит событие. Передаем туда координаты, откуда пришли на случай, если придется убегать
                        Interaction(-vertical, -horizontal);
                        Redraw();
                    }
                    //Лечение зависит от текущей энергии
                    int healChance = DataBase.RNG.Next(5);
                    if (Sam.EP > healChance)
                        ChangeHealth(1);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                Redraw();
            }
        }
        void Redraw()
        {
            //Перерисовка поля
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
            //Рисуем видимые клетки и монстров. Остальное - черным
            int coordY = Sam.Position.Y + y - 2;
            int coordX = Sam.Position.X + x - 2;
            if (coordY >= 0 && coordY < 41 && coordX >= 0 && coordX < 41)
            {
                Tile tmp = Dungeon.Map[coordY, coordX];
                if (Table[y, x].Visible)
                {
                    //Все изображения хранятся в базе данных
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
            //Отвратительная проверка видимости. Но лучше я ничего не придумал
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
                        //Проверка завершения игры
                        if (Sam.RightHand == 'g' || Sam.LeftHand == 'g')
                        {
                            WinWindow endWin = new WinWindow();
                            endWin.Owner = this;
                            endWin.ShowDialog();
                        }
                        break;
                    }
                case 't':
                    {
                        //Нашли сокровище. Во второй раз его не взять
                        WeaponChangeWindow wcw = new WeaponChangeWindow('g', Sam, LeftHand, RightHand);
                        wcw.ShowDialog();
                        if (Sam.LeftHand == 'g' || Sam.RightHand == 'g')
                            Dungeon.MonsterTier = 3;
                        if (wcw.ChestExplored)
                            Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID = 'e';
                        break;
                    }
                case 'w':
                    {
                        //Нас могут утащить под воду
                        if (DataBase.RNG.Next(10) == 0)
                        {
                            StartBattle(new Tentacle(Dungeon, Sam, Table), false);
                        }
                        break;
                    }
                case 'c':
                    {
                        //Роемся в сундуке. Ну, или деремся с мимиком
                        char chest = Dungeon.Chests[Sam.Position];
                        if (chest != 'm')
                        {

                            Dungeon.MonsterTier = 2;
                            WeaponChangeWindow wcw = new WeaponChangeWindow(chest, Sam, LeftHand, RightHand);
                            wcw.ShowDialog();
                            if (wcw.ChestExplored)
                                Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID = 'e';
                        }
                        else
                        {
                            StartBattle(new Mimic(Dungeon, Sam, Table), false);
                        }
                        break;
                    }
                case 'e':
                    {
                        //Деремся с монстром, если он тут есть
                        foreach (var beast in Dungeon.Monsters)
                        {
                            if (beast.Position.Equals(Sam.Position))
                            {
                                fleed = StartBattle(beast, true);
                                if (fleed)
                                {
                                    Sam.Position.Y += backY;
                                    Sam.Position.X += backX;
                                    //Гоблин убегает
                                    if (beast is Goblin)
                                        beast.Wander(Sam, Dungeon);
                                }
                            }
                        }
                        break;
                    }
            }
            Redraw();
            //Если кого-то убили - убираем с доски. Монстры двигаются только если мы не убегали. Иначе побег не имеет смысла, потому что сразу догонят
            if (!fleed)
            {
                RemoveMonsters();
                WanderingMonsters();
            }
        }
        void WanderingMonsters()
        {
            //Движение монстров. Если напали они - нельзя убегать
            foreach (var beast in Dungeon.Monsters)
            {
                beast.Wander(Sam, Dungeon);
                if (beast.Position.Equals(Sam.Position))
                {
                    StartBattle(beast, false);
                }
            }
            Dungeon.GenerateMonster(Sam, Table);
            RemoveMonsters();
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
                //Выбираем награду
                if (beast is Goblin)
                {
                    WeaponChooseWindow weapon = new WeaponChooseWindow(Sam, LeftHand, RightHand);
                    weapon.Owner = this;
                    weapon.ShowDialog();
                }
                //Убираем мимика
                else if (beast is Mimic)
                {
                    Dungeon.Chests.Remove(Sam.Position);
                    Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID = 'e';
                }
                //Телепорт ко входу
                else if (beast is Shadow)
                {
                    if (Dungeon.Map[Dungeon.Start.Y + 1, Dungeon.Start.X].ID == '0' || Dungeon.Map[Dungeon.Start.Y + 1, Dungeon.Start.X].ID == 'b')
                        Sam.Position = new Coords(Dungeon.Start.Y, Dungeon.Start.X + 1);
                    else Sam.Position = new Coords(Dungeon.Start.Y + 1, Dungeon.Start.X);
                }
                //Находим лут
                else if (beast.Loot.Count > 0)
                {
                    WeaponChangeWindow wcw = new WeaponChangeWindow(beast.Loot[DataBase.RNG.Next(beast.Loot.Count)], Sam, LeftHand, RightHand);
                    wcw.ShowDialog();
                }
                if (beast.Food && DataBase.RNG.Next(3) == 0)
                {
                    var food = new FoodWindow();
                    food.Owner = this;
                    food.ShowDialog();
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
        //Убираем убитых монстров, чтобы они не ходили, будучи уже убитыми
        void RemoveMonsters()
        {
            foreach (var beast in Killed)
                Dungeon.Monsters.Remove(beast);
            Killed.Clear();
        }
        //Завершение игры :/
        void Death(object sender, RoutedEventArgs e)
        {
            var endLose = new DeathWindow();
            endLose.Owner = this;
            endLose.ShowDialog();
        }
        //Изменение и отрисовка здоровья, энергии и еды
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
        //Тратим еду, либо спим и призываем много-много монстров
        void Fatigue(object sender, RoutedEventArgs e)
        {
            if (!InBattle)
            {
                if (Sam.Food > 0)
                {
                    var eat = new EatWindow();
                    eat.Owner = this;
                    eat.ShowDialog();
                    FoodChange(-1);
                    EnergyChange(5);
                }
                //Спать в воде нельзя - мгновенная смерть
                else if (Dungeon.Map[Sam.Position.Y, Sam.Position.X].ID == 'w')
                {
                    var drown = new DrownWindow();
                    drown.Owner = this;
                    drown.ShowDialog();
                    Sam.HP = 0;
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                        for (int j = 0; j < 5; j++)
                            Table[i, j].Lbl.Background = Brushes.Black;
                    var sleep = new SleepWindow();
                    sleep.Owner = this;
                    sleep.Show();
                    while (Dungeon.Monsters.Count < 10 * Dungeon.MonsterTier)
                        Dungeon.GenerateMonster(Sam, Table);
                    EnergyChange(5);
                }
            }
        }
        //Возвращаем окно меню
        private void MainWindow_Closing(object sender, EventArgs e)
        {
            Owner.Visibility = Visibility.Visible;
            Application.Current.MainWindow = Owner;
        }
        //Вывод информации в журнал
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
