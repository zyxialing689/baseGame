using System.Collections.Generic;

public class AIStateConfig
{
    public int id;
    public AIStateType stateType;

    public List<AITransition> transitions = new List<AITransition>();
}