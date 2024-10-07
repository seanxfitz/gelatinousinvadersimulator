using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : Area2D, PlayerDamageable
{
    int size = 40;
    public int Size
    {
        get { return size; }
        private set { 
            size = value;
            EmitSignal(SignalName.OnSizeChanged, value);
        }
    }
    int sizeScaleThreshold = 10;
    int availableSize;
    [Export] int chargeSpeedMS = 100;
    [Export] int launchSpeedMS = 75;
    [Export] float launchVelocity = 0.03f;
    [Export] int blobSpeedMS = 100;
    int blastCost = 3;

    AnimatedSprite2D animatedSprite;

    [Signal] public delegate void OnSizeChangedEventHandler(int size);
    [Signal] public delegate void PlayerWillDieEventHandler();
    [Signal] public delegate void PlayerWinsEventHandler();

    bool chargingAttack = false;

    public Absorbable BlastTarget;

    public override void _EnterTree()
    {
        base._EnterTree();
        The.Player = this;
        AreaEntered += HandleOtherAreaEntered;
        animatedSprite = GetNode<AnimatedSprite2D>("Sprite2D");
        animatedSprite.Play();
    }

    public override void _Ready()
    {
        base._Ready();
        OnSizeChanged += The.UI.PlayerUI.HandlePlayerSizeChange;
        Size = size;
        availableSize = size;
        ScaleForSize();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        OnSizeChanged -= The.UI.PlayerUI.HandlePlayerSizeChange;
    }

    private void ScaleForSize()
    {
        if (size == 0) return;

        var dimension = size / sizeScaleThreshold;
        this.Scale = new Vector2(dimension + 1, dimension + 1);
        Position = new Vector2(Mathf.Floor(160 - (Scale.X / 2)), Position.Y);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (The.Game.Started == false) return;

        if (ChargeAttackIsValid() && Input.IsActionJustPressed("Click") && GetViewport().GetMousePosition().DistanceTo(Position) > MinimumChargeAttackDistance())
        {
            // Spawn shooter
            chargingAttack = true;
            var targeting = Scenes.MassTargeter;
            The.Environment.AddChild(targeting);
            targeting.Position = GetViewport().GetMousePosition();
            targeting.Initialize(availableSize - 1, chargeSpeedMS);
            targeting.OnChargeReleased += HandleChargeAttackRelease;
        } 
        else if (Input.IsActionJustPressed("RightClick") && BlastAttackIsValid())
        {
            LaunchBlastAttack();
        }
    }

    private bool ChargeAttackIsValid()
    {
        return chargingAttack == false && availableSize > 1;
    }

    private bool BlastAttackIsValid()
    {
        return chargingAttack == false && availableSize > blastCost && BlastTarget != null;
    }

    private float MinimumChargeAttackDistance()
    {
        return 3 * Scale.X;
    }

    private void HandleChargeAttackRelease(int releasedAmount, Vector2 target, MassTargeter targeter)
    {
        targeter.OnChargeReleased -= HandleChargeAttackRelease;

        availableSize -= releasedAmount;
        LaunchChargeAttack(releasedAmount, target).Forget();
        chargingAttack = false;
    }

    private async GDTask LaunchChargeAttack(int amount, Vector2 target)
    {
        GD.Print($"launching {amount} blobs");
        var firingOffsetPosition = Position + (3 * Scale.X * Position.DirectionTo(target));
        var time = launchVelocity * firingOffsetPosition.DistanceTo(target);
        foreach (var _ in Enumerable.Range(0, amount))
        {
            var blob = Scenes.Blob;
            The.Environment.AddChild(blob);
            blob.Position = firingOffsetPosition;
            var randX = RNG.Combat.GetInRange(-8f, 8f);
            var randY = RNG.Combat.GetInRange(-8f, 8f);
            blob.Launch((target + new Vector2(randX, randY)).Round(), time, blobSpeedMS).Forget();
            await GDTask.Delay(launchSpeedMS);

            Size -= 1;
            ScaleForSize();
        }
    }

    private void LaunchBlastAttack()
    {
        if (BlastTarget != null && IsInstanceValid(BlastTarget))
        {
            Size -= blastCost;
            availableSize -= blastCost;
            var attack = Scenes.BlastAttack;
            The.Environment.AddChild(attack);
            attack.Position = Position;

            attack.Initialize(BlastTarget);
        }
    }

    private void HandleOtherAreaEntered(Area2D other)
    {
        if (other is Blob blob && blob.CurrentState != Blob.State.launching)
        {
            Size += blob.Size;
            availableSize += blob.Size;
            ScaleForSize();
            blob.QueueFree();
            if (size > 100)
            {
                EmitSignal(SignalName.PlayerWins);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (Size <= 0 || Size > 100) return;

        Size -= amount;
        availableSize -= amount;
        ScaleForSize();

        if (Size <= 0) {
            animatedSprite.Play("Death");
            SetDeferred(PropertyName.Monitoring, false);
            EmitSignal(SignalName.PlayerWillDie);
        }
    }
}
