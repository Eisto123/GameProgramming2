using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoronatorSharp;
using System.Linq;
using System;

using AlanZucconi.AI.PF;

namespace AlanZucconi.Voronoi
{
    public class Voronoi : Voronator,
        IPathfindingCost<Vector2>
    {
        public List<Cell> Cells;

        public Voronoi(IList<Vector2> points) : base(points)
        {
            Recalculate(points);
        }

        public Voronoi(IList<Vector2> points, Vector2 clipMin, Vector2 clipMax) : base(points, clipMin, clipMax)
        {
            Recalculate(points);
        }

        public void Recalculate(IList<Vector2> points)
        {
            Cells = new List<Cell>();

            // ------------------------------------------------
            List<Vector2> centroids = GetRelaxedPoints();

            // Calculates the cells
            for (int i = 0; i < points.Count; i++)
            {
                List<Vector2> vertices = GetClippedPolygon(i);
                if (vertices == null)
                    continue;

                

                Cell cell = new Cell();
                cell.Index = i;
                cell.Centre = points[i];
                //cell.Centroid = centroids[i]; // These are centroids for the unclipped polygons
                cell.Centroid = GetCentroid(vertices); // Cenrtoid for the clipped polygons
                cell.Vertices = vertices;
                cell.Status = GetPolygonStatus(i);

                // No cell is null, but some cells might be missing
                Cells.Add(cell);
            }

            // ------------------------------------------------
            // Calculate neighbours
            foreach (Cell cell in Cells)
            {
                //if (cell == null)
                //    continue;

                cell.Neighbours =
                    ClippedNeighbors(cell.Index)
                    .Select(index => GetCell(index))
                    .ToList();
            }
        }
        
        // Get the cell with index "index"
        public Cell GetCell(int index)
        {
            int i = Cells.BinarySearch
            (
                new Cell { Index = index },
                new CellComparer()
                //(x, y) => x.Index.CompareTo(y.Index)
            );
            return i >= 0 ? Cells[i] : null;
        }
        private class CellComparer : IComparer<Cell>
        {
            public int Compare(Cell x, Cell y)
            {
                return x.Index.CompareTo(y.Index);
            }
        }




        // Returns a list of cells that are sharing this vertex
        // If this is not an actual vertex, it returns nothing
        public IEnumerable<Cell> NeighbouringCell(Vector2 vertex)
        {
            return Cells
                .Where(cell => cell.Vertices.Contains(vertex));
        }

        // Given a vertex, returns the connected vertices
        // alongside the edges of the cells.
        // The list of vertices is unique.
        public IEnumerable<Vector2> NeighbouringVertex(Vector2 vertex)
        {
            HashSet<Vector2> uniqueVertices = new HashSet<Vector2>();

            foreach (var cell in Cells)
            {
                if (cell.Vertices.Contains(vertex))
                {
                    int index = cell.Vertices.IndexOf(vertex);
                    int previousIndex = (index - 1 + cell.Vertices.Count) % cell.Vertices.Count;
                    int nextIndex = (index + 1) % cell.Vertices.Count;

                    uniqueVertices.Add(cell.Vertices[previousIndex]);
                    uniqueVertices.Add(cell.Vertices[nextIndex]);
                }
            }

            return uniqueVertices;
        }

        
        // Returns the closest cell to a given point
        // The distance is calculate from the centre (not the centroid) of the cell
        public Cell ClosestCell (Vector2 point)
        {
            return Cells
                .MinBy(cell => Vector2.Distance(cell.Centre, point));
        }

        // Returns the closest vertex
        public Vector2 ClosestVertex (Vector2 point)
        {
            return Cells
                .SelectMany(cell => cell.Vertices)
                .MinBy(vertex => Vector2.Distance(vertex, point));
        }

        #region Pathfindin
        // Pathfinding
        /*
        public IEnumerable<Vector2> Outgoing(Vector2 vertex)
        {
            return NeighbouringVertex(vertex);
        }
        */
        IEnumerable<(Vector2, Edge)> IPathfindingCost<Vector2, Edge>.Outgoing(Vector2 vertex)
        {
            return
                NeighbouringVertex(vertex)
                .Select(v => (v, (Edge) Vector2.Distance(v, vertex)));
        }

        // Pathfinding between two vertices
        // startVerex and endVertex HAVE to be vertices.
        // The returned path includes the start and goal nodes:
        //  Pathfinding(a, -) = null         // unreachable goal
        //  Pathfinding(-, a) = null         // unreachable start
        //  Pathfinding(a, a) = [a]          // already on the node
        //  Pathfinding(a, c) = [a, b, c]
        public IEnumerable<Vector2> VertexPathfinding (Vector2 startVertex, Vector2 endVertex)
        {
            return this
                .Dijkstra(startVertex, endVertex)
                .Select(x => x.Item1);
        }

        // This is as VertexPathfinding, but start and end do not need to be vertices
        public IEnumerable<Vector2> Pathfinding(Vector2 start, Vector2 end)
        {
            return this
                .Dijkstra(ClosestVertex(start), ClosestVertex(end))
                .Select(x => x.Item1)
                .Prepend(start)
                .Append(end);
        }

        #endregion
    }

    public class Cell
    {
        public int Index; // The index of this cell inside the Voronator class
        public Vector2 Centre;
        public Vector2 Centroid;
        public List<Vector2> Vertices;
        public Voronoi.PolygonStatus Status;

        public List<Cell> Neighbours;


        // Loops trough all vertices of the cell
        public void DebugDraw(Color color)
        {
            for (int vi = 0; vi < Vertices.Count; vi++)
            {
                Debug.DrawLine
                (
                    Vertices[vi],
                    Vertices[(vi + 1) % Vertices.Count],
                    color
                    //XYtoXZ(Vertices[vi]),
                    //XYtoXZ(Vertices[(vi + 1) % Vertices.Count]),
                    //Status == Voronator.PolygonStatus.Normal
                    //? color
                    //: Color.white
                );
            }
        }

        /*
        private Vector3 XYtoXZ(Vector2 v2)
        {
            return new Vector3
            (
                v2.x,
                0,
                v2.y
            );
        }
        */
    }
}