using Fractural.Tasks;
using Godot;
using System;

public partial class Ui : Control
{
    [Export] Label TitleLabel;
    Vector2 titlePosition;
    [Export] Button Button;
    Vector2 buttonPosition;
    [Export] public PlayerUI PlayerUI;

    public override void _EnterTree()
    {
        base._EnterTree();
        The.UI = this;
        Button.ButtonUp += HandleButtonPress;
        titlePosition = TitleLabel.Position;
        buttonPosition = Button.Position;
    }

    private void HandleButtonPress()
    {
        GDTask.Create(async () =>
        {
            await (TitleLabel.LerpPosition(new Vector2(TitleLabel.Position.X, -20), 0.5f), Button.LerpPosition(new Vector2(Button.Position.X, 200), 0.5f));
            The.Game.Start();
        });
    }

    public void SetRetryState()
    {
        GD.Print("Setting UI to retry");
        Button.Text = "Retry";

        GDTask.Create(async () =>
        {
            await (TitleLabel.LerpPosition(titlePosition, 0.5f), Button.LerpPosition(buttonPosition, 0.5f));
        });
    }

    public void SetVictoryState()
    {
        GD.Print("Setting UI to Victory");
        Button.Text = "Replay";
        TitleLabel.Text = "The GELATINOUS INVADER has grown incomprehensibly large...";

        GDTask.Create(async () =>
        {
            await (TitleLabel.LerpPosition(titlePosition, 0.5f), Button.LerpPosition(buttonPosition, 0.5f));
        });
    }
}
