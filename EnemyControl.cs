using Godot;
using System;

using f64 = double;
using f32 = float;

public enum EnemyState {
	Chase,
	Contact
}

[GlobalClass]
public partial class EnemyControl : CharacterBody2D
{
	[Export] f32 Speed = 300.0f;
	[Export] HealthComponent health;
	[Export] Node2D root;
	[Export] Node2D goal;
	[Export] NavigationAgent2D nav;
	[Export] MeleeAttack meleeAttack;
	
	EnemyState _state;
	Timer<f64> _navtick;

	public override void _Ready()
	{
		health.die += OnDie;
		nav.TargetPosition = goal.GlobalPosition;
		_navtick           = new(0, 0);        
	}

	public override void _Process(f64 delta) 
	{
		_navtick.Tick(delta);
		if (_navtick.Elapsed) {
			if(nav.TargetPosition != goal.GlobalPosition) {
				nav.TargetPosition = goal.GlobalPosition;
			}
			_navtick.Restart();
		}
	}

	public override void _PhysicsProcess(f64 delta)
	{
		ChasePhysicsProcess(delta);
	}

	public void ChasePhysicsProcess(f64 delta) {
		if (!nav.IsTargetReached()) {
			var dir  = ToLocal(nav.GetNextPathPosition()).Normalized();
			Vector2 newVelocity = dir * Speed * 20 * (f32)delta;
			if (nav.AvoidanceEnabled) {
				nav.Velocity = newVelocity;
			}
			else {
				Velocity = newVelocity;
				MoveAndSlide();
			}
		}
	}

	void OnDie() 
	{
		root.QueueFree();
	}
}
