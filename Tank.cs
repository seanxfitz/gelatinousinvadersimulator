using Fractural.Tasks;
using Godot;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

public partial class Tank : Absorbable
{

    enum State
    {
        waiting, approaching, attacking, underAttack, dying
    }

    State currentState = State.waiting;

    CancellationTokenSource ApproachCancellationTokenSource;
    CancellationTokenSource AttackCancellationTokenSource;

    Vector2 approachTarget;
    [Export] public int moveSpeedMS = 280;

    private int size = 8;
    float minShellDistance = 20;
    public override int Size
    {
        get { return size; }
        protected set { size = value; }
    }
    int attackWait = 400;
    int attackSpeedMS = 3750;

    HashSet<Blob> attachedBlobs = new HashSet<Blob>();

    Area2D VisionRadius;
    AnimatedSprite2D animatedSprite;

    [Export] AudioStream explosion;
    [Export] AudioStream shot;

    public override void _EnterTree()
    {
        base._EnterTree();
        base.health = 16;
        VisionRadius = GetNode<Area2D>("VisionRadius");
        VisionRadius.AreaEntered += HandleVisionRadiusEntered;
        animatedSprite = GetNode<AnimatedSprite2D>("Sprite2D");

        MouseEntered += HandleMouseEntered;
        MouseExited += HandleMouseExited;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        switch (currentState)
        {
            case State.waiting:
                ApproachPlayer().Forget();
                break;
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ApproachCancellationTokenSource?.Cancel();
        AttackCancellationTokenSource?.Cancel();
    }

    private void HandleMouseEntered()
    {
        The.Player.BlastTarget = this;
    }

    private void HandleMouseExited()
    {
        if(The.Player.BlastTarget == this)
        {
            The.Player.BlastTarget = null;
        }
    }

    private async GDTask ApproachPlayer()
    {
        ApproachCancellationTokenSource = new CancellationTokenSource();
        currentState = State.approaching;
        approachTarget = The.Player.Position;
        var direction = Position.DirectionTo(approachTarget);
        if (direction.X < 0)
        {
            animatedSprite.FlipH = true;
        }

        while (currentState == State.approaching && ApproachCancellationTokenSource.IsCancellationRequested == false)
        {
            direction = Position.DirectionTo(approachTarget);
            Position += direction.Normalized();
            await GDTask.Delay(moveSpeedMS, PlayerLoopTiming.Process, ApproachCancellationTokenSource.Token);
        }
    }

    private void HandleVisionRadiusEntered(Node2D other)
    {
        if (IsPlayerVisible() == false) return;
        if (currentState == State.underAttack || currentState == State.attacking) { return; }

        TryAttackTarget().Forget();
    }

    private async GDTask TryAttackTarget()
    {
        currentState = State.attacking;
        AttackCancellationTokenSource = new CancellationTokenSource();

        await GDTask.Delay(attackWait, PlayerLoopTiming.Process, AttackCancellationTokenSource.Token);

        if (AttackCancellationTokenSource.Token.IsCancellationRequested) { return; }
        if (currentState == State.underAttack) { return; }

        var targets = VisionRadius.GetOverlappingAreas().Where(area => 
                                                                IsInstanceValid(area) &&
                                                                area is PlayerDamageable &&
                                                                Position.DistanceSquaredTo(area.Position) > Mathf.Pow(minShellDistance, 2));
        if (targets.Any() == false)
        {
            currentState = State.waiting;
            return;
        }

        var target = RNG.Combat.RandomPick(targets);

        var shotDirection = Position.DirectionTo(target.Position).Normalized();
        var shell = Scenes.TankShell;
        The.Environment.AddChild(shell);
        shell.Position = this.Position + (shotDirection * 8);
        shell.Initialize(shotDirection);
        audioPlayer.Stream = shot;
        audioPlayer.Play();
        attackWait = attackSpeedMS;
        TryAttackTarget().Forget();
    }

    #region Absorbable

    public override Vector2 AttachBlob(Blob blob)
    {
        currentState = State.underAttack;
        attachedBlobs.Add(blob);

        switch (attachedBlobs.Count) {
            case 1:
                return Position + new Vector2(-4, 4);
            case 2:
                return Position + new Vector2(4, 4);
            case 3:
                return Position + new Vector2(-4, -1);
            default:
                return Position + new Vector2(4, 1);
        }
    }

    public override Vector2 NextBlobPosition()
    {
        switch (attachedBlobs.Count)
        {
            case 0:
                return Position + new Vector2(-4, 4);
            case 1:
                return Position + new Vector2(4, 4);
            case 2:
                return Position + new Vector2(-4, -1);
            default:
                return Position + new Vector2(4, 1);
        }
    }

    public override void DetachBlob(Blob blob)
    {
        if (attachedBlobs.Remove(blob))
        {
            currentState = State.waiting;
        }
    }

    public override bool CanAttachBlob()
    {
        return attachedBlobs.Count < 4;
    }

    public override bool TickAbsorb(int rate)
    {
        health -= rate;
        if (health <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    #endregion

    bool dying = false;
    private void Die()
    {
        if (dying)
        {
            return;
        }
        audioPlayer.Stream = explosion;
        audioPlayer.VolumeDb = -4;
        audioPlayer.Play();
        dying = true;
        VisionRadius.Monitoring = false;
        SetDeferred("Monitorable", false);

        EmitSignal(SignalName.WillDie);

        animatedSprite.Play("Death");
        AddChild(Scenes.Explosion);
        animatedSprite.AnimationFinished += () =>
        {
            QueueFree();
        };
    }
}
