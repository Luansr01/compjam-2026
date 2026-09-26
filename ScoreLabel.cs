using Godot;
using System;

public partial class ScoreLabel : Label
{
	i32 _score = 0;

	public i32 Score => _score;

	public override void _Ready()
	{
		UpdateScoreDisplay();
	}

	private void UpdateScoreDisplay()
	{
		Text = $"Score: {_score}";
	}
}
