using Godot;

[Tool]
public partial class Spawn : Node2D
{
	[Export] PackedScene node;
	[Export] f32        outerRadius = 500.0f;
	[Export] f32        innerRadius = 300.0f;
	[Export] f64        spawnRate;
	[Export] i32        spawnCap;

	[Export] f64 enemiesPerMinute    = 8.0;
	[Export] f64 rateGrowthPerMinute = 0.8;
	[Export] f64 speedPerMinute      = 0.08;
	[Export] f64 damagePerMinute     = 0.10;

	[Export] f64 pointsPerMinute     = 3.0;

	[Export] f64 healthPerMinute = 0.0;

	const f32 RingWidth   = 2.0f;
	const f32 MarkerSize  = 14.0f;

	static readonly Color OuterColor = new(0.35f, 0.90f, 0.55f, 0.9f);
	static readonly Color InnerColor = new(1.00f, 0.60f, 0.30f, 0.9f);

	f64 _elapsed;

	Timer<f64> _spwanRate;

	f32 Inner => Mathf.Clamp(innerRadius, 0.0f, outerRadius);

	public i32 CurrentCap => spawnCap + (i32)(enemiesPerMinute * Minutes);

	f64 Minutes => _elapsed / 60.0;

	public override void _Ready()
	{
		_spwanRate = new(0, spawnRate);
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (!Engine.IsEditorHint()) return;

		f32 inner = Inner;

		DrawCircle(Vector2.Zero, outerRadius, OuterColor, filled: false, width: RingWidth, antialiased: true);
		if (inner > 0.0f)
			DrawCircle(Vector2.Zero, inner, InnerColor, filled: false, width: RingWidth, antialiased: true);

		DrawLine(new Vector2(-MarkerSize, 0), new Vector2(MarkerSize, 0), OuterColor, RingWidth, true);
		DrawLine(new Vector2(0, -MarkerSize), new Vector2(0, MarkerSize), OuterColor, RingWidth, true);

		for (int i = 0; i < 4; i++) {
			Vector2 spoke = Vector2.FromAngle(i * Mathf.Pi / 2);
			DrawLine(spoke * inner, spoke * outerRadius, OuterColor, RingWidth, true);
		}
	}

	public override void _Process(f64 delta)
	{
		if (Engine.IsEditorHint()) {
			QueueRedraw();
			return;
		}

		_elapsed += delta;
		f64 minutes = Minutes;

		_spwanRate.time = spawnRate / (1.0 + rateGrowthPerMinute * minutes);

		_spwanRate.Tick(delta);
		if (GetChildCount() < CurrentCap && _spwanRate.Elapsed) {
			_spwanRate.Restart();
			SpawnObject(Random.SampleRing(Inner, outerRadius));
		}
	}

	public void SpawnObject(Vector2 spawnPosition)
	{
		if (node == null)
		{
			GD.PrintErr("PackedScene not assigned in the Inspector!");
			return;
		}

		Node2D newObject = node.Instantiate<Node2D>();
		newObject.Position = spawnPosition;
		AddChild(newObject);
		ApplyDifficulty(newObject);
	}

	void ApplyDifficulty(Node2D enemy)
	{
		f64 minutes = Minutes;

		if (enemy.FirstOrDefaultNodeOfType<EnemyControl>() is var control) {
			control.ScaleSpeed((f32)(1.0 + speedPerMinute * minutes));
			control.SetKillDifficulty(1.0 + pointsPerMinute * minutes);
		}

		if (enemy.FirstOrDefaultNodeOfType<MeleeAttack>() is var attack)
			attack.SetDamageScale(1.0 + damagePerMinute * minutes);

		if (healthPerMinute > 0 &&
			enemy.FirstOrDefaultNodeOfType<HealthComponent>() is var health)
			health.SetBonusMaxHealth(health.BaseMaxHealth * healthPerMinute * minutes);
	}

	public void ResetDifficulty()
	{
		_elapsed = 0;
		_spwanRate.Restart();
	}
}
