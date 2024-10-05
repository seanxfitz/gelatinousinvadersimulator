using Godot;
using System;

public partial class PlayerUI : Control
{
    [Export] Label sizeLabel;

    public override void _EnterTree()
    {
        base._EnterTree();
        sizeLabel = GetNode<Label>("Label");
    }

    public override void _Ready()
    {
        base._Ready();
        sizeLabel.Text = The.Player.Size.ToString();
        The.Player.OnSizeChanged += HandlePlayerSizeChange;
    }

    private void HandlePlayerSizeChange(int newSize)
    {
        sizeLabel.Text = newSize.ToString();
    }
}
