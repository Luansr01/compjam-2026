using Godot;

[GlobalClass]
public partial class ItemButton : Button
{
	[Export] ActiveItem item;

	public override void _Ready()
	{
		Pressed += OnPressed;
		if (ActiveItemManager.Instance != null) ActiveItemManager.Instance.changed += Refresh;
		if (ScoreManager.Instance != null) ScoreManager.Instance.scoreChanged += OnScoreChanged;
		Refresh();
	}

	public override void _ExitTree()
	{
		if (ActiveItemManager.Instance != null) ActiveItemManager.Instance.changed -= Refresh;
		if (ScoreManager.Instance != null) ScoreManager.Instance.scoreChanged -= OnScoreChanged;
	}

	void OnPressed() => ActiveItemManager.Instance?.TryUse(item);

	void OnScoreChanged(i32 score) => Refresh();

	void Refresh()
	{
		if (ActiveItemManager.Instance == null) return;

		i32 cost = ActiveItemManager.Instance.Cost(item);
		f64 left = ActiveItemManager.Instance.Remaining(item);

		Text        = left > 0.0 ? $"{LabelFor(item)}  {left:0.0}s" : $"{LabelFor(item)}\n{cost} pts";
		TooltipText = left > 0.0 ? "Still active" : $"Costs {cost} points.";
		Disabled    = left > 0.0 || !ActiveItemManager.Instance.CanAfford(item);
	}

	static string LabelFor(ActiveItem item) => item switch {
		ActiveItem.Taunt     => "Taunt",
		ActiveItem.Shockwave => "Shockwave",
		_                    => "Item",
	};
}
