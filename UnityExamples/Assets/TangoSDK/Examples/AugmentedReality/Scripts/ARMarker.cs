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
    /// <summary>
    /// Renders for this marker
    /// </summary>
    private Dictionary<int, LineRenderer> renderers;

    /// <summary>
    /// Unique identifikator
    /// </summary>
    private static int ID = 0;

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
        renderers = new Dictionary<int, LineRenderer>();
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

    public void addLine(int markerId, Vector3 markerPosition)
    {
        LineRenderer renderer = null;
        lineSetup(out renderer, gameObject.transform.position, markerPosition);
        renderers.Add(markerId, renderer);
    }

    public bool deleteLineToMarker(int markerId)
    {
        // markerId is not in neighbours - ret false
        if (!renderers.ContainsKey(markerId))
        {
            return false;
        }

        renderers.Remove(markerId);

        return true;
    }

    private void lineSetup(out LineRenderer line, Vector3 originalP, Vector3 nextP)
    {
        line = gameObject.AddComponent<LineRenderer>();

        line.sortingLayerName = "OnTop";
        line.sortingOrder = 5;

        // transform
        line.numPositions = 2;
        line.SetPosition(0, originalP);
        line.SetPosition(1, nextP);
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.useWorldSpace = true;
    }
}
