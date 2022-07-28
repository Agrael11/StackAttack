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

namespace LevelEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int _saneMinimum = 16;
        int _saneMaximum = 128;
        List<StackAttack.Engine.Texture.TextureDefinition> textureDefinitions =  new();
        List<StackAttack.Engine.Tile.TileDefinition> tileDefinitions = new();
        List<StackAttack.Engine.Sprite.SpriteDefinition> spriteDefinitions = new();
        TileMap foreground = new();
        TileMap background = new();
        PlayerStartData playerStart = new();
        List<GameObjectStartData> gameObjectStartDatas = new();
        Bitmap editorBitmap;
        Bitmap previewBitmap;
        private bool placeplayer = false;
        int stimer = 0;

        public void UpdateBitmaps()
        {
            int width = int.Parse(WidthTextBox.Text);
            int height = int.Parse(HeightTextBox.Text);
            previewBitmap = new Bitmap(width * 16, height * 16);
            using (Graphics g = Graphics.FromImage(previewBitmap))
            {
                g.Clear(System.Drawing.Color.Transparent);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        g.DrawRectangle(Pens.Black, new System.Drawing.Rectangle(x * 16, y * 16, 16, 16));
                    }
                }
                foreach (TileData tile in background.Tiles)
                {
                    string id = tile.TileID;
                    int X = tile.TileX;
                    int Y = tile.TileY;
                    var tileDefinition = tileDefinitions.Where(t => t.TileID == id).ToList()[0];
                    id = tileDefinition.TextureID;
                    var textureDefinition = textureDefinitions.Where(t => t.TextureID == id).ToList()[0];
                    Bitmap img = (Bitmap)Bitmap.FromFile(textureDefinition.FileName);
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            System.Drawing.Color c = img.GetPixel(tileDefinition.X + x, tileDefinition.Y + y);
                            g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 16 + x * 4, Y * 16 + y * 4, 4, 4));
                        }
                    }
                }
                foreach (TileData tile in foreground.Tiles)
                {
                    string id = tile.TileID;
                    int X = tile.TileX;
                    int Y = tile.TileY;
                    var tileDefinition = tileDefinitions.Where(t => t.TileID == id).ToList()[0];
                    id = tileDefinition.TextureID;
                    var textureDefinition = textureDefinitions.Where(t => t.TextureID == id).ToList()[0];
                    Bitmap img = (Bitmap)Bitmap.FromFile(textureDefinition.FileName);
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            System.Drawing.Color c = img.GetPixel(tileDefinition.X + x, tileDefinition.Y + y);
                            g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 16 + x * 4, Y * 16 + y * 4, 4, 4));
                        }
                    }
                }
                foreach (GameObjectStartData gameObject in gameObjectStartDatas)
                {
                    string id = gameObject.SpriteID;
                    var spriteDefinition = spriteDefinitions.Where(t => t.SpriteID == id).ToList()[0];
                    int X = gameObject.ObjectX;
                    int Y = gameObject.ObjectY;
                    id = spriteDefinition.TextureID;
                    var textureDefinition = textureDefinitions.Where(t => t.TextureID == id).ToList()[0];
                    Bitmap img = (Bitmap)Bitmap.FromFile(textureDefinition.FileName);
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            if (gameObject.Heading == StackAttack.Engine.Headings.North)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + x, spriteDefinition.Y + y);
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (gameObject.Heading == StackAttack.Engine.Headings.South)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + x, spriteDefinition.Y + (3 - y));
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (gameObject.Heading == StackAttack.Engine.Headings.West)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + y, spriteDefinition.Y + x);
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (gameObject.Heading == StackAttack.Engine.Headings.East)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + y, spriteDefinition.Y + (3 - x));
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                        }
                    }
                }
                {
                    string id = "Player";
                    var spriteDefinition = spriteDefinitions.Where(t => t.SpriteID == id).ToList()[0];
                    int X = playerStart.PlayerX;
                    int Y = playerStart.PlayerY;
                    id = spriteDefinition.TextureID;
                    var textureDefinition = textureDefinitions.Where(t => t.TextureID == id).ToList()[0];
                    Bitmap img = (Bitmap)Bitmap.FromFile(textureDefinition.FileName);
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            if (playerStart.Heading == StackAttack.Engine.Headings.North)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + x, spriteDefinition.Y + y);
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (playerStart.Heading == StackAttack.Engine.Headings.South)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + x, spriteDefinition.Y + (3 - y));
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (playerStart.Heading == StackAttack.Engine.Headings.West)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + y, spriteDefinition.Y + x);
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (playerStart.Heading == StackAttack.Engine.Headings.East)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + y, spriteDefinition.Y + (3 - x));
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                        }
                    }
                }
            }
            PreviewImage.Image = previewBitmap;
            editorBitmap = new Bitmap(width * 16, height * 16);
            using (Graphics g = Graphics.FromImage(editorBitmap))
            {
                g.Clear(System.Drawing.Color.Transparent);
                g.DrawImage(previewBitmap, new System.Drawing.Point(0, 0));
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        g.DrawRectangle(Pens.Black, new System.Drawing.Rectangle(x * 16, y * 16, 16, 16));
                    }
                }
                switch (LayerComboBox.SelectedIndex)
                {
                    case 0:
                        foreach (TileData tile in background.Tiles)
                        {
                            string id = tile.TileID;
                            int X = tile.TileX;
                            int Y = tile.TileY;
                            var tileDefinition = tileDefinitions.Where(t => t.TileID == id).ToList()[0];
                            id = tileDefinition.TextureID;
                            var textureDefinition = textureDefinitions.Where(t => t.TextureID == id).ToList()[0];
                            Bitmap img = (Bitmap)Bitmap.FromFile(textureDefinition.FileName);
                            for (int y = 0; y < 4; y++)
                            {
                                for (int x = 0; x < 4; x++)
                                {
                                    System.Drawing.Color c = img.GetPixel(tileDefinition.X + x, tileDefinition.Y + y);
                                    g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 16 + x * 4, Y * 16 + y * 4, 4, 4));
                                }
                            }
                        }
                        break;
                    case 1:
                        foreach (TileData tile in foreground.Tiles)
                        {
                            string id = tile.TileID;
                            int X = tile.TileX;
                            int Y = tile.TileY;
                            var tileDefinition = tileDefinitions.Where(t => t.TileID == id).ToList()[0];
                            id = tileDefinition.TextureID;
                            var textureDefinition = textureDefinitions.Where(t => t.TextureID == id).ToList()[0];
                            Bitmap img = (Bitmap)Bitmap.FromFile(textureDefinition.FileName);
                            for (int y = 0; y < 4; y++)
                            {
                                for (int x = 0; x < 4; x++)
                                {
                                    System.Drawing.Color c = img.GetPixel(tileDefinition.X + x, tileDefinition.Y + y);
                                    g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 16 + x * 4, Y * 16 + y * 4, 4, 4));
                                }
                            }
                        }
                        break;
                    case 2:
                        foreach (GameObjectStartData gameObject in gameObjectStartDatas)
                        {
                            string id = gameObject.SpriteID;
                            var spriteDefinition = spriteDefinitions.Where(t => t.SpriteID == id).ToList()[0];
                            int X = gameObject.ObjectX;
                            int Y = gameObject.ObjectY;
                            id = spriteDefinition.TextureID;
                            var textureDefinition = textureDefinitions.Where(t => t.TextureID == id).ToList()[0];
                            Bitmap img = (Bitmap)Bitmap.FromFile(textureDefinition.FileName);
                            for (int y = 0; y < 4; y++)
                            {
                                for (int x = 0; x < 4; x++)
                                {
                                    if (gameObject.Heading == StackAttack.Engine.Headings.North)
                                    {
                                        System.Drawing.Color c = img.GetPixel(spriteDefinition.X + x, spriteDefinition.Y + y);
                                        g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                                    }
                                    else if (gameObject.Heading == StackAttack.Engine.Headings.South)
                                    {
                                        System.Drawing.Color c = img.GetPixel(spriteDefinition.X + x, spriteDefinition.Y + (3 - y));
                                        g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                                    }
                                    else if (gameObject.Heading == StackAttack.Engine.Headings.West)
                                    {
                                        System.Drawing.Color c = img.GetPixel(spriteDefinition.X + y, spriteDefinition.Y + x);
                                        g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                                    }
                                    else if (gameObject.Heading == StackAttack.Engine.Headings.East)
                                    {
                                        System.Drawing.Color c = img.GetPixel(spriteDefinition.X + y, spriteDefinition.Y + (3-x));
                                        g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                                    }
                                }
                            }
                        }
                        break;
                }
                {
                    string id = "Player";
                    var spriteDefinition = spriteDefinitions.Where(t => t.SpriteID == id).ToList()[0];
                    int X = playerStart.PlayerX;
                    int Y = playerStart.PlayerY;
                    id = spriteDefinition.TextureID;
                    var textureDefinition = textureDefinitions.Where(t => t.TextureID == id).ToList()[0];
                    Bitmap img = (Bitmap)Bitmap.FromFile(textureDefinition.FileName);
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            if (playerStart.Heading == StackAttack.Engine.Headings.North)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + x, spriteDefinition.Y + y);
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (playerStart.Heading == StackAttack.Engine.Headings.South)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + x, spriteDefinition.Y + (3 - y));
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (playerStart.Heading == StackAttack.Engine.Headings.West)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + y, spriteDefinition.Y + x);
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                            else if (playerStart.Heading == StackAttack.Engine.Headings.East)
                            {
                                System.Drawing.Color c = img.GetPixel(spriteDefinition.X + y, spriteDefinition.Y + (3 - x));
                                g.FillRectangle(new System.Drawing.SolidBrush(c), new System.Drawing.Rectangle(X * 4 + x * 4, Y * 4 + y * 4, 4, 4));
                            }
                        }
                    }
                }
            }
            Editor.Image = editorBitmap;
        }

        public MainWindow()
        {
            StackAttack.Game.LoadDefinitionData("textureDefinitions.json", ref textureDefinitions);
            StackAttack.Game.LoadDefinitionData("tileDefinitions.json", ref tileDefinitions);
            StackAttack.Game.LoadDefinitionData("spriteDefinitions.json", ref spriteDefinitions);
            InitializeComponent();
            UpdateBitmaps();
        }

        private void CheckSaneNumber(ref object sender)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox is null)
                return;

            int number = int.Parse(textBox.Text);
            if (number < _saneMinimum) number = _saneMinimum;
            else if (number > _saneMaximum) number = _saneMaximum;
            textBox.Text = number.ToString();
            EditorHost.Width = int.Parse(WidthTextBox.Text) * 16;
            EditorHost.Height = int.Parse(HeightTextBox.Text) * 16;
            PreviewImageHost.Width = int.Parse(WidthTextBox.Text) * 16;
            PreviewImageHost.Height = int.Parse(HeightTextBox.Text) * 16;
            UpdateBitmaps();
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

        bool editor = false;
        bool editorDown = false;
        int lastX = -1;
        int lastY = -1;

        private void Editor_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (editor)
            {
                editorDown = true;
                Editor_MouseMove(sender, e);
            }
        }

        private void Editor_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (editor && editorDown)
            {
                int X = (e.X / 16);
                int Y = (e.Y / 16);
                if (X == lastX && Y == lastY)
                    return;
                lastX = X;
                lastY = Y;
                if (placeplayer)
                {
                    StackAttack.Engine.Headings headings = StackAttack.Engine.Headings.North;
                    switch (HeadingComboBox.SelectedIndex)
                    {
                        case 0: headings = StackAttack.Engine.Headings.North; break;
                        case 1: headings = StackAttack.Engine.Headings.South; break;
                        case 2: headings = StackAttack.Engine.Headings.East; break;
                        case 3: headings = StackAttack.Engine.Headings.West; break;
                    }
                    playerStart = new PlayerStartData(X * 4, Y * 4, headings);
                    placeplayer = false;
                    return;
                }
                if (LayerComboBox.SelectedIndex == 0)
                {
                    TileData data = new(((ComboBoxItem)TileComboBox.SelectedItem).Content.ToString(), X, Y, 0);
                    if (background.Tiles.Select(t => (t.TileX == X && t.TileY == Y)).Count() > 0)
                    {
                        for (int i = background.Tiles.Count - 1; i >= 0; i--)
                        {
                            if (background.Tiles[i].TileX == X && background.Tiles[i].TileY == Y)
                            {
                                background.Tiles.RemoveAt(i);
                            }
                        }
                    }
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        background.Tiles.Add(data);
                    }
                }
                else if (LayerComboBox.SelectedIndex == 1)
                {
                    TileData data = new(((ComboBoxItem)TileComboBox.SelectedItem).Content.ToString(), X, Y, 0);
                    if (foreground.Tiles.Select(t => (t.TileX == X && t.TileY == Y)).Count() > 0)
                    {
                        for (int i = foreground.Tiles.Count - 1; i >= 0; i--)
                        {
                            if (foreground.Tiles[i].TileX == X && foreground.Tiles[i].TileY == Y)
                            {
                                foreground.Tiles.RemoveAt(i);
                            }
                        }
                    }
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        foreground.Tiles.Add(data);
                    }
                }
                else if (LayerComboBox.SelectedIndex == 2)
                {
                    StackAttack.Engine.Headings headings = StackAttack.Engine.Headings.North;
                    switch (HeadingComboBox.SelectedIndex)
                    {
                        case 0: headings = StackAttack.Engine.Headings.North; break;
                        case 1: headings = StackAttack.Engine.Headings.South; break;
                        case 2: headings = StackAttack.Engine.Headings.East; break;
                        case 3: headings = StackAttack.Engine.Headings.West; break;
                    }
                    GameObjectStartData startData = new GameObjectStartData(((ComboBoxItem)ObjectComboBox.SelectedItem).Content.ToString(), X * 4, Y * 4, headings, ((ComboBoxItem)ObjectComboBox.SelectedItem).Content.ToString());
                    if (gameObjectStartDatas.Select(t => (t.ObjectX == X *4 && t.ObjectY == Y * 4)).Count() > 0)
                    {
                        for (int i = gameObjectStartDatas.Count - 1; i >= 0; i--)
                        {
                            if (gameObjectStartDatas[i].ObjectX == X * 4&& gameObjectStartDatas[i].ObjectY == Y *4)
                            {
                                gameObjectStartDatas.RemoveAt(i);
                            }
                        }
                    }
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        gameObjectStartDatas.Add(startData);
                    }
                }
                stimer++;
                if (stimer == 10)
                {
                    stimer = 0;
                    UpdateBitmaps();
                }
            }
        }

        private void Editor_MouseEnter(object sender, EventArgs e)
        {
            editor = true;
        }

        private void Editor_MouseLeave(object sender, EventArgs e)
        {
            editor = false;
            editorDown = false;
            UpdateBitmaps();
        }

        private void Editor_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            editorDown = false;
            UpdateBitmaps();
        }

        private void LayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBitmaps();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Json Files (*.json)|*.json";
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
                EditorHost.Width = int.Parse(WidthTextBox.Text) * 16;
                EditorHost.Height = int.Parse(HeightTextBox.Text) * 16;
                PreviewImageHost.Width = int.Parse(WidthTextBox.Text) * 16;
                PreviewImageHost.Height = int.Parse(HeightTextBox.Text) * 16;
                UpdateBitmaps();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Json Files (*.json)|*.json";
            if (dialog.ShowDialog() == true)
            {
                LevelData level = new();
                level.LevelWidth = int.Parse(WidthTextBox.Text);
                level.LevelHeight = int.Parse(HeightTextBox.Text);
                level.Foreground = foreground;
                level.Background = background;
                level.GameObjectStartDatas = gameObjectStartDatas;
                level.PlayerStartData = playerStart;
                StackAttack.Game.SaveDefintionData(dialog.FileName, level);
            }
        }

        private void PlacePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            placeplayer = true;
        }
    }
}
