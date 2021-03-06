﻿using System;
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
    /// Логика взаимодействия для WeaponChooseWindow.xaml
    /// </summary>
    public partial class WeaponChooseWindow : Window
    {
        public char weapon;
        Hero Sam;
        Label LeftHand;
        Label RightHand;
        Image image;
        public WeaponChooseWindow(Hero sam, Label lHand, Label rHand)
        {
            InitializeComponent();
            Sam = sam;
            weapon = '\0';
            image = new Image();
            image.Stretch = Stretch.UniformToFill;
            LeftHand = lHand;
            RightHand = rHand;
            SwordButton.ToolTip = DataBase.Weapons['s'].InfoText;
            PickAxeButton.ToolTip = DataBase.Weapons['p'].InfoText;
            RopeButton.ToolTip = DataBase.Weapons['r'].InfoText;
            ShieldButton.ToolTip = DataBase.Weapons['b'].InfoText;
            TorchButton.ToolTip = DataBase.Weapons['t'].InfoText;
            switch (sam.LeftHand)
            {
                case 's':
                    {
                        LeftHandButton.ToolTip = "Current: sword";
                        break;
                    }
                case 'p':
                    {
                        LeftHandButton.ToolTip = "Current: pickaxe";
                        break;
                    }
                case 'r':
                    {
                        LeftHandButton.ToolTip = "Current: rope";
                        break;
                    }
                case 'b':
                    {
                        LeftHandButton.ToolTip = "Current: buckler";
                        break;
                    }
                case 't':
                    {
                        LeftHandButton.ToolTip = "Current: torch";
                        break;
                    }
                case 'g':
                    {
                        LeftHandButton.ToolTip = "Current: treasure";
                        break;
                    }
                default:
                    {
                        LeftHandButton.ToolTip = "Current: nothing";
                        break;
                    }
            }
        
        switch (sam.RightHand)
            {
                case 's':
                    {
                        RightHandButton.ToolTip = "Current: sword";
                        break;
                    }
                case 'p':
                    {
                        RightHandButton.ToolTip = "Current: pickaxe";
                        break;
                    }
                case 'r':
                    {
                        RightHandButton.ToolTip = "Current: rope";
                        break;
                    }
                case 'b':
                    {
                        RightHandButton.ToolTip = "Current: buckler";
                        break;
                    }
                case 't':
                    {
                        RightHandButton.ToolTip = "Current: torch";
                        break;
                    }
                case 'g':
                    {
                        RightHandButton.ToolTip = "Current: treasure";
                        break;
                    }
                default:
                    {
                        RightHandButton.ToolTip = "Current: nothing";
                        break;
                    }
            }
        }

        private void RopeButton_Click(object sender, RoutedEventArgs e)
        {
            weapon = 'r';
            image.Source = DataBase.WeaponImages[weapon];
            ChosenWeapon.Content = image;
        }

        private void ShieldButton_Click(object sender, RoutedEventArgs e)
        {
            weapon = 'b';
            image.Source = DataBase.WeaponImages[weapon];
            ChosenWeapon.Content = image;
        }

        private void SwordButton_Click(object sender, RoutedEventArgs e)
        {
            weapon = 's';
            image.Source = DataBase.WeaponImages[weapon];
            ChosenWeapon.Content = image;
        }

        private void PickAxeButton_Click(object sender, RoutedEventArgs e)
        {
            weapon = 'p';
            image.Source = DataBase.WeaponImages[weapon];
            ChosenWeapon.Content = image;
        }

        private void TorchButton_Click(object sender, RoutedEventArgs e)
        {
            weapon = 't';
            image.Source = DataBase.WeaponImages[weapon];
            ChosenWeapon.Content = image;
        }

        private void LeftHand_Click(object sender, RoutedEventArgs e)
        {
            if (weapon != '\0')
            {
                if (Sam.LeftHand != 'g')
                {
                    Sam.LeftHand = weapon;
                    LeftHand.Content = image;
                    Close();
                }
            }
            else MessageBox.Show("Make a choice.");
        }

        private void RightHand_Click(object sender, RoutedEventArgs e)
        {
            if (weapon != '\0')
            {
                if (Sam.RightHand != 'g')
                {
                    Sam.RightHand = weapon;
                    RightHand.Content = image;
                    Close();
                }
            }
            else MessageBox.Show("Make a choice.");
        }
    }
}
