using Godot;
using System;

public abstract partial class Absorbable : Area2D
{
    public abstract bool TickAbsorb(int rate);
    public abstract Vector2 AttachBlob(Blob blob);
    public abstract Vector2 NextBlobPosition();
    public abstract void DetachBlob(Blob blob);

    public abstract bool CanAttachBlob();

    public virtual int health { get; protected set; } = 1;
    public abstract int Size {  get; protected set; }

    [Signal] public delegate void WillDieEventHandler();
    [Export] protected AudioStreamPlayer2D audioPlayer;

    public bool IsPlayerVisible()
    {
        return Position.X >= 2 && Position.X <= 318 && Position.Y >= 2 && Position.Y <= 178;
    }
}
