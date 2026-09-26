using Godot;
using System;

using f64 = double;

public partial class Nexus : StaticBody2D
{
	[Export] private HealthComponent health;
	[Export] private Sprite2D sprite;
	
	private Timer _damageTestTimer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		health.Die += this.Die;
	}

	public void TakeDamage(f64 damage)
	{
		health.TakeDamage(damage);
	}

	public void Die(){
		Utils.DEBUG( () => {
			GD.Print("Nexus died!");
		});
	}
}
