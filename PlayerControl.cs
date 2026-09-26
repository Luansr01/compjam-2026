using Godot;
using System;

[GlobalClass]
public partial class PlayerControl : CharacterBody2D
{
	[Export] HealthComponent health;
	[Export] MeleeAttack     attack;
	[Export] Sprite2D        sprite;
	[Export] f32 Speed = 300.0f;

	// Upgrade hook; the exported Speed stays the unmodified base.
	public f32 SpeedScale = 1.0f;

	public HealthComponent Health => health;
	public MeleeAttack     Attack => attack;

	public override void _Ready() {
		health.die += OnDie; 
		UpgradeManager.Instance?.ApplyAll(this);
	}
	
	void OnDie() {
		if (sprite != null) sprite.Visible = false;
	}

	public override void _PhysicsProcess(f64 delta)
	{
		if (health.IsDead) return;

		attack.AttackDirection = GetGlobalMousePosition();
		Vector2 velocity = Velocity;
		f32      speed    = Speed * SpeedScale;

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
