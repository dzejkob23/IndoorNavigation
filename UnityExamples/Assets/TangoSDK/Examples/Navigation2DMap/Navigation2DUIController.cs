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

    private double[,] graph2D;
    private List<GameObject> lineRenderers;

    private const int SCALE_XY = 1;
    private const int BOUD_SCALED_XY = 1;

    public Texture2D texture;
    private LineRenderer renderer;
    public Material mat;

    public Canvas m_canvas;
    public GameObject buttonPref;

    public void Start()
    {
        Dictionary<int, Vector2>  newMarkersPosition = scaleMarkersPositions(AreaLearningInGameController.Instance.getGraph().getMarkersPosition());
        graph2D = AreaLearningInGameController.Instance.getGraph().get2DGraph();
        lineRenderers = new List<GameObject>();
        //drawConnectionsBetweenMarkers();

        drawGUI(newMarkersPosition);
    }

    /// <summary>
    /// Scene switching GUI.
    /// </summary>
    private void drawGUI(Dictionary<int, Vector2> newMarkersPosition)
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

            Debug.Log("#CHECK - name of marker: " + marker.Key.ToString());
            drawButton(marker.Key.ToString(), marker.Value.x, marker.Value.y);
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
        //AndroidHelper.ShowAndroidToastMessage("Button ID: " + name);
    }

    private Vector4 findMinXY(Dictionary<int, Vector3> markers)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

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
            if (marker.Value.z > maxY)
            {
                maxY = marker.Value.z;
            }
            if (marker.Value.x > maxX)
            {
                maxX = marker.Value.x;
            }
        }

        return new Vector4(maxX, minX, maxY, minY);
    }

    private Vector2 computeScalingForDisplay(Dictionary<int, Vector3> markers)
    {
        Vector4 scaling = findMinXY(markers);

        float markersWidth = Math.Abs(scaling[0]) + Math.Abs(scaling[1]);
        float markersHeight = Math.Abs(scaling[2]) + Math.Abs(scaling[3]);

        float scaledX =  markersWidth / Screen.width;
        float scaledY =  markersHeight / Screen.height;

        Debug.Log("#CHECK *** V4: " + scaling[0] + " " + scaling[1] + " " + scaling[2] + " " + scaling[3]);
        Debug.Log("#CHECK *** V2 " + scaledX + " " + scaledY);

        return new Vector2(scaledX, scaledY);
    }

    private Dictionary<int, Vector2> scaleMarkersPositions(Dictionary<int, Vector3> markers)
    {
        Vector2 scaling = computeScalingForDisplay(markers);
        Dictionary<int, Vector2> scaledPositions = new Dictionary<int, Vector2>();

        foreach (KeyValuePair<int, Vector3> marker in markers)
        {
            Debug.Log("#CHECK - BEFOR: " + marker.Value.x + " " + marker.Value.z);           

            float x = marker.Value.x * scaling[0];
            float z = marker.Value.z * scaling[1];

            Debug.Log("#CHECK - AFTER: " + x + " " + z);

            scaledPositions.Add(marker.Key, new Vector2(x, z));
        }

        return scaledPositions;
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
        Vector3 p1 = new Vector3(0, 0, -1);
        Vector3 p2 = new Vector3(200, 200, -1);

        renderer = gameObject.AddComponent<LineRenderer>();

        renderer.sortingLayerName = "OnTop";
        renderer.sortingOrder = 5;

        // transform
        renderer.numPositions = 2;
        renderer.SetPosition(0, p1);
        renderer.SetPosition(1, p2);
        renderer.startWidth = 5;
        renderer.endWidth = 5;
        renderer.useWorldSpace = true;
    }

    private void drawButton(String name, float x, float y)
    {
        GameObject newButton = Instantiate(buttonPref) as GameObject;
        newButton.transform.SetParent(gameObject.transform, true);
        newButton.GetComponent<Image>().color = Color.red;
        newButton.GetComponentInChildren<Text>().text = name;
        newButton.transform.position = new Vector3(x, y, 0);
        newButton.GetComponent<Button>().onClick.AddListener( () => { doDOdo(); } );
    }

    private void doDOdo()
    {
        AndroidHelper.ShowAndroidToastMessage("oasnasdgjkln");
    }
}
