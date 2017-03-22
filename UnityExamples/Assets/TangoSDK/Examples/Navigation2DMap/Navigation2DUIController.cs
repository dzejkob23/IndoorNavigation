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

    private Dictionary<int, Vector3> markersList;

    private Vector2 shiftXY;
    private const int SCALE_XY = 200;
    private const int BOUD_SCALED_XY = 100;


    public Texture2D texture;


    public void Start()
    {
        markersList = AreaLearningInGameController.Instance.markerListNewScene;
        findMinXY();
    }

    /// <summary>
    /// Scene switching GUI.
    /// </summary>
    private void OnGUI()
    {
        
        if (markersList == null || markersList.Keys.Count == 0 || markersList.Values.Count == 0)
        {
            AndroidHelper.ShowAndroidToastMessage("List of markers is EMPTY!");
            return;
        }

        foreach (KeyValuePair<int, Vector3> marker in markersList)
        {
            if (marker.Value == null)
            {
                AndroidHelper.ShowAndroidToastMessage("value");
                break;
            }
            
            Vector3 tmpVect = marker.Value;

            int x = (int) (((marker.Value.x - shiftXY.x) * SCALE_XY) + BOUD_SCALED_XY);
            int z = (int) (((marker.Value.z - shiftXY.y) * SCALE_XY) + BOUD_SCALED_XY);

            AndroidHelper.ShowAndroidToastMessage("ID_" + marker.Key.ToString() + ": " + x + " " + z);

            drawReactangle(marker.Key.ToString(), x, z);
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

    private void findMinXY()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;

        foreach (KeyValuePair<int, Vector3> marker in markersList)
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

        shiftXY = new Vector2(minX, minY);
    }
}
