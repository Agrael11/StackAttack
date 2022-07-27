using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine.Map
{
    public struct TileMap
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public List<TileData> Tiles { get; set; } = new();

        public TileMap()
        {
        }

        public TileMap Clone()
        {
            TileMap tiles = new TileMap();
            tiles.X = X;
            tiles.Y = Y;
            foreach (TileData tileData in Tiles)
            {
                tiles.Tiles.Add(tileData.Clone());
            }
            return tiles;
        }
    }
}
