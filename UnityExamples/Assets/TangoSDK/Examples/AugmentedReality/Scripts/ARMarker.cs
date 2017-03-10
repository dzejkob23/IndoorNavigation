//-----------------------------------------------------------------------
// <copyright file="ARMarker.cs" company="Google">
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Location marker script to show hide/show animations.
///
/// Instead of calling destroy on this, send the "Hide" message.
/// </summary>
public class ARMarker : MonoBehaviour
{
    public LineRenderer line0;
    public LineRenderer line1;
    public LineRenderer line2;
    public LineRenderer line3;

    private byte [] isFull = {0, 0, 0, 0};
    private int[] whichMarker = { -1, -1, -1, -1};

    /// <summary>
    /// Unique identifikator
    /// </summary>
    private static int ID = 0;

    /// <summary>
    /// Marker neighbours.
    /// </summary>
    public List<int> m_listNeighbours;

    /// <summary>
    /// The type of the location mark.
    /// 
    /// This field is used in the Area Learning example for identify the marker type.
    /// </summary>
    public int m_type = 0;

    /// <summary>
    /// The Tango time stamp when this object is created
    /// 
    /// This field is used in the Area Learning example, the timestamp is save for the position adjustment when the
    /// loop closure happens.
    /// </summary>
    public float m_timestamp = -1.0f;

    /// <summary>
    /// The marker's transformation with respect to the device frame.
    /// </summary>
    public Matrix4x4 m_deviceTMarker = new Matrix4x4();

    /// <summary>
    /// The animation playing.
    /// </summary>
    private Animation m_anim;

    /// <summary>
    /// Start this instance
    /// </summary>
    public void Start()
    {
        ID++;
    }

    /// <summary>
    /// Awake this instance.
    /// </summary>
    private void Awake()
    {
        // The animation should be started in Awake and not Start so that it plays on its first frame.
        m_anim = GetComponent<Animation>();
        m_anim.Play("ARMarkerShow", PlayMode.StopAll);
    }

    /// <summary>
    /// Plays an animation, then destroys.
    /// </summary>
    private void Hide()
    {
        m_anim.Play("ARMarkerHide", PlayMode.StopAll);
    }

    /// <summary>
    /// Callback for the animation system.
    /// </summary>
    private void HideDone()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Get instance ID
    /// </summary>
    /// <returns>Instance ID</returns>
    public int getID()
    {
        return ID;
    }

    public bool addLine(int markerId)
    {
        int it;

        if ((it = isEmptyRenderer()) == -1)
        {
            return false;
        }

        switch(it)
        {
            case 0:
                line0 = gameObject.AddComponent<LineRenderer>();
                lineSetup(line0);
                break;
            case 1:
                line1 = gameObject.AddComponent<LineRenderer>();
                lineSetup(line1);
                break;
            case 2:
                line2 = gameObject.AddComponent<LineRenderer>();
                lineSetup(line2);
                break;
            case 3:
                line3 = gameObject.AddComponent<LineRenderer>();
                lineSetup(line3);
                break;
            default:
                return false;
        }

        isFull[it]++;
        whichMarker[it] = markerId;

        return true;
    }

    public bool deleteMarkerLine(int markerId)
    {
        int idOut;

        // markerId is not in neighbours - ret false
        if (!m_listNeighbours.Contains(markerId))
        {
            return false;
        }

        // is markerId in connection with some of renderers of lines
        if (!isExistMarkerId(markerId, out idOut))
        {
            return false;
        }

        m_listNeighbours.Remove(markerId);

        switch (idOut)
        {
            case 0:
                Destroy(line0);
                break;
            case 1:
                Destroy(line1);
                break;
            case 2:
                Destroy(line2);
                break;
            case 3:
                Destroy(line3);
                break;
            default:
                return false;
        }

        isFull[idOut]--;
        whichMarker[idOut] = -1;

        return true;
    }

    private int isEmptyRenderer()
    {
        for (int i = 0; i < isFull.Length; i++)
        {
            if (isFull[i] != 1)
            {
                return i;
            }
        }

        return -1;
    }

    private bool isExistMarkerId(int idIn, out int idOut)
    {
        idOut = -1;

        for (int i = 0; i < whichMarker.Length; i++)
        {
            if (idIn == whichMarker[i])
            {
                idOut = i;
                return true;
            }
        }

        return false;
    }

    private void lineSetup(LineRenderer line)
    {
        line.sortingLayerName = "OnTop";
        line.sortingOrder = 5;
        line.numPositions = 2;
        line.startWidth = 0.5f;
        line.endWidth = 0.5f;
        line.useWorldSpace = true;
    }
}
