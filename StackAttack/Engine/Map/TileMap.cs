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
        public List<TileData> _tiles { get; set; } = new();

        public TileMap()
        {
        }
    }
}
