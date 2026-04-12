public static class AIStateFactory
{
    public static AIState Create(AIStateType type)
    {
        switch (type)
        {
            case AIStateType.Idle:
                return new IdleState();

            case AIStateType.Move:
                return new MoveState();
            case AIStateType.Attack:
                return new AttackState();

            default:
                return null;
        }
    }
}