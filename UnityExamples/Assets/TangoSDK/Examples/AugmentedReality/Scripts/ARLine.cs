using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ARLine : MonoBehaviour
{
    private int idM1;
    private int idM2;
    public LineRenderer renderer;



    public void setupLine(int id1, int id2, LineRenderer lRenderer)
    {
        idM1 = id1;
        idM2 = id2;
        renderer = lRenderer;
    }

    public bool containsId(int id)
    {
        if (id != idM1 && id != idM2)
        {
            return false;
        }

        return true;
    }

    public LineRenderer getRenderer()
    {
        return renderer;
    }

}
