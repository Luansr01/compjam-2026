using Godot;
using System;

public partial class NexusHpBar : TextureProgressBar
{
	[Export] Nexus nexus;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Value = nexus.Health.CurrentHealth;
		MaxValue = nexus.Health.MaxHealth;
	}
}
