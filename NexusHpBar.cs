using Godot;

public partial class NexusHpBar : TextureProgressBar
{
	[Export] Nexus nexus;
	[Export] Label readout;

	public override void _Process(double delta)
	{
		if (nexus == null || !GodotObject.IsInstanceValid(nexus))
			nexus = FindNexus();
		if (nexus == null) return;

		HealthComponent health = nexus.Health;
		if (health == null) return;

		Value                  = health.CurrentHealth;
		MaxValue               = health.MaxHealth;

		if (readout != null) readout.Text = $"{health.CurrentHealth:0} / {health.MaxHealth:0}";
	}

	Nexus FindNexus() => GetTree().Root.FirstOrDefaultNodeOfType<Nexus>();
}
