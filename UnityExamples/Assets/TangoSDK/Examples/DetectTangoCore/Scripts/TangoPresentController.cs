﻿//-----------------------------------------------------------------------
// <copyright file="TangoPresentController.cs" company="Google">
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
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enables and disables GameObjects based on whether Project Tango is available and up-to-date.
/// </summary>
public class TangoPresentController : MonoBehaviour
{
    /// <summary>
    /// Download Tango Core.
    /// </summary>
    public GameObject download;

    /// <summary>
    /// Start app button.
    /// </summary>
    public GameObject start;

    /// <summary>
    /// When playing in the editor, whether this script will act as if Project Tango is
    /// present, out of date, or not present.
    /// </summary>
    public TangoState m_editorPlayModeState;

    /// <summary>
    /// List of GameObjects that will be enabled if Tango is present and up to date and
    /// disabled otherwise.
    /// </summary>
    public GameObject[] m_enableIfTangoPresent;

    /// <summary>
    /// List of GameObjects that will be enabled if Tango is present but not up to date
    /// and disabled otherwise.
    /// </summary>
    public GameObject[] m_enableIfTangoOutOfDate;

    /// <summary>
    /// List of GameObjects that will be disabled if Tango is present and enabled 
    /// otherwise.
    /// </summary>
    public GameObject[] m_disableIfTangoPresent;

    /// <summary>
    /// Different install states Tango can be in.
    /// </summary>
    public enum TangoState
    {
        NotPresent,
        OutOfDate,
        Present
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    public void Awake()
    {
        TangoState tangoState;

#if UNITY_EDITOR
        tangoState = m_editorPlayModeState;
#else
        if (!AndroidHelper.IsTangoCorePresent())
        {
            tangoState = TangoState.NotPresent;
            download.SetActive(true);
        }
        else if (!AndroidHelper.IsTangoCoreUpToDate())
        {
            tangoState = TangoState.OutOfDate;
            download.SetActive(true);
        }
        else
        {
            tangoState = TangoState.Present;
            start.SetActive(true);
        }
#endif

        foreach (GameObject obj in m_enableIfTangoPresent)
        {
            obj.SetActive(tangoState == TangoState.Present);
        }

        foreach (GameObject obj in m_enableIfTangoOutOfDate)
        {
            obj.SetActive(tangoState == TangoState.OutOfDate);
        }

        foreach (GameObject obj in m_disableIfTangoPresent)
        {
            obj.SetActive(tangoState == TangoState.NotPresent);
        }
    }
}
