using System.Collections.Generic;

public class AIStateConfig
{
    public int id;
    public string stateType;

    public List<AITransition> transitions = new List<AITransition>();
}