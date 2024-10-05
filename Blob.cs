using Fractural.Tasks;
using Godot;
using System;
using System.Linq;
using System.Threading;
using System.Timers;

public partial class Blob : Area2D
{
    public enum State
    {
        waiting, launching, returning, attacking, dissolving
    }

    public State CurrentState { get; private set;} = State.waiting;

    Vector2 LaunchTarget = Vector2.Zero;
    int moveSpeedMS;
    int attackSpeedMS = 80;

    CancellationTokenSource tokenSource;

    Area2D VisionRadius;

    float attackRange = 1;
    [Export] int dissolveStrength = 1;
    int dissolveTickMS = 1000;

    Soldier attackTarget;

    public override void _EnterTree()
    {
        base._EnterTree();
        VisionRadius = GetNode<Area2D>("VisionRadius");
        VisionRadius.AreaEntered += HandleAreaEnteredVision;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        switch (CurrentState)
        {
            case State.waiting:
                if (VisionRadius.HasOverlappingAreas())
                {
                    var bodies = VisionRadius.GetOverlappingAreas().Where(body => body is Soldier soldier && soldier.CanAttachBlob());

                    if (bodies.Count() == 0)
                    {
                        ReturnToPlayer().Forget();
                        return;
                    }

                    GD.Print($"blob has bodies in vision and can attach {bodies.Count() > 0}");
                    var target = RNG.Combat.RandomPick(bodies) as Soldier;
                    AttackTarget(target).Forget();
                }
                else
                {
                    ReturnToPlayer().Forget();
                }
                break;
        }
    }

    public async GDTask Launch(Vector2 position, float launchDuration, int moveSpeedMS)
    {
        this.moveSpeedMS = moveSpeedMS;
        CurrentState = State.launching;
        await this.EasePosition(position, launchDuration, Easing.EaseOutSine);
        CurrentState = State.waiting;
    }

    public async GDTask ReturnToPlayer()
    {
        tokenSource = new CancellationTokenSource();
        CurrentState = State.returning;
        var targetPos = The.Player.Position;
        var dir = Position.DirectionTo(targetPos).Normalized();
        while (CurrentState == State.returning && tokenSource.Token.IsCancellationRequested == false)
        {
            await GDTask.Delay(moveSpeedMS, PlayerLoopTiming.PhysicsProcess, tokenSource.Token);
            dir = Position.DirectionTo(targetPos).Normalized();
            Position += dir;
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        tokenSource?.Cancel();
        attackTarget?.DetachBlob(this);
    }

    private void HandleAreaEnteredVision(Area2D other)
    {
        if (other is Soldier soldier && CurrentState != State.attacking && CurrentState != State.dissolving && CurrentState != State.launching)
        {
            AttackTarget(soldier).Forget();
        }
    }

    private async GDTask AttackTarget(Soldier other)
    {
        CurrentState = State.attacking;
        attackTarget = other;
        attackTarget.WillDie += HandleTargetDeath;
        while(CurrentState == State.attacking)
        {
            if (attackTarget == null || IsInstanceValid(attackTarget) == false)
            {
                CurrentState = State.waiting;
                return;
            }

            if (attackTarget.Position.DistanceSquaredTo(Position) > attackRange * attackRange)
            {
                Position += Position.DirectionTo(attackTarget.Position).Normalized();
                await GDTask.Delay(attackSpeedMS, PlayerLoopTiming.PhysicsProcess);
            }
            else if (attackTarget.CanAttachBlob())
            {
                DissolveTarget(attackTarget).Forget();
            } 
            else
            {
                CurrentState = State.waiting;
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
        CurrentState = State.waiting;
    }

    private async GDTask DissolveTarget(Soldier other)
    {
        CurrentState = State.dissolving;
        attackTarget.AttachBlob(this);
        while(IsInstanceValid(other) && other.health > 0)
        {
            await GDTask.Delay(dissolveTickMS);
            Position = other.Position;
            other.TickDissolve(dissolveStrength);
        }
        CurrentState = State.waiting;
    }
}
