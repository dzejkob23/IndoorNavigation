using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Line : MonoBehaviour
{
    private new LineRenderer renderer;

    public void lineSetup(Vector3 originalP, Vector3 nextP, float widthOfLine)
    {
        renderer = gameObject.AddComponent<LineRenderer>();

        renderer.sortingLayerName = "OnTop";
        renderer.sortingOrder = 5;

        // transform
        renderer.numPositions = 2;
        renderer.SetPosition(0, originalP);
        renderer.SetPosition(1, nextP);
        renderer.startWidth = widthOfLine;
        renderer.endWidth = widthOfLine;
        renderer.useWorldSpace = true;
        renderer.material = (Material) Resources.Load("checker_texture", typeof (Material));
    }

    public int getNumPositions()
    {
        return renderer.numPositions;
    }

    public void getPositions(Vector3 [] positions)
    {
        renderer.GetPositions(positions);
    }

    public void originSetupLine(LineRenderer renderer)
    {

    }

    public void navigateSetupLine()
    {

    }
}