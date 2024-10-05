using Fractural.Tasks;
using Godot;
using System;

public interface Dissolvable
{
    void TickDissolve(int rate);
    void AttachBlob(Blob blob);
    void DetachBlob(Blob blob);

    bool CanAttachBlob();
}

public partial class Soldier : Area2D, Dissolvable
{

    enum State
    {
        waiting, approaching, attacking
    }

    State currentState = State.waiting;

    Vector2 approachTarget;
    [Export] public int moveSpeedMS = 400;

    Area2D VisionRadius;

    [Signal] public delegate void WillDieEventHandler();

    public int health { get; private set; } = 1;

    public override void _EnterTree()
    {
        base._EnterTree();
        VisionRadius = GetNode<Area2D>("VisionRadius");
        VisionRadius.AreaEntered += HandleVisionRadiusEntered;
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

    private async GDTask ApproachPlayer()
    {
        currentState = State.approaching;
        approachTarget = The.Player.Position;
        var direction = Position.DirectionTo(approachTarget);
        Vector2 movementVector;
        while(currentState == State.approaching)
        {
            await GDTask.Delay(moveSpeedMS);

            direction = Position.DirectionTo(approachTarget);
            movementVector = direction.Normalized();
            Position += movementVector;
        }
    }

    private void HandleVisionRadiusEntered(Node2D other)
    {
        currentState = State.attacking;
    }

    Blob beingDissolvedBy;

    public void AttachBlob(Blob blob)
    {
        beingDissolvedBy = blob;
    }

    public void DetachBlob(Blob blob)
    {
        if(beingDissolvedBy == blob)
        {
            beingDissolvedBy = null;
        }
    }

    public bool CanAttachBlob()
    {
        return beingDissolvedBy == null;
    }

    public void TickDissolve(int rate)
    {
        health -= rate;
        if (health <= 0)
        {
            EmitSignal(SignalName.WillDie);
            QueueFree();
        }
    }
}
