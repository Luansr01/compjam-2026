using Godot;
using System;

using f64 = double;
using f32 = float;

public partial class MelleAttack : Node2D
{
	[Export] Area2D   attackArea;
	[Export] Sprite2D attackSprite;
	[Export] f32      sustain;

	f32 _sustain = 0.0f;

	public override void _Ready()
	{
		attackArea.BodyEntered += OnBodyEnter;
	}

	public override void _Process(f64 delta)
	{
		if (_sustain >= sustain) {
			_sustain = 0.0f;
			FlipState();
		}
		
		if (Mathf.IsZeroApprox(_sustain)) {
			if (Input.IsActionJustPressed("Interact")) {
				_sustain += (f32)delta;
				FlipState();
			}
		}
		else {
			_sustain += (f32)delta;
		}
	}

	void FlipState() {
		attackArea.ProcessMode = attackArea.ProcessMode == ProcessModeEnum.Disabled ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
		attackSprite.Visible   = !attackSprite.Visible;
	}

	void OnBodyEnter(Node2D n) {
	}
}
