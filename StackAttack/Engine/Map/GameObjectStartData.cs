﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine.Map
{
    public struct GameObjectStartData
    {
        public GameObjectStartData()
        {

        }

        public GameObjectStartData(string gameObjectTypeID, int objectX, int objectY, Headings heading)
        {
            GameObjectTypeID = gameObjectTypeID;
            ObjectX = objectX;
            ObjectY = objectY;
            Heading = heading;
        }

        public string GameObjectTypeID { get; set; } = "";
        public int ObjectX { get; set; } = 0;
        public int ObjectY { get; set; } = 0;
        public Headings Heading { get; set; } = Headings.North;
    }
}
