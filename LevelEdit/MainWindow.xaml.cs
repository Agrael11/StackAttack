using StackAttack.Engine.Map;
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
using static StackAttack.Engine.Sprite;
using static StackAttack.Engine.Tile;

namespace LevelEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly int _saneMinimum = 16;
        readonly int _saneMaximum = 128;
        readonly List<StackAttack.Engine.Texture.TextureDefinition> textureDefinitions =  new();
        readonly List<StackAttack.Engine.Tile.TileDefinition> tileDefinitions = new();
        readonly List<StackAttack.Engine.Sprite.SpriteDefinition> spriteDefinitions = new();
        TileMap foreground = new();
        TileMap background = new();
        PlayerStartData playerStart = new();
        List<GameObjectStartData> gameObjectStartDatas = new();
        private bool placeplayer = false;
        int active = 1;

        TileMapView backgroundView;
        TileMapView foregroundView;
        TileMapView objectView;

        public MainWindow()
        {
            StackAttack.Game.LoadDefinitionData("textureDefinitions.json", ref textureDefinitions);
            StackAttack.Game.LoadDefinitionData("tileDefinitions.json", ref tileDefinitions);
            StackAttack.Game.LoadDefinitionData("spriteDefinitions.json", ref spriteDefinitions);
            InitializeComponent();
            
            backgroundView = new();
            backgroundView.GridZoom = 8;
            backgroundView.TilePressed += StandardView_TilePressed;
            
            foregroundView = new();
            foregroundView.GridZoom = 8;
            foregroundView.TilePressed += StandardView_TilePressed;
            
            objectView = new();
            objectView.GridZoom = 8;
            objectView.TilePressed += ObjectView_TilePressed;

            gridEditor.Children.Add(foregroundView);
            gridEditor.Children.Add(objectView);
            gridEditor.Children.Add(backgroundView);
            WindowState = WindowState.Maximized;
        }
        private void CheckSaneNumber(ref object sender)
        {
            if (sender is not TextBox textBox)
                return;

            int number = int.Parse(textBox.Text);
            if (number < _saneMinimum) number = _saneMinimum;
            else if (number > _saneMaximum) number = _saneMaximum;
            textBox.Text = number.ToString();
            backgroundView.GridWidth = int.Parse(WidthTextBox.Text);
            backgroundView.GridHeight = int.Parse(HeightTextBox.Text);
            foregroundView.GridWidth = int.Parse(WidthTextBox.Text);
            foregroundView.GridHeight = int.Parse(HeightTextBox.Text);
            objectView.GridWidth = int.Parse(WidthTextBox.Text);
            objectView.GridHeight = int.Parse(HeightTextBox.Text);
        }

        private void CheckKeyIsNum(ref object sender, ref KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                CheckSaneNumber(ref sender);
            }
            if (e.Key != Key.D0 && e.Key != Key.D1 && e.Key != Key.D2 && e.Key != Key.D3 && e.Key != Key.D4 && e.Key != Key.D5 && e.Key != Key.D6 && e.Key != Key.D7 && e.Key != Key.D8 && e.Key != Key.D9 && e.Key != Key.Back && e.Key != Key.NumPad0 && e.Key != Key.NumPad1 && e.Key != Key.NumPad2 && e.Key != Key.NumPad3 && e.Key != Key.NumPad4 && e.Key != Key.NumPad5 && e.Key != Key.NumPad6 && e.Key != Key.NumPad7 && e.Key != Key.NumPad8 && e.Key != Key.NumPad9 && e.Key != Key.Delete && e.Key != Key.Right && e.Key != Key.Left && e.Key != Key.Tab)
            {
                e.Handled = true;
            }
        }

        private void NumericTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CheckKeyIsNum(ref sender, ref e);
        }

        private void NumericTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSaneNumber(ref sender);
        }

        private void NumericTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSaneNumber(ref sender);
        }

        private void LayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (foregroundView != null && foregroundView != null && objectView != null)
            {
                gridEditor.Children.Clear();
                //0 BG
                if (LayerComboBox.SelectedIndex == 0)
                {
                    gridEditor.Children.Add(foregroundView);
                    gridEditor.Children.Add(objectView);
                    gridEditor.Children.Add(backgroundView);
                    active = 1;
                }
                //1 FG
                else if (LayerComboBox.SelectedIndex == 1)
                {
                    gridEditor.Children.Add(backgroundView);
                    gridEditor.Children.Add(objectView);
                    gridEditor.Children.Add(foregroundView);
                    active = 2;
                }
                //2 OBJ
                else if (LayerComboBox.SelectedIndex == 2)
                {
                    gridEditor.Children.Add(backgroundView);
                    gridEditor.Children.Add(foregroundView);
                    gridEditor.Children.Add(objectView);
                    active = 3;
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new()
            {
                Filter = "Json Files (*.json)|*.json"
            };
            if (dialog.ShowDialog() == true)
            {
                
                LevelData level = new();
                StackAttack.Game.LoadDefinitionData(dialog.FileName, ref level);
                WidthTextBox.Text = level.LevelWidth.ToString();
                HeightTextBox.Text = level.LevelHeight.ToString();
                foreground = level.Foreground;
                background = level.Background;
                gameObjectStartDatas = level.GameObjectStartDatas;
                playerStart = level.PlayerStartData;
                LevelTextBox.Text = level.NextLevel;
                for (int i = 0; i < MusicComboBox.Items.Count; i++)
                {
                    string content = (string)((ComboBoxItem)MusicComboBox.Items[i]).Content;
                    if (content == level.Music)
                    {
                        MusicComboBox.SelectedIndex = i;
                        break;
                    }
                }
                GoalComboBox.SelectedIndex = level.Goal;
                backgroundView.GridWidth = int.Parse(WidthTextBox.Text);
                backgroundView.GridHeight = int.Parse(HeightTextBox.Text);
                foregroundView.GridWidth = int.Parse(WidthTextBox.Text);
                foregroundView.GridHeight = int.Parse(HeightTextBox.Text);
                objectView.GridWidth = int.Parse(WidthTextBox.Text);
                objectView.GridHeight = int.Parse(HeightTextBox.Text);
                string item = "";
                TileDefinition[] selectedDefs;
                int X;
                int Y;
                TileView view;
                System.Drawing.Image bmp = System.Drawing.Image.FromFile("Textures\\TilesAndSprites.png");
                foreach (TileData data in background.Tiles)
                {
                    item = data.TileID;
                    selectedDefs = tileDefinitions.Where(t => t.TileID == item).ToArray();
                    if (selectedDefs.Length <= 0)
                        return;

                    X = selectedDefs[0].X;
                    Y = selectedDefs[0].Y;
                    view = backgroundView.GetAt(data.TileX, data.TileY);
                    view.SetTile((Bitmap)bmp, X, Y, 0);
                }
                foreach (TileData data in foreground.Tiles)
                {
                    item = data.TileID;
                    selectedDefs = tileDefinitions.Where(t => t.TileID == item).ToArray();
                    if (selectedDefs.Length <= 0)
                        return;

                    X = selectedDefs[0].X;
                    Y = selectedDefs[0].Y;
                    view = foregroundView.GetAt(data.TileX, data.TileY);
                    view.SetTile((Bitmap)bmp, X, Y, 0);
                }
                SpriteDefinition[] spriteDefs;
                int rotation;
                foreach (GameObjectStartData data in gameObjectStartDatas)
                {
                    view = objectView.GetAt(data.ObjectX/4, data.ObjectY/4);
                    item = data.GameObjectTypeID;
                    spriteDefs = spriteDefinitions.Where(t => t.SpriteID == item).ToArray();

                    if (spriteDefs.Length <= 0)
                        return;

                    X = spriteDefs[0].X;
                    Y = spriteDefs[0].Y;
                    switch (data.Heading)
                    {
                        case StackAttack.Engine.Headings.South:
                            rotation = 180;
                            break;
                        case StackAttack.Engine.Headings.East:
                            rotation = 90;
                            break;
                        case StackAttack.Engine.Headings.West:
                            rotation = 270;
                            break;
                        case StackAttack.Engine.Headings.North:
                        default:
                            rotation = 0;
                            break;
                    }
                    view.SetTile((Bitmap)bmp, X, Y, rotation);
                }

                view = objectView.GetAt(playerStart.PlayerX / 4, playerStart.PlayerY / 4);

                item = "Player";
                spriteDefs = spriteDefinitions.Where(t => t.SpriteID == item).ToArray();
                if (spriteDefs.Length <= 0)
                    return;

                X = spriteDefs[0].X;
                Y = spriteDefs[0].Y;
                switch (playerStart.Heading)
                {
                    case StackAttack.Engine.Headings.South:
                        rotation = 180;
                        break;
                    case StackAttack.Engine.Headings.East:
                        rotation = 90;
                        break;
                    case StackAttack.Engine.Headings.West:
                        rotation = 270;
                        break;
                    case StackAttack.Engine.Headings.North:
                    default:
                        rotation = 0;
                        break;
                }
                view.SetTile((Bitmap)bmp, X, Y, rotation);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new()
            {
                Filter = "Json Files (*.json)|*.json"
            };
            if (dialog.ShowDialog() == true)
            {
                LevelData level = new()
                {
                    LevelWidth = int.Parse(WidthTextBox.Text),
                    LevelHeight = int.Parse(HeightTextBox.Text),
                    Foreground = foreground,
                    Background = background,
                    GameObjectStartDatas = gameObjectStartDatas,
                    PlayerStartData = playerStart,
                    NextLevel = LevelTextBox.Text,
                    Music = (string)((ComboBoxItem)MusicComboBox.SelectedItem).Content,
                    Goal = GoalComboBox.SelectedIndex
                };
                StackAttack.Game.SaveDefintionData(dialog.FileName, level);
            }
        }

        private void PlacePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            placeplayer = true;
        }

        private void zoom_plus_Click(object sender, RoutedEventArgs e)
        {
            backgroundView.GridZoom++;
            foregroundView.GridZoom++;
            objectView.GridZoom++;
        }

        private void zoom_minus_Click(object sender, RoutedEventArgs e)
        {
            backgroundView.GridZoom--;
            foregroundView.GridZoom--;
            objectView.GridZoom--;
        }

        private void StandardView_TilePressed(object? sender, MouseEventArgs e)
        {
            if (sender is null)
                return;

            if (active <= 2)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    string item = (string)((ComboBoxItem)TileComboBox.SelectedItem).Content;
                    TileDefinition[] selectedDefs = tileDefinitions.Where(t => t.TileID == item).ToArray();
                    if (selectedDefs.Length <= 0)
                        return;
                    
                    int X = selectedDefs[0].X;
                    int Y = selectedDefs[0].Y;
                    TileView view = (TileView)sender;
                    System.Drawing.Image bmp = System.Drawing.Image.FromFile("Textures\\TilesAndSprites.png");
                    view.SetTile((Bitmap)bmp, X, Y, 0);
                    if (active <= 1)
                    {
                        for (int i = foreground.Tiles.Count - 1; i >= 0; i--)
                        {
                            if (foreground.Tiles[i].TileX == view.MyX && foreground.Tiles[i].TileY == view.MyY)
                            {
                                foreground.Tiles.RemoveAt(i);
                            }
                        }
                        foreground.Tiles.Add(new TileData(item, view.MyX, view.MyY, 0));
                    }
                    else if (active == 2)
                    {
                        for (int i = background.Tiles.Count - 1; i >= 0; i--)
                        {
                            if (background.Tiles[i].TileX == view.MyX && background.Tiles[i].TileY == view.MyY)
                            {
                                background.Tiles.RemoveAt(i);
                            }
                        }
                        background.Tiles.Add(new TileData(item, view.MyX, view.MyY, 0));
                    }
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    TileView view = (TileView)sender;
                    view.ResetTile();
                    if (active <= 1)
                    {
                        for (int i = foreground.Tiles.Count-1; i >= 0; i--)
                        {
                            if (foreground.Tiles[i].TileX == view.MyX && foreground.Tiles[i].TileY == view.MyY)
                            {
                                foreground.Tiles.RemoveAt(i);
                            }
                        }
                    }
                    else if (active == 2)
                    {
                        for (int i = background.Tiles.Count - 1; i >= 0; i--)
                        {
                            if (background.Tiles[i].TileX == view.MyX && background.Tiles[i].TileY == view.MyY)
                            {
                                background.Tiles.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        private void ObjectView_TilePressed(object? sender, MouseEventArgs e)
        {
            if (sender is null)
                return;
            if (active == 3)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (!placeplayer)
                    {
                        TileView view = (TileView)sender;
                        if (playerStart.PlayerX == view.MyX * 4 && playerStart.PlayerY == view.MyY * 4)
                            return;

                        string item = (string)((ComboBoxItem)ObjectComboBox.SelectedItem).Content;
                        SpriteDefinition[] selectedDefs = spriteDefinitions.Where(t => t.SpriteID == item).ToArray();
                        if (selectedDefs.Length <= 0)
                            return;

                        int X = selectedDefs[0].X;
                        int Y = selectedDefs[0].Y;
                        System.Drawing.Image bmp = System.Drawing.Image.FromFile("Textures\\TilesAndSprites.png");
                        StackAttack.Engine.Headings heading;
                        int rotation;
                        switch (HeadingComboBox.SelectedIndex)
                        {
                            case 1:
                                rotation = 180;
                                heading = StackAttack.Engine.Headings.South;
                                break;
                            case 2:
                                rotation = 90;
                                heading = StackAttack.Engine.Headings.East;
                                break;
                            case 3:
                                rotation = 270;
                                heading = StackAttack.Engine.Headings.West;
                                break;
                            case 0:
                            default:
                                rotation = 0; heading = StackAttack.Engine.Headings.North;
                                break;
                        }

                        for (int i = gameObjectStartDatas.Count - 1; i >= 0; i--)
                        {
                            if ((gameObjectStartDatas[i].ObjectX == view.MyX * 4) && (gameObjectStartDatas[i].ObjectY == view.MyY * 4))
                            {
                                gameObjectStartDatas.RemoveAt(i);
                            }
                        }

                        gameObjectStartDatas.Add(new GameObjectStartData(item, view.MyX * 4, view.MyY * 4, heading, item));
                        view.SetTile((Bitmap)bmp, X, Y, rotation);
                    }
                    else
                    {
                        placeplayer = false;
                        SpriteDefinition[] selectedDefs = spriteDefinitions.Where(t => t.SpriteID == "Player").ToArray();
                        if (selectedDefs.Length <= 0)
                            return;

                        int X = selectedDefs[0].X;
                        int Y = selectedDefs[0].Y;
                        TileView view = (TileView)sender;
                        System.Drawing.Image bmp = System.Drawing.Image.FromFile("Textures\\TilesAndSprites.png");
                        StackAttack.Engine.Headings heading;
                        int rotation;
                        switch (HeadingComboBox.SelectedIndex)
                        {
                            case 1:
                                rotation = 180;
                                heading = StackAttack.Engine.Headings.South;
                                break;
                            case 2:
                                rotation = 90;
                                heading = StackAttack.Engine.Headings.East;
                                break;
                            case 3:
                                rotation = 270;
                                heading = StackAttack.Engine.Headings.West;
                                break;
                            case 0:
                            default:
                                rotation = 0; heading = StackAttack.Engine.Headings.North;
                                break;
                        }

                        playerStart = new PlayerStartData(view.MyX * 4, view.MyY * 4, heading);
                        view.SetTile((Bitmap)bmp, X, Y, rotation);
                    }
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    TileView view = (TileView)sender;
                    if (playerStart.PlayerX == view.MyX * 4 && playerStart.PlayerY == view.MyY)
                        return;

                    view.ResetTile();
                    for (int i = gameObjectStartDatas.Count - 1; i >= 0; i--)
                    {
                        if ((gameObjectStartDatas[i].ObjectX == view.MyX * 4) && (gameObjectStartDatas[i].ObjectY == view.MyY * 4))
                        {
                            gameObjectStartDatas.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
