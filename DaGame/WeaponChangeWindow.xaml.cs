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
    /// Логика взаимодействия для WeaponChangeWindow.xaml
    /// </summary>
    public partial class WeaponChangeWindow : Window
    {
        char WpnFound;
        Hero Sam;
        Label LeftHand;
        Label RightHand;
        public bool ChestExplored = false;
        public WeaponChangeWindow(char item, Hero sam, Label lHand, Label rHand)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WpnFound = item;
            Sam = sam;
            LeftHand = lHand;
            RightHand = rHand;
            WeaponInfoText.Text = DataBase.Weapons[item].InfoText;
        }

        private void LeftHand_Click(object sender, RoutedEventArgs e)
        {
            if (Sam.LeftHand != 'g')
            {
                Sam.LeftHand = WpnFound;
                var image = new Image();
                image.Source = DataBase.WeaponImages[WpnFound];
                image.Stretch = Stretch.UniformToFill;
                LeftHand.Content = image;
                ChestExplored = true;
                Close();
            }
            else MessageBox.Show("One look at the treasure - and it hypnotizes you. Your hand can not drop it anymore.");
        }

        private void RightHand_Click(object sender, RoutedEventArgs e)
        {
            if (Sam.RightHand != 'g')
            {
                Sam.RightHand = WpnFound;
                var image = new Image();
                image.Source = DataBase.WeaponImages[WpnFound];
                image.Stretch = Stretch.UniformToFill;
                RightHand.Content = image;
                ChestExplored = true;
                Close();
            }
            else MessageBox.Show("One look at the treasure - and it hypnotizes you. Your hand can not drop it anymore.");
        }
        private void Leave_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
