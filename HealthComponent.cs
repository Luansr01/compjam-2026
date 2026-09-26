using Godot;
using System;

using f64 = double;

[GlobalClass]
public partial class HealthComponent : Node2D
{
	[Export] private f64 _maxHealth;
	[Export] private f64 _currentHealth;
	private bool _isDead;

	public Action Die;

	public override void _Ready()
	{
		this._currentHealth = this._maxHealth;
		_isDead = false;
		this.Die += () => _isDead = true;
	}

	public void TakeDamage(f64 damage){
		if(_isDead) return;
		this._currentHealth -= damage;
		if(this._currentHealth < 0) {
			this._currentHealth = 0;
			Die();
			return;
		}
		
		Utils.DEBUG( () => {
				GD.Print($"{this.GetParent().Name} took {damage} damage. | New HP: {_currentHealth}/{_maxHealth}");
		});
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
