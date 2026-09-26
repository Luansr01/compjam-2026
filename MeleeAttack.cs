using Godot;
using System;

[GlobalClass]
public partial class MeleeAttack : Node2D
{
	[Export] Area2D   attackArea;
	[Export] AnimatedSprite2D attackSprite;
	[Export] f64      damage;
	[Export] f64      sustain;
	[Export] f64      cooldown;
	[Export] u32      team;

	[Export] f64      slowFactor;
	[Export] f64      slowDuration;

	// Which sound this attack makes. Left as None on the enemies' contact
	// damage, so only the player's swing is audible.
	[Export] SfxCue cue = SfxCue.None;
	
	public event Area2D.BodyEnteredEventHandler BodyEntered { add => attackArea.BodyEntered += value; remove => attackArea.BodyEntered -= value; } 

	public Vector2 AttackDirection = new(1, 0);

	public f64 damageScale   = 1.0;
	public f64 cooldownScale = 1.0;

	public f64 rageScale = 1.0;

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
		Sfx.Instance?.Play(cue);
	}

	void SetState(bool state) {
		if (_enabled == state) return;

		ResetDetection();
		if(attackSprite != null) attackSprite.Visible   = state;
		_enabled               = state;
	}

	void OnBodyEnter(Node2D n) {
		if (!_enabled) return;
		
		if (n.FirstOrDefaultNodeOfType<HealthComponent>() is var health && health.Team != team) {
			health.TakeDamage(damage * damageScale * rageScale);

			if (slowFactor > 0
			 && n.FirstOrDefaultNodeOfType<EnemyControl>() is var control
			 && GodotObject.IsInstanceValid(control))
				control.ApplySlow(slowFactor, slowDuration);
		} 
	}

	public async void ResetDetection()
	{
		SceneTree tree = GetTree();
		if (attackArea == null || tree == null) return;

		attackArea.Monitoring = false;
		attackArea.Monitorable = false;

		await ToSignal(tree, SceneTree.SignalName.PhysicsFrame);

		if (attackArea == null || !GodotObject.IsInstanceValid(attackArea)) return;

		attackArea.Monitoring = true;
		attackArea.Monitorable = true;
	}
}
