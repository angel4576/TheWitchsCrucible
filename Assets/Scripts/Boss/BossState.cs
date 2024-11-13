public abstract class BossState
{
    protected Boss currentBoss;
    public abstract void OnEnter(Boss boss);
    public abstract void LogicUpdate();
    public abstract void PhysicsUpdate();
    public abstract void OnExit();
}
