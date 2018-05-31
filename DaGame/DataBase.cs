using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace DaGame
{
    
    public class Weapon
    {
        public double Damage { get; set; }
        public string InfoText { get; set; }
        public string BattleText { get; set; }
        public Weapon(double multiplier, string iText, string bText)
        {
            Damage = multiplier;
            InfoText = iText;
            BattleText = bText;
        }
    }
    
    public static class DataBase
    {
        public static Random RNG;
        public static List<Type> MonsterTypes;
        public static Dictionary<char, Weapon> Weapons;

        public static Image HeroImage;
        public static Dictionary<char, ImageBrush> TileImages;
        public static Dictionary<char, BitmapImage> WeaponImages;
        public static Dictionary<string, BitmapImage> MonsterImages;
        static DataBase()
        {
            RNG = new Random(DateTime.Now.Millisecond);

            TileImages = new Dictionary<char, ImageBrush>();
            WeaponImages = new Dictionary<char, BitmapImage>();
            MonsterImages = new Dictionary<string, BitmapImage>();

            char[] tileCodes = new char[] { 's', 'h', '0', 'b', 't', 'w', 'e', 'c' };
            foreach (char code in tileCodes)
            {
                var image = new ImageBrush();
                string path = @"pack://application:,,,/Resources/Images/tile_" + code + @".png";
                image.ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));
                image.Stretch = System.Windows.Media.Stretch.UniformToFill;
                TileImages.Add(code, image);
            }

            //e - empty hand
            //s - sword
            //p - pickaxe
            //r - rope
            //b - buckler
            //t - torch
            //g - gold -- t for treasure is already there :/
            Weapons = new Dictionary<char, Weapon>();
            Weapons.Add('e', new Weapon(1, "Just your bare hand.", "A punch.\nDeals no additional damage."));
            Weapons.Add('s', new Weapon(2, "A sword. It's still sharp, so the strike would be quite deadly.", "A powerful swing with your sword. Deals double damage."));
            Weapons.Add('p', new Weapon(1.5, "A pickaxe. Can be very useful in combat, if you would land strikes properly. You also wander, if you can destroy something with this.", "A precise strike with your pickaxe. Deals good damage and may also crit for triple damage"));
            Weapons.Add('r', new Weapon(1.15, "A rope with a hook on it's end. It can be rather easy to explore some darker caverns now and to shackle beast in there as well.", "A lash, that shackles the target, decreasing their damage. Deals slightly more damage."));
            Weapons.Add('b', new Weapon(1.25, "A wooden shield. Finally, you've got some protection against these monsters.", "A shield bash, that stuns the target and may cause them to skip turns. Deals moderate damage."));
            Weapons.Add('t', new Weapon(1.25, "A burning torch. Now you can at least properly see in this dark. And also bring some fire to your fights.", "A swing with your torch. Deals moderate damage and ignites the target."));
            Weapons.Add('g', new Weapon(1.15, "The treasure! You need to get to the exit as fast as you can. There is no reason to be in this caves now.", "A heavier blow. Deals slightly more damage."));

            char[] wpnCodes = new char[] { 's', 'r', 'b', 't', 'g', 'e', 'p' };
            foreach (char code in wpnCodes)
            {
                string path = @"pack://application:,,,/Resources/Images/item_" + code + @".png";
                var image = new BitmapImage(new Uri(path, UriKind.Absolute));
                WeaponImages.Add(code, image);
            }

            MonsterTypes = new List<Type>();
            MonsterTypes.Add(typeof(Ghost));
            MonsterTypes.Add(typeof(Statue));
            MonsterTypes.Add(typeof(Goblin));
            MonsterTypes.Add(typeof(Rat));
            MonsterTypes.Add(typeof(Spider));
            MonsterTypes.Add(typeof(Ooze));
            MonsterTypes.Add(typeof(Zombie));
            MonsterTypes.Add(typeof(Skeleton));
            MonsterTypes.Add(typeof(Shadow));
            MonsterTypes.Add(typeof(Bat));
            MonsterTypes.Add(typeof(Mimic));
            MonsterTypes.Add(typeof(Tentacle));

            string[] mnstrCodes = new string[] { "Ghost", "Statue", "Goblin", "Rat", "Spider", "Ooze", "Zombie", "Skeleton", "Shadow", "Bat", "Mimic", "Tentacle" };
            foreach (string code in mnstrCodes)
            {
                string path = @"pack://application:,,,/Resources/Images/" + code + @".png";
                var image = new BitmapImage(new Uri(path, UriKind.Absolute));
                MonsterImages.Add(code, image);
            }

            HeroImage = new Image();
            HeroImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/Hero.png", UriKind.Absolute));
            HeroImage.Stretch = System.Windows.Media.Stretch.UniformToFill;

        }
    }
}
