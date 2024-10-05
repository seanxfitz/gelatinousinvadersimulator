using Godot;
using System;

public partial class Player : Node2D
{
    int size = 24;
    int sizeThreshold = 6;


    private Vector2 ScaleForSize()
    {
        var dimension = size / sizeThreshold;
        return new Vector2(dimension, dimension);
    }
}
