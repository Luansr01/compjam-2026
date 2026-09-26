using Godot;

public partial class Vignette : TextureRect
{
	f64 _t;

	public override void _Process(double delta)
	{
		PlayerControl player = GetTree().Root.FirstOrDefaultNodeOfType<PlayerControl>();
		HealthComponent health = player?.Health;

		if (health == null || health.MaxHealth <= 0.0 || health.IsDead) {
			Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
			return;
		}

		f64 fraction = health.CurrentHealth / health.MaxHealth;
		if (fraction >= Tuning.VignetteLowHealth || Tuning.VignetteLowHealth <= 0.0) {
			Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
			return;
		}

		_t += delta;

		f32 depth = (f32)((Tuning.VignetteLowHealth - fraction) / Tuning.VignetteLowHealth);
		f32 pulse = 0.55f + 0.45f * Mathf.Sin((f32)_t * (f32)Tuning.VignettePulseHz * Mathf.Tau);

		Modulate = new Color(1.0f, 1.0f, 1.0f, depth * (f32)Tuning.VignetteMaxAlpha * pulse);
	}
}
