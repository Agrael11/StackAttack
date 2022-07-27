using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StackAttack.Engine.Map
{
    public struct TileData
    {
        public TileData()
        {
        }

        public TileData(string tileID, int tileX, int tileY, float tileRotationDeg)
        {
            TileID = tileID;
            TileX = tileX;
            TileY = tileY;
            TileRotationDeg = tileRotationDeg;
        }

        public string TileID { get; set; } = "";
        public int TileX { get; set; } = 0;
        public int TileY { get; set; } = 0;
        public float TileRotationDeg { get; set; } = 0;

        public float GetTileRotationRad()
        { 
            return (float)MathHelper.DegreesToRadians(TileRotationDeg);
        }
        public void SetTileRotationRad(float value)
        { 
            TileRotationDeg = (float)MathHelper.RadiansToDegrees(value);
        }
        public override bool Equals(object? obj)
        {
            return obj is TileData data &&
                   TileID == data.TileID &&
                   TileX == data.TileX &&
                   TileY == data.TileY;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TileID, TileX, TileY, TileRotationDeg);
        }

        public TileData Clone()
        {
            return new TileData(TileID, TileX, TileY, TileRotationDeg);
        }
    }
}
