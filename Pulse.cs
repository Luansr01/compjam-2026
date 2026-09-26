using Godot;

public partial class Pulse : Node2D
{
	[Export] public f32   maxRadius = 800.0f;
	[Export] public f32   seconds   = 0.75f;
	[Export] public int   ringCount = 6;
	[Export] public f32   coreWidth = 54.0f;
	[Export] public f32   bandWidth = 40.0f;
	[Export] public Color coreColor = new(1.00f, 1.00f, 1.00f, 1.00f);
	[Export] public Color edgeColor = new(0.45f, 0.82f, 1.00f, 0.95f);

	f32 _t;

	public override void _Process(double delta)
	{
		_t += (f32)delta;
		QueueRedraw();
		if (_t >= seconds) QueueFree();
	}

	public override void _Draw()
	{
		f32 k     = Mathf.Clamp(_t / seconds, 0.0f, 1.0f);
		f32 eased = Ease(k);

		Color hot = coreColor;
		hot.A = coreColor.A * (1.0f - k) * (1.0f - k);
		if (hot.A > 0.01f)
			DrawCircle(Vector2.Zero, maxRadius * eased * 0.62f, hot, filled: false, width: coreWidth * (1.0f - k), antialiased: true);

		for (int i = 0; i < ringCount; i++)
		{
			f32 delay = i / (f32)ringCount * 0.38f;
			f32 t     = Mathf.Clamp((k - delay) / (1.0f - delay), 0.0f, 1.0f);
			if (t <= 0.0f) continue;

			f32    radius = maxRadius * Ease(t) * (1.0f - i * 0.11f);
			Color  band   = edgeColor;
			band.A = edgeColor.A * (1.0f - t) * (1.0f - t);

			DrawArc(Vector2.Zero, radius, 0.0f, Mathf.Tau, 72, band, bandWidth * (1.0f - t * 0.6f), true);

			Color rim = coreColor;
			rim.A = coreColor.A * (1.0f - t);
			DrawArc(Vector2.Zero, radius, 0.0f, Mathf.Tau, 72, rim, 2.5f, true);
		}
	}

	static f32 Ease(f32 t) => 1.0f - Mathf.Pow(1.0f - t, 3.0f);
}
