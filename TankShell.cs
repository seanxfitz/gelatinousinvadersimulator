using Fractural.Tasks;
using Godot;
using System.Drawing;
using System.Linq;
using System.Threading;

public partial class TankShell : Area2D
{
    Vector2 Direction;
    [Export] int speedMS = 25;
    [Export] float distance = 60;
    [Export] AnimatedSprite2D animatedSprite;
    [Export] Area2D DetonationArea;
    [Export] AudioStreamPlayer2D streamPlayer;

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

        while (!cancellationTokenSource.Token.IsCancellationRequested && startingPosition.DistanceSquaredTo(Position) < distance * distance)
        {
            Position += Direction;
            await GDTask.Delay(speedMS, PlayerLoopTiming.Process, cancellationTokenSource.Token);
        }
        Detonate();
    }

    private void HandleAreaEntered(Area2D other)
    {
        Detonate();
    }

    private void Detonate()
    {
        cancellationTokenSource.Cancel();

        animatedSprite.SelfModulate = Colors.Transparent;

        for(int i = 0; i < 4; i ++)
        {
            var explosion = Scenes.Explosion;
            AddChild(explosion);
            explosion.Position = new Vector2(RNG.Generation.GetInRange(-8, 8), RNG.Generation.GetInRange(-8, 8));
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
                damageable.TakeDamage(4);
            }
        }
        streamPlayer.Play();
    }
}
