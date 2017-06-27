//-----------------------------------------------------------------------
// <copyright file="PinchZoom.cs" author="Jakub Zíka" student="A15N0087P">
//
// License is according University of West Bohemmia licenses.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// PinchZoom gives using touches on screen.
/// </summary>
class PinchZoom : MonoBehaviour
{
    /// <summary>
    /// Prespective camera zoom speed.
    /// </summary>
    private static float PERSPECTIVE_ZOOM_SPEED = 1;        // The rate of change of the field of view in perspective mode.
    /// <summary>
    /// Orthographic camera zoom speed.
    /// </summary>
    private static float ORTHOGRPHIC_ZOOM_SPEED = 1;        // The rate of change of the orthographic size in orthographic mode.
    /// <summary>
    /// Orthographic camera draging speed.
    /// </summary>
    private static float ORTHOGRAPHIC_DRAG_SPEED = 2;
    /// <summary>
    /// Indicator of relocalization using.
    /// </summary>
    public bool isUsingRelocalization = true;
    /// <summary>
    /// Start point in 2D scene for position indicator.
    /// </summary>
    private Vector3 worldStartPoint;

    /// <summary>
    /// Update method.
    /// </summary>
    void Update()
    {
        if (Camera.main == null)
        {
            return;
        }

        _CameraMove();
        _CameraZoom();
    }

    /// <summary>
    /// Camera moving.
    /// </summary>
    private void _CameraMove()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        // zkusit pouze podminku "if (Input.touchCount == 1)" - chvalne, co to udela
        {
            // Move camera
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            transform.Translate(-touchDeltaPosition.x * ORTHOGRAPHIC_DRAG_SPEED, -touchDeltaPosition.y * ORTHOGRAPHIC_DRAG_SPEED, 0);

            // Stop using relocalization - mobing only with touching the screen
            isUsingRelocalization = false;
        }
    }

    /// <summary>
    /// Camera zooming.
    /// </summary>
    private void _CameraZoom()
    {
        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // If the camera is orthographic...
            if (Camera.main.orthographic)
            {
                // ... change the orthographic size based on the change in distance between the touches.
                Camera.main.orthographicSize += deltaMagnitudeDiff * ORTHOGRPHIC_ZOOM_SPEED;

                // Make sure the orthographic size never drops below zero.
                Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize, 0.1f);
            }
            else
            {
                // Otherwise change the field of view based on the change in distance between the touches.
                Camera.main.fieldOfView += deltaMagnitudeDiff * PERSPECTIVE_ZOOM_SPEED;

                // Clamp the field of view to make sure it's between 0 and 180.
                Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 0.1f, 179.9f);
            }
        }
    }
}
