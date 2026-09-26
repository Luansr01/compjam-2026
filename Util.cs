using Godot;
using System;
using System.Collections.Generic;

public static class Util {
    public static T FirstOrDefaultNodeOfType<T>(this Node root) where T : Node
    {
        if (root == null || !GodotObject.IsInstanceValid(root)) return null;

        Queue<Node> queue = [];
        queue.Enqueue(root);

        while (queue.TryDequeue(out var current))
        {
            if (!GodotObject.IsInstanceValid(current)) continue;

            if (current is T component)
                return component;
            
            foreach (Node child in current.GetChildren())
                queue.Enqueue(child);
        }
        return null; 
    }
}

public static class Random {
    public static Vector2 SampleRing(f32 innerRadius, f32 outerRadius) {
        f32 squared = Mathf.Lerp(innerRadius * innerRadius, outerRadius * outerRadius, GD.Randf());
        f32 radius  = Mathf.Sqrt(squared);
        f32 theta   = GD.Randf() * Mathf.Tau;
        return new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta));
    }
}

public static class Numbers {
	public static void Damage(Node host, Vector2 at, f64 amount, Color tint, f32 size = 36.0f, f32 seconds = 0.7f)
	{
		if (host == null || !GodotObject.IsInstanceValid(host)) return;

		DamageText text = new();
		host.GetTree().Root.AddChild(text);
		text.Play(at, new Vector2((GD.Randf() - 0.5f) * 60.0f, -90.0f), amount, tint, size, seconds);
	}
}

public static class Flash {
    static readonly Color Hurt = new(1.0f, 0.4f, 0.4f);

    public static Tween Hit(Node owner, Sprite2D sprite, f32 seconds) {
        if (sprite == null) return null;

        sprite.Modulate = Hurt;
        Tween tween = owner.CreateTween();
        tween.TweenProperty(sprite, "modulate", Colors.White, seconds);
        return tween;
    }
}
