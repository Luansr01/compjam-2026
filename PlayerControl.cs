using Godot;
using System;

[GlobalClass]
public partial class PlayerControl : CharacterBody2D
{
	[Export] HealthComponent health;
	[Export] MeleeAttack     attack;
	[Export] AnimatedSprite2D sprite;
	[Export] f32 Speed = 100.0f;


	public f32 SpeedScale = 1.0f;

	public HealthComponent Health => health;
	public MeleeAttack     Attack => attack;

	public f32 DesperationSpeed {
		get {
			if (health == null || health.MaxHealth <= 0.0) return 1.0f;

			f64 fraction = health.CurrentHealth / health.MaxHealth;
			if (fraction >= Tuning.DesperationThreshold || Tuning.DesperationThreshold <= 0.0) return 1.0f;

			f32 k = (f32)((Tuning.DesperationThreshold - fraction) / Tuning.DesperationThreshold);
			return 1.0f + k * (f32)Tuning.DesperationBonus;
		}
	}

	Tween _flash;

	public override void _Ready() {
		health.Die   += OnDie;
		health.damaged += OnDamaged;
		UpgradeManager.Instance?.ApplyAll(this);
	}
	
	void OnDie() {
		if (sprite != null) sprite.Visible = false;
	}

	void OnDamaged(f64 amount)
	{
		_flash = Flash.Hit(this, sprite, (f32)Tuning.HitFlashSeconds);
		Numbers.Damage(this, GlobalPosition, amount, new Color(1.0f, 0.45f, 0.45f));
		Sfx.Instance?.PlayerHurt();
	}

	public override void _Process(double delta)
	{
		UpdateDesperation();
	}

	/// The lower the player gets, the harder they are to actually kill. Paired
	/// with the speed bonus this makes the last stretch look dire while quietly
	/// making it survivable.
	void UpdateDesperation()
	{
		if (health == null || health.MaxHealth <= 0.0) return;
		if (Tuning.DesperationThreshold <= 0.0 || Tuning.DesperationThreshold >= 1.0) return;

		f64 fraction = health.CurrentHealth / health.MaxHealth;
		if (fraction >= Tuning.DesperationThreshold) {
			health.IncomingDamageScale = 1.0;
			return;
		}

		f32 down = (f32)((Tuning.DesperationThreshold - fraction) / Tuning.DesperationThreshold);
		health.IncomingDamageScale = 1.0 + down * (f32)(Tuning.DesperationDamageFloor - 1.0);
	}

	public override void _PhysicsProcess(f64 delta)
	{
		if (health.IsDead) return;

		attack.AttackDirection = GetGlobalMousePosition();
		Vector2 velocity = Velocity;
		f32      speed    = Speed * SpeedScale * DesperationSpeed;

		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * speed;
			velocity.Y = direction.Y * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	
	public override void _Input(InputEvent e) 
	{
		if (health.IsDead) return;
		if (e.IsActionPressed("Interact")) {
			attack.Trigger();
		}
	}
}
