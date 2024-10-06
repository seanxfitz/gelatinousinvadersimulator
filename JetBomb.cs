using Fractural.Tasks;
using Godot;
using System.Linq;
using System.Threading;

public partial class JetBomb : Area2D
{
    [Export] Area2D DetonationArea;
    [Export] AnimatedSprite2D AnimatedSprite;
    [Export] AudioStreamPlayer2D streamPlayer;
    CancellationTokenSource cts;
    Vector2 Direction;
    int moveSpeedMS = 32;
    public override void _EnterTree()
    {
        base._EnterTree();
        AreaEntered += HandleAreaEntered;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        cts?.Cancel();
    }

    public void Initialize(Vector2 target)
    {
        Direction = Position.DirectionTo(target).Normalized();
        if (Direction.X < 0)
        {
            AnimatedSprite.FlipH = true;
        }

        Fire().Forget();
    }

    private async GDTask Fire()
    {
        cts = new CancellationTokenSource();
        while(cts.IsCancellationRequested == false)
        {
            Position += Direction;
            await GDTask.Delay(moveSpeedMS, PlayerLoopTiming.Process, cts.Token);
        }
    }

    private void HandleAreaEntered(Area2D other)
    {
        Detonate();
    }

    private void Detonate()
    {
        cts.Cancel();

        AnimatedSprite.Play("Detonation");

        for (int i = 0; i < 8; i++)
        {
            var explosion = Scenes.Explosion;
            AddChild(explosion);
            explosion.Position = new Vector2(RNG.Generation.GetInRange(-16, 16), RNG.Generation.GetInRange(-16, 16));
            if (i == 0)
            {
                explosion.AnimationFinished += () =>
                {
                    QueueFree();
                };
            }
        }


        var areas = DetonationArea.GetOverlappingAreas();

        if (areas.Count > 0)
        {
            foreach (var damageable in areas.Where(area => area is PlayerDamageable).Select(area => area as PlayerDamageable))
            {
                damageable.TakeDamage(10);
            }
        }
        streamPlayer.Play();
    }
}
