using Fractural.Tasks;
using Godot;
using System;

public partial class MassTargeter : Sprite2D
{
    [Export] ProgressBar progressBar;
    [Export] Label label;

    [Export] int current = 0;
    [Export] int max;
    [Export] int chargeSpeedMS;

    [Signal] public delegate void OnChargeReleasedEventHandler(int amount, Vector2 target, MassTargeter targeter);

    public override void _EnterTree()
    {
        base._EnterTree();
        progressBar = GetNode<ProgressBar>("ProgressBar");
        label = GetNode<Label>("Label");
        progressBar.Value = 0;
        progressBar.Step = 1;
        label.Text = current.ToString();
    }

    public void Initialize(int max, int chargeSpeed)
    {
        this.max = max;
        this.chargeSpeedMS = chargeSpeed;

        Charge().Forget();
    }

    private async GDTask Charge()
    {
        progressBar.MaxValue = max;
        while (Input.IsActionPressed("Click"))
        {

            if (current >= max)
            {
                await GDTask.NextFrame();
                continue;
            }

            current += 1;
            progressBar.Value = current;
            label.Text = current.ToString();

            await GDTask.Delay(chargeSpeedMS);

        }

        EmitSignal(SignalName.OnChargeReleased, current, Position, this);
        QueueFree();
    }
}
