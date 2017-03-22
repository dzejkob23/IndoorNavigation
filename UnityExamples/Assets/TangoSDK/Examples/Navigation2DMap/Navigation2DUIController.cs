using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Navigation2DUIController : MonoBehaviour
{
    private const int BUTTON_SIZE_WIDTH = 100;
    private const int BUTTON_SIZE_HEIGHT = 100;

    private double width = Screen.width;
    private double height = Screen.height;

    private Dictionary<int, Vector2> newMarkersPosition;
    private double[,] graph2D;
    private List<GameObject> lineRenderers;

    private const int SCALE_XY = 200;
    private const int BOUD_SCALED_XY = 100;

    public Texture2D texture;
    private LineRenderer renderer;

    public void Start()
    {
        newMarkersPosition = scaleMarkersPositions(AreaLearningInGameController.Instance.getGraph().getMarkersPosition());
        graph2D = AreaLearningInGameController.Instance.getGraph().get2DGraph();
        lineRenderers = new List<GameObject>();
        //drawConnectionsBetweenMarkers();
        //drawLine();
    }

    /// <summary>
    /// Scene switching GUI.
    /// </summary>
    private void OnGUI()
    {
        
        if (newMarkersPosition == null || newMarkersPosition.Keys.Count == 0 || newMarkersPosition.Values.Count == 0)
        {
            AndroidHelper.ShowAndroidToastMessage("List of markers is EMPTY!");
            return;
        }

        foreach (KeyValuePair<int, Vector2> marker in newMarkersPosition)
        {
            if (marker.Value == null)
            {
                AndroidHelper.ShowAndroidToastMessage("value");
                break;
            }
            
            drawReactangle(marker.Key.ToString(), (int) marker.Value.x, (int) marker.Value.y);
        }
    }

    private void drawReactangle(String name, int x, int y)
    {
        GUI.color = new Color(1.0f, 0, 0);  // Red color 
        Rect tmpButton = new Rect(x, y, BUTTON_SIZE_WIDTH, BUTTON_SIZE_HEIGHT);
        GUI.DrawTexture(tmpButton, texture);

        if (GUI.Button(tmpButton, "<size=20>" + name + "</size>"))
        {
            doSomething(name);
        }

        GUI.color = Color.white;            // White color
    }

    private void doSomething(String name)
    {
        AndroidHelper.ShowAndroidToastMessage("Button ID: " + name);
    }

    private Vector2 findMinXY(Dictionary<int, Vector3> markers)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;

        foreach (KeyValuePair<int, Vector3> marker in markers)
        {
            if (marker.Value.x < minX)
            {
                minX = marker.Value.x;
            }
            if (marker.Value.z < minY)
            {
                minY = marker.Value.z;
            }
        }

        return new Vector2(minX, minY);
    }

    private Dictionary<int, Vector2> scaleMarkersPositions(Dictionary<int, Vector3> markers)
    {
        Vector2 shiftXY = findMinXY(markers);
        newMarkersPosition = new Dictionary<int, Vector2>();

        foreach (KeyValuePair<int, Vector3> marker in markers)
        {
            int x = (int)(((marker.Value.x - shiftXY.x) * SCALE_XY) + BOUD_SCALED_XY);
            int z = (int)(((marker.Value.z - shiftXY.y) * SCALE_XY) + BOUD_SCALED_XY);

            newMarkersPosition.Add(marker.Key, new Vector2(x, z));
        }

        return newMarkersPosition;
    }
 /*
    private void drawConnectionsBetweenMarkers()
    {
        for (int i = 0; i < graph2D.GetLength(0); i++)
        {
            Vector2 firstPosition = new Vector2();
            bool isFillFirst = newMarkersPosition.TryGetValue(i, out firstPosition);

            for (int j = i + 1; j < graph2D.GetLength(0); j++)
            {
                if (!isFillFirst)
                {
                    break;
                }

                if (graph2D[i, j] == 0)
                {
                    break;
                }

                Vector2 secondPosition = new Vector2();
                bool isFillSecond = newMarkersPosition.TryGetValue(j, out firstPosition);

                if (!isFillSecond)
                {
                    break;
                }

                GameObject tmp = new GameObject();
                tmp.transform.SetParent(gameObject.transform);
                tmp.AddComponent<Line>().lineSetup(firstPosition, secondPosition);
                lineRenderers.Add(tmp);
            }
        }
    }
*/
    private void drawLine()
    {
        Vector2 p1 = new Vector2(500, 500);
        Vector2 p2 = new Vector2(800, 800);

        renderer = new LineRenderer();

        renderer.sortingLayerName = "OnTop";
        renderer.sortingOrder = 5;

        // transform
        renderer.numPositions = 2;
        renderer.SetPosition(0, p1);
        renderer.SetPosition(1, p2);
        renderer.startWidth = 0.01f;
        renderer.endWidth = 0.01f;
        renderer.useWorldSpace = true;
    }
}
