using Godot;
using System;
using Fractural.Tasks;

public static class Node2DExtensions
{

    public static async GDTask Shake(this Control node, float magnitude, float duration)
    {
        var startingPosition = node.Position;
        GD.Print(startingPosition);
        double time = 0;
        while (time < duration)
        {
            var x = GD.RandRange(startingPosition.X - (magnitude / 2f), startingPosition.X + (magnitude / 2f));
            var y = GD.RandRange(startingPosition.Y - (magnitude / 2f), startingPosition.Y + (magnitude / 2f));
            node.Position = new Vector2((float)x, (float)y);
            await GDTask.NextFrame();
            time += node.GetProcessDeltaTime();
        }

        node.Position = startingPosition;
        GD.Print(node.Position);
    }

    public static async GDTask LerpPosition(this Node2D node2D, Vector2 targetPosition, float duration)
    {
        var startingPosition = node2D.Position;
        var time = 0f;
        while (time < duration)
        {
            node2D.Position = startingPosition.Lerp(targetPosition, time / duration);
            await node2D.ToSignal(node2D.GetTree(), SceneTree.SignalName.ProcessFrame);
            time += (float)node2D.GetProcessDeltaTime();
        }
        node2D.Position = targetPosition;
    }

    public static async GDTask Punch(this Node2D node, Vector2 direction, float intensity, float duration)
    {
        GD.Print("punching");
        var finalPosition = node.Position;
        await node.LerpPosition(finalPosition + (direction * intensity), duration / 4);
        var remaining = 3 * duration / 4;
        await node.EasePosition(finalPosition, remaining, Easing.Spring);
        GD.Print("Punch complete");
    }

    public static async GDTask EasePosition(this Node2D node, Vector2 targetPosition, float duration, System.Func<float, float, float, float> easeFunction)
    {
        var startingPosition = node.Position;
        var time = 0f;
        while (time < duration)
        {
            node.Position = Vector2Extensions.EaseWithFunction(easeFunction, startingPosition, targetPosition, time / duration);
            await node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);
            time += (float)node.GetProcessDeltaTime();
        }
        node.Position = targetPosition;
    }

    public static async GDTask ShakeTask(this Node2D node, float magnitude, float duration)
    {
        var startingPosition = node.Position;
        double time = 0;
        while (time < duration)
        {
            var x = GD.RandRange(startingPosition.X - (magnitude / 2f), startingPosition.X + (magnitude / 2f));
            var y = GD.RandRange(startingPosition.Y - (magnitude / 2f), startingPosition.Y + (magnitude / 2f));
            node.Position = new Vector2((float)x, (float)y);
            await GDTask.NextFrame();
            time += node.GetProcessDeltaTime();
        }

        node.Position = startingPosition;
    }

    public static async GDTask FadeOut(this Node2D node, float duration)
    {
        var color = node.Modulate;
        double time = 0;

        while(time < duration)
        {
            color.A = (float)Mathf.Lerp(1, 0, time / duration);
            node.Modulate = color;
            await GDTask.NextFrame();
            time += node.GetProcessDeltaTime();
        }

        color.A = 0;
        node.Modulate = color;
    }

    public static async GDTask FadeIn(this Node2D node, float duration)
    {
        var color = node.Modulate;
        double time = 0;

        while (time < duration)
        {
            color.A = (float)Mathf.Lerp(0, 1, time / duration);
            node.Modulate = color;
            await GDTask.NextFrame();
            time += node.GetProcessDeltaTime();
        }

        color.A = 1;
        node.Modulate = color;
    }
}
