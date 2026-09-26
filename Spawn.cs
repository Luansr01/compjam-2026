using Godot;
using System;
using System.Collections.Generic;

public partial class Spawn : Node
{
	[Export] PackedScene node;
	[Export] CircleShape2D circle;
	[Export] f64         spawnRate;
	[Export] i32         spawnCap;

	Timer<f64> _spwanRate;

	public override void _Ready()
	{
		_spwanRate = new(0, spawnRate);
	}

	public override void _Process(f64 delta)
	{
		_spwanRate.Tick(delta);
		if (GetChildCount() < spawnCap) {
			if (_spwanRate.Elapsed) {
				_spwanRate.Restart();
				SpawnObject(Random.SampleSphere(circle.Radius));                  
			}
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
	}
}
