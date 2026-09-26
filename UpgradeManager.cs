using Godot;
using System;
using System.Collections.Generic;

public enum UpgradeKind {
	MeleeDamage,
	AttackSpeed,
	MoveSpeed,
	MaxHealth,
}

/// Purchased upgrade levels for the current run, registered as the
/// "UpgradeManager" autoload. Levels are the source of truth and the player
/// re-reads them on _Ready, so a purchase made before or after the player
/// spawns ends up in the same place. Reset by reloading the scene.
public partial class UpgradeManager : Node
{
	public static UpgradeManager Instance { get; private set; }

	[Export] f32 costGrowth = 1.6f;

	[Export] f64 damagePerLevel    = 0.25;
	[Export] f64 cooldownPerLevel  = 0.10;
	[Export] f32 moveSpeedPerLevel = 0.15f;
	[Export] f64 maxHealthPerLevel = 25.0;

	readonly Dictionary<UpgradeKind, i32> _levels = [];

	/// Fires when a level is bought. Score changes raise ScoreManager's own event.
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
		Apply(kind);
		changed?.Invoke();
		return true;
	}

	/// Push every level onto the player. Called once from PlayerControl._Ready,
	/// after its child nodes exist and MeleeAttack has built its timers.
	public void ApplyAll(PlayerControl player)
	{
		foreach (UpgradeKind kind in Enum.GetValues<UpgradeKind>()) Apply(kind, player);
	}

	/// Every effect is derived from the level rather than nudged per purchase, so
	/// applying twice lands in the same place as applying once.
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
				// Geometric decay so later levels keep paying off.
				player.Attack.SetCooldownScale(Math.Pow(1.0 - cooldownPerLevel, level));
				break;
			case UpgradeKind.MoveSpeed:
				player.SpeedScale = 1.0f + moveSpeedPerLevel * level;
				break;
			case UpgradeKind.MaxHealth:
				player.Health.SetBonusMaxHealth(maxHealthPerLevel * level);
				break;
		}
	}

	PlayerControl FindPlayer() => GetTree().Root.FirstOrDefaultNodeOfType<PlayerControl>();

	static i32 BaseCost(UpgradeKind kind) => kind switch {
		UpgradeKind.MeleeDamage => 15,
		UpgradeKind.AttackSpeed => 20,
		UpgradeKind.MoveSpeed   => 15,
		UpgradeKind.MaxHealth   => 25,
		_                       => 20,
	};
}
