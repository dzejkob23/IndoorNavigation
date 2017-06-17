using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class KeyPair
{
    public int [] keys { get; private set; }

    public KeyPair(int first, int second)
    {
        keys = new int[2];
        keys[0] = first;
        keys[1] = second; 
    }
}