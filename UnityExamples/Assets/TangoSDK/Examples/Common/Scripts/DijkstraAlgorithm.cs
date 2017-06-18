//-----------------------------------------------------------------------
// <copyright file="DijkstraAlgorithm.cs">
//
// Download from: https://sourceforge.net/projects/dijkstra-csharp/
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DijkstraAlgorithm
{
    /* Takes adjacency matrix in the following format, for a directed graph (2-D array)
     * Ex. node 1 to 3 is accessible at a cost of 4
     *        0  1  2  3  4 
     *   0  { 0, 2, 5, 0, 0},
     *   1  { 0, 0, 0, 4, 0},
     *   2  { 0, 6, 0, 0, 8},
     *   3  { 0, 0, 0, 0, 9},
     *   4  { 0, 0, 0, 0, 0}
     */

    /* Resulting arrays with distances to nodes and how to get there */
    public double[] dist { get; private set; }
    public int[] path { get; private set; }
    public int[] sPath { get; private set; }

    /* Holds queue for the nodes to be evaluated */
    private List<int> queue = new List<int>();

    /* Sets up initial settings */
    private void Initialize(int s, int len)
    {
        dist = new double[len];
        path = new int[len];

        /* Set distance to all nodes to infinity - alternatively use Int.MaxValue for use of Int type instead */
        for (int i = 0; i < len; i++)
        {
            dist[i] = Double.PositiveInfinity;

            queue.Add(i);
        }

        /* Set distance to 0 for starting point and the previous node to null (-1) */
        dist[s] = 0;
        path[s] = -1;
    }

    /* Retrives next node to evaluate from the queue */
    private int GetNextVertex()
    {
        double min = Double.PositiveInfinity;
        int Vertex = -1;

        /* Search through queue to find the next node having the smallest distance */
        foreach (int j in queue)
        {
            if (dist[j] <= min)
            {
                min = dist[j];
                Vertex = j;
            }
        }

        queue.Remove(Vertex);

        return Vertex;

    }

    /* Takes a graph as input an adjacency matrix (see top for details) and a starting node */
    public DijkstraAlgorithm(double[,] G, int s, int t)
    {
        /* Check graph format and that the graph actually contains something */
        if (G.GetLength(0) < 1 || G.GetLength(0) != G.GetLength(1))
        {
            throw new ArgumentException("Graph error, wrong format or no nodes to compute");
        }

        int len = G.GetLength(0);

        Initialize(s, len);

        while (queue.Count > 0)
        {
            int u = GetNextVertex();
            
            // There I can stop code if "u" equals "target point".
            if (u == t)
            {
                break;
            }

            /* Find the nodes that u connects to and perform relax */
            for (int v = 0; v < len; v++)
            {
                /* Checks for edges with negative weight */
                if (G[u, v] < 0)
                {
                    throw new ArgumentException("Graph contains negative edge(s)");
                }

                /* Check for an edge between u and v */
                if (G[u, v] > 0)
                {
                    /* Edge exists, relax the edge */
                    if (dist[v] > dist[u] + G[u, v])
                    {
                        dist[v] = dist[u] + G[u, v];
                        path[v] = u;
                    }
                }
            }
        }

        sPath = shortedPath(s, t);
    }

    private int[] shortedPath(int start, int target)
    {
        Debug.Log("#DIJKSTRA - start:" + start + " target:" + target);

        List<int> shortedPathList = new List<int>();
        int current = target;

        if (path == null)
        {
            return null;
        }

        // Path does not exist
        if (path[target] == 0)
        {
            return null;
        }

        while (current != start)
        {
            shortedPathList.Add(current);
            current = path[current];
        }

        shortedPathList.Add(current);
        return shortedPathList.ToArray();
    }
}