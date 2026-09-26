using Godot;
using System;

using f64 = double;
using f32 = float;

[GlobalClass]
public partial class MeleeAttack : Node2D
{
	[Export] Area2D   attackArea;
	[Export] Sprite2D attackSprite;
	[Export] f64      damage;
	[Export] f64      sustain;
	[Export] f64      cooldown;
	
	public event Area2D.BodyEnteredEventHandler BodyEntered { add => attackArea.BodyEntered += value; remove => attackArea.BodyEntered -= value; } 

	public Vector2 AttackDirection = new(1, 0);

	Timer<f64> _sustain;
	Timer<f64> _cooldown;

	bool _trigger;
	bool _enabled;

	public bool Trigger() => _trigger = true;

	public override void _Ready()
	{
		attackArea.BodyEntered += OnBodyEnter;
		if (attackSprite != null) attackSprite.Visible    = false;
		_enabled                = false;
		_sustain                = new(0, sustain);
		_cooldown               = new(0, cooldown);
	}

	public override void _PhysicsProcess(f64 delta) {
	}

	public override void _Process(f64 delta)
	{
		if (!_sustain.Elapsed) {
			_sustain.Tick(delta);
			return;
		}

		SetState(false);
		
		if (!_cooldown.Elapsed) {
			_cooldown.Tick(delta);
			return;
		}

		if (_trigger) {
			Attack();
		}

		_trigger = false;
	}

	void Attack() {
		_sustain.Restart();
		_cooldown.Restart();
		LookAt(AttackDirection);
		SetState(true);
	}

	void SetState(bool state) {
		ResetDetection();
		if(attackSprite != null) attackSprite.Visible   = state;
		_enabled               = state;
	}

	void OnBodyEnter(Node2D n) {
		if (!_enabled) return;
		
		GD.Print(n.Name);
		if (n.FirstOrDefaultNodeOfType<HealthComponent>() is var health) {
			health.TakeDamage(damage);
		} 
	}

	public async void ResetDetection()
	{
		attackArea.Monitoring = false;
		attackArea.Monitorable = false;

		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		attackArea.Monitoring = true;
		attackArea.Monitorable = true;
	}
}
