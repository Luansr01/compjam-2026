using Godot;

public partial class Pulse : Node2D
{
	f32 _t;

	public override void _Process(double delta)
	{
		_t += (f32)delta;
		QueueRedraw();
		if (_t >= (f32)Tuning.PulseSeconds) QueueFree();
	}

	public override void _Draw()
	{
		f32 k     = Mathf.Clamp(_t / (f32)Tuning.PulseSeconds, 0.0f, 1.0f);
		f32 eased = Ease(k);

		Color hot = Tuning.PulseCoreColor;
		hot.A = Tuning.PulseCoreColor.A * (1.0f - k) * (1.0f - k);
		if (hot.A > 0.01f)
			DrawCircle(Vector2.Zero, (f32)Tuning.ShockwaveRadius * eased * 0.62f, hot, filled: false, width: (f32)Tuning.PulseCoreWidth * (1.0f - k), antialiased: true);

		for (int i = 0; i < Tuning.PulseRingCount; i++)
		{
			f32 delay = i / (f32)Tuning.PulseRingCount * 0.38f;
			f32 t     = Mathf.Clamp((k - delay) / (1.0f - delay), 0.0f, 1.0f);
			if (t <= 0.0f) continue;

			f32    radius = (f32)Tuning.ShockwaveRadius * Ease(t) * (1.0f - i * 0.11f);
			Color  band   = Tuning.PulseEdgeColor;
			band.A = Tuning.PulseEdgeColor.A * (1.0f - t) * (1.0f - t);

			DrawArc(Vector2.Zero, radius, 0.0f, Mathf.Tau, 72, band, (f32)Tuning.PulseBandWidth * (1.0f - t * 0.6f), true);

			Color rim = Tuning.PulseCoreColor;
			rim.A = Tuning.PulseCoreColor.A * (1.0f - t);
			DrawArc(Vector2.Zero, radius, 0.0f, Mathf.Tau, 72, rim, 2.5f, true);
		}
	}

	static f32 Ease(f32 t) => 1.0f - Mathf.Pow(1.0f - t, 3.0f);
}
