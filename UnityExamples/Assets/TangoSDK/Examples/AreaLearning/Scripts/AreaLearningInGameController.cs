//-----------------------------------------------------------------------
// <copyright file="AreaLearningInGameController.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Tango;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// AreaLearningGUIController is responsible for the main game interaction.
/// 
/// This class also takes care of loading / save persistent data(marker), and loop closure handling.
/// </summary>
public class AreaLearningInGameController : MonoBehaviour, ITangoPose, ITangoEvent, ITangoDepth
{
    /// <summary>
    /// Evalutated graph
    /// </summary>
    private Graph graph;

    /// <summary>
    /// Navigation toggle
    /// </summary>
    private bool isNavigation = false;

    /// <summary>
    /// Last created marker.
    /// </summary>
    private GameObject lastCreatedMarker;

    /// <summary>
    /// Connect two markers.
    /// </summary>
    private ARMarker[] m_connectMarkers;

    /// <summary>
    /// Finish button.
    /// </summary>
    public UnityEngine.UI.Button m_finishButton;

    /// <summary>
    /// Join button.
    /// </summary>
    public UnityEngine.UI.Button m_joinButton;

    /// <summary>
    /// Prefabs of different colored markers.
    /// </summary>
    public GameObject[] m_markPrefabs;

    /// <summary>
    /// The point cloud object in the scene.
    /// </summary>
    public TangoPointCloud m_pointCloud;

    /// <summary>
    /// The canvas to place 2D game objects under.
    /// </summary>
    public Canvas m_canvas;

    /// <summary>
    /// The touch effect to place on taps.
    /// </summary>
    public RectTransform m_prefabTouchEffect;

    /// <summary>
    /// Saving progress UI text.
    /// </summary>
    public UnityEngine.UI.Text m_savingText;

    /// <summary>
    /// The Area Description currently loaded in the Tango Service.
    /// </summary>
    [HideInInspector]
    public AreaDescription m_curAreaDescription;

#if UNITY_EDITOR
    /// <summary>
    /// Handles GUI text input in Editor where there is no device keyboard.
    /// If true, text input for naming new saved Area Description is displayed.
    /// </summary>
    private bool m_displayGuiTextInput;

    /// <summary>
    /// Handles GUI text input in Editor where there is no device keyboard.
    /// Contains text data for naming new saved Area Descriptions.
    /// </summary>
    private string m_guiTextInputContents;

    /// <summary>
    /// Handles GUI text input in Editor where there is no device keyboard.
    /// Indicates whether last text input was ended with confirmation or cancellation.
    /// </summary>
    private bool m_guiTextInputResult;
#endif

    /// <summary>
    /// If set, then the depth camera is on and we are waiting for the next depth update.
    /// </summary>
    private bool m_findPlaneWaitingForDepth;

    /// <summary>
    /// A reference to TangoARPoseController instance.
    /// 
    /// In this class, we need TangoARPoseController reference to get the timestamp and pose when we place a marker.
    /// The timestamp and pose is used for later loop closure position correction. 
    /// </summary>
    private TangoARPoseController m_poseController;

    /// <summary>
    /// List of markers placed in the scene.
    /// </summary>
    private Dictionary<int, GameObject> m_markerList;

    /// <summary>
    /// Reference to the newly placed marker.
    /// </summary>
    private GameObject newMarkObject = null;

    /// <summary>
    /// Current marker type.
    /// </summary>
    private int m_currentMarkType = 0;

    /// <summary>
    /// Current method type.
    /// </summary>
    private int m_currentMethodType = 0;
    
    /// <summary>
    /// If set, this is the selected marker.
    /// </summary>
    private ARMarker m_selectedMarker;

    /// <summary>
    /// If set, this is the rectangle bounding the selected marker.
    /// </summary>
    private Rect m_selectedRect;

    /// <summary>
    /// If the interaction is initialized.
    /// 
    /// Note that the initialization is triggered by the relocalization event. We don't want user to place object before
    /// the device is relocalized.
    /// </summary>
    private bool m_initialized = false;

    /// <summary>
    /// A reference to TangoApplication instance.
    /// </summary>
    private TangoApplication m_tangoApplication;

    /// <summary>
    /// Temporary thread
    /// </summary>
    private Thread m_saveThread;

    /// <summary>
    /// Unity Start function.
    /// 
    /// We find and assign pose controller and tango application, and register this class to callback events.
    /// </summary>
    public void Start()
    {
        m_poseController = FindObjectOfType<TangoARPoseController>();

        m_tangoApplication = FindObjectOfType<TangoApplication>();

        m_connectMarkers = new ARMarker[2];
        m_markerList = new Dictionary<int, GameObject>();
        graph = new Graph();

        if (m_tangoApplication != null)
        {
            m_tangoApplication.Register(this);
        }
    }

    /// <summary>
    /// Unity Update function.
    /// 
    /// Mainly handle the touch event and place mark in place.
    /// </summary>
    public void Update()
    {
        if (m_saveThread != null && m_saveThread.ThreadState != ThreadState.Running)
        {
            // After saving an Area Description or mark data, we reload the scene to restart the game.
            _UpdateMarkersForLoopClosures();
            _SaveMarkerToDisk();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (!m_initialized)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject(0) || GUIUtility.hotControl != 0)
        {
            return;
        }

        if (!m_finishButton.gameObject.activeSelf && m_markerList.Count != 0)
        {
            m_finishButton.gameObject.SetActive(true);
        }

        if (Input.touchCount == 1)
        {
            _reactionOnTouch();
        }
    }

    /// <summary>
    /// Do something on touch on screen
    /// </summary>
    private void _reactionOnTouch()
    {
        Touch t = Input.GetTouch(0);
        Vector2 guiPosition = new Vector2(t.position.x, Screen.height - t.position.y);
        Camera cam = Camera.main;
        RaycastHit hitInfo;

        if (t.phase != TouchPhase.Began)
        {
            return;
        }

        if (!isNavigation)
        {
            if (m_selectedRect.Contains(guiPosition))
            {
                // do nothing, the button will handle it
            }
            else if (Physics.Raycast(cam.ScreenPointToRay(t.position), out hitInfo))
            {
                _selectCreatedMarker(hitInfo);
            }
            else
            {
                _placeNewMarker(t);
            }
        }
        else
        {
            AndroidHelper.ShowAndroidToastMessage("Touch on screen is TURN OFF!");
        }
    }

    public void startNavigation(int start, int target)
    {
        if (target == -1 || start == -1)
        {
            AndroidHelper.ShowAndroidToastMessage("You must select target navigation point/marker!");
        }

        DijkstraAlgorithm dijkstra = new DijkstraAlgorithm(graph.get2DGraph(), start, target);
        int[] navigatePath = dijkstra.sPath;
        // TODO - disable unused markers
        // TODO - mark shorted path
    }

    /// <summary>
    /// Select craete marker via touch
    /// </summary>
    /// <param name="hitInfo"></param>
    private void _selectCreatedMarker(RaycastHit hitInfo)
    {
        // Found a marker, select it (so long as it isn't disappearing)!
        GameObject tapped = hitInfo.collider.gameObject;
        if (!tapped.GetComponent<Animation>().isPlaying)
        {
            m_selectedMarker = tapped.GetComponent<ARMarker>();
        }
    }

    /// <summary>
    /// Place new ar marker to scene
    /// </summary>
    /// <param name="t"></param>
    private void _placeNewMarker(Touch t)
    {
        // Place a new point at that location, clear selection
        m_selectedMarker = null;
        StartCoroutine(_WaitForDepthAndFindPlane(t.position));

        // Because we may wait a small amount of time, this is a good place to play a small
        // animation so the user knows that their input was received.
        RectTransform touchEffectRectTransform = Instantiate(m_prefabTouchEffect) as RectTransform;
        touchEffectRectTransform.transform.SetParent(m_canvas.transform, false);
        Vector2 normalizedPosition = t.position;
        normalizedPosition.x /= Screen.width;
        normalizedPosition.y /= Screen.height;
        touchEffectRectTransform.anchorMin = touchEffectRectTransform.anchorMax = normalizedPosition;
    }

    /// <summary>
    /// Application onPause / onResume callback.
    /// </summary>
    /// <param name="pauseStatus"><c>true</c> if the application about to pause, otherwise <c>false</c>.</param>
    public void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && m_initialized)
        {
            // When application is backgrounded, we reload the level because the Tango Service is disconected. All
            // learned area and placed marker should be discarded as they are not saved.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// Unity OnGUI function.
    /// 
    /// Mainly for removing markers.
    /// </summary>
    public void OnGUI()
    {
        if (m_selectedMarker != null)
        {
            Renderer selectedRenderer = m_selectedMarker.GetComponent<Renderer>();
            
            // GUI's Y is flipped from the mouse's Y
            Rect screenRect = _WorldBoundsToScreen(Camera.main, selectedRenderer.bounds);
            float yMin = Screen.height - screenRect.yMin;
            float yMax = Screen.height - screenRect.yMax;
            screenRect.yMin = Mathf.Min(yMin, yMax);
            screenRect.yMax = Mathf.Max(yMin, yMax);
            
            if (GUI.Button(screenRect, "<size=30>Hide</size>"))
            {
                int selected_id = m_selectedMarker.gameObject.GetComponent<ARMarker>().getID();
                m_markerList.Remove(selected_id);
                m_selectedMarker.SendMessage("Hide");
                m_selectedMarker = null;
                m_selectedRect = new Rect();
            }
            else
            {
                m_selectedRect = screenRect;
            }
        }
        else
        {
            m_selectedRect = new Rect();
        }

#if UNITY_EDITOR
        // Handle text input when there is no device keyboard in the editor.
        if (m_displayGuiTextInput)
        {
            Rect textBoxRect = new Rect(100,
                                        Screen.height - 200,
                                        Screen.width - 200,
                                        100);

            Rect okButtonRect = textBoxRect;
            okButtonRect.y += 100;
            okButtonRect.width /= 2;

            Rect cancelButtonRect = okButtonRect;
            cancelButtonRect.x = textBoxRect.center.x;

            GUI.SetNextControlName("TextField");
            GUIStyle customTextFieldStyle = new GUIStyle(GUI.skin.textField);
            customTextFieldStyle.alignment = TextAnchor.MiddleCenter;
            m_guiTextInputContents = 
                GUI.TextField(textBoxRect, m_guiTextInputContents, customTextFieldStyle);
            GUI.FocusControl("TextField");

            if (GUI.Button(okButtonRect, "OK")
                || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
            {
                m_displayGuiTextInput = false;
                m_guiTextInputResult = true;
            }
            else if (GUI.Button(cancelButtonRect, "Cancel"))
            {
                m_displayGuiTextInput = false;
                m_guiTextInputResult = false;
            }
        }
#endif
    }

    /// <summary>
    /// Set the marker type.
    /// </summary>
    /// <param name="type">Marker type.</param>
    public void SetCurrentMarkType(int type)
    {
        if (type != m_currentMarkType)
        {
            m_currentMarkType = type;
        }
    }

    /// <summary>
    /// Set the method type.
    /// </summary>
    /// <param name="type"></param>
    public void SetCurrentMethodType(int type)
    {
        if (type != m_currentMethodType)
        {
            m_currentMethodType = type;
        }

        if (type != 1)
        {
            m_joinButton.gameObject.SetActive(false);
        }
        else
        {
            m_joinButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Save the game.
    /// 
    /// Save will trigger 3 things:
    /// 
    /// 1. Save the Area Description if the learning mode is on.
    /// 2. Bundle adjustment for all marker positions, please see _UpdateMarkersForLoopClosures() function header for 
    ///     more details.
    /// 3. Save all markers to xml, save the Area Description if the learning mode is on.
    /// 4. Reload the scene.
    /// </summary>
    public void Save()
    {
        StartCoroutine(_DoSaveCurrentAreaDescription());
    }

    /// <summary>
    /// Create connection between two independent markers
    /// </summary>
    public void JoinMarkers()
    {
        if (m_selectedMarker != null && m_connectMarkers[0] == null)
        {
            m_connectMarkers[0] = m_selectedMarker;
            m_selectedMarker = null;
        }

        if (m_selectedMarker != null && m_connectMarkers[1] == null)
        {
            m_connectMarkers[1] = m_selectedMarker;
            m_selectedMarker = null;
        }

        if (m_connectMarkers[0] != null && m_connectMarkers[1] != null)
        {
            Debug.Log("#CHECK - Created connection from ID: " + m_connectMarkers[0].getID() + " to ID: " + m_connectMarkers[1].getID());
            m_connectMarkers[0].addLine(m_connectMarkers[1].getID(), m_connectMarkers[1].transform.position);

            m_connectMarkers[0] = null;
            m_connectMarkers[1] = null;
            m_selectedMarker = null;

            AndroidHelper.ShowAndroidToastMessage("Connection is created.");
        }
    }

    public void FinishMarkers()
    {
        graph.CreateEvaluatedGraph(m_markerList);
        show2DMapScene();
    }

    private void show2DMapScene()
    {
        SceneManager.LoadScene("Navigation2DMap", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Navigation2DMap"));
    }

    /// <summary>
    /// This is called each time a Tango event happens.
    /// </summary>
    /// <param name="tangoEvent">Tango event.</param>
    public void OnTangoEventAvailableEventHandler(Tango.TangoEvent tangoEvent)
    {
        // We will not have the saving progress when the learning mode is off.
        if (!m_tangoApplication.m_areaDescriptionLearningMode)
        {
            return;
        }

        if (tangoEvent.type == TangoEnums.TangoEventType.TANGO_EVENT_AREA_LEARNING
            && tangoEvent.event_key == "AreaDescriptionSaveProgress")
        {
            m_savingText.text = "Saving. " + (float.Parse(tangoEvent.event_value) * 100) + "%";
        }
    }

    /// <summary>
    /// OnTangoPoseAvailable event from Tango.
    /// 
    /// In this function, we only listen to the Start-Of-Service with respect to Area-Description frame pair. This pair
    /// indicates a relocalization or loop closure event happened, base on that, we either start the initialize the
    /// interaction or do a bundle adjustment for all marker position.
    /// </summary>
    /// <param name="poseData">Returned pose data from TangoService.</param>
    public void OnTangoPoseAvailable(Tango.TangoPoseData poseData)
    {
        // This frame pair's callback indicates that a loop closure or relocalization has happened. 
        //
        // When learning mode is on, this callback indicates the loop closure event. Loop closure will happen when the
        // system recognizes a pre-visited area, the loop closure operation will correct the previously saved pose 
        // to achieve more accurate result. (pose can be queried through GetPoseAtTime based on previously saved
        // timestamp).
        // Loop closure definition: https://en.wikipedia.org/wiki/Simultaneous_localization_and_mapping#Loop_closure
        //
        // When learning mode is off, and an Area Description is loaded, this callback indicates a
        // relocalization event. Relocalization is when the device finds out where it is with respect to the loaded
        // Area Description. In our case, when the device is relocalized, the markers will be loaded because we
        // know the relatvie device location to the markers.
        if (poseData.framePair.baseFrame == 
            TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION &&
            poseData.framePair.targetFrame ==
            TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE &&
            poseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
        {
            // When we get the first loop closure/ relocalization event, we initialized all the in-game interactions.
            if (!m_initialized)
            {
                m_initialized = true;
                if (m_curAreaDescription == null)
                {
                    Debug.Log("AndroidInGameController.OnTangoPoseAvailable(): m_curAreaDescription is null");
                    return;
                }

                _LoadMarkerFromDisk();
            }
        }
    }

    /// <summary>
    /// This is called each time new depth data is available.
    /// 
    /// On the Tango tablet, the depth callback occurs at 5 Hz.
    /// </summary>
    /// <param name="tangoDepth">Tango depth.</param>
    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        // Don't handle depth here because the PointCloud may not have been updated yet.  Just
        // tell the coroutine it can continue.
        m_findPlaneWaitingForDepth = false;
    }

    /// <summary>
    /// Actually do the Area Description save.
    /// </summary>
    /// <returns>Coroutine IEnumerator.</returns>
    private IEnumerator _DoSaveCurrentAreaDescription()
    {
#if UNITY_EDITOR
        // Work around lack of on-screen keyboard in editor:
        if (m_displayGuiTextInput || m_saveThread != null)
        {
            yield break;
        }

        m_displayGuiTextInput = true;
        m_guiTextInputContents = "Unnamed";
        while (m_displayGuiTextInput)
        {
            yield return null;
        }

        bool saveConfirmed = m_guiTextInputResult;
#else
        if (TouchScreenKeyboard.visible || m_saveThread != null)
        {
            yield break;
        }
        
        TouchScreenKeyboard kb = TouchScreenKeyboard.Open("Unnamed");
        while (!kb.done && !kb.wasCanceled)
        {
            yield return null;
        }

        bool saveConfirmed = kb.done;
#endif
        if (saveConfirmed)
        {
            // Disable interaction before saving.
            m_initialized = false;
            m_savingText.gameObject.SetActive(true);
            if (m_tangoApplication.m_areaDescriptionLearningMode)
            {
                m_saveThread = new Thread(delegate()
                {
                    // Start saving process in another thread.
                    m_curAreaDescription = AreaDescription.SaveCurrent();
                    AreaDescription.Metadata metadata = m_curAreaDescription.GetMetadata();
#if UNITY_EDITOR
                    metadata.m_name = m_guiTextInputContents;
#else
                    metadata.m_name = kb.text;
#endif
                    m_curAreaDescription.SaveMetadata(metadata);
                });
                m_saveThread.Start();
            }
            else
            {
                _SaveMarkerToDisk();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    /// <summary>
    /// Correct all saved marks when loop closure happens.
    /// 
    /// When Tango Service is in learning mode, the drift will accumulate overtime, but when the system sees a
    /// preexisting area, it will do a operation to correct all previously saved poses
    /// (the pose you can query with GetPoseAtTime). This operation is called loop closure. When loop closure happens,
    /// we will need to re-query all previously saved marker position in order to achieve the best result.
    /// This function is doing the querying job based on timestamp.
    /// </summary>
    private void _UpdateMarkersForLoopClosures()
    {
        // Adjust mark's position each time we have a loop closure detected.
        foreach (KeyValuePair<int, GameObject> obj in m_markerList)
        {
            ARMarker tempMarker = obj.Value.GetComponent<ARMarker>();
            if (tempMarker.m_timestamp != -1.0f)
            {
                TangoCoordinateFramePair pair;
                TangoPoseData relocalizedPose = new TangoPoseData();

                pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION;
                pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;
                PoseProvider.GetPoseAtTime(relocalizedPose, tempMarker.m_timestamp, pair);

                Matrix4x4 uwTDevice = m_poseController.m_uwTss
                                      * relocalizedPose.ToMatrix4x4()
                                      * m_poseController.m_dTuc;

                Matrix4x4 uwTMarker = uwTDevice * tempMarker.m_deviceTMarker;

                obj.Value.transform.position = uwTMarker.GetColumn(3);
                obj.Value.transform.rotation = Quaternion.LookRotation(uwTMarker.GetColumn(2), uwTMarker.GetColumn(1));
            }
        }
    }

    /// <summary>
    /// Write marker list to an xml file stored in application storage.
    /// </summary>
    private void _SaveMarkerToDisk()
    {
        // Compose a XML data list.
        List<MarkerData> xmlDataList = new List<MarkerData>();
        foreach (KeyValuePair<int, GameObject> obj in m_markerList)
        {
            // Add marks data to the list, we intentionally didn't add the timestamp, because the timestamp will not be
            // useful when the next time Tango Service is connected. The timestamp is only used for loop closure pose
            // correction in current Tango connection.
            MarkerData temp = new MarkerData();
            temp.m_type = obj.Value.GetComponent<ARMarker>().m_type;
            temp.m_position = obj.Value.transform.position;
            temp.m_orientation = obj.Value.transform.rotation;
            temp.m_neighbours = new int[obj.Value.GetComponent<ARMarker>().lines.Keys.Count];
            obj.Value.GetComponent<ARMarker>().lines.Keys.CopyTo(temp.m_neighbours, 0);
            temp.m_id = obj.Value.GetComponent<ARMarker>().getID();

            xmlDataList.Add(temp);
        }

        string path = Application.persistentDataPath + "/" + m_curAreaDescription.m_uuid + ".xml";
        var serializer = new XmlSerializer(typeof(List<MarkerData>));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, xmlDataList);
        }
    }

    /// <summary>
    /// Load marker list xml from application storage.
    /// </summary>
    private void _LoadMarkerFromDisk()
    {
        // Attempt to load the exsiting markers from storage.
        string path = Application.persistentDataPath + "/" + m_curAreaDescription.m_uuid + ".xml";

        var serializer = new XmlSerializer(typeof(List<MarkerData>));
        var stream = new FileStream(path, FileMode.Open);

        List<MarkerData> xmlDataList = serializer.Deserialize(stream) as List<MarkerData>;

        if (xmlDataList == null)
        {
            Debug.Log("AndroidInGameController._LoadMarkerFromDisk(): xmlDataList is null");
            return;
        }

        // Load marker data from XML file
        m_markerList.Clear();
        foreach (MarkerData mark in xmlDataList)
        {
            // Instantiate all markers' gameobject.
            GameObject temp_instance = Instantiate(m_markPrefabs[mark.m_type],
                                          mark.m_position,
                                          mark.m_orientation) as GameObject;

            // Set other values
            temp_instance.GetComponent<ARMarker>().setAllParameters(mark.m_id, mark.m_neighbours);

            // Add re-created marker to list
            int temp_id = temp_instance.GetComponent<ARMarker>().getID();
            m_markerList.Add(temp_id, temp_instance);
        }

        // Draw connections between markers
        foreach (KeyValuePair<int, GameObject> marker in m_markerList)
        {
            // Draw all lines to neighbours
            int[] neighboursIds = marker.Value.GetComponent<ARMarker>().getOldNeighboursIds();
            int capacity = neighboursIds.Length;
            GameObject neighbour;

            for (int i = 0; i < capacity; i++)
            {
                if (m_markerList.TryGetValue(neighboursIds[i], out neighbour))
                {
                    marker.Value.GetComponent<ARMarker>().addLine(neighboursIds[i], neighbour.transform.position);
                }
            }
        }
    }

    /// <summary>
    /// Convert a 3D bounding box represented by a <c>Bounds</c> object into a 2D 
    /// rectangle represented by a <c>Rect</c> object.
    /// </summary>
    /// <returns>The 2D rectangle in Screen coordinates.</returns>
    /// <param name="cam">Camera to use.</param>
    /// <param name="bounds">3D bounding box.</param>
    private Rect _WorldBoundsToScreen(Camera cam, Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        Bounds screenBounds = new Bounds(cam.WorldToScreenPoint(center), Vector3.zero);
        
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, +extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, +extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, -extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, -extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, +extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, +extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, -extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, -extents.y, -extents.z)));
        return Rect.MinMaxRect(screenBounds.min.x, screenBounds.min.y, screenBounds.max.x, screenBounds.max.y);
    }

    /// <summary>
    /// Wait for the next depth update, then find the plane at the touch position.
    /// </summary>
    /// <returns>Coroutine IEnumerator.</returns>
    /// <param name="touchPosition">Touch position to find a plane at.</param>
    private IEnumerator _WaitForDepthAndFindPlane(Vector2 touchPosition)
    {
        m_findPlaneWaitingForDepth = true;
        
        // Turn on the camera and wait for a single depth update.
        m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
        while (m_findPlaneWaitingForDepth)
        {
            yield return null;
        }

        m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
        
        // Find the plane.
        Camera cam = Camera.main;
        Vector3 planeCenter;
        Plane plane;
        if (!m_pointCloud.FindPlane(cam, touchPosition, out planeCenter, out plane))
        {
            yield break;
        }
        
        // Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
        Vector3 up = plane.normal;
        Vector3 forward;
        if (Vector3.Angle(plane.normal, cam.transform.forward) < 175)
        {
            Vector3 right = Vector3.Cross(up, cam.transform.forward).normalized;
            forward = Vector3.Cross(right, up).normalized;
        }
        else
        {
            // Normal is nearly parallel to camera look direction, the cross product would have too much
            // floating point error in it.
            forward = Vector3.Cross(up, cam.transform.right);
        }

        // Instantiate marker object.
        lastCreatedMarker = newMarkObject;
        newMarkObject = Instantiate(m_markPrefabs[m_currentMarkType],
                                    planeCenter,
                                    Quaternion.LookRotation(forward, up)) as GameObject;

        ARMarker markerScript = newMarkObject.GetComponent<ARMarker>();
        int newMarker_id = markerScript.getID();

        Debug.Log("#CHECK - new marker id: " + newMarker_id);

        markerScript.m_type = m_currentMarkType;
        markerScript.m_timestamp = (float) m_poseController.m_poseTimestamp;
        
        Matrix4x4 uwTDevice = Matrix4x4.TRS(m_poseController.m_tangoPosition,
                                            m_poseController.m_tangoRotation,
                                            Vector3.one);
        Matrix4x4 uwTMarker = Matrix4x4.TRS(newMarkObject.transform.position,
                                            newMarkObject.transform.rotation,
                                            Vector3.one);
        markerScript.m_deviceTMarker = Matrix4x4.Inverse(uwTDevice) * uwTMarker;

        m_markerList.Add(newMarker_id, newMarkObject);

        m_selectedMarker = null;

        // Create independent marker
        if (m_currentMethodType == 2)
        {
            yield break;
        }

        // Created first marker => can not create connection
        if (lastCreatedMarker != null)
        {
            ARMarker lastTmp = lastCreatedMarker.GetComponent<ARMarker>();
            ARMarker newTmp = newMarkObject.GetComponent<ARMarker>();
            lastTmp.addLine(newTmp.getID(), newTmp.transform.position);
        }
    } 

    public Dictionary<int, GameObject> getMarkerList()
    {
        return m_markerList;
    }

    public Graph getGraph()
    {
        return graph;
    }

    public Vector3 getCurrentPosition()
    {
        return m_poseController.m_tangoPosition;
    }

    public void addLineWithIds(int id1, int id2)
    {
        GameObject go1;
        GameObject go2;

        m_markerList.TryGetValue(id1, out go1);
        m_markerList.TryGetValue(id2, out go2);

        ARMarker marker1 = go1.GetComponent<ARMarker>();
        ARMarker marker2 = go2.GetComponent<ARMarker>();

        marker1.addLine(marker2.getID(), marker2.transform.position);
    }

    public void toggleNavigationScene()
    {
        isNavigation = !isNavigation;
    }

    public bool getIsNavigation()
    {
        return isNavigation;
    }

    public void disableAllMarkers()
    {
        foreach (KeyValuePair<int, GameObject> obj in m_markerList)
        {
            foreach (KeyValuePair<int, GameObject> line in obj.Value.GetComponent<ARMarker>().lines)
            {
                line.Value.SetActive(false);
            }
            obj.Value.SetActive(false);
        }
    }

    public void showNavigationMarkers(int [] shortestPath)
    {
        for (int i = 0; i < shortestPath.Length; i++ )
        {
            GameObject tmpMarker = null;
            if (m_markerList.TryGetValue(shortestPath[i], out tmpMarker))
            {
                // enable marker in shoted path
                tmpMarker.SetActive(true);
                for (int j = 0; j < shortestPath.Length; j++)
                {
                    GameObject tmpRenderer = null;
                    if (tmpMarker.GetComponent<ARMarker>().lines.TryGetValue(shortestPath[j], out tmpRenderer))
                    {
                        tmpRenderer.SetActive(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Data container for marker.
    /// 
    /// Used for serializing/deserializing marker to xml.
    /// </summary>
    [System.Serializable]
    public class MarkerData
    {
        /// <summary>
        /// Marker's type.
        /// 
        /// Red, green or blue markers. In a real game scenario, this could be different game objects
        /// (e.g. banana, apple, watermelon, persimmons).
        /// </summary>
        [XmlElement("type")]
        public int m_type;
        
        /// <summary>
        /// Position of the this mark, with respect to the origin of the game world.
        /// </summary>
        [XmlElement("position")]
        public Vector3 m_position;
        
        /// <summary>
        /// Rotation of the this mark.
        /// </summary>
        [XmlElement("orientation")]
        public Quaternion m_orientation;

        /// <summary>
        /// Arrays of neighbours of current marker
        /// </summary>
        [XmlElement("neighbours")]
        public int[] m_neighbours;

        /// <summary>
        /// ID of current marker.
        /// </summary>
        [XmlElement("id")]
        public int m_id;
    }
}
