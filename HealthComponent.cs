using Godot;
using System;

public partial class HealthComponent : Node2D
{
	[Export] private int _maxHealth;
	[Export] private int _currentHealth;
	private bool _isDead;

	public Action die;

	public override void _Ready()
	{
		this._currentHealth = this._maxHealth;
		_isDead = false;
		this.die += () => _isDead = true;
	}

	public void TakeDamage(int damage){
		this._currentHealth -= damage;
		if(this._currentHealth < 0) {
			this._currentHealth = 0;
			die();
		}
	}

	public int GetHealth(){
		return this._currentHealth;
	}

	public int GetMaxHealth(){
		return this._maxHealth;
	}

	public void SetMaxHealth(uint newHealth){
		this._maxHealth = (int) newHealth;
	}
}
