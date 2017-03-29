using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Navigation2DUIController : MonoBehaviour
{
    private double[,] graph2D;
    private List<GameObject> lineRenderers;
    private Dictionary<int, GameObject> buttons;

    private const int SCALE_XY = 1;
    private const int BOUD_SCALED_XY = 1;
    private const int SCALING = 100;

    public Texture2D texture;
    private LineRenderer renderer;
    public Material mat;

    public GameObject cube;
    public GameObject button;

    public void Start()
    {
        Dictionary<int, Vector2>  newMarkersPosition = scaleMarkersPositions(AreaLearningInGameController.Instance.getGraph().getMarkersPosition());
        graph2D = AreaLearningInGameController.Instance.getGraph().get2DGraph();
        lineRenderers = new List<GameObject>();
        buttons = new Dictionary<int, GameObject>();
        drawGUI(newMarkersPosition);
        drawConnectionsBetweenMarkers(newMarkersPosition);
        markNearestMarker(newMarkersPosition);
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
            drawButton(marker.Key, marker.Value.x, marker.Value.y);
        }
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

    private Dictionary<int, Vector2> scaleMarkersPositions(Dictionary<int, Vector3> markers)
    {
        Dictionary<int, Vector2> scaledPositions = new Dictionary<int, Vector2>();

        foreach (KeyValuePair<int, Vector3> marker in markers)
        {
            float x = marker.Value.x * SCALING;
            float z = marker.Value.z * SCALING;

            scaledPositions.Add(marker.Key, new Vector2(x, z));
        }

        return scaledPositions;
    }


    private void drawConnectionsBetweenMarkers(Dictionary<int, Vector2> newMarkersPosition)
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
                bool isFillSecond = newMarkersPosition.TryGetValue(j, out secondPosition);

                if (!isFillSecond)
                {
                    break;
                }
                
                GameObject tmp = new GameObject();
                tmp.transform.SetParent(gameObject.transform);
                tmp.AddComponent<Line>().lineSetup(new Vector3(firstPosition.x, firstPosition.y, 0),
                                                   new Vector3(secondPosition.x, secondPosition.y, 0),
                                                   5f);
                lineRenderers.Add(tmp);
            }
        }
    }

    private void markNearestMarker(Dictionary<int, Vector2> markers)
    {
        Vector3 myPosition3D = AreaLearningInGameController.Instance.getCurrentPosition();
        Vector2 myPosition2D = new Vector2(myPosition3D.x * SCALING, myPosition3D.z * SCALING);
        GameObject nearestButton = null;

        int nearestID = -1;
        float nearestDistance = float.MaxValue;

        foreach (KeyValuePair<int, Vector2> marker in markers)
        {
            float dist = Vector2.Distance(myPosition2D, marker.Value);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestID = marker.Key;
            }
        }

        if (buttons.TryGetValue(nearestID, out nearestButton))
        {
            nearestButton.GetComponent<Image>().color = Color.yellow;
        }
    }

    private void drawButton(int buttonId, float x, float y)
    {
        /*
        GameObject arcube = Instantiate(cube) as GameObject;
        arcube.transform.SetParent(gameObject.transform, true);
        arcube.GetComponentInChildren<TextMesh>().text = name;
        arcube.transform.position = new Vector3(x, y, 0);
        */
       
        GameObject newButton = Instantiate(button) as GameObject;
        newButton.transform.SetParent(gameObject.transform, true);
        newButton.GetComponent<Image>().color = Color.red;
        newButton.GetComponentInChildren<Text>().text = buttonId.ToString();
        newButton.transform.position = new Vector3(x, y, 0);
        newButton.GetComponent<Button>().onClick.AddListener( () => { doDOdo(); } );

        buttons.Add(buttonId, newButton);
    }

    private void doDOdo()
    {
        AndroidHelper.ShowAndroidToastMessage("oasnasdgjkln");
    }
}
