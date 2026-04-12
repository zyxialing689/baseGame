using System;
using UnityEngine;

[Serializable]
public class AIEditorConnection
{
    public int fromNodeId;
    public int toNodeId;
    public int conditionType;
    public Vector4 param;       // ⭐ 必须有
}