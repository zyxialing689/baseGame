using System;
using System.Collections.Generic;

[Serializable]
public class AIEditorData
{
    public List<AIEditorNode> nodes = new List<AIEditorNode>();
    public List<AIEditorConnection> connections = new List<AIEditorConnection>();
}