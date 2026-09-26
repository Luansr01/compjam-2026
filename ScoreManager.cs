using Godot;
using System;

/// Run-wide score state, registered as the "ScoreManager" autoload. Nodes that
/// come and go (spawned enemies) report here so the total has one owner that
/// outlives any single entity.
public partial class ScoreManager : Node
{
	public static ScoreManager Instance { get; private set; }

	[Export] i32 pointsPerKill = 1;

	public i32 Score { get; private set; }

	public event Action<i32> scoreChanged;

	public override void _EnterTree() => Instance = this;

	public override void _ExitTree()
	{
		if (Instance != this) return;
		Instance      = null;
		scoreChanged  = null;
	}

	public void AddKill()
	{
		Score += pointsPerKill;
		scoreChanged?.Invoke(Score);
	}

	/// Spend points, e.g. on an upgrade. Returns false and changes nothing if the
	/// score can't cover it, so callers can use this as the purchase check.
	public bool TrySpend(i32 amount)
	{
		if (amount < 0 || Score < amount) return false;
		Score -= amount;
		scoreChanged?.Invoke(Score);
		return true;
	}
}
