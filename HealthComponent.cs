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

	public f64 IncomingDamageScale { get; set; } = 1.0;

	public event Action<f64> damaged;
	
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

		f64 effective = damage * IncomingDamageScale;

		this._currentHealth -= effective;
		damaged?.Invoke(effective);
		if(this._currentHealth <= 0) {
			this._currentHealth = 0;
			die?.Invoke();
		}
	}

	public void SetMaxHealth(f64 newHealth){
		this._maxHealth = (f64) newHealth;
	}

	/// Kill outright, ignoring any resistance. Used when something outside combat
	/// ends an entity's run, so a scaled-down last stand cannot survive it.
	public void Kill()
	{
		if (_isDead) return;

		_currentHealth = 0;
		die?.Invoke();
	}

	public void Drain(f64 amount)
	{
		if (_isDead || amount <= 0.0) return;

		this._currentHealth -= amount * IncomingDamageScale;
		if (this._currentHealth <= 0) {
			this._currentHealth = 0;
			die?.Invoke();
		}
	}

	public void HealFraction(f64 fraction)
	{
		if (_isDead) return;

		f64 healed = _currentHealth + _maxHealth * fraction;
		_currentHealth = healed < _maxHealth ? healed : _maxHealth;
	}

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
