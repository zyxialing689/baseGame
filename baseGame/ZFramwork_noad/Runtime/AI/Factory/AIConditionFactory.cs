public static class AIConditionFactory
{
    public static IAICondition Create(AIConditionType type)
    {
        switch (type)
        {
            case AIConditionType.WaitTime:
                return new TimeCondition();

            case AIConditionType.Distance:
                return new DistanceCondition();

            default:
                return new TrueCondition();
        }
    }
}