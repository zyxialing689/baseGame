using System;
using System.Collections.Generic;
using UnityEngine;

public class PathRequest
{
    public Vector3 start;
    public Vector3 end;
    public Action<List<Vector3>> callback;

    public PathRequest(Vector3 start, Vector3 end, Action<List<Vector3>> callback)
    {
        this.start = start;
        this.end = end;
        this.callback = callback;
    }
}