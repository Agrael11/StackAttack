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

        public Helpers.Rectanglei Rectangle 
        {
            get 
            {
                Vector2i Size = new Vector2i(4, 4);
                (bool returnResult, Tile? returnTile) = ContentManager.Get<Tile>(TileID);
                if (returnResult && returnTile is not null)
                {
                    Size = returnTile.Size;
                }
                return new Helpers.Rectanglei(TileX * 4, TileY * 4, Size);
            }
        }
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
