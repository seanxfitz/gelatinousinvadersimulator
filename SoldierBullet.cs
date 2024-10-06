using Fractural.Tasks;
using Godot;
using System;
using System.Threading;

public partial class SoldierBullet : Area2D
{
    Vector2 Direction;
    [Export] int speedMS = 30;
    [Export] float distance = 40;

    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public override void _EnterTree()
    {
        base._EnterTree();
        AreaEntered += HandleAreaEntered;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        cancellationTokenSource.Cancel();
    }

    public void Initialize(Vector2 direction)
    {
        Direction = direction;
        Fire().Forget();
    }

    private async GDTask Fire()
    {
        var startingPosition = Position;

        while(!cancellationTokenSource.IsCancellationRequested && startingPosition.DistanceSquaredTo(Position) < distance * distance)
        {
            Position += Direction;
            await GDTask.Delay(speedMS, PlayerLoopTiming.Process, cancellationTokenSource.Token);
        }
        QueueFree();
    }

    private void HandleAreaEntered(Area2D other)
    {
        cancellationTokenSource.Cancel();

        if (other is PlayerDamageable damageable)
        {
            damageable.TakeDamage(1);
        }

        QueueFree();
    }
}
