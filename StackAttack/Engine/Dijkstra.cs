using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine
{
    internal static class Dijkstra
    {
        public class Node
        {
            public int X = 0;
            public int Y = 0;
            public int Distance = int.MaxValue;
            public Node? Previous = null;

            public Node(int x, int y, int distance = int.MaxValue, Node? previous = null)
            {
                X = x;
                Y = y;
                Distance = distance;
                Previous = previous;
            }

            public override bool Equals(object? obj)
            {
                return obj is Node node &&
                       X == node.X &&
                       Y == node.Y;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y);
            }
        }

        public static List<Node>? DoDijkstra(Vector2i start, Vector2i end, Game data)
        {
            List<Node> Unvisited = new();
            List<Node> Visited = new();

            Node? currentNode = new Node(start.X, start.Y, 0);
            Unvisited.Add(currentNode);
            while (!(currentNode.X == end.X && currentNode.Y == end.Y))
            {
                if (Unvisited.Count == 0) return null;
                Unvisited = Unvisited.OrderBy(t => t.Distance).ToList();
                currentNode = Unvisited[0];
                Unvisited.RemoveAt(0);
                Visited.Add(currentNode);
                AddIfSafe(new Node(currentNode.X - 1, currentNode.Y, currentNode.Distance + 1, currentNode), ref Visited, ref Unvisited, data);
                AddIfSafe(new Node(currentNode.X + 1, currentNode.Y, currentNode.Distance + 1, currentNode), ref Visited, ref Unvisited, data);
                AddIfSafe(new Node(currentNode.X, currentNode.Y - 1, currentNode.Distance + 1, currentNode), ref Visited, ref Unvisited, data);
                AddIfSafe(new Node(currentNode.X, currentNode.Y + 1, currentNode.Distance + 1, currentNode), ref Visited, ref Unvisited, data);
            }
            List<Node> nodes = new();
            while (currentNode != null)
            {
                nodes.Add(currentNode);
                currentNode = currentNode.Previous;
            }
            Unvisited.Clear();
            Visited.Clear();
            nodes.Reverse();
            return nodes;
        }

        public static void AddIfSafe(Node node, ref List<Node> nodes1, ref List<Node> nodes2, Game data)
        {
            foreach (var oldNode in nodes1)
            {
                if (oldNode.X == node.X && oldNode.Y == node.Y)
                    return;
            }
            if (node.X < 0 || node.X >= data.level.LevelWidth)
                return;
            if (node.Y < 0 || node.Y >= data.level.LevelHeight)
                return;
            foreach (var tile in data.Foreground.Tiles)
            {
                if (tile.TileID == "BlueDoor")
                    continue;
                if (tile.TileX == node.X && tile.TileY == node.Y)
                    return;
            }
            foreach (var oldNode in nodes2)
            {
                if (oldNode.X == node.X && oldNode.Y == node.Y)
                {
                    if (oldNode.Distance > node.Distance)
                    {
                        oldNode.Distance = node.Distance;
                        oldNode.Previous = node.Previous;
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            nodes2.Add(node);
        }
    }
}
