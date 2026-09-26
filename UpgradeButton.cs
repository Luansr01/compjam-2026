using Godot;
using System;

[GlobalClass]
public partial class UpgradeButton : Button
{
	[Export] UpgradeKind kind;

	public override void _Ready()
	{
		Pressed += OnPressed;
		if (UpgradeManager.Instance != null) UpgradeManager.Instance.changed += Refresh;
		if (ScoreManager.Instance    != null) ScoreManager.Instance.scoreChanged += OnScoreChanged;
		Refresh();
	}

	public override void _ExitTree()
	{
		if (UpgradeManager.Instance != null) UpgradeManager.Instance.changed -= Refresh;
		if (ScoreManager.Instance    != null) ScoreManager.Instance.scoreChanged -= OnScoreChanged;
	}

	void OnPressed() => UpgradeManager.Instance?.TryPurchase(kind);

	void OnScoreChanged(i32 score) => Refresh();

	void Refresh()
	{
		if (UpgradeManager.Instance == null) return;

		i32 cost  = UpgradeManager.Instance.Cost(kind);
		i32 level = UpgradeManager.Instance.Level(kind);

		Text        = $"{LabelFor(kind)}  Lv{level}\n{cost} pts";
		TooltipText = $"Level {level + 1} costs {cost} points.";
		Disabled    = !UpgradeManager.Instance.CanAfford(kind)
				   || !UpgradeManager.Instance.IsUseful(kind);
	}

	static string LabelFor(UpgradeKind kind) => kind switch {
		UpgradeKind.MeleeDamage => "Damage",
		UpgradeKind.AttackSpeed => "Attack Speed",
		UpgradeKind.MoveSpeed   => "Move Speed",
		UpgradeKind.Heal        => "Heal",
		UpgradeKind.NexusHeal   => "Nexus Heal",
		_                       => "Upgrade",
	};
}
