//-----------------------------------------------------------------------
// <copyright file="Line.cs" author="Jakub Zíka" student="A15N0087P">
//
// License is according West Bohemmia licenses.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Script gives behaviour to LineRenderer in 3D scene.
/// </summary>
public class Line : MonoBehaviour
{
    /// <summary>
    /// Prepare LineRenderer.
    /// </summary>
    private new LineRenderer renderer;
    /// <summary>
    /// Material of original line.
    /// </summary>
    private Material originLine;
    /// <summary>
    /// Material of navigation line.
    /// </summary>
    private Material navigateLine;
    /// <summary>
    /// Material for temporary line.
    /// </summary>
    private Material fosforGreenLine;

    /// <summary>
    /// Use method if is object awake. Prepare materials.
    /// </summary>
    public void Awake()
    {
        originLine = (Material)Resources.Load("checker_texture", typeof(Material));
        navigateLine = (Material)Resources.Load("line", typeof(Material));
        fosforGreenLine = (Material)Resources.Load("axis_green", typeof(Material));
    }

    /// <summary>
    /// Settings of line visualisation.
    /// </summary>
    /// <param name="originalP">Start point.</param>
    /// <param name="nextP">End point.</param>
    /// <param name="widthOfLine">Width of line.</param>
    /// <param name="material">Material of line.</param>
    public void lineSetup(Vector3 originalP, Vector3 nextP, float widthOfLine, Material material)
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
        renderer.material = material;
    }

    /// <summary>
    /// Change properties of line in runnig app.
    /// </summary>
    /// <param name="widthOfLine">New width of line.</param>
    /// <param name="material">New material of line.</param>
    public void resetLineSetup(float widthOfLine, Material material)
    {
        renderer.startWidth = widthOfLine;
        renderer.endWidth = widthOfLine;
        renderer.material = material;
    }

    /// <summary>
    /// Return number of vertices.
    /// </summary>
    /// <returns>NCount of vertices.</returns>
    public int getNumPositions()
    {
        return renderer.numPositions;
    }

    /// <summary>
    /// Save LineRenderer positions to input parameter.
    /// </summary>
    /// <param name="positions">Storage for positions.</param>
    public void getPositions(Vector3 [] positions)
    {
        renderer.GetPositions(positions);
    }
}