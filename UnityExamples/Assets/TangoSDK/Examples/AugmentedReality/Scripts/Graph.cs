using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Graph
{

    private double[,] graph2D;

    private double DistancePoint2Point(Vector3 p1, Vector3 p2)
    {
        return Math.Sqrt(Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2) + Math.Pow(p2.z - p1.z, 2));
    }

    public IEnumerator CreateEvaluatedGraph(Dictionary<int, GameObject> m_markerDictionary)
    {
        int maxId = m_markerDictionary.Keys.Max() + 1;
        graph2D = new double[maxId, maxId];

        foreach (KeyValuePair<int, GameObject> marker in m_markerDictionary)
        {
            int markerId = marker.Key;
            Dictionary<int, GameObject> renderers = marker.Value.GetComponent<ARMarker>().getRendersDictionary();

            foreach (KeyValuePair<int, GameObject> rendererObj in renderers)
            {
                // get points of line from renderer (it contains marker and neighbour poisition)
                int neighbourId = rendererObj.Key;
                Line renderer = rendererObj.Value.GetComponent<Line>();
                Vector3[] neighbourPositions = new Vector3 [renderer.getNumPositions()];
                renderer.getPositions(neighbourPositions);

                // fill marker and neighbour position
                if (neighbourPositions.Length < 2)
                {
                    AndroidHelper.ShowAndroidToastMessage("Positions of marker " + markerId + " are EMPTY!");
                    yield return null;
                }

                Vector3 markerPosition = neighbourPositions[0];
                Vector3 neighbourPosition = neighbourPositions[1];

                // compute distance between both markers
                double distance = DistancePoint2Point(markerPosition, neighbourPosition);

                AndroidHelper.ShowAndroidToastMessage("... " + neighbourId + " ...");

                // filled for markers in both directions
                graph2D[markerId, neighbourId] = distance;
                graph2D[neighbourId, markerId] = distance;
            }
        }

        AndroidHelper.ShowAndroidToastMessage("Evaluated graph was created!");

        yield return null;
    }
}
