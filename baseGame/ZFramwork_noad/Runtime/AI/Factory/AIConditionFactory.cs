public static class AIConditionFactory
{
    public static IAICondition Create(AIConditionType type)
    {
        switch (type)
        {
            case AIConditionType.HasTarget:
                return new HasTargetCondition();

            case AIConditionType.True:
            default:
                return new TrueCondition();
        }
    }
}