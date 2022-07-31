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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LevelEdit
{
    /// <summary>
    /// Interaction logic for TileMapView.xaml
    /// </summary>
    public partial class TileMapView : UserControl
    {
        public TileMapView()
        {
            InitializeComponent();
            GenerateColumnsAndRows();
            GenerateTiles();
        }

        int width = 16;
        int height = 16;
        
        public int GridWidth { 
            get
            {
                return width;
            }
            set
            {
                if (width != value)
                {
                    int oldWidth = width;
                    width = value;
                    GenerateColumnsAndRows();
                    UpdateTiles(oldWidth, height);
                    Width = zoom * GridWidth * 4;
                    Height = zoom * GridHeight * 4;
                    mainGrid.Width = Width;
                    mainGrid.Height = Height;
                }
            }
        }

        public TileView GetAt(int X, int Y)
        {
            return views[$"{X}-{Y}"];
        }

        public int GridHeight
        {
            get
            {
                return height;
            }
            set
            {
                if (height != value)
                {
                    int oldHeight = height;
                    height = value;
                    GenerateColumnsAndRows();
                    UpdateTiles(width, oldHeight);
                    Width = zoom * GridWidth * 4;
                    Height = zoom * GridHeight * 4;
                    mainGrid.Width = Width;
                    mainGrid.Height = Height;
                }
            }
        }

        int zoom = 4;
        public int GridZoom { 
            get
            {
                return zoom;
            }
            set
            {
                zoom = value;
                Width = zoom * width * 4;
                Height = zoom * height * 4;
                GenerateColumnsAndRows();
            }
        }

        Dictionary<string, TileView> views = new();

        public void GenerateColumnsAndRows()
        {
            mainGrid.ColumnDefinitions.Clear();
            mainGrid.RowDefinitions.Clear();
            for (int y = 0; y < width; y++)
            {
                ColumnDefinition definition = new();
                definition.Width = new GridLength(4 * zoom);
                mainGrid.ColumnDefinitions.Add(definition);
            }
            for (int x = 0; x < height; x++)
            {
                RowDefinition definition = new();
                definition.Height = new GridLength(4 * zoom);
                mainGrid.RowDefinitions.Add(definition);
            }
        }

        public void GenerateTiles()
        {
            mainGrid.Children.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!views.ContainsKey($"{x}-{y}"))
                    {
                        TileView view = new();
                        view.SetValue(Grid.ColumnProperty, x);
                        view.MyX = x;
                        view.MyY = y;
                        view.SetValue(Grid.RowProperty, y);
                        view.VerticalAlignment = VerticalAlignment.Stretch;
                        view.HorizontalAlignment = HorizontalAlignment.Stretch;
                        view.MouseEnter += View_MouseEnter;
                        view.MouseLeave += View_MouseLeave;
                        view.MouseDown += View_MouseDown;
                        view.MouseUp += View_MouseUp;
                        view.Pressed += View_Pressed;
                        mainGrid.Children.Add(view);
                        views.Add($"{x}-{y}", view);
                    }
                    else
                    {
                        mainGrid.Children.Add(views[$"{x}-{y}"]);
                    }
                }
            }
        }

        public event EventHandler<MouseEventArgs> TilePressed;

        private void View_Pressed(object? sender, MouseEventArgs e)
        {
            TilePressed?.Invoke(sender, e);
        }

        public void ResizeTiles()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    views[$"{x}-{y}"].Width = 4 * zoom;
                    views[$"{x}-{y}"].Height = 4 * zoom;
                }
            }
        }

        public void UpdateTiles(int oldWidth, int oldHeight)
        {
            if (width < oldWidth)
            {
                for (int x = oldWidth - 1; x < width; x++)
                {
                    for (int y = 0; y < oldHeight; y++)
                    {
                        mainGrid.Children.Remove(views[$"{x}-{y}"]);
                        views.Remove($"{x}-{y}");
                    }
                }
            }
            if (height < oldHeight)
            {
                for (int x = 0; x < oldWidth; x++)
                {
                    for (int y = oldHeight - 1; y < height; y++)
                    {
                        if (views.ContainsKey($"{x}-{y}"))
                        {
                            mainGrid.Children.Remove(views[$"{x}-{y}"]);
                            views.Remove($"{x}-{y}");
                        }
                    }
                }
            }
            GenerateTiles();
        }     

        private void View_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TileView view = ((TileView)sender);
            view.image_MouseUp(sender, e);
        }

        private void View_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TileView view = ((TileView)sender);
            view.image_MouseDown(sender, e);
        }

        private void View_MouseLeave(object sender, MouseEventArgs e)
        {
            TileView view = ((TileView)sender);
            view.image_MouseLeave(sender, e);
        }

        private void View_MouseEnter(object sender, MouseEventArgs e)
        {
            TileView view = ((TileView)sender);
            view.image_MouseEnter(sender, e);
        }
    }
}
