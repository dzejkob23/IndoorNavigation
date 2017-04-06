using System;
using System.Collections;
using System.Collections.Generic;
using Tango;
using UnityEngine;

public class Navigation3DUIController : MonoBehaviour, ITangoLifecycle
{
    private TangoApplication m_tangoApplication;

    // Use this for initialization
    void Start()
    {
        m_tangoApplication = FindObjectOfType<TangoApplication>();
        if (m_tangoApplication != null)
        {
            m_tangoApplication.Register(this);
            m_tangoApplication.RequestPermissions();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        if (permissionsGranted)
        {
            AreaDescription[] list = AreaDescription.GetList();
            AreaDescription tmp = null;
            AreaDescription.Metadata tmpMetadata = null;

            if (list.Length > 0)
            {
                tmp = list[0];
                tmpMetadata = tmp.GetMetadata();

                foreach (AreaDescription area in list)
                {
                    AreaDescription.Metadata metadata = area.GetMetadata();

                    if (metadata.m_dateTime > tmpMetadata.m_dateTime)
                    {
                        tmp = area;
                        tmpMetadata = metadata;
                    }
                }

                m_tangoApplication.Startup(tmp);
            }
            else
            {
                // No Area Description
                String message = "There is not any are description!";

                Debug.Log(message);
                AndroidHelper.ShowAndroidToastMessage(message);
            }
        }
    }

    public void OnTangoServiceConnected()
    {
        Debug.Log("Used method: OnTangoServiceConnected ... empty method body");
    }

    public void OnTangoServiceDisconnected()
    {
        Debug.Log("Used method: OnTangoServiceDisconnected ... empty method body");
    }
}
