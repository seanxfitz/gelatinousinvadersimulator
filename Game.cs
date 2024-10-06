using Godot;
using System;

public static class The
{
    // You just lost the
    public static Game Game;
    public static Player Player;
    public static GameEnvironment Environment;
    public static Ui UI;
}

public partial class Game : Node2D
{
    public bool Started { get; private set; } = false;
    private bool retry = false;
    public override void _EnterTree()
    {
        base._EnterTree();
        The.Game = this;
        RNG.Initialize(RNG.RandomSeed());
    }

    public void Start()
    {
        if (retry)
        {
            The.Environment.QueueFree();
            The.Environment = Scenes.GameEnvironment;
            AddChild(The.Environment);
        }

        Started = true;
        The.Player.PlayerWillDie += HandlePlayerDeath;
        The.Player.PlayerWins += HandlePlayerVictory;
    }

    private void HandlePlayerDeath()
    {
        Started = false;
        retry = true;
        The.Player.PlayerWillDie -= HandlePlayerDeath;
        The.Player.PlayerWins -= HandlePlayerVictory;
        The.UI.SetRetryState();
    }

    private void HandlePlayerVictory()
    {
        Started = false;
        retry = true;
        The.Player.PlayerWillDie -= HandlePlayerDeath;
        The.Player.PlayerWins -= HandlePlayerVictory;

        The.UI.SetVictoryState();
    }
}
