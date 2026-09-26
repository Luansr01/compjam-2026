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
	
	public f64 BaseMaxHealth { get; private set; }
	
	private bool _isDead;

	public override void _Ready()
	{
		BaseMaxHealth      = this._maxHealth;
		this._currentHealth = this._maxHealth;
		_isDead = false;
		this.die += () => _isDead = true;
	}

	public void TakeDamage(f64 damage) {
		if (_isDead) return;

		this._currentHealth -= damage;
		if(this._currentHealth <= 0) {
			this._currentHealth = 0;
			die?.Invoke();
		}
	}


	public void SetMaxHealth(f64 newHealth){
		this._maxHealth = (f64) newHealth;
	}

	/// Set the total upgrade bonus rather than nudging health by a step, so
	/// applying the same level twice is a no-op. The difference is handed over as
	/// current health, otherwise a purchase would only matter after the next hit.
	public void SetBonusMaxHealth(f64 bonus)
	{
		if (_isDead) return;

		f64 target = BaseMaxHealth + bonus;
		_currentHealth += target - _maxHealth;
		_maxHealth      = target;

		if (_currentHealth > _maxHealth) _currentHealth = _maxHealth;
		if (_currentHealth < 0)          _currentHealth = 0;
	}
}
