using Godot;
using System;

[GlobalClass]
public partial class HpBar : TextureProgressBar
{
	[Export] PlayerControl player;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		Value = player.Health.CurrentHealth;
		MaxValue = player.Health.MaxHealth;
	}
}
