using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DaGame
{
    
    public class Hero
    {
        int _hp;
        int _ep;
        int _food;
        public int HP
        {
            get { return _hp; }
            set
            {
                if (value > 10)
                    _hp = 10;
                else if (value <= 0)
                {
                    //Вызов события "смерть"
                    _hp = 0;
                    Application.Current.MainWindow.RaiseEvent(new RoutedEventArgs(MainWindow.NoHealthEvent));
                }
                else _hp = value;
            }
        }
        public int EP
        {
            get { return _ep; }
            set
            {
                if (value > 5)
                    _ep = 5;
                else if (value <= 0)
                {
                    //Вызов события "усталость"
                    _ep = 0;
                    Application.Current.MainWindow.RaiseEvent(new RoutedEventArgs(MainWindow.NoEnergyEvent));
                }
                else _ep = value;
            }
        }
        public int Food
        {
            get { return _food; }
            set
            {
                if (value > 3)
                    _food = 3;
                else if (value <= 0)
                {
                    _food = 0;
                }
                else _food = value;
            }
        }
        public Coords Position { get; set; }
        public char LeftHand { get; set; }
        public char RightHand { get; set; }
        
        public Hero(Coords start)
        {
            HP = 10;
            EP = 5;
            Food = 1;
            Position = new Coords(start.Y, start.X);
            LeftHand = 'e';
            RightHand = 'e';
        }
    }    
}
