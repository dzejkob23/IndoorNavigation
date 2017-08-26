//-----------------------------------------------------------------------
// <copyright file="KeyPair.cs" author="Jakub Zíka" student="A15N0087P">
//
// License is according University of West Bohemmia licenses.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// KeyPair initialize primary keys which represent marker on one line.
/// </summary>
public class KeyPair
{
    /// <summary>
    /// Array with keys
    /// </summary>
    public int [] keys { get; private set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="first">First key.</param>
    /// <param name="second">Second key.</param>
    public KeyPair(int first, int second)
    {
        keys = new int[2];
        keys[0] = first;
        keys[1] = second; 
    }
}