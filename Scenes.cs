using Godot;
using System;

public class Scenes
{
    public static MassTargeter MassTargeter => ResourceLoader.Load<PackedScene>("uid://cy56t0f4pdp5f").Instantiate<MassTargeter>();
    public static Blob Blob => ResourceLoader.Load<PackedScene>("uid://b7tv1s20mknp4").Instantiate<Blob>();

    public static Soldier Soldier => ResourceLoader.Load<PackedScene>("uid://db31pa4xidx3k").Instantiate<Soldier>();
    public static Tank Tank => ResourceLoader.Load<PackedScene>("uid://dn6ps145nk2b0").Instantiate<Tank>();
    public static Jet Jet => ResourceLoader.Load<PackedScene>("uid://c0cmsgdl6xoiv").Instantiate<Jet>();
    public static SoldierBullet SoldierBullet => ResourceLoader.Load<PackedScene>("uid://bh0asv4daq1sp").Instantiate<SoldierBullet>();
    public static TankShell TankShell => ResourceLoader.Load<PackedScene>("uid://fa035trrvoxn").Instantiate<TankShell>();

    public static JetBomb JetBomb => ResourceLoader.Load<PackedScene>("uid://c3cm46n5v082").Instantiate<JetBomb>();

    public static AnimatedSprite2D Explosion => ResourceLoader.Load<PackedScene>("uid://biw8j5lgf3dxb").Instantiate<AnimatedSprite2D>();
    public static AnimatedSprite2D PlayerDamageFX => ResourceLoader.Load<PackedScene>("uid://bxnisrm86w1ky").Instantiate<AnimatedSprite2D>();

    public static BlastAttack BlastAttack => ResourceLoader.Load<PackedScene>("uid://blfu4cywlevyu").Instantiate<BlastAttack>();

    public static GameEnvironment GameEnvironment => ResourceLoader.Load<PackedScene>("uid://bqg2y41jnnw8n").Instantiate<GameEnvironment>();
}
