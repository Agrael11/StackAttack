using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine.Map
{
    public struct PlayerStartData
    {
        public PlayerStartData()
        {
        }

        public PlayerStartData(int playerX, int playerY, Headings heading)
        {
            PlayerX = playerX;
            PlayerY = playerY;
            Heading = heading;
        }

        public string GameObjectTypeID { get; } = "Player";
        public int PlayerX { get; set; } = 0;
        public int PlayerY { get; set; } = 0;
        public Headings Heading { get; set; } = Headings.North;
        public string SpriteID { get; set; } = "";
    }
}
