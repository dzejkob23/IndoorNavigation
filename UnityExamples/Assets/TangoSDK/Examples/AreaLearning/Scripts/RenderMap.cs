using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class RenderMap
{
    private List<LineRenderer> renders;

    public RenderMap()
    {
        renders = new List<LineRenderer>();
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

        renders.Add(render);
    }

    class RenderStruct
    {
           
    }
}
