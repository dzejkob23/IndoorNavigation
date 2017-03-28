using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ARCube : MonoBehaviour
{
    private void OnMouseEnter()
    {
        String tmp = gameObject.GetComponentInChildren<TextMesh>().text;
        Debug.Log("#CHECK ... TOUCH TOUCH TOUCH ... " + tmp);
    }
}
