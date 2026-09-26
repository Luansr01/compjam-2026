using Godot;
using System;

using f64 = double;
using f32 = float;

public partial class PlayerControl : CharacterBody2D
{
	[Export] HealthComponent health;
	[Export] MeleeAttack attack;
	[Export] f32 Speed = 300.0f;

	public override void _PhysicsProcess(f64 delta)
	{
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
		if (e.IsActionPressed("Interact")) {
			attack.Trigger();
		}
	}
}
