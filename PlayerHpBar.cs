using Godot;

[GlobalClass]
public partial class PlayerHpBar : TextureProgressBar
{
	[Export] PlayerControl player;
	[Export] Label        readout;

	public override void _Process(double delta)
	{
		if (player == null || !GodotObject.IsInstanceValid(player))
			player = FindPlayer();
		if (player == null) return;

		HealthComponent health = player.Health;
		if (health == null) return;

		Value    = health.CurrentHealth;
		MaxValue = health.MaxHealth;

		if (readout != null) readout.Text = $"{health.CurrentHealth:0} / {health.MaxHealth:0}";
	}

	PlayerControl FindPlayer() =>
		GetTree().Root.FirstOrDefaultNodeOfType<PlayerControl>();
}
