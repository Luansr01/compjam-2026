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
		_lastDamage = amount;
		_flash = Flash.Hit(this, sprite, (f32)Tuning.HitFlashSeconds);
		Numbers.Damage(this, GlobalPosition, amount, new Color(1.0f, 0.95f, 0.6f), 30.0f);
	}

	public const string Group = "enemy";

	const int MaxChainDepth = 8;
	static int _chainDepth;

	bool _overkilled;
	f64 _lastDamage;

	public override void _Ready()
	{
		AddToGroup(Group);

		goal        = GetTree().Root.FirstOrDefaultNodeOfType<Nexus>();
		health.die += OnDie;
		health.damaged += OnDamaged;
		health.overkilled += OnOverkilled;
		nav.TargetPosition = goal.GlobalPosition;
		_navtick           = new(0, 0);
		_slow              = new(0, 0);
		meleeAttack.BodyEntered += OnBodyEnter;
	}

	void OnOverkilled(f64 excess) => _overkilled = true;

	/// The killing blow is judged here rather than inside the overkill callback,
	/// so a kill that lands exactly on zero can still qualify through
	/// ExplosionMinShare, and so the blast is emitted once per death rather than
	/// re-entered from inside a damage call.
	bool ShouldExplode()
	{
		if (_overkilled) return true;

		return Tuning.ExplosionMinShare > 0.0
			&& _lastDamage >= Tuning.ExplosionMinShare * health.MaxHealth;
	}

	/// Damages and shoves everything nearby. Those kills can detonate in turn, so
	/// a packed wave chains; _chainDepth bounds that, since the nesting is
	/// synchronous and each level copies the group out of the tree.
	void Detonate()
	{
		if (_chainDepth >= MaxChainDepth) return;
		_chainDepth++;

		f64 radius = Tuning.ExplosionRadius;

		foreach (Node node in GetTree().GetNodesInGroup(Group))
		{
			if (node is not EnemyControl other) continue;
			if (other == this) continue;
			if (!GodotObject.IsInstanceValid(other)) continue;
			if (other.GlobalPosition.DistanceTo(GlobalPosition) > radius) continue;

			other.health.TakeDamage(Tuning.ExplosionDamage);
			other.ApplySlow(Tuning.ExplosionSlowFactor, Tuning.ExplosionSlowDuration);

			Vector2 away = other.GlobalPosition - GlobalPosition;
			if (away.LengthSquared() > 0.01f)
				other.ApplyKnockback(away.Normalized() * (f32)Tuning.ExplosionKnockback);
		}

		_chainDepth--;

		Pulse burst = new()
		{
			maxRadius = (f32)radius,
			seconds   = (f32)Tuning.ExplosionSeconds,
			ringCount = 3,
			coreWidth = 46.0f,
			bandWidth = 26.0f,
			coreColor = new Color(1.0f, 0.95f, 0.7f, 1.0f),
			edgeColor = new Color(1.0f, 0.45f, 0.15f, 0.95f),
		};

		GetTree().Root.AddChild(burst);
		burst.Position = GlobalPosition;
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
		if (ShouldExplode()) Detonate();

		ScoreManager.Instance?.AddKill(KillDifficulty * NexusScoreMultiplier());
		QueueFree();
	}

	f64 NexusScoreMultiplier()
	{
		if (goal is not Nexus nexus) return 1.0;
		return nexus.ScoreMultiplier;
	}
}
