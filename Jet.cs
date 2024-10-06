using Fractural.Tasks;
using Godot;
using System;
using System.Threading;

public partial class Jet : Absorbable
{
    [Export] AnimatedSprite2D animatedSprite;
    CancellationTokenSource cts;
    [Export] float bombingRange = 60;
    [Export] int moveSpeedMS = 60;
    [Export] AudioStream takeOff;
    [Export] AudioStream explosion;

    bool bombDropped = false;

    public override int Size { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    public override void _ExitTree()
    {
        base._ExitTree();
        cts.Cancel();
    }

    public void Initialize(Vector2 position)
    {
        Position = position;
        Fly().Forget();

        MouseEntered += HandleMouseEntered;
        MouseExited += HandleMouseExited;
    }

    private async GDTask Fly()
    {
        audioPlayer.Stream = takeOff;
        audioPlayer.VolumeDb = -12;
        audioPlayer.Play();
        var direction = Position.DirectionTo(The.Player.Position).Normalized();
        
        if(direction.X < 0)
        {
            animatedSprite.FlipH = true;
        }

        cts = new CancellationTokenSource();

        while (cts.IsCancellationRequested == false && bombDropped == false)
        {

            await GDTask.Delay(moveSpeedMS, PlayerLoopTiming.Process, cts.Token);
            Position += new Vector2(direction.X, 0);

            if (Mathf.Abs(Position.X - The.Player.Position.X) <= bombingRange)
            {
                DropBomb(The.Player.Position);
            }
        }

        while(cts.IsCancellationRequested == false && IsOnScreen())
        {
            await GDTask.Delay(moveSpeedMS, PlayerLoopTiming.Process, cts.Token);
            Position += new Vector2(direction.X, -1);
        }

        cts.Cancel();
        QueueFree();
    }

    private void DropBomb(Vector2 target)
    {
        bombDropped = true;
        var bomb = Scenes.JetBomb;
        The.Environment.AddChild(bomb);
        bomb.Position = Position;
        bomb.Initialize(target);
    }

    private bool IsOnScreen()
    {
        return Position.X > -24 && Position.X < 344 && Position.Y > -12;
    }

    private void HandleMouseEntered()
    {
        GD.Print("Mouse entered");
        The.Player.BlastTarget = this;
    }

    private void HandleMouseExited()
    {
        if (The.Player.BlastTarget == this)
        {
            The.Player.BlastTarget = null;
        }
    }

    public override bool TickAbsorb(int rate)
    {
        cts.Cancel();
        audioPlayer.Stream = explosion;
        audioPlayer.Play();
        audioPlayer.Finished += () => QueueFree();
        return true;
    }

    public override Vector2 AttachBlob(Blob blob)
    {
        return Vector2.Zero;
    }

    public override Vector2 NextBlobPosition()
    {
        return Vector2.Zero;
    }

    public override void DetachBlob(Blob blob)
    {
        return;
    }

    public override bool CanAttachBlob()
    {
        return false;
    }
}
