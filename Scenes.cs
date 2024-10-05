using Godot;
using System;

public class Scenes
{
    public static MassTargeter MassTargeter => ResourceLoader.Load<PackedScene>("uid://cy56t0f4pdp5f").Instantiate<MassTargeter>();
    public static Blob Blob => ResourceLoader.Load<PackedScene>("uid://b7tv1s20mknp4").Instantiate<Blob>();

    public static Soldier Soldier => ResourceLoader.Load<PackedScene>("uid://db31pa4xidx3k").Instantiate<Soldier>();
}
