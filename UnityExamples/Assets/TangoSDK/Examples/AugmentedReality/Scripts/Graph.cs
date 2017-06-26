//-----------------------------------------------------------------------
// <copyright file="Graph.cs" author="Jakub Zíka" student="A15N0087P">
//
// License is according University of West Bohemmia licenses.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Script represents instance of navigation graph.
/// </summary>
public class Graph
{
    /// <summary>
    /// Matrix of neighbours.
    /// </summary>
    private double[,] graph2D;
    /// <summary>
    /// Markers describe by IDs and positions.
    /// </summary>
    private Dictionary<int, Vector3> markersPositions;

    /// <summary>
    /// Create evaluated graph by distances.
    /// </summary>
    /// <param name="m_markerDictionary">Dictionary with markers.</param>
    public void CreateEvaluatedGraph(Dictionary<int, GameObject> m_markerDictionary)
    {
        int maxId = m_markerDictionary.Keys.Max() + 1;
        graph2D = new double[maxId, maxId];
        markersPositions = new Dictionary<int, Vector3>();

        foreach (KeyValuePair<int, GameObject> marker in m_markerDictionary)
        {
            int markerId = marker.Key;
            Dictionary<int, GameObject> renderers = marker.Value.GetComponent<ARMarker>().GetRendersDictionary();
            markersPositions.Add(marker.Key, marker.Value.transform.position);

            foreach (KeyValuePair<int, GameObject> rendererObj in renderers)
            {
                // get points of line from renderer (it contains marker and neighbour poisition)
                int neighbourId = rendererObj.Key;
                Line renderer = rendererObj.Value.GetComponent<Line>();
                Vector3[] neighbourPositions = new Vector3 [renderer.GetNumPositions()];
                renderer.GetPositions(neighbourPositions);

                // fill marker and neighbour position
                if (neighbourPositions.Length < 2)
                {
                    AndroidHelper.ShowAndroidToastMessage("Positions of marker " + markerId + " are EMPTY!");
                    return;
                }

                Vector3 markerPosition = neighbourPositions[0];
                Vector3 neighbourPosition = neighbourPositions[1];

                // compute distance between both markers
                double distance = Vector3.Distance(markerPosition, neighbourPosition);

                // filled for markers in both directions
                graph2D[markerId, neighbourId] = distance;
                graph2D[neighbourId, markerId] = distance;
            }
        }

        AndroidHelper.ShowAndroidToastMessage("Evaluated graph was created!");
    }

    /// <summary>
    /// Return markers positions.
    /// </summary>
    /// <returns>Markers positions.</returns>
    public Dictionary<int, Vector3> GetMarkersPosition()
    {
        return markersPositions;
    }

    /// <summary>
    /// Return 2D graph in matrix of distances.
    /// </summary>
    /// <returns>Graph with distances.</returns>
    public double[,] Get2DGraph()
    {
        return graph2D;
    }

    /// <summary>
    /// Set 2D graph in matric of distances.
    /// </summary>
    /// <param name="graph2D">2D graph with distances.</param>
    public void Set2DGraph(double [,] graph2D)
    {
        this.graph2D = graph2D;
    }
}
