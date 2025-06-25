
using System;
using UnityEngine;
public class ConnectionPoint
{
    public bool isConnection;
    public bool isEnable;
    public Rect rect;
    public ConnectionPointType type;
    public AINode node;
    public GUIStyle style;
    public GUIStyle selectStyle;
    public GUIStyle defaultStyle;
    public Action<ConnectionPoint> OnClickConnectionPoint;
    public ConnectionPoint(AINode node, ConnectionPointType type, GUIStyle style, GUIStyle selectStyle, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        isEnable = true;
        this.node = node;
        this.type = type;
        this.style = style;
        defaultStyle = style;
        this.selectStyle = selectStyle;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 10f);
        isConnection = false;
    }
    public void Draw()
    {
        if (isEnable)
        {
            if (isConnection)
            {
                style = selectStyle;
            }
            else
            {
                style = defaultStyle;
            }
            rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;
            switch (type)
            {
                case ConnectionPointType.In:
                    rect.x = node.rect.x - rect.width;
                    break;
                case ConnectionPointType.Out:
                    rect.x = node.rect.x + node.rect.width;
                    break;
            }
            if (GUI.Button(rect, "", style))
            {
                if (OnClickConnectionPoint != null)
                {
                    OnClickConnectionPoint(this);
                }
            }
        }

    }
}