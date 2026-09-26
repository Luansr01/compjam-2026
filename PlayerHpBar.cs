using Godot;

[GlobalClass]
public partial class HpBar : TextureProgressBar
{
	[Export] PlayerControl player;
	[Export] Label        readout;

	public override void _Process(double delta)
	{
		if (player == null || readout == null) return;

		HealthComponent health = player.Health;
		Value                  = health.CurrentHealth;
		MaxValue               = health.MaxHealth;
		readout.Text           = $"{health.CurrentHealth:0} / {health.MaxHealth:0}";
	}
}
