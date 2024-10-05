using Godot;
using System;
using System.Linq;

public partial class GameEnvironment : Node2D
{

    [Export] double spawnInterval = 5;
    double timer = 0;
    public override void _EnterTree()
    {
        base._EnterTree();
        The.Environment = this;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        timer += delta;
        if(timer > spawnInterval)
        {
            // spawn
            timer = 0;
            SpawnSoldierGroup();
        }
    }


    private void SpawnSoldierGroup()
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

        var spawnPosition = new Vector2(x, y);

        var amount = RNG.Generation.GetInRange(3, 7);

        for(int posX = 0; posX < amount / 2; posX++)
        {
            for(int posY = 0; posY < amount / 2; posY++)
            {
                var soldier = Scenes.Soldier;
                AddChild(soldier);
                soldier.Position = spawnPosition + new Vector2(posX * 8, posY * 8);
            }
        }
    }
}
