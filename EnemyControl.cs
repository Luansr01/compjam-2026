global using f64 = double;
global using f32 = float;
global using i32 = int;
global using u32 = uint;

using Godot;
using System;


public enum EnemyState {
	Chase,
	Contact
}

[GlobalClass]
public partial class EnemyControl : CharacterBody2D
{
	[Export] f32 Speed = 300.0f;
	[Export] HealthComponent health;
	[Export] NavigationAgent2D nav;
	[Export] MeleeAttack meleeAttack;
	
	EnemyState _state;
	Timer<f64> _navtick;
	Node2D goal;

	public override void _Ready()
	{
		goal        = GetTree().Root.FirstOrDefaultNodeOfType<Nexus>();
		health.die += OnDie;
		nav.TargetPosition = goal.GlobalPosition;
		_navtick           = new(0, 0);        
		meleeAttack.BodyEntered += OnBodyEnter;
	}

	public override void _Process(f64 delta) 
	{
		meleeAttack.Trigger();
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
		if (!nav.IsTargetReached()) {
			var dir  = ToLocal(nav.GetNextPathPosition()).Normalized();
			Vector2 newVelocity = dir * Speed * 50 * (f32)delta;
			if (nav.AvoidanceEnabled) {
				nav.Velocity = newVelocity;
			}
			else {
				Velocity = newVelocity;
				MoveAndSlide();
			}
		}
	}

	public void OnBodyEnter(Node2D n) 
	{
		meleeAttack.Trigger();
	}

	void OnDie() 
	{
		ScoreManager.Instance?.AddKill();
		QueueFree();
	}
}
