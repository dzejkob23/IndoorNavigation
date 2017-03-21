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

    public Texture2D texture;

    /// <summary>
    /// The names of all the scenes this can switch between.
    /// </summary>
    private readonly string[] m_sceneNames =
    {
        
    };

    /// <summary>
    /// Scene switching GUI.
    /// </summary>
    private void OnGUI()
    {
        drawReactangle("1", 100, 100);
        drawReactangle("2", 500, 100);
        drawReactangle("3", 500, 500);
        drawReactangle("4", 100, 500);
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
        AndroidHelper.ShowAndroidToastMessage("Dement cislo: " + name);
    }

}
