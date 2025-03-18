using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.AI.PF
{
    // Used for Dijkstra and A*
    public interface IPathfindingCost<N, E>
        where E : IEdge
    {
        // List of connected nodes from "node", and the edge that let do them
        IEnumerable<(N, E)> Outgoing(N node);
    }

    // Used for the edges that connect two nodes
    // This is important for things such as Planning,
    // in which the list of traversed edges is the actual plan to execute
    public interface IEdge
    {
        float Cost { get; }
    }







    // Used for when the cost is simply a float
    public interface IPathfindingCost<N> : IPathfindingCost<N,Edge>
    {

    }
    // A connection between two nodes which has a cost as a float
    public class Edge : IEdge
    {
        public Edge(float cost) => Cost = cost;

        public float Cost { get; }

        public static implicit operator float (Edge edge) => edge.Cost;
        public static implicit operator Edge (float cost) => new Edge(cost);
    }







    // Calculates the heuristic between two nodes
    // This is used for algorithms like A*
    public interface IHeuristic<N>
    {
        float Heuristic(N a, N b);
    }

    
}