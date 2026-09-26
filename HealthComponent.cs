using Godot;
using System;

using f64 = double;

public partial class HealthComponent : Node2D
{
	[Export] private f64 _maxHealth;
	[Export] private f64 _currentHealth;
	private bool _isDead;

	public Action die;

	public override void _Ready()
	{
		this._currentHealth = this._maxHealth;
		_isDead = false;
		this.die += () => _isDead = true;
	}

	public void TakeDamage(f64 damage){
		this._currentHealth -= damage;
		if(this._currentHealth < 0) {
			this._currentHealth = 0;
			die();
		}
	}

	public f64 GetHealth(){
		return this._currentHealth;
	}

	public f64 GetMaxHealth(){
		return this._maxHealth;
	}

	public void SetMaxHealth(f64 newHealth){
		this._maxHealth = (f64) newHealth;
	}
}
