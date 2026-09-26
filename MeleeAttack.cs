using Godot;
using System;

using f64 = double;
using f32 = float;

public partial class MeleeAttack : Node2D
{
	[Export] Area2D   attackArea;
	[Export] Sprite2D attackSprite;
	[Export] CollisionObject2D attackObject;
	[Export] f64      damage;
	[Export] f64      sustain;
		
	f64 _sustain = 0.0;
	bool _trigger;
	bool _enabled;

	public bool Trigger() => _trigger = true;

	public override void _Ready()
	{
		attackArea.BodyEntered += OnBodyEnter;
		attackSprite.Visible    = false;
		_enabled                = false;
	}


	public override void _Process(f64 delta)
	{
		if (_sustain >= sustain) {
			_sustain = 0.0;
			FlipState();
		}
		
		if (Mathf.IsZeroApprox(_sustain)) {
			if (_trigger) {
				_trigger = false;
				_sustain += delta;
				FlipState();
			}
		}
		else {
			_sustain += (f32)delta;
		}
	}

	void FlipState() {
		ResetDetection();
		attackSprite.Visible   = !attackSprite.Visible;
		_enabled               = !_enabled;
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
