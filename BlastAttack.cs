using Fractural.Tasks;
using Godot;
using System;
using System.Threading;
using System.Linq;
public partial class BlastAttack : Area2D
{

    [Export] int speedMS = 32;
    [Export] AnimatedSprite2D animatedSprite;
    [Export] Area2D DetonationArea;
    [Export] AudioStreamPlayer2D streamPlayer;
    Absorbable target;
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    int detonationMaskValue;
    public override void _EnterTree()
    {
        base._EnterTree();
        AreaEntered += HandleAreaEntered;
        streamPlayer.Finished += () => streamPlayer.Play();
    }

    public void Initialize(Absorbable target)
    {
        if (target.GetCollisionLayerValue(9))
        {
            detonationMaskValue = 9;
        } 
        else
        {
            detonationMaskValue = 10;
        }
        this.target = target;
        Fire().Forget();
    }

    private async GDTask Fire()
    {

        while (IsInstanceValid(target) && cancellationTokenSource.IsCancellationRequested == false)
        {
            var dir = Position.DirectionTo(target.Position).Normalized();
            Position += dir;
            await GDTask.Delay(speedMS, PlayerLoopTiming.Process, cancellationTokenSource.Token);
        }

        if (cancellationTokenSource.IsCancellationRequested == false)
        {
            Detonate();
        }
    }

    private void HandleAreaEntered(Area2D other)
    {
        if (other == target)
        {
            Detonate();
        }
    }

    private void Detonate()
    {
        cancellationTokenSource.Cancel();

        animatedSprite.SelfModulate = Colors.Transparent;

        for (int i = 0; i < 4; i++)
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

        if (areas.Any())
        {
            GD.Print("Got areas " + areas.Count);
            foreach (var damageable in areas.Where(area => area.GetCollisionLayerValue(detonationMaskValue) && area is Absorbable).Select(area => area as Absorbable))
            {
                damageable.TickAbsorb(100);
            }
        }
    }
}
