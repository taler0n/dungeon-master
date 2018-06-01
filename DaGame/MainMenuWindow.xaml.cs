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
using System.IO;
using System.Media;

namespace DaGame
{
    /// <summary>
    /// Логика взаимодействия для MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        //Сразу начинаем играть музыку
        SoundPlayer music;
        public MainMenuWindow()
        {
            InitializeComponent();
            music = new SoundPlayer();
            music.Stream = Properties.Resources.Music;
            music.PlayLooping();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var game = new MainWindow();
            game.Owner = this;
            Visibility = Visibility.Hidden;
            game.Show();
        }

        private void TutorialButton_Click(object sender, RoutedEventArgs e)
        {
            var help = new HelpWindow();
            help.ShowDialog();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
