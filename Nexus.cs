using Godot;

public partial class Nexus : StaticBody2D
{
	[Export] HealthComponent health;
	[Export] Sprite2D        sprite;
	[Export] AudioStreamPlayer hitSound;


	public HealthComponent Health => health;

	public f64 Elapsed => _elapsed;

	public f64 DrainRate => Tuning.DrainStartPerSecond + Tuning.DrainPerMinute * (_elapsed / 60.0);

	public f64 ScoreMultiplier {
		get {
			if (health == null || health.MaxHealth <= 0.0) return 1.0;

			f64 missing = 1.0 - health.CurrentHealth / health.MaxHealth;
			return 1.0 + missing * Tuning.LowHealthScoreBonus;
		}
	}

	Tween _flash;

	f64 _elapsed;

	public override void _Ready()
	{
		health.damaged += OnDamaged;
		health.die    += OnDie;
	}

	/// Losing the Nexus loses the run, so the player dies with it. Resolved here
	/// rather than by the player subscribing, because the Nexus is the later
	/// sibling in the tree and a type search from PlayerControl._Ready would not
	/// find it yet.
	void OnDie()
	{
		PlayerControl player = GetTree().Root.FirstOrDefaultNodeOfType<PlayerControl>();
		if (player != null && player.Health != null)
			player.Health.Kill();

		GetTree().Root.FirstOrDefaultNodeOfType<GameOver>()?.Present();
	}

	public override void _Process(f64 delta)
	{
		UpdatePity();

		if (!Tuning.DrainEnabled) return;

		_elapsed += delta;
		health.Drain(DrainRate * delta);
	}

	void UpdatePity()
	{
		if (health == null || health.MaxHealth <= 0.0) return;
		if (Tuning.PityThreshold <= 0.0 || Tuning.PityThreshold >= 1.0) return;

		f64 fraction = health.CurrentHealth / health.MaxHealth;

		if (fraction >= Tuning.PityThreshold) {
			f32 up = (f32)((fraction - Tuning.PityThreshold) / (1.0 - Tuning.PityThreshold));
			health.IncomingDamageScale = 1.0 + up * (Tuning.FullHealthDamageScale - 1.0);
			return;
		}

		f32 down = (f32)((Tuning.PityThreshold - fraction) / Tuning.PityThreshold);
		health.IncomingDamageScale = 1.0 + down * (Tuning.PityFloor - 1.0);
	}

	void OnDamaged(f64 amount)
	{
		_flash = Flash.Hit(this, sprite, (f32)Tuning.HitFlashSeconds);
		hitSound?.Play();
		Numbers.Damage(this, GlobalPosition, amount, new Color(1.0f, 0.6f, 0.3f), 44.0f);
	}
}
