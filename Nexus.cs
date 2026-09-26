using Godot;

public partial class Nexus : StaticBody2D
{
	[Export] HealthComponent health;
	[Export] Sprite2D        sprite;
	[Export] AudioStreamPlayer hitSound;
	[Export] f32             HitFlashSeconds = 0.15f;

	[Export] bool DrainEnabled       = true;
	[Export] f64  DrainStartPerSecond = 1.0;
	[Export] f64  DrainPerMinute      = 1.5;

	[Export] f64 PityThreshold = 0.35;
	[Export] f64 PityFloor     = 0.25;

	[Export] f64 LowHealthScoreBonus = 1.5;

	public HealthComponent Health => health;

	public f64 DrainRate => DrainStartPerSecond + DrainPerMinute * (_elapsed / 60.0);

	public f64 ScoreMultiplier {
		get {
			if (health == null || health.MaxHealth <= 0.0) return 1.0;

			f64 missing = 1.0 - health.CurrentHealth / health.MaxHealth;
			return 1.0 + missing * LowHealthScoreBonus;
		}
	}

	Tween _flash;

	f64 _elapsed;

	public override void _Ready()
	{
		health.damaged += OnDamaged;
	}

	public override void _Process(f64 delta)
	{
		UpdatePity();

		if (!DrainEnabled) return;

		_elapsed += delta;
		health.Drain(DrainRate * delta);
	}

	void UpdatePity()
	{
		if (health == null || health.MaxHealth <= 0.0 || PityThreshold <= 0.0) return;

		f64 fraction = health.CurrentHealth / health.MaxHealth;
		if (fraction >= PityThreshold) {
			health.IncomingDamageScale = 1.0;
			return;
		}

		f32 k = (f32)((PityThreshold - fraction) / PityThreshold);
		health.IncomingDamageScale = 1.0 + k * (PityFloor - 1.0);
	}

	void OnDamaged(f64 amount)
	{
		_flash = Flash.Hit(this, sprite, HitFlashSeconds);
		hitSound?.Play();
	}
}
