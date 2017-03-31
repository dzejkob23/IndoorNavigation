using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Navigation2DUIController : MonoBehaviour
{
    private const int SCALING = 100;
    private static Color NEAREST_BUTTON_COLOR = Color.yellow;
    private static Color NAVIGATION_BUTTON_COLOR = Color.gray;
    private static Color NEWLINE_BUTTON_COLOR = Color.green;
    private static Color BASIC_BUTTON_COLOR = Color.red;

    // Add line in 2D
    private int[] connectMarkersId = { -1, -1 };

    // Global values from previous scene
    Dictionary<int, Vector2> newMarkersPosition;
    private double[,] graph2D;

    // Selected marker to navigate
    public GameObject button;
    private int lastSelectedID = -1;
    private int newSelectedID = -1;
    private int nearestID = -1;
    private Color lastSelectedColor;

    // GUI buttons and lines
    public Button navigateButton;
    public Button modifyButton;
    public Button finishButton;
    public Button addLineButton;
    private Dictionary<int, GameObject> buttons;
    private List<GameObject> lineRenderers;

    public void Start()
    {
        // Get values from previous scene
        newMarkersPosition = scaleMarkersPositions(AreaLearningInGameController.Instance.getGraph().getMarkersPosition());
        graph2D = AreaLearningInGameController.Instance.getGraph().get2DGraph();

        // Initialization
        lineRenderers = new List<GameObject>();
        buttons = new Dictionary<int, GameObject>();

        // Check if global values are prepared
        if (newMarkersPosition == null || newMarkersPosition.Count == 0)
        {
            return;
        }

        // Prepare environment
        drawGuiButtons(newMarkersPosition);
        drawConnectionsBetweenButtons(newMarkersPosition, graph2D);
        markNearestMarker(newMarkersPosition);
    }

    public void Update()
    {
        selectMarkerToNavigate();
    }

    private void selectMarkerToNavigate()
    {
        // mark the nearest marker to user position - return
        if (newSelectedID == nearestID)
        {
            return;
        }

        // select new marker for navigation
        if (newSelectedID != lastSelectedID)
        {
            GameObject tmpButton = new GameObject();
            GameObject nearestButton = new GameObject();

            if (!buttons.TryGetValue(newSelectedID, out tmpButton))
            {
                Debug.Log("#CHECK - Can not find marker with ID " + newSelectedID + " ! - method Update");
                return;
            }

            if (!buttons.TryGetValue(nearestID, out nearestButton))
            {
                Debug.Log("#CHECK - Can not find nearest marker with ID " + newSelectedID + " ! - method Update");
                return;
            }

            lastSelectedColor = tmpButton.GetComponent<Image>().color;
            tmpButton.GetComponent<Image>().color = NAVIGATION_BUTTON_COLOR;

            bool isLastInButtons = buttons.TryGetValue(lastSelectedID, out tmpButton);
            lastSelectedID = newSelectedID;

            if (!isLastInButtons)
            {

                Debug.Log("#CHECK - Can not find marker with ID " + lastSelectedID + " ! - method Update");
                return;
            }

            tmpButton.GetComponent<Image>().color = lastSelectedColor;
            nearestButton.GetComponent<Image>().color = NEAREST_BUTTON_COLOR;
        }
    }

    /// <summary>
    /// Scene switching GUI.
    /// </summary>
    private void drawGuiButtons(Dictionary<int, Vector2> newMarkersPosition)
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


    private void drawConnectionsBetweenButtons(Dictionary<int, Vector2> newMarkersPosition, double[,] graph2D)
    {
        for (int i = 0; i < graph2D.GetLength(0); i++)
        {
            Vector2 firstPosition = new Vector2();
            bool isFillFirst = newMarkersPosition.TryGetValue(i, out firstPosition);

            for (int j = i + 1; j < graph2D.GetLength(0); j++)
            {

                Debug.Log("#CHECK_GRAPH\n" + graph2D[i,j].ToString());

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

                addLine(firstPosition, secondPosition);
            }
        }
    }

    private void addLine(Vector2 firstPosition, Vector2 secondPosition)
    {
        GameObject tmp = new GameObject();
        tmp.transform.SetParent(gameObject.transform);
        tmp.AddComponent<Line>().lineSetup(new Vector3(firstPosition.x, firstPosition.y, 0),
                                           new Vector3(secondPosition.x, secondPosition.y, 0),
                                           5f);
        lineRenderers.Add(tmp);
    }

    private void markNearestMarker(Dictionary<int, Vector2> markers)
    {
        Vector3 myPosition3D = AreaLearningInGameController.Instance.getCurrentPosition();
        Vector2 myPosition2D = new Vector2(myPosition3D.x * SCALING, myPosition3D.z * SCALING);
        GameObject nearestButton = null;

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
        GameObject newButton = Instantiate(button) as GameObject;
        newButton.transform.SetParent(gameObject.transform, true);
        newButton.GetComponent<Image>().color = BASIC_BUTTON_COLOR;
        newButton.GetComponentInChildren<Text>().text = buttonId.ToString();
        newButton.transform.position = new Vector3(x, y, 0);
        newButton.GetComponent<Button>().onClick.AddListener( () => { doOnButton(buttonId); } );

        buttons.Add(buttonId, newButton);
    }

    private void doOnButton(int currentId)
    {
        newSelectedID = currentId;
        AndroidHelper.ShowAndroidToastMessage("Selected " + currentId + " to navigate!");
    }

    public void moveToNavigation()
    {
        AndroidHelper.ShowAndroidToastMessage("Navigation started ...");
        // SceneManager.LoadScene("ARNavigation");
    }

    public void modify2DMap()
    {
        navigateButton.gameObject.SetActive(false);
        modifyButton.gameObject.SetActive(false);
        finishButton.gameObject.SetActive(true);
        addLineButton.gameObject.SetActive(true);
    }

    public void update2DGraph()
    {
        navigateButton.gameObject.SetActive(true);
        modifyButton.gameObject.SetActive(true);
        finishButton.gameObject.SetActive(false);
        addLineButton.gameObject.SetActive(false);
    }

    public void addLineIn2D()
    {
        GameObject tmp;

        if (newSelectedID == -1)
        {
            return;
        }

        if (newSelectedID != -1 && connectMarkersId[0] == -1)
        {
            connectMarkersId[0] = newSelectedID;
            buttons.TryGetValue(connectMarkersId[0], out tmp);
            tmp.GetComponent<Image>().color = NEWLINE_BUTTON_COLOR;
            return;
        }

        if (newSelectedID != -1
            && connectMarkersId[1] == -1 
            && connectMarkersId[0] != newSelectedID)
        {
            connectMarkersId[1] = newSelectedID;
            buttons.TryGetValue(connectMarkersId[1], out tmp);
            tmp.GetComponent<Image>().color = NEWLINE_BUTTON_COLOR;
        }
        else
        {
            AndroidHelper.ShowAndroidToastMessage("Select second marker!");
            return;
        }

        if (graph2D[connectMarkersId[0], connectMarkersId[1]] != 0)
        {
            AndroidHelper.ShowAndroidToastMessage("Connecion exists! Cache was cleaned!");
            connectMarkersId[0] = -1;
            connectMarkersId[1] = -1;
            return;
        }

        if (connectMarkersId[0] != -1 && connectMarkersId[1] != -1)
        {
            Vector2 firstPosition;
            Vector2 secondPosition;

            newMarkersPosition.TryGetValue(connectMarkersId[0], out firstPosition);
            newMarkersPosition.TryGetValue(connectMarkersId[1], out secondPosition);

            addLine(firstPosition, secondPosition);

            firstPosition = new Vector2(firstPosition.x / SCALING, firstPosition.y / SCALING);
            secondPosition = new Vector2(secondPosition.x / SCALING, secondPosition.y / SCALING);

            float distance = Vector2.Distance(firstPosition, secondPosition);
            graph2D[connectMarkersId[0], connectMarkersId[1]] = distance;
            graph2D[connectMarkersId[1], connectMarkersId[0]] = distance;

            // TODO - check if this settings is OK
            connectMarkersId[0] = -1;
            connectMarkersId[1] = -1;
            //newSelectedID = -1;

            AndroidHelper.ShowAndroidToastMessage("Created NEW connection.");
        }
    }
}
