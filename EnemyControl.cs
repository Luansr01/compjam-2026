global using f64 = double;
global using f32 = float;
global using i32 = int;
global using u32 = uint;

using Godot;
using System;

public enum EnemyState {
	Chase,
	Contact
}

[GlobalClass]
public partial class EnemyControl : CharacterBody2D
{
	[Export] f32 Speed = 300.0f;
	[Export] HealthComponent health;
	[Export] NavigationAgent2D nav;
	[Export] MeleeAttack meleeAttack;
	[Export] Sprite2D sprite;
	[Export] f32 HitFlashSeconds = 0.15f;

	[Export] f64 KillDifficulty = 1.0;

	[Export] f32 KnockbackFriction = 1400.0f;

	EnemyState _state;
	Timer<f64> _navtick;
	Timer<f64> _slow;
	Node2D goal;
	PlayerControl tauntTarget;

	f32 _slowScale = 1.0f;

	Vector2 _knockback;

	public f32 MoveSpeed => Speed * _slowScale;

	Tween _flash;

	void OnDamaged(f64 amount)
	{
		_flash = Flash.Hit(this, sprite, HitFlashSeconds);
		Numbers.Damage(this, GlobalPosition, amount, new Color(1.0f, 0.95f, 0.6f), 30.0f);
	}

	public override void _Ready()
	{
		goal        = GetTree().Root.FirstOrDefaultNodeOfType<Nexus>();
		health.die += OnDie;
		health.damaged += OnDamaged;
		nav.TargetPosition = goal.GlobalPosition;
		_navtick           = new(0, 0);
		_slow              = new(0, 0);
		meleeAttack.BodyEntered += OnBodyEnter;
	}

	Node2D CurrentGoal()
	{
		if (ActiveItemManager.Instance is { Taunting: true }) {
			tauntTarget ??= GetTree().Root.FirstOrDefaultNodeOfType<PlayerControl>();
			if (tauntTarget != null && GodotObject.IsInstanceValid(tauntTarget)) return tauntTarget;
		}
		return goal;
	}

	public void ApplySlow(f64 factor, f64 duration)
	{
		if (duration <= 0.0) return;

		_slowScale = Mathf.Min(_slowScale, Mathf.Clamp((f32)factor, 0.05f, 1.0f));
		_slow.time = duration;
		_slow.Restart();
	}

	public void ApplyKnockback(Vector2 impulse) => _knockback += impulse;

	public void TakeShockwave(f64 damage, Vector2 origin, f32 force, f64 slowFactor, f64 slowDuration)
	{
		health.TakeDamage(damage);

		Vector2 away = GlobalPosition - origin;
		if (away.LengthSquared() > 0.01f) ApplyKnockback(away.Normalized() * force);

		ApplySlow(slowFactor, slowDuration);
	}

	public override void _Process(f64 delta)
	{
		meleeAttack.Trigger();

		meleeAttack.rageScale = ActiveItemManager.Instance is { Taunting: true } items
			? items.TauntDamageScale
			: 1.0;

		Node2D target = CurrentGoal();

		_navtick.Tick(delta);
		if (_navtick.Elapsed) {
			if (nav.TargetPosition != target.GlobalPosition) {
				nav.TargetPosition = target.GlobalPosition;
			}
			_navtick.Restart();
		}

		if (!_slow.Elapsed) {
			_slow.Tick(delta);
			if (_slow.Elapsed) _slowScale = 1.0f;
		}
	}

	public override void _PhysicsProcess(f64 delta)
	{
		Vector2 dir = Vector2.Zero;
		if (!nav.IsTargetReached())
			dir = ToLocal(nav.GetNextPathPosition()).Normalized();

		Vector2 newVelocity = dir * MoveSpeed * 50 * (f32)delta + _knockback;

		if (nav.AvoidanceEnabled) {
			nav.Velocity = newVelocity;
		}
		else {
			Velocity = newVelocity;
			MoveAndSlide();
		}

		if (_knockback.LengthSquared() > 0.0f)
			_knockback = _knockback.MoveToward(Vector2.Zero, KnockbackFriction * (f32)delta);
	}

	public void SetKillDifficulty(f64 difficulty) => KillDifficulty = difficulty;

	public void ScaleSpeed(f32 scale) => Speed *= scale;

	public void OnBodyEnter(Node2D n) 
	{
		meleeAttack.Trigger();
	}

	void OnDie()
	{
		ScoreManager.Instance?.AddKill(KillDifficulty * NexusScoreMultiplier());
		QueueFree();
	}

	f64 NexusScoreMultiplier()
	{
		if (goal is not Nexus nexus) return 1.0;
		return nexus.ScoreMultiplier;
	}
}
