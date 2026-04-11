public static class AIStateFactory
{
    public static AIState Create(string type)
    {
        switch (type)
        {
            case "Idle":
                return new IdleState();

            case "Move":
                return new MoveState();

            default:
                return null;
        }
    }
}