using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DaGame
{
    /// <summary>
    /// Interaction logic for BattleWindow.xaml
    /// </summary>
    public partial class BattleWindow : Window
    {
        public bool Fleed = false;
        Hero Sam;
        Monster Enemy;
        bool LeaveEnabled;
        bool AttackEnabled = true;
        bool MonsterEffectApplied = false;
        int OnFire = 0;
        int Shackled = 0;
        int Stunned = 0;
        int BaseDamage;
        public BattleWindow(Hero sam, Monster enemy, bool initiative)
        {
            InitializeComponent();
            heroPicture.Content = DataBase.HeroImage;
            var imageM = new Image();
            imageM.Source = DataBase.MonsterImages[enemy.GetType().Name];
            imageM.Stretch = Stretch.UniformToFill;
            enemyPicture.Content = imageM;

            var imageA1 = new Image();
            imageA1.Source = DataBase.WeaponImages[sam.LeftHand];
            imageA1.Stretch = Stretch.UniformToFill;
            Atk1Button.Content = imageA1;

            var imageA2 = new Image();
            imageA2.Source = DataBase.WeaponImages[sam.RightHand];
            imageA2.Stretch = Stretch.UniformToFill;
            Atk2Button.Content = imageA2;

            var imageS = new Image();
            imageS.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/energy.png", UriKind.Absolute));
            imageS.Stretch = Stretch.UniformToFill;
            SpecAtkButton.Content = imageS;

            var imageP = new Image();
            imageP.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/pass.png", UriKind.Absolute));
            imageP.Stretch = Stretch.UniformToFill;
            PassButton.Content = imageP;

            var imageAM = new Image();
            imageAM.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/item_s.png", UriKind.Absolute));
            imageAM.Stretch = Stretch.UniformToFill;
            AtkMButton.Content = imageAM;

            var imageSM = new Image();
            imageSM.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/energy.png", UriKind.Absolute));
            imageSM.Stretch = Stretch.UniformToFill;
            SpecAtkMButton.Content = imageSM;

            Atk1Button.ToolTip = DataBase.Weapons[sam.LeftHand].BattleText;
            Atk2Button.ToolTip = DataBase.Weapons[sam.RightHand].BattleText;
            PassButton.ToolTip = "Do nothing.";
            if (initiative)
                LeaveButton.ToolTip = "Escape the battle.";
            else LeaveButton.ToolTip = "You was ambushed by the monster and can't escape!";
            SpecAtkButton.ToolTip = "A powerful double blow with both your hands. Requires energy.";
            SpecAtkMButton.ToolTip = enemy.SpecialText;
            switch (enemy.Damage)
            {
                case 0:
                    {
                        AtkMButton.ToolTip = "Does nothing.";
                        break;
                    }
                case 1:
                    {
                        AtkMButton.ToolTip = "A light punch. Deals low damage.";
                        break;
                    }
                case 2:
                    {
                        AtkMButton.ToolTip = "A normal strike. Deals moderate damage.";
                        break;
                    }
                case 3:
                    {
                        AtkMButton.ToolTip = "A heavy blow. Deals high damage.";
                        break;
                    }
            }
            Sam = sam;
            Enemy = enemy;
            LeaveEnabled = initiative;
            BaseDamage = 1;
            RedrawBars();
        }

        private void Atk1Button_Click(object sender, RoutedEventArgs e)
        {
            if (AttackEnabled)
            {
                AttackEnabled = false;
                Attack(Sam.LeftHand);
                if (MonsterEffectApplied && Enemy is Zombie)
                    Sam.HP--;
                MonsterAttack();
                AttackEnabled = true;
            }
        }

        private void Atk2Button_Click(object sender, RoutedEventArgs e)
        {
            if (AttackEnabled)
            {
                AttackEnabled = false;
                Attack(Sam.RightHand);
                if (MonsterEffectApplied && Enemy is Zombie)
                    Sam.HP--;
                MonsterAttack();
                AttackEnabled = true;
            }
        }

        private void SpecAtkButton_Click(object sender, RoutedEventArgs e)
        {
            if (AttackEnabled && Sam.EP > 0)
            {
                AttackEnabled = false;
                Attack(Sam.LeftHand);
                Attack(Sam.RightHand);
                Sam.EP--;
                if (MonsterEffectApplied && Enemy is Zombie)
                    Sam.HP--;
                MonsterAttack();
                AttackEnabled = true;
            }
        }
        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (AttackEnabled)
            {
                AttackEnabled = false;
                if (MonsterEffectApplied && Enemy is Zombie)
                    Sam.HP--;
                MonsterAttack();
                AttackEnabled = true;
            }
        }

        void Attack(char weapon)
        {
            int rand = DataBase.RNG.Next(0, 10);
            int additional = 0;
            if (rand > 7)
            {
                additional++;
                if (weapon == 'p')
                    additional++;
            }
            else if (rand < 1)
                additional--;
            double multiplier = DataBase.Weapons[weapon].Damage;
            if (MonsterEffectApplied && Enemy is Ooze)
                multiplier = 1;
            DamageMonster((int)Math.Round((BaseDamage + additional) * multiplier));
            if (weapon == 'r')
                Shackled = 3;
            if (weapon == 't')
                OnFire = 3;
            if (weapon == 'b')
                Stunned = 3;
            RedrawBars();
        }

        void MonsterAttack()
        {
            if (Enemy.HP > 0)
            {
                bool tied = false;
                if (Shackled > 0)
                {
                    tied = true;
                    Shackled--;
                }
                bool vertigo = false;
                if (Stunned > 0)
                {
                    if (DataBase.RNG.Next(2) > 0)
                        vertigo = true;
                    Stunned--;
                }
                if (!vertigo)
                {
                    if (Enemy.EP == 3)
                    {
                        if (Enemy is Shadow)
                            BaseDamage--;
                        else if (Enemy is Goblin)
                        {
                            Fleed = true;
                            Close();
                        }
                        else if (Sam.LeftHand != 'b' && Sam.RightHand != 'b')
                        {
                            MonsterEffectApplied = true;
                            if (Enemy is Spider)
                                LeaveEnabled = false;
                        }
                        Enemy.SpecialAttack(Sam, tied);
                        Enemy.EP = 0;
                        if (Enemy is Statue && Sam.HP > 0)
                            DamageMonster(100);
                    }
                    else
                    {
                        Enemy.Attack(Sam, tied);
                        Enemy.EP++;
                    }
                }
                if (OnFire > 0)
                {
                    DamageMonster(1);
                    OnFire--;
                }
                RedrawBars();
            }
        }
        void DamageMonster(int delta)
        {
            Enemy.HP -= delta;
            if (Enemy.HP == 0)
            {
                Close();
            }
        }
        private void LeaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeaveEnabled)
            {
                Fleed = true;
                Close();
            }
        }
        void RedrawBars()
        {
            int count = Sam.HP;
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
            count = Sam.EP;
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
            count = Enemy.HP;
            for (int i = 0; i < 10; i++)
            {
                string name = "HPM" + i;
                if (count > 0)
                {
                    ((Label)FindName(name)).Visibility = Visibility.Visible;
                    count--;
                }
                else ((Label)FindName(name)).Visibility = Visibility.Hidden;
            }
            count = Enemy.EP;
            for (int i = 0; i < 3; i++)
            {
                string name = "EPM" + i;
                if (count > 0)
                {
                    ((Label)FindName(name)).Visibility = Visibility.Visible;
                    count--;
                }
                else ((Label)FindName(name)).Visibility = Visibility.Hidden;
            }
        }

        private void BattleWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Fleed && Sam.HP > 0 && Enemy.HP > 0)
                e.Cancel = true;
        }
    }
}
