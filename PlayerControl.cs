using Godot;
using System;

[GlobalClass]
public partial class PlayerControl : CharacterBody2D
{
	[Export] HealthComponent health;
	[Export] MeleeAttack     attack;
	[Export] Sprite2D        sprite;
	[Export] f32 Speed = 300.0f;

	public HealthComponent Health => health;

	public override void _Ready() {
		health.die += OnDie; 
	}
	
	void OnDie() {
		if (sprite != null) sprite.Visible = false;
	}

	public override void _PhysicsProcess(f64 delta)
	{
		if (health.IsDead) return;

		attack.AttackDirection = GetGlobalMousePosition();
		Vector2 velocity = Velocity;

		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Y = direction.Y * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
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
