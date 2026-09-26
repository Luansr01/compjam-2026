using Godot;
using System;
using System.Collections.Generic;

public enum ActiveItem {
	Taunt,
	Shockwave,
}

public partial class ActiveItemManager : Node
{
	public static ActiveItemManager Instance { get; private set; }

	[Export] f32 costGrowth = 1.4f;

	[Export] f64 tauntBaseCost    = 40.0;
	[Export] f64 tauntDuration    = 8.0;
	[Export] f64 tauntDamageScale = 2.0;

	[Export] f64 shockwaveBaseCost     = 60.0;
	[Export] f64 shockwaveCooldown     = 6.0;
	[Export] f64 shockwaveRadius       = 800.0;
	[Export] f64 shockwaveDamage       = 40.0;
	[Export] f32 shockwaveKnockback    = 900.0f;
	[Export] f64 shockwaveSlowFactor   = 0.35;
	[Export] f64 shockwaveSlowDuration = 3.0;

	public event Action changed;

	Timer<f64> _taunt;
	Timer<f64> _shockwave;

	readonly Dictionary<ActiveItem, i32> _uses = [];

	bool _detonatePending;

	public bool Taunting => !_taunt.Elapsed;

	public f64 TauntDamageScale => tauntDamageScale;

	public override void _EnterTree() => Instance = this;

	public override void _ExitTree()
	{
		if (Instance != this) return;
		Instance = null;
		changed  = null;
	}

	public override void _Ready()
	{
		_taunt     = new(0, 0);
		_shockwave = new(0, 0);
	}

	public override void _Process(f64 delta)
	{
		if (!_taunt.Elapsed) {
			_taunt.Tick(delta);
			changed?.Invoke();
		}
		if (!_shockwave.Elapsed) {
			_shockwave.Tick(delta);
			changed?.Invoke();
		}
	}

	public override void _PhysicsProcess(f64 delta)
	{
		if (!_detonatePending) return;

		_detonatePending = false;
		Detonate();
	}

	public i32 Uses(ActiveItem item) => _uses.GetValueOrDefault(item);

	public f64 Remaining(ActiveItem item) => item switch {
		ActiveItem.Taunt     => _taunt.Elapsed     ? 0.0 : _taunt.current,
		ActiveItem.Shockwave => _shockwave.Elapsed ? 0.0 : _shockwave.current,
		_                    => 0.0,
	};

	public bool IsActive(ActiveItem item) => Remaining(item) > 0.0;

	public i32 Cost(ActiveItem item)
	{
		f64 baseCost = item == ActiveItem.Taunt ? tauntBaseCost : shockwaveBaseCost;
		return (i32)Math.Round(baseCost * Math.Pow(costGrowth, Uses(item)));
	}

	public bool CanAfford(ActiveItem item) =>
		ScoreManager.Instance != null && ScoreManager.Instance.Score >= Cost(item);

	public bool TryUse(ActiveItem item)
	{
		if (ScoreManager.Instance == null) return false;
		if (IsActive(item)) return false;
		if (!ScoreManager.Instance.TrySpend(Cost(item))) return false;

		_uses[item] = Uses(item) + 1;

		switch (item)
		{
			case ActiveItem.Taunt:
				_taunt.time = tauntDuration;
				_taunt.Restart();
				break;
			case ActiveItem.Shockwave:
				_detonatePending = true;
				_shockwave.time = shockwaveCooldown;
				_shockwave.Restart();
				break;
		}

		changed?.Invoke();
		return true;
	}

	void Detonate()
	{
		PlayerControl player = GetTree().Root.FirstOrDefaultNodeOfType<PlayerControl>();
		if (player == null) return;

		Vector2 origin = player.GlobalPosition;

		CircleShape2D shape = new() { Radius = (f32)shockwaveRadius };
		PhysicsShapeQueryParameters2D query = new()
		{
			Shape             = shape,
			Transform         = new Transform2D(0.0f, origin),
			CollideWithBodies = true,
			CollideWithAreas  = false,
		};

		World2D world = GetViewport()?.World2D;
		if (world == null) return;

		foreach (Godot.Collections.Dictionary hit in world.DirectSpaceState.IntersectShape(query, 64))
		{
			if (hit["collider"].AsGodotObject() is not Node2D body) continue;
			if (body.FirstOrDefaultNodeOfType<EnemyControl>() is not EnemyControl enemy) continue;
			if (!GodotObject.IsInstanceValid(enemy)) continue;

			enemy.TakeShockwave(
				shockwaveDamage, origin, shockwaveKnockback,
				shockwaveSlowFactor, shockwaveSlowDuration);
		}

		Pulse ring = new() { maxRadius = (f32)shockwaveRadius };
		GetTree().Root.AddChild(ring);
		ring.Position = origin;
	}
}
