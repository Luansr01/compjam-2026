using Godot;
using System;

public partial class ScoreLabel : Label
{
	public i32 Score => ScoreManager.Instance?.Score ?? 0;

	public override void _Ready()
	{
		if (ScoreManager.Instance != null) ScoreManager.Instance.scoreChanged += OnScoreChanged;
		UpdateScoreDisplay();
	}

	public override void _ExitTree()
	{
		if (ScoreManager.Instance != null) ScoreManager.Instance.scoreChanged -= OnScoreChanged;
	}

	void OnScoreChanged(i32 score) => UpdateScoreDisplay();

	private void UpdateScoreDisplay()
	{
		Text = $"Score: {Score}";
	}
}
