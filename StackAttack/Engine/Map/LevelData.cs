using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine.Map
{
    public class LevelData
    {
        public int LevelWidth { get; set; }
        public int LevelHeight { get; set; }
        public TileMap Background { get; set; }
        public TileMap Foreground { get; set; }
        public PlayerStartData PlayerStartData { get; set; }
        public List<GameObjectStartData> GameObjectStartDatas { get; set; } = new();
        public string NextLevel { get; set; } = "";
    }
}
