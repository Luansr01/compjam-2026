using Godot;
using System;
using System.Collections.Generic;

public enum UpgradeKind {
	MeleeDamage,
	AttackSpeed,
	MoveSpeed,
	Heal,
	NexusHeal,
}

public partial class UpgradeManager : Node
{
	public static UpgradeManager Instance { get; private set; }

	[Export] f32 costGrowth = 1.8f;

	[Export] f64 damagePerLevel    = 0.25;
	[Export] f64 cooldownPerLevel  = 0.10;
	[Export] f32 moveSpeedPerLevel = 0.15f;

	[Export] f64 healFractionPerLevel = 0.10;

	readonly Dictionary<UpgradeKind, i32> _levels = [];

	public event Action changed;

	public override void _EnterTree() => Instance = this;

	public override void _ExitTree()
	{
		if (Instance != this) return;
		Instance = null;
		changed  = null;
	}

	public i32 Level(UpgradeKind kind) => _levels.GetValueOrDefault(kind);

	public i32 Cost(UpgradeKind kind) =>
		Mathf.RoundToInt(BaseCost(kind) * Mathf.Pow(costGrowth, Level(kind)));

	public bool CanAfford(UpgradeKind kind) =>
		ScoreManager.Instance != null && ScoreManager.Instance.Score >= Cost(kind);

	public bool TryPurchase(UpgradeKind kind)
	{
		if (ScoreManager.Instance == null) return false;
		if (!ScoreManager.Instance.TrySpend(Cost(kind))) return false;

		_levels[kind] = Level(kind) + 1;

		if (kind is UpgradeKind.Heal or UpgradeKind.NexusHeal) Heal(kind);
		else Apply(kind);

		changed?.Invoke();
		return true;
	}

	void Heal(UpgradeKind kind)
	{
		f64 fraction = healFractionPerLevel * Level(kind);
		HealTarget(kind)?.HealFraction(fraction);
	}

	public bool IsUseful(UpgradeKind kind)
	{
		if (kind is not (UpgradeKind.Heal or UpgradeKind.NexusHeal)) return true;

		HealthComponent target = HealTarget(kind);
		return target != null && target.CurrentHealth < target.MaxHealth;
	}

	HealthComponent HealTarget(UpgradeKind kind) => kind == UpgradeKind.NexusHeal
		? FindNexus()?.Health
		: FindPlayer()?.Health;

	public void ApplyAll(PlayerControl player)
	{
		foreach (UpgradeKind kind in Enum.GetValues<UpgradeKind>()) Apply(kind, player);
	}

	public void Apply(UpgradeKind kind, PlayerControl player = null)
	{
		player ??= FindPlayer();
		if (player == null) return;

		i32 level = Level(kind);
		switch (kind)
		{
			case UpgradeKind.MeleeDamage:
				player.Attack.SetDamageScale(1.0 + damagePerLevel * level);
				break;
			case UpgradeKind.AttackSpeed:
				player.Attack.SetCooldownScale(Math.Pow(1.0 - cooldownPerLevel, level));
				break;
			case UpgradeKind.MoveSpeed:
				player.SpeedScale = 1.0f + moveSpeedPerLevel * level;
				break;
			case UpgradeKind.Heal:
			case UpgradeKind.NexusHeal:
				break;
		}
	}

	PlayerControl FindPlayer() => GetTree().Root.FirstOrDefaultNodeOfType<PlayerControl>();

	Nexus FindNexus() => GetTree().Root.FirstOrDefaultNodeOfType<Nexus>();

	static i32 BaseCost(UpgradeKind kind) => kind switch {
		UpgradeKind.MeleeDamage => 5,
		UpgradeKind.AttackSpeed => 10,
		UpgradeKind.MoveSpeed   => 15,
		UpgradeKind.Heal        => 20,
		UpgradeKind.NexusHeal   => 30,
		_                       => 25,
	};
}
