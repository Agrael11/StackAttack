using OpenTK.Mathematics;
using StackAttack.Engine.Helpers;
using StackAttack.Engine.Map;

namespace StackAttack.Engine
{
    internal static class RayCasting
    {

        public static (bool result, GameObject? resultObject, TileData? tile) CastRay(Vector2 sourcePosition, float angle, GameObject source, TileMap collisionMap, List<GameObject> gameObjects, bool draw, int CameraX, int CameraY, params Type[] TargetTypes)
        {
            Vector2 angleVector = ExtensionsAndHelpers.FromAngle(angle, 32);
            Vector2 destinationPosition = new Vector2(sourcePosition.X + angleVector.X, sourcePosition.Y + angleVector.Y);
            Rectangle srcR = new Rectangle(sourcePosition, angleVector.X, angleVector.Y);


            float closest = float.MaxValue;
            TileData? closestTile = null;

            foreach (TileData tile in collisionMap.Tiles)
            {
                Rectangle colR = tile.GetRectangle();
                var result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X, colR.Y, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X2, colR.X2, colR.Y, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y, colR.Y);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y2, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
            }

            GameObject? closestObject = null;

            if (TargetTypes is not null && (TargetTypes.Length > 0))
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    if (gameObject == source)
                        continue;

                    if (!TargetTypes.Contains(gameObject.GetType()))
                        continue;

                    Rectangle colR = new Rectangle(gameObject.Location, new Vector2(4, 4));
                    var result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X, colR.Y, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X2, colR.X2, colR.Y, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y, colR.Y);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y2, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                }
            }

            if (draw) Line.Draw(new(sourcePosition.X - CameraX, sourcePosition.Y - CameraY), new(destinationPosition.X - CameraX, destinationPosition.Y - CameraY), 1f);

            if (closestObject is null)
            {
                if (closestTile is null)
                {
                    return (false, null, null);
                }
                return (true, null, closestTile);
            }
            return (true, closestObject, null);
        }

        public static (bool result, GameObject? resultObject, TileData? tile) CastRay(Vector2 sourcePosition, Vector2 destinationPosition, GameObject source, TileMap collisionMap, List<GameObject> gameObjects, bool draw, int CameraX, int CameraY, params Type[] TargetTypes)
        {
            Rectangle srcR = new Rectangle(sourcePosition, destinationPosition.X - sourcePosition.X, destinationPosition.Y - sourcePosition.Y);


            float closest = float.MaxValue;
            TileData? closestTile = null;

            foreach (TileData tile in collisionMap.Tiles)
            {
                Rectangle colR = tile.GetRectangle();
                var result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X, colR.Y, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X2, colR.X2, colR.Y, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y, colR.Y);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y2, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
            }

            GameObject? closestObject = null;

            if (TargetTypes is not null && (TargetTypes.Length > 0))
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    if (gameObject == source)
                        continue;

                    if (!TargetTypes.Contains(gameObject.GetType()))
                        continue;

                    Rectangle colR = new Rectangle(gameObject.Location, new Vector2(4, 4));
                    var result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X, colR.Y, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X2, colR.X2, colR.Y, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y, colR.Y);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y2, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                }
            }

            if (draw) Line.Draw(new(sourcePosition.X - CameraX, sourcePosition.Y - CameraY), new(destinationPosition.X - CameraX, destinationPosition.Y - CameraY), 1f);

            if (closestObject is null)
            {
                if (closestTile is null)
                {
                    return (false, null, null);
                }
                return (true, null, closestTile);
            }
            return (true, closestObject, null);
        }

        public static (bool intersects, Vector2 point) LinesCollisions(float x1, float x2, float y1, float y2, float x3, float x4, float y3, float y4)
        {
            float D = ((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4));
            float t = (((x1 - x3) * (y3 - y4)) - ((y1 - y3) * (x3 - x4))) / D;
            float u = (((x1 - x3) * (y1 - y2)) - ((y1 - y3) * (x1 - x2))) / D;

            bool intersect = (t >= 0) && (t <= 1) && (u >= 0) && (u <= 1);
            if (!intersect)
                return (false, Vector2.Zero);

            float Px = x1 + (t * (x2 - x1));
            float Py = y1 + (t * (y2 - y1));

            return (true, new Vector2(Px, Py));
        }
    }
}