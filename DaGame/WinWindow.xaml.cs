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
    /// Логика взаимодействия для WinWindow.xaml
    /// </summary>
    public partial class WinWindow : Window
    {
        public WinWindow()
        {
            InitializeComponent();
        }
        void WinWindow_Closed(object sender, EventArgs e)
        {
            Owner.Close();
        }
    }
}
