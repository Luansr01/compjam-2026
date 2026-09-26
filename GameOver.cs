using Godot;

public partial class GameOver : CanvasLayer
{
	[Export] Label scoreLabel;
	[Export] Label timeLabel;
	[Export] Button restartButton;

	public override void _Ready()
	{
		Visible = false;

		// Every export is a NodePath the scene has to fill in, and C# cannot catch
		// a missing one at compile time, so a null here would otherwise take out
		// _Ready and leave the screen permanently invisible.
		if (restartButton == null)
			GD.PushError("GameOver: restartButton is not wired up in game_over.tscn.");
		else
			restartButton.Pressed += Restart;
	}

	/// Named Present rather than Show because CanvasItem already defines Show().
	public void Present()
	{
		if (scoreLabel != null)
			scoreLabel.Text = $"Score: {ScoreManager.Instance?.Score ?? 0}";

		Nexus nexus = GetTree().Root.FirstOrDefaultNodeOfType<Nexus>();
		if (timeLabel != null)
			timeLabel.Text = nexus == null ? "" : $"Survived {nexus.Elapsed:0}s";

		Visible = true;
		GetTree().Paused = true;
	}

	void Restart()
	{
		// Autoloads outlive a scene reload, so the run state has to be cleared by
		// hand or the next run starts with the last one's score and free upgrades.
		ScoreManager.Instance?.Reset();
		UpgradeManager.Instance?.Reset();
		ActiveItemManager.Instance?.Reset();

		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
}
