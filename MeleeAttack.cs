using Godot;
using System;

[GlobalClass]
public partial class MeleeAttack : Node2D
{
	[Export] Area2D   attackArea;
	[Export] Sprite2D attackSprite;
	[Export] f64      damage;
	[Export] f64      sustain;
	[Export] f64      cooldown;
	[Export] u32      team;
	
	public event Area2D.BodyEnteredEventHandler BodyEntered { add => attackArea.BodyEntered += value; remove => attackArea.BodyEntered -= value; } 

	public Vector2 AttackDirection = new(1, 0);

	// Upgrades scale the exported values instead of overwriting them, so the
	// scene keeps its authored numbers and a reset is just putting 1.0 back.
	public f64 damageScale   = 1.0;
	public f64 cooldownScale = 1.0;

	Timer<f64> _sustain;
	Timer<f64> _cooldown;

	bool _trigger;
	bool _enabled;

	public bool Trigger() => _trigger = true;

	public void SetDamageScale(f64 scale) => damageScale = scale;

	public void SetCooldownScale(f64 scale)
	{
		cooldownScale  = scale;
		_cooldown.time = Math.Max(0.1, cooldown * cooldownScale);
		// A swing already counting down must not outlast the shorter period.
		if (_cooldown.current > _cooldown.time) _cooldown.current = _cooldown.time;
	}

	public override void _Ready()
	{
		BodyEntered += OnBodyEnter;
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
		GD.Print($"Hey a {n.Name}");
		if (!_enabled) return;
		
		GD.Print($"Hiting {n.Name}");
		if (n.FirstOrDefaultNodeOfType<HealthComponent>() is var health && health.Team != team) {
			GD.Print($"Hiting {n.Name} for {damage * damageScale}");
			health.TakeDamage(damage * damageScale);
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
