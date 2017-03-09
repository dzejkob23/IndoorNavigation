using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class RenderMap
{
    private List<ARLine> renders;

    public RenderMap()
    {
        renders = new List<ARLine>();
    }

    public void renderLine(ARMarker m1, ARMarker m2)
    {
        LineRenderer render = new LineRenderer();

        render.sortingLayerName = "onTop";
        render.sortingOrder = 5;
        render.startWidth = 0.5f;
        render.endWidth = 0.5f;
        render.useWorldSpace = true;

        render.numPositions = 2;
        render.SetPosition(0, m1.transform.position);
        render.SetPosition(0, m2.transform.position);

        ARLine line = new ARLine();
        line.setupLine(m1.getID(), m2.getID(), render);
        renders.Add(line);
    }

    public bool deleteRendererViaMarker(int markerId)
    {
        int capacity = renders.Capacity;

        for (int i = 0; i < capacity; i++)
        {
            if (renders[i].containsId(markerId))
            {
                Destroy(renders[i].getRenderer().gameObject);
                return true;
            }
        }

        return false;
    }
}
