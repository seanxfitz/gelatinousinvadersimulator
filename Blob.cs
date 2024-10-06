using Fractural.Tasks;
using Godot;
using System;
using System.Linq;
using System.Threading;

interface PlayerDamageable
{
    void TakeDamage(int amount);
}

public partial class Blob : Area2D, PlayerDamageable
{
    public enum State
    {
        waiting, launching, returning, attacking, dissolving
    }

    public State CurrentState { get; private set; } = State.waiting;

    Vector2 LaunchTarget = Vector2.Zero;
    int moveSpeedMS;
    int attackSpeedMS = 60;

    int health = 2;
    int size = 1;
    public int Size
    {
        get { return size; }
        private set
        {
            size = value;
            ScaleForSize();
        }
    }

    CancellationTokenSource launchTokenSource;
    CancellationTokenSource returnTokenSource;
    CancellationTokenSource attackTokenSource;
    CancellationTokenSource absorbTokenSource;

    Area2D VisionRadius;

    bool hasAbsorbedTarget = false;

    float attackRange = 2;
    [Export] int dissolveStrength = 1;
    int dissolveTickMS = 1000;

    Absorbable attackTarget;
    AnimatedSprite2D animatedSprite;
    [Export] AudioStreamPlayer2D streamPlayer;
    bool dying = false;
    public override void _EnterTree()
    {
        base._EnterTree();
        VisionRadius = GetNode<Area2D>("VisionRadius");
        VisionRadius.AreaEntered += HandleAreaEnteredVision;
        animatedSprite = GetNode<AnimatedSprite2D>("Sprite2D");
        animatedSprite.Play();
        The.Player.PlayerWillDie += HandlePlayerDeath;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        CancelAllActions();
        if (IsInstanceValid(attackTarget))
        {
            attackTarget.DetachBlob(this);
        }
        The.Player.PlayerWillDie -= HandlePlayerDeath;
    }

    private void CancelAllActions()
    {
        returnTokenSource?.Cancel();
        attackTokenSource?.Cancel();
        absorbTokenSource?.Cancel();
        launchTokenSource?.Cancel();
    }

    public void HandlePlayerDeath()
    {
        Die();
    }

    private void HandleAreaEnteredVision(Area2D other)
    {
        if (other is Absorbable absorbable && 
            CurrentState != State.attacking && 
            CurrentState != State.dissolving && 
            CurrentState != State.launching && 
            hasAbsorbedTarget == false && 
            absorbable.IsPlayerVisible())
        {
            AttackTarget(absorbable).Forget();
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0 && !dying)
        {
            Die();
        }
    }

    private void Die()
    {
        if(dying) { return; }

        dying = true;
        SetDeferred("Monitorable", false);
        SetDeferred("Monitoring", false);
        VisionRadius.Monitoring = false;

        returnTokenSource?.Cancel();
        attackTokenSource?.Cancel();
        absorbTokenSource?.Cancel();
        launchTokenSource?.Cancel();

        streamPlayer.Play();
        animatedSprite.Play("Death");
        animatedSprite.AnimationFinished += () =>
        {
            QueueFree();
        };
    }

    private void ResolveNextState()
    {
        if (dying == false && hasAbsorbedTarget == false && VisionRadius.HasOverlappingAreas())
        {
            var bodies = VisionRadius.GetOverlappingAreas().Where(area => IsInstanceValid(area) && area is Absorbable absorbable && absorbable.CanAttachBlob() && absorbable.IsPlayerVisible());

            if (bodies.Any() == false)
            {
                ReturnToPlayer().Forget();
                return;
            }

            var target = RNG.Combat.RandomPick(bodies) as Absorbable;
            AttackTarget(target).Forget();
        }
        else
        {
            ReturnToPlayer().Forget();
        }
    }

    private void ScaleForSize()
    {
        switch(size)
        {
            case < 4:
                Scale = Vector2.One;
                break;
            case < 8:
                Scale = new Vector2(2, 2);
                break;
            case > 8:
                Scale = new Vector2(3,3);
                break;
        }
    }

#region StateTasks

    public async GDTask Launch(Vector2 position, float launchDuration, int moveSpeedMS)
    {
        launchTokenSource = new CancellationTokenSource();
        this.moveSpeedMS = moveSpeedMS;
        CurrentState = State.launching;
        await this.EasePosition(position, launchDuration, Easing.EaseOutSine, launchTokenSource.Token);
        ResolveNextState();
    }

    public async GDTask ReturnToPlayer()
    {
        returnTokenSource = new CancellationTokenSource();
        CurrentState = State.returning;
        var targetPos = The.Player.Position;
        var dir = Position.DirectionTo(targetPos).Normalized();
        while (CurrentState == State.returning && returnTokenSource.Token.IsCancellationRequested == false)
        {
            await GDTask.Delay(moveSpeedMS, PlayerLoopTiming.Process, returnTokenSource.Token);
            dir = Position.DirectionTo(targetPos).Normalized();
            Position += dir;
        }
    }

    private async GDTask AttackTarget(Absorbable other)
    {
        CurrentState = State.attacking;
        attackTarget = other;

        attackTokenSource = new CancellationTokenSource();
        while (CurrentState == State.attacking && attackTokenSource.IsCancellationRequested == false)
        {
            if (attackTarget == null || IsInstanceValid(attackTarget) == false)
            {
                ResolveNextState();
                break;
            }

            if (attackTarget.NextBlobPosition().DistanceSquaredTo(Position) > 2)
            {
                Position += Position.DirectionTo(attackTarget.NextBlobPosition()).Normalized();
                await GDTask.Delay(attackSpeedMS, PlayerLoopTiming.Process, attackTokenSource.Token);
            }
            else if (attackTarget.CanAttachBlob())
            {
                AbsorbTarget(attackTarget).Forget();
                break;
            }
            else
            {
                ResolveNextState();
                break;
            }
        }
    }

    private void HandleTargetDeath()
    {
        if (attackTarget != null)
        {
            attackTarget.WillDie -= HandleTargetDeath;
            attackTarget = null;
        }
        if (dying) return;
        Die();
    }

    private async GDTask AbsorbTarget(Absorbable other)
    {
        attackTokenSource.Cancel();

        absorbTokenSource = new CancellationTokenSource();

        attackTarget.WillDie += HandleTargetDeath;

        CurrentState = State.dissolving;
        Position = attackTarget.AttachBlob(this);
        animatedSprite.Play("Absorbing");
        while (IsInstanceValid(other) && other.health > 0 && absorbTokenSource.IsCancellationRequested == false)
        {
            await GDTask.Delay(dissolveTickMS, PlayerLoopTiming.Process, absorbTokenSource.Token);
            if (other.TickAbsorb(dissolveStrength))
            {
                other.WillDie -= HandleTargetDeath;
                Size += other.Size;
                health += other.Size;
                hasAbsorbedTarget = true;
                animatedSprite.Play("AbsorbedIdle");
                ResolveNextState();
                return;
            }
        }
        
        animatedSprite.Play("Idle");
        ResolveNextState();
    }

    #endregion
}
