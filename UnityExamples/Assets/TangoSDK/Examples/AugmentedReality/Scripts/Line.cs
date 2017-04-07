using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Line : MonoBehaviour
{
    private new LineRenderer renderer;
    private Material originLine;
    private Material navigateLine;
    private Material fosforGreenLine;

    public void Awake()
    {
        originLine = (Material)Resources.Load("checker_texture", typeof(Material));
        navigateLine = (Material)Resources.Load("line", typeof(Material));
        fosforGreenLine = (Material)Resources.Load("axis_green", typeof(Material));
    }

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
        renderer.material = fosforGreenLine;
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