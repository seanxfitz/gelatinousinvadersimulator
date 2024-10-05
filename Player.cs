using Fractural.Tasks;
using Godot;
using System;
using System.Linq;
using System.Reflection.Metadata;

public static class The
{
    public static Player Player;
    public static GameEnvironment Environment;
}

public partial class Player : Area2D
{
    int size = 100;
    public int Size
    {
        get { return size; }
        private set { 
            size = value;
            EmitSignal(SignalName.OnSizeChanged, value);
        }
    }
    int sizeThreshold = 10;
    int availableSize;
    [Export] int startingSize = 40;
    [Export] int chargeSpeedMS = 100;
    [Export] int launchSpeedMS = 75;
    [Export] float launchVelocity = 0.05f;
    [Export] int blobSpeedMS = 120;

    [Signal] public delegate void OnSizeChangedEventHandler(int size);

    bool chargingAttack = false;

    public override void _EnterTree()
    {
        base._EnterTree();
        size = startingSize;
        availableSize = startingSize;
        The.Player = this;
        RNG.Initialize(RNG.RandomSeed());
        AreaEntered += HandleOtherAreaEntered;
    }

    public override void _Ready()
    {
        base._Ready();
        ScaleForSize();
    }

    private void ScaleForSize()
    {
        var dimension = size / sizeThreshold;
        this.Scale = new Vector2(dimension + 1, dimension + 1);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (ChargeAttackIsValid() && Input.IsActionJustPressed("Click"))
        {
            // Spawn shooter
            chargingAttack = true;
            var targeting = Scenes.MassTargeter;
            The.Environment.AddChild(targeting);
            targeting.Position = GetViewport().GetMousePosition();
            targeting.Initialize(availableSize - sizeThreshold, chargeSpeedMS);
            targeting.OnChargeReleased += HandleChargeAttackRelease;
        }
    }

    private bool ChargeAttackIsValid()
    {
        return chargingAttack == false && availableSize > sizeThreshold;
    }

    private void HandleChargeAttackRelease(int releasedAmount, Vector2 target)
    {
        availableSize -= releasedAmount;
        LaunchChargeAttack(releasedAmount, target).Forget();
        chargingAttack = false;
    }

    private async GDTask LaunchChargeAttack(int amount, Vector2 target)
    {
        GD.Print($"launching {amount} blobs");
        foreach(var _ in Enumerable.Range(0, amount))
        {
            var blob = Scenes.Blob;
            The.Environment.AddChild(blob);
            blob.Position = Position;
            var randX = RNG.Combat.GetInRange(-8f, 8f);
            var randY = RNG.Combat.GetInRange(-8f, 8f);
            var time = 0.05f * target.DistanceTo(Position);
            GD.Print(time);
            blob.Launch((target + new Vector2(randX, randY)).Round(), time, blobSpeedMS).Forget();
            await GDTask.Delay(launchSpeedMS);

            Size -= 1;
            ScaleForSize();
        }
    }

    private void HandleOtherAreaEntered(Area2D other)
    {
        if (other is Blob blob && blob.CurrentState != Blob.State.launching)
        {
            Size += 1;
            availableSize += 1;
            ScaleForSize();
            blob.QueueFree();
        }
    }
}
