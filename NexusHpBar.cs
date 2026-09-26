using Godot;

public partial class NexusHpBar : TextureProgressBar
{
	[Export] Nexus nexus;
	[Export] Label readout;

	public override void _Process(double delta)
	{
		if (nexus == null || readout == null) return;

		HealthComponent health = nexus.Health;
		Value                  = health.CurrentHealth;
		MaxValue               = health.MaxHealth;
		readout.Text           = $"{health.CurrentHealth:0} / {health.MaxHealth:0}";
	}
}
