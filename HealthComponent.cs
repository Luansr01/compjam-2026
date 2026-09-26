using Godot;
using System;

[GlobalClass]
public partial class HealthComponent : Node2D
{
	[Export] f64 _maxHealth;
	[Export] f64 _currentHealth;
	[Export] u32 team;
	
	public f64 CurrentHealth => _currentHealth;
	public f64 MaxHealth => _maxHealth;

	public u32 Team => team;
	public bool IsDead => _isDead;
	public event Action die;
	
	private bool _isDead;

	public override void _Ready()
	{
		this._currentHealth = this._maxHealth;
		_isDead = false;
		this.die += () => _isDead = true;
	}

	public void TakeDamage(f64 damage) {
		this._currentHealth -= damage;
		if(this._currentHealth < 0) {
			this._currentHealth = 0;
			die();
		}
	}


	public void SetMaxHealth(f64 newHealth){
		this._maxHealth = (f64) newHealth;
	}
}
