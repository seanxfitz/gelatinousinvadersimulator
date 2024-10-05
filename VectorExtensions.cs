using System.Collections;
using System.Collections.Generic;
using System;
using Godot;

static class Vector2IExtensions
{
    public static Vector2I[] orthogonals = new Vector2I[] { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };
    public static Vector2I AsVector2I(this Vector3 vector)
    {
        return new Vector2I((int)vector.X, (int)vector.Y);
    }

    public static Vector2I FlipXY(this Vector2I vector)
    {
        return new Vector2I(vector.Y, vector.X);
    }

    public static Vector2I DirectionFrom(this Vector2I vector, Vector2I other)
    {
        var heading = vector - other;

        if(heading.Length() != 0)
            return  heading / (int)heading.Length();
        return Vector2I.Zero;
    }

    public static int GetDistance(this Vector2I a, Vector2I b)
    {
        int dstX = Mathf.Abs(a.X - b.X);
        int dstY = Mathf.Abs(a.Y - b.Y);
        return dstX + dstY;
    }

    public static int GetMaxStraightSteps(this Vector2I a, Vector2I b)
    {
        int dstX = Mathf.Abs(a.X - b.X);
        int dstY = Mathf.Abs(a.Y - b.Y);

        return Mathf.Max(dstX, dstY);
    }
}

static class Vector3Extensions
{
    public static Vector3 EaseWithFunction(Func<float, float, float, float> function, Vector3 start, Vector3 end, float progress)
    {
        return new Vector3(function(start.X, end.X, progress), function(start.Y, end.Y, progress), function(start.Z, end.Z, progress));
    }

    public static Vector3 DirectionTo(this Vector3 vector, Vector3 other)
    {
        var heading = other - vector;
        return heading.Normalized();
    }
}

static class Vector2Extensions
{
    public static Vector2 EaseWithFunction(Func<float, float, float, float> function, Vector2 start, Vector2 end, float progress)
    {
        return new Vector2(function(start.X, end.X, progress), function(start.Y, end.Y, progress));
    }

    public static Vector2 DirectionTo(this Vector2 vector, Vector2 other)
    {
        var heading = other - vector;
        return heading.Normalized();
    }
}
