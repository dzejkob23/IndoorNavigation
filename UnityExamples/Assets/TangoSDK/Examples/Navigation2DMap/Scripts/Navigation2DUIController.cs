//-----------------------------------------------------------------------
// <copyright file="Navigation2DUIController.cs" author="Jakub Zíka" student="A15N0087P">
//
// License is according West Bohemmia licenses.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tango;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controller script for 2D scene.
/// </summary>
public class Navigation2DUIController : MonoBehaviour
{
    /// <summary>
    /// Scaling constant.
    /// </summary>
    private const int SCALING = 300;
    /// <summary>
    /// Nearest marker button color.
    /// </summary>
    private static Color NEAREST_BUTTON_COLOR = Color.yellow;
    /// <summary>
    /// Navigation marker button color.
    /// </summary>
    private static Color NAVIGATION_BUTTON_COLOR = Color.gray;
    /// <summary>
    /// New line between buttons color.
    /// </summary>
    private static Color NEWLINE_BUTTON_COLOR = Color.green;
    /// <summary>
    /// Basic marker button color.
    /// </summary>
    private static Color BASIC_BUTTON_COLOR = Color.red;
    /// <summary>
    /// Add line in 2D.
    /// </summary>
    private int[] connectMarkersId = { -1, -1 };

    // Global values from previous scene
    /// <summary>
    /// Scaling new positions of markers on 2D surface.
    /// </summary>
    private Dictionary<int, Vector2> newMarkersPosition;
    /// <summary>
    /// 2D graph.
    /// </summary>
    private double[,] graph2D;

    // Selected marker to navigate
    /// <summary>
    /// Touching button prepared prefab.
    /// </summary>
    public GameObject button;
    /// <summary>
    /// Indicator last selected button.
    /// </summary>
    private int lastSelectedID = -1;
    /// <summary>
    /// Indicator new selected button.
    /// </summary>
    private int newSelectedID = -1;
    /// <summary>
    /// Indicator nearest marker button.
    /// </summary>
    private int nearestID = -1;
    /// <summary>
    /// Previous nearest marker button.
    /// </summary>
    private int previousNearestID = -1;
    /// <summary>
    /// Last color of selected marker button.
    /// </summary>
    private Color lastSelectedColor;

    // GUI buttons and lines
    /// <summary>
    /// Instance of navigation button.
    /// </summary>
    public Button navigateButton;
    /// <summary>
    /// Instance of modify button.
    /// </summary>
    public Button modifyButton;
    /// <summary>
    /// Instance of finish button;
    /// </summary>
    public Button finishButton;
    /// <summary>
    /// Instance of add line button.
    /// </summary>
    public Button addLineButton;
    /// <summary>
    /// Dictionary of marker buttons.
    /// </summary>
    private Dictionary<int, GameObject> buttons;
    /// <summary>
    /// Dictionary of lines between marker buttons.
    /// </summary>
    private Dictionary<KeyPair, GameObject> lineRenderers;
    /// <summary>
    /// Previous scene AreaLearning controller.
    /// </summary>
    AreaLearningInGameController areaLearning;
    /// <summary>
    /// Tango App controller.
    /// </summary>
    TangoApplication tangoApp;
    /// <summary>
    /// Tango position senzor controller.
    /// </summary>
    TangoARPoseController poseController;

    // Navigation elements
    /// <summary>
    /// Position indicator icon in 2D scene.
    /// </summary>
    public GameObject navigationIcon;
    /// <summary>
    /// Relocalization button. Center scene on indicator icon.
    /// </summary>
    public Button relocalizationButton;
    /// <summary>
    /// Relocalization material - centered.
    /// </summary>
    public Material relocalizationMaterial;
    /// <summary>
    /// Relocalization material - doesn't centered.
    /// </summary>
    public Material noRelocalizationMaterial;
    /// <summary>
    /// Background of 2D scene.
    /// </summary>
    public Material line2DMap;

    /// <summary>
    /// Start method.
    /// </summary>
    public void Start()
    {
        tangoApp = FindObjectOfType<TangoApplication>();
        poseController = FindObjectOfType<TangoARPoseController>();
        areaLearning = FindObjectOfType<AreaLearningInGameController>();

        poseController.GetComponentInParent<Camera>().enabled = false;
        GameObject.FindGameObjectWithTag("AreaLearning").SetActive(false);

        // Get values from previous scene
        newMarkersPosition = scaleMarkersPositions(areaLearning.getGraph().getMarkersPosition());
        graph2D = areaLearning.getGraph().get2DGraph();

        // Initialization
        lineRenderers = new Dictionary<KeyPair, GameObject>();
        buttons = new Dictionary<int, GameObject>();

        // Check if global values are prepared
        if (newMarkersPosition == null || newMarkersPosition.Count == 0)
        {
            return;
        }

        // Prepare environment
        drawGuiButtons(newMarkersPosition);
        drawConnectionsBetweenButtons(newMarkersPosition, graph2D);
    }

    /// <summary>
    /// Update method.
    /// </summary>
    public void Update()
    {
        // Select marker to navigate
        selectMarkerToNavigate();

        // Mark the nearest navigation marker
        markNearestMarker(newMarkersPosition);

        // Update position navigation icon on screen
        setPositionNavigationIcon();
    }

    /// <summary>
    /// Set position icon position by position controller.
    /// </summary>
    private void setPositionNavigationIcon()
    {
        // Prepare data
        Vector3 currentPosition = poseController.m_tangoPosition;
        Vector3 scaledCurrentPosititon = new Vector3(currentPosition.x * SCALING, currentPosition.z * SCALING, 0);
        Quaternion currentRotation = poseController.m_tangoRotation;

        // Move navigation icon
        navigationIcon.transform.position = scaledCurrentPosititon;
        navigationIcon.transform.rotation = new Quaternion(0, 0, currentRotation.z, currentRotation.w);

        if (!Camera.main.GetComponent<PinchZoom>().isUsingRelocalization)
        {
            relocalizationButton.GetComponent<RawImage>().material = noRelocalizationMaterial;
            return;
        }
        else
        {
            relocalizationButton.GetComponent<RawImage>().material = relocalizationMaterial;
        }

        // Move main camera
        scaledCurrentPosititon.z = -500;
        Camera.main.transform.position = scaledCurrentPosititon;
    }

    /// <summary>
    /// Set relocalization icon on value true.
    /// </summary>
    public void relocalizeNavigationIconPosition()
    {
        Camera.main.GetComponent<PinchZoom>().isUsingRelocalization = true;
    }

    /// <summary>
    /// Select marker to navigate.
    /// </summary>
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
                return;
            }

            if (!buttons.TryGetValue(nearestID, out nearestButton))
            {
                return;
            }

            lastSelectedColor = tmpButton.GetComponent<Image>().color;
            tmpButton.GetComponent<Image>().color = NAVIGATION_BUTTON_COLOR;

            bool isLastInButtons = buttons.TryGetValue(lastSelectedID, out tmpButton);
            lastSelectedID = newSelectedID;

            if (!isLastInButtons)
            {
                return;
            }

            tmpButton.GetComponent<Image>().color = lastSelectedColor;
            nearestButton.GetComponent<Image>().color = NEAREST_BUTTON_COLOR;
        }
    }

    /// <summary>
    /// Draw GUI buttons by input positions.
    /// </summary>
    /// <param name="newMarkersPosition">Position for buttons in 2D</param>
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

            drawButton(marker.Key, marker.Value.x, marker.Value.y);
        }
    }

    /// <summary>
    /// Method finds the shortest and the fahrest position to [0,0].
    /// </summary>
    /// <param name="markers">Dictionarz of markers.</param>
    /// <returns>Min and Max values.</returns>
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

    /// <summary>
    /// Scaling markers positions from 3D to 2D.
    /// </summary>
    /// <param name="markers">Dictionary of markers.</param>
    /// <returns>New scaled positions in 2D.</returns>
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

    /// <summary>
    /// Draw connection between buttons.
    /// </summary>
    /// <param name="newMarkersPosition">Markers positions in 2D.</param>
    /// <param name="graph2D">Matrix of neighbours between markers.</param>
    private void drawConnectionsBetweenButtons(Dictionary<int, Vector2> newMarkersPosition, double[,] graph2D)
    {
        for (int i = 0; i < graph2D.GetLength(0); i++)
        {
            Vector2 firstPosition = new Vector2();
            bool isFillFirst = newMarkersPosition.TryGetValue(i, out firstPosition);

            for (int j = i + 1; j < graph2D.GetLength(0); j++)
            {
                Vector2 secondPosition = new Vector2();
                bool isFillSecond = newMarkersPosition.TryGetValue(j, out secondPosition);

                if (graph2D[i, j] != 0.0f && isFillFirst && isFillSecond)
                {
                    addLine(firstPosition, secondPosition, i, j);
                }
            }
        }
    }

    /// <summary>
    /// Add line to 2D scene.
    /// </summary>
    /// <param name="firstPosition">Start position of line.</param>
    /// <param name="secondPosition">End position of line.</param>
    /// <param name="from">Line from id marker.</param>
    /// <param name="to">Line to id marker.</param>
    private void addLine(Vector2 firstPosition, Vector2 secondPosition, int from, int to)
    {
        GameObject tmp = new GameObject();
        tmp.transform.SetParent(gameObject.transform);
        tmp.AddComponent<Line>().lineSetup(new Vector3(firstPosition.x, firstPosition.y, 0),
                                           new Vector3(secondPosition.x, secondPosition.y, 0),
                                           5f,
                                           line2DMap);
        lineRenderers.Add(new KeyPair(from, to), tmp);
    }

    /// <summary>
    /// Find nearest the nearest marker to device position.
    /// </summary>
    /// <param name="markers">Dictionary of markers.</param>
    private void markNearestMarker(Dictionary<int, Vector2> markers)
    {
        Vector3 myPosition3D = areaLearning.getCurrentPosition();
        Vector2 myPosition2D = new Vector2(myPosition3D.x * SCALING, myPosition3D.z * SCALING);
        GameObject nearestButton = null;

        float nearestDistance = float.MaxValue;

        previousNearestID = nearestID;

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
            nearestButton.GetComponent<Image>().color = NEAREST_BUTTON_COLOR;
        }

        if (buttons.TryGetValue(previousNearestID, out nearestButton) && previousNearestID != nearestID)
        {
            nearestButton.GetComponent<Image>().color = BASIC_BUTTON_COLOR;
        }
    }

    /// <summary>
    /// Draw button by id and position in 2D.
    /// </summary>
    /// <param name="buttonId">Button ID.</param>
    /// <param name="x">Button position X.</param>
    /// <param name="y">Button position Y.</param>
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

    /// <summary>
    /// Save button ID on touch.
    /// </summary>
    /// <param name="currentId"></param>
    private void doOnButton(int currentId)
    {
        newSelectedID = currentId;
    }

    /// <summary>
    /// Switch scena from 2D to 3D.
    /// </summary>
    public void show3DMapScene()
    {
        // before switching
        // enable 3D camere with augmented reality rendering
        poseController.GetComponentInParent<Camera>().enabled = true;

        foreach (GameObject canvas in GameObject.FindGameObjectsWithTag("Navigation2DMap"))
        {
            canvas.SetActive(false);
        }

        areaLearning.canvas3DTo2D.SetActive(true);
        SceneManager.UnloadSceneAsync("Navigation2DMap");
    }

    /// <summary>
    /// Switch scene from 2D to navigation.
    /// </summary>
    public void moveToNavigation()
    {
        if (nearestID == -1 || newSelectedID == -1)
        {
            AndroidHelper.ShowAndroidToastMessage("Please choose navigate marker!");
            return;
        }

        // create dijkstra function and computed shorted path for navigation
        DijkstraAlgorithm dijkstra = new DijkstraAlgorithm(graph2D, nearestID, newSelectedID);

        if (dijkstra.sPath == null)
        {
            AndroidHelper.ShowAndroidToastMessage("To this NavPoint does not have any path! Please repair graph.");
            return;
        }

        // before switching
        // disable all markers with renderers
        areaLearning.enableDisableAllMarkers(false);
        // toggle scene settings from "create navigation map" to "navigate in 3D space"
        areaLearning.toggleNavigationScene();
        // enable 3D camere with augmented reality rendering
        poseController.GetComponentInParent<Camera>().enabled = true;
        // show only markers for navigation
        areaLearning.showNavigationMarkers(dijkstra.sPath);

        foreach (GameObject canvas in GameObject.FindGameObjectsWithTag("Navigation2DMap"))
        {
            canvas.SetActive(false);
        }

        areaLearning.canvas2DTo3D.SetActive(true);
        SceneManager.UnloadSceneAsync("Navigation2DMap");
    }

    /// <summary>
    /// Switch buttons on screen for modifing lines on 2D graph.
    /// </summary>
    public void modify2DMap()
    {
        navigateButton.gameObject.SetActive(false);
        modifyButton.gameObject.SetActive(false);
        finishButton.gameObject.SetActive(true);
        addLineButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Switch buttons on screen for moving to navigation in 3D.
    /// </summary>
    public void update2DGraph()
    {
        navigateButton.gameObject.SetActive(true);
        modifyButton.gameObject.SetActive(true);
        finishButton.gameObject.SetActive(false);
        addLineButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Add or delete line in 2D.
    /// </summary>
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

        // DELETE LINE
        if (connectMarkersId[0] != -1
            && connectMarkersId[1] != -1
            && graph2D[connectMarkersId[0], connectMarkersId[1]] != 0)
        {
            KeyPair tmpKeyPair = null;

            foreach (KeyValuePair<KeyPair, GameObject> renderer in lineRenderers)
            {
                if (renderer.Key.keys[0] == connectMarkersId[0]
                    && renderer.Key.keys[1] == connectMarkersId[1])
                {
                    tmpKeyPair = renderer.Key;
                    break;
                }
            }

            if (tmpKeyPair == null)
            {
                foreach (KeyValuePair<KeyPair, GameObject> renderer in lineRenderers)
                {
                    if (renderer.Key.keys[0] == connectMarkersId[1]
                        && renderer.Key.keys[1] == connectMarkersId[0])
                    {
                        tmpKeyPair = renderer.Key;
                        break;
                    }
                }
            }

            if (tmpKeyPair != null)
            {
                GameObject tmpObject;
                lineRenderers.TryGetValue(tmpKeyPair, out tmpObject);
                Destroy(tmpObject);
                lineRenderers.Remove(tmpKeyPair);
            }

            areaLearning.removeLineBetweenMarkers(connectMarkersId[0], connectMarkersId[1]);

            graph2D[connectMarkersId[0], connectMarkersId[1]] = 0;
            graph2D[connectMarkersId[1], connectMarkersId[0]] = 0;

            connectMarkersId[0] = -1;
            connectMarkersId[1] = -1;

            AndroidHelper.ShowAndroidToastMessage("Connecion is deleted.");
            return;
        }

        // CREATE LINE
        if (connectMarkersId[0] != -1
            && connectMarkersId[1] != -1
            && graph2D[connectMarkersId[0], connectMarkersId[1]] == 0)
        {
            Vector2 firstPosition;
            Vector2 secondPosition;

            newMarkersPosition.TryGetValue(connectMarkersId[0], out firstPosition);
            newMarkersPosition.TryGetValue(connectMarkersId[1], out secondPosition);

            addLine(firstPosition, secondPosition, connectMarkersId[0], connectMarkersId[1]);

            firstPosition = new Vector2(firstPosition.x / SCALING, firstPosition.y / SCALING);
            secondPosition = new Vector2(secondPosition.x / SCALING, secondPosition.y / SCALING);

            float distance = Vector2.Distance(firstPosition, secondPosition);
            graph2D[connectMarkersId[0], connectMarkersId[1]] = distance;
            graph2D[connectMarkersId[1], connectMarkersId[0]] = distance;

            areaLearning.addLineWithIds(connectMarkersId[0], connectMarkersId[1]);

            connectMarkersId[0] = -1;
            connectMarkersId[1] = -1;

            AndroidHelper.ShowAndroidToastMessage("Created is created.");
        }
    }



}
