using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStarSolver
{
    private class Node
    {
        public int X, Y;
        public float G; // Cost from start to this node
        public float H; // Heuristic cost to goal
        public float F => G + H;
        public Node Parent;

        public Node(int x, int y, float g, float h, Node parent = null)
        {
            X = x;
            Y = y;
            G = g;
            H = h;
            Parent = parent;
        }

        public Vector2Int Position => new Vector2Int(X, Y);
    }

    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),    // Up
        new Vector2Int(1, 0),    // Right
        new Vector2Int(0, -1),   // Down
        new Vector2Int(-1, 0),   // Left
    };

    public static int GetShortestPathLength(int[,] binaryMaze)
    {
        int rows = binaryMaze.GetLength(0);
        int cols = binaryMaze.GetLength(1);

        Vector2Int start = new Vector2Int(0, cols-1);
        Vector2Int goal = new Vector2Int(rows - 1, 0);

        if (binaryMaze[start.x, start.y] == 0 || binaryMaze[goal.x, goal.y] == 0)
            return -1;

        var openList = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(start.x, start.y, 0, Heuristic(start, goal));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            openList.Sort((a, b) => a.F.CompareTo(b.F));
            Node current = openList[0];
            openList.RemoveAt(0);

            if (current.X == goal.x && current.Y == goal.y)
            {
                return ReconstructPathLength(current);
            }

            closedSet.Add(current.Position);

            foreach (var dir in Directions)
            {
                int newX = current.X + dir.x;
                int newY = current.Y + dir.y;

                if (!IsInBounds(newX, newY, rows, cols) || binaryMaze[newX, newY] == 0)
                    continue;

                Vector2Int neighborPos = new Vector2Int(newX, newY);
                if (closedSet.Contains(neighborPos))
                    continue;

                float tentativeG = current.G + Vector2Int.Distance(current.Position, neighborPos);

                Node neighbor = new Node(newX, newY, tentativeG, Heuristic(neighborPos, goal), current);

                Node existingOpen = openList.Find(n => n.X == newX && n.Y == newY);
                if (existingOpen != null && existingOpen.F <= neighbor.F)
                    continue;

                openList.Add(neighbor);
            }
        }

        return -1; // No path found
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Using Diagonal Distance
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dy);
    }

    private static int ReconstructPathLength(Node node)
    {
        int length = 0;
        while (node.Parent != null)
        {
            length++;
            node = node.Parent;
        }
        return length;
    }

    private static bool IsInBounds(int x, int y, int rows, int cols)
    {
        return x >= 0 && y >= 0 && x < rows && y < cols;
    }
}
