using Fractural.Tasks;
using Godot;
using System;
using System.Linq;
using System.Threading;

public partial class Soldier : Absorbable
{
    enum State
    {
        waiting, approaching, attacking, underAttack, dying
    }

    State currentState = State.waiting;

    CancellationTokenSource ApproachCancellationTokenSource;
    CancellationTokenSource AttackCancellationTokenSource;

    Vector2 approachTarget;
    [Export] public int moveSpeedMS = 350;

    private int size = 1;
    public override int Size
    {
        get { return size; }
        protected set { size = value; }
    }

    int attackSpeedMS = 1200;

    Blob attachedBlob;

    Area2D VisionRadius;
    AnimatedSprite2D animatedSprite;
    [Export] AudioStream shot;

    public override void _EnterTree()
    {
        base._EnterTree();
        VisionRadius = GetNode<Area2D>("VisionRadius");
        VisionRadius.AreaEntered += HandleVisionRadiusEntered;
        animatedSprite = GetNode<AnimatedSprite2D>("Sprite2D");
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

    private async GDTask ApproachPlayer()
    {
        ApproachCancellationTokenSource = new CancellationTokenSource();
        currentState = State.approaching;
        approachTarget = The.Player.Position;
        var direction = Position.DirectionTo(approachTarget);
        Vector2 movementVector;
        while (currentState == State.approaching && ApproachCancellationTokenSource.IsCancellationRequested == false)
        {
            direction = Position.DirectionTo(approachTarget);
            movementVector = direction.Normalized();
            Position += movementVector;
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

        await GDTask.Delay(attackSpeedMS, PlayerLoopTiming.Process, AttackCancellationTokenSource.Token);

        if (AttackCancellationTokenSource.Token.IsCancellationRequested) {  return; }
        if (currentState == State.underAttack) { return; }

        var targets = VisionRadius.GetOverlappingAreas().Where(area => IsInstanceValid(area) && (area is PlayerDamageable));
        if (targets.Count() == 0)
        {
            currentState = State.waiting;
            return;
        }

        var target = RNG.Combat.RandomPick(targets);

        var shotDirection = Position.DirectionTo(target.Position).Normalized();
        var bullet = Scenes.SoldierBullet;
        The.Environment.AddChild(bullet);
        bullet.Position = this.Position + shotDirection;
        bullet.Initialize(shotDirection);
        audioPlayer.Stream = shot;
        audioPlayer.Play();
        TryAttackTarget().Forget();
    }

    #region Absorbable

    public override Vector2 AttachBlob(Blob blob)
    {
        attachedBlob = blob;
        currentState = State.underAttack;
        return Position;
    }

    public override Vector2 NextBlobPosition()
    {
        return Position;
    }

    public override void DetachBlob(Blob blob)
    {
        if(attachedBlob == blob)
        {
            attachedBlob = null;

            currentState = State.waiting;
        }
    }

    public override bool CanAttachBlob()
    {
        return attachedBlob == null;
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

    private void Die()
    {
        VisionRadius.Monitoring = false;
        SetDeferred(PropertyName.Monitorable, false);

        animatedSprite.Play("Death");
        animatedSprite.AnimationFinished += () =>
        {
            EmitSignal(SignalName.WillDie);
            QueueFree();
        };
    }
}
