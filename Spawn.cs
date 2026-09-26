using Godot;

[Tool]
public partial class Spawn : Node2D
{
	[Export] PackedScene node;

	const f32 RingWidth  = 2.0f;
	const f32 MarkerSize = 14.0f;

	static readonly Color OuterColor = new(0.35f, 0.90f, 0.55f, 0.9f);
	static readonly Color InnerColor = new(1.00f, 0.60f, 0.30f, 0.9f);

	f64 _elapsed;
	f32 _threat;

	Timer<f64> _spwanRate;

	f32 Outer => (f32)Tuning.OuterRadius;

	f32 Inner => Mathf.Clamp((f32)Tuning.InnerRadius, 0.0f, Outer);

	public f32 Threat => _threat;

	public i32 CurrentCap =>
		Tuning.SpawnCap
		+ (i32)(Tuning.EnemiesPerMinute * Minutes)
		+ (i32)(Tuning.ThreatCapBonus * _threat);

	f64 Minutes => _elapsed / 60.0;

	public override void _Ready()
	{
		_spwanRate = new(0, Tuning.SpawnRate);
		QueueRedraw();

		Nexus nexus = GetTree().Root.FirstOrDefaultNodeOfType<Nexus>();
		if (nexus != null && nexus.Health != null)
			nexus.Health.damaged += OnNexusDamaged;
	}

	public override void _Draw()
	{
		if (!Engine.IsEditorHint()) return;

		f32 outer = Outer;
		f32 inner = Inner;

		DrawCircle(Vector2.Zero, outer, OuterColor, filled: false, width: RingWidth, antialiased: true);
		if (inner > 0.0f)
			DrawCircle(Vector2.Zero, inner, InnerColor, filled: false, width: RingWidth, antialiased: true);

		DrawLine(new Vector2(-MarkerSize, 0), new Vector2(MarkerSize, 0), OuterColor, RingWidth, true);
		DrawLine(new Vector2(0, -MarkerSize), new Vector2(0, MarkerSize), OuterColor, RingWidth, true);

		for (int i = 0; i < 4; i++) {
			Vector2 spoke = Vector2.FromAngle(i * Mathf.Pi / 2);
			DrawLine(spoke * inner, spoke * outer, OuterColor, RingWidth, true);
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

		if (_threat > 0.0f)
			_threat = Mathf.Max(0.0f, _threat - (f32)(Tuning.ThreatDecayPerSecond * delta));

		_spwanRate.time = Tuning.SpawnRate
			/ (1.0 + Tuning.RateGrowthPerMinute * minutes + Tuning.ThreatRateBonus * _threat);

		_spwanRate.Tick(delta);
		if (GetChildCount() < CurrentCap && _spwanRate.Elapsed) {
			_spwanRate.Restart();
			SpawnObject(Random.SampleRing(Inner, Outer));
		}
	}

	void OnNexusDamaged(f64 amount) =>
		_threat = Mathf.Min(1.0f, _threat + (f32)Tuning.ThreatPerHit);

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
			control.ScaleSpeed((f32)(1.0 + Tuning.SpeedPerMinute * minutes));
			control.SetKillDifficulty(ScoreManager.Instance?.KillDifficultyAt(minutes) ?? 1.0);
		}

		if (enemy.FirstOrDefaultNodeOfType<MeleeAttack>() is var attack)
			attack.SetDamageScale(1.0 + Tuning.DamagePerMinute * minutes);

		if (Tuning.HealthPerMinute > 0 &&
			enemy.FirstOrDefaultNodeOfType<HealthComponent>() is var health)
			health.SetBonusMaxHealth(health.BaseMaxHealth * Tuning.HealthPerMinute * minutes);
	}

	public void ResetDifficulty()
	{
		_elapsed = 0;
		_threat  = 0.0f;
		_spwanRate.Restart();
	}
}
