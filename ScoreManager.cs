using Godot;
using System;
using System.Collections.Generic;

public partial class ScoreManager : Node
{
	public static ScoreManager Instance { get; private set; }

	public i32 Score { get; private set; }

	public event Action<i32> scoreChanged;

	public override void _EnterTree() => Instance = this;

	public override void _ExitTree()
	{
		if (Instance != this) return;
		Instance      = null;
		scoreChanged  = null;
	}

	/// Worth multiplier for an enemy that has been alive through this much run
	/// time. Snapshotted onto the enemy when it spawns.
	public f64 KillDifficultyAt(f64 minutes) => 1.0 + Tuning.PointsPerMinute * minutes;

	/// Extra worth while the Nexus is hurting, so a comeback can be funded.
	public f64 LowHealthMultiplier(f64 healthFraction) =>
		1.0 + (1.0 - healthFraction) * Tuning.LowHealthScoreBonus;

	public void AddKill(f64 difficulty = 1.0)
	{
		Score += (i32)Math.Round(Tuning.PointsPerKill * difficulty);
		scoreChanged?.Invoke(Score);
	}

	public bool TrySpend(i32 amount)
	{
		if (amount < 0 || Score < amount) return false;
		Score -= amount;
		scoreChanged?.Invoke(Score);
		return true;
	}
}
