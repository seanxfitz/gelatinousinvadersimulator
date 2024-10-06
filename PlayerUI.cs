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

    public void HandlePlayerSizeChange(int newSize)
    {
        sizeLabel.Text = newSize.ToString();
    }
}
