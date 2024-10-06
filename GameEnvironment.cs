using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameEnvironment : Node2D
{

    [Export] double spawnInterval = 5;
    double timer = 4;

    double jetChance = 0.0;
    double tankChance = 0.2;
    public override void _EnterTree()
    {
        base._EnterTree();
        The.Environment = this;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (The.Game.Started == false) return;

        timer += delta;
        if (timer > spawnInterval)
        {
            timer = 0;
            spawnInterval = Mathf.Max(spawnInterval - 0.0125, 1.5);

            if (RNG.Generation.PassWithChance(tankChance))
            {
                var spawnPositon = GenerateSpawnPosition();
                var tank = Scenes.Tank;
                AddChild(tank);
                tank.Position = spawnPositon;
            } 
            else if(RNG.Generation.PassWithChance(jetChance))
            {
                var spawnPositionX = RNG.Generation.PassWithChance(0.5) ? -16 : 336;
                var spawnPositionY = RNG.Generation.GetInRange(12, The.Player.Position.Y - (The.Player.Scale.Y * 6) - 16);
                var jet = Scenes.Jet;
                AddChild(jet);
                jet.Initialize(new Vector2(spawnPositionX, spawnPositionY));
            }
            else
            {
                jetChance = Mathf.Min(jetChance + 0.01, 0.15);
                SpawnSoldierGroup(GenerateSpawnPosition());
            }
        }
    }

    private void SpawnSoldierGroup(Vector2 spawnPosition)
    {
        var rows = RNG.Generation.GetInRange(2, 4);
        var cols = RNG.Generation.GetInRange(1, 4);

        for(int posX = 0; posX < rows; posX++)
        {
            for(int posY = 0; posY < cols; posY++)
            {
                var soldier = Scenes.Soldier;
                AddChild(soldier);
                soldier.Position = spawnPosition + new Vector2(posX * 8, posY * 8);
            }
        }
    }

    private Vector2 GenerateSpawnPosition()
    {
        int x, y;

        if (RNG.Generation.PassWithChance(0.5))
        {
            // spawn top or bottom
            y = RNG.Generation.PassWithChance(0.5) ? -12 : 192;
            x = RNG.Generation.GetInRange(20, 300);
        }
        else
        {
            // spawn left or right
            y = RNG.Generation.GetInRange(20, 160);
            x = RNG.Generation.PassWithChance(0.5) ? -12 : 332;
        }

        return new Vector2(x, y);
    }
}
