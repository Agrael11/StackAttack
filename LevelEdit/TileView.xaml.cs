using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LevelEdit
{
    /// <summary>
    /// Interaction logic for TileView.xaml
    /// </summary>
    public partial class TileView : UserControl
    {
        public TileView()
        {
            InitializeComponent();
        }

        public int MyX = 0;
        public int MyY = 0;

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public void SetTile(Bitmap texture, int X, int Y, int rotation = 0)
        {
            using Bitmap bmp = new(4, 4);
            using Graphics g = Graphics.FromImage(bmp);
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    System.Drawing.Color c = texture.GetPixel(X + x, Y + y);
                    if (rotation == 0)
                    {
                        bmp.SetPixel(x, y, c);
                    }
                    else if (rotation == 90)
                    {
                        bmp.SetPixel((3 - y), x, c);
                    }
                    else if (rotation == 180)
                    {

                        bmp.SetPixel((3 - x), (3 - y), c);
                    }
                    else if (rotation == 270)
                    {
                        bmp.SetPixel(y, (3 - x), c);
                    }
                }
            }
            image.Source = BitmapToImageSource(bmp);
        }

        public void ResetTile()
        {
            image.Source = null;
        }

        bool clicked = false;
        bool hover = false;
        
        public event EventHandler<MouseEventArgs> Pressed;

        public void image_MouseEnter(object sender, MouseEventArgs e)
        {
            hover = true;
            //border.BorderThickness = new Thickness(Width / 10);
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                clicked = true;
                border.BorderBrush = new SolidColorBrush(Colors.Red);
                Pressed?.Invoke(this, e);
            }
            else
            {
                border.BorderBrush = new SolidColorBrush(Colors.LimeGreen);
            }
        }

        public void image_MouseLeave(object sender, MouseEventArgs e)
        {
            clicked = false;
            hover = false;
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            //border.BorderThickness = new Thickness(Width / 20);
        }

        public void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            clicked = false;
            if (hover)
            {
                border.BorderBrush = new SolidColorBrush(Colors.LimeGreen);
                //border.BorderThickness = new Thickness(Width / 10);
            }
            else
            {
                border.BorderBrush = new SolidColorBrush(Colors.Black);
                //border.BorderThickness = new Thickness(Width / 20);
            }
        }

        public void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clicked = true;
            //border.BorderThickness = new Thickness(Width / 10);
            border.BorderBrush = new SolidColorBrush(Colors.Red);
            Pressed?.Invoke(this, e);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //border.BorderThickness = new Thickness(e.NewSize.Width/15);
        }
    }
}
