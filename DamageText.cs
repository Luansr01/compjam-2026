using Godot;

public partial class DamageText : Label
{
	public void Play(Vector2 at, Vector2 drift, f64 amount, Color tint, f32 size, f32 seconds)
	{
		GlobalPosition = at - Size * 0.5f;
		Modulate       = tint;
		ZIndex         = 100;

		AddThemeFontSizeOverride("font_size", (int)size);
		AddThemeColorOverride("font_outline_color", Colors.Black);
		AddThemeConstantOverride("outline_size", 7);

		Text = $"{amount:0}";

		Tween tween = CreateTween();
		tween.SetParallel();
		tween.TweenProperty(this, "global_position", GlobalPosition + drift, seconds)
		     .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "modulate:a", 0.0f, seconds * 0.55f)
		     .SetDelay(seconds * 0.45f);
		tween.Chain().TweenCallback(Callable.From(QueueFree));
	}
}
