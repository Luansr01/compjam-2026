using Godot;
using System;

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

	public void AddKill(f64 difficulty = 1.0)
	{
		Score += (i32)Math.Round(pointsPerKill * difficulty);
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
