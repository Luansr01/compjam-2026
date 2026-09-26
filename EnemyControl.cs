using Godot;
using System;

using f64 = double;

public partial class EnemyControl : CharacterBody2D
{
	[Export] float Speed = 300.0f;
	[Export] HealthComponent health;
	[Export] Node2D root;

	public override void _Ready() 
	{
		health.die += OnDie;
	}

	public override void _Process(f64 delta) 
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{

	}

	void OnDie() 
	{
		root.QueueFree();
	}
}
