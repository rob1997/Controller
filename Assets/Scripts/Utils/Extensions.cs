using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 GetXz(this Vector3 value)
    {
        value.y = 0;

        return value;
    }
    
    public static Vector3 GetYz(this Vector3 value)
    {
        value.x = 0;

        return value;
    }
    
    public static Vector3 GetXy(this Vector3 value)
    {
        value.z = 0;

        return value;
    }
}
