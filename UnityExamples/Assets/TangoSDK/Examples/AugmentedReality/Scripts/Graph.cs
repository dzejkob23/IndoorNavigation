using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Graph
{

    public double DistancePoint2Point(Vector3 p1, Vector3 p2)
    {
        return Math.Sqrt(Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2) + Math.Pow(p2.z - p1.z, 2));
    }

    private IEnumerator CreateEvaluatedGraph(List<GameObject> m_markerList)
    {
        // TODO
        int capacity = m_markerList.Capacity;
        int maxId = m_markerList[capacity - 1].GetComponent<ARMarker>().getID();

        for (int i = 0; i < capacity; i++)
        {

        }

        yield return null;
    }
}
