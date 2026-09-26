using Godot;
using System.Collections.Generic;

public enum SfxCue {
	None,
	PlayerAttack,
}

/// Every sound effect in the game, in one place. Registered as the "Sfx"
/// autoload so streams are assigned on a single node instead of being scattered
/// across the scenes that trigger them.
///
/// Plays through a small pool of voices rather than one player per sound, so a
/// burst of hits layers instead of cutting each other off. The Nexus in
/// particular takes damage continuously from a whole wave, which retriggers a
/// single player every time and stutters.
public partial class Sfx : Node
{
	public static Sfx Instance { get; private set; }

	[Export] AudioStream playerAttack;
	[Export] AudioStream playerHurt;
	[Export] AudioStream nexusHit;

	[Export] int voices = 4;
	[Export] f64 volumeDb = -3.0;

	readonly List<AudioStreamPlayer> _pool = [];

	int _next;

	public override void _EnterTree() => Instance = this;

	public override void _ExitTree()
	{
		if (Instance == this) Instance = null;
	}

	public override void _Ready()
	{
		for (int i = 0; i < voices; i++) {
			AudioStreamPlayer voice = new() { Name = $"Voice{i}" };
			AddChild(voice);
			_pool.Add(voice);
		}
	}

	public void Play(SfxCue cue)
	{
		switch (cue)
		{
			case SfxCue.PlayerAttack: Play(playerAttack); break;
		}
	}

	public void PlayerHurt() => Play(playerHurt);

	public void NexusHit() => Play(nexusHit);

	void Play(AudioStream stream)
	{
		if (stream == null || _pool.Count == 0) return;

		for (int i = 0; i < _pool.Count; i++) {
			int slot = (_next + i) % _pool.Count;
			if (_pool[slot].Playing) continue;

			Start(slot, stream);
			return;
		}

		// Every voice is busy, so reuse the next one. Cutting the oldest is better
		// than dropping the sound silently, and rotating means it is never the
		// same voice being cut twice in a row.
		Start(_next, stream);
	}

	void Start(int slot, AudioStream stream)
	{
		_pool[slot].Stream   = stream;
		_pool[slot].VolumeDb = (f32)volumeDb;
		_pool[slot].Play();
		_next = (slot + 1) % _pool.Count;
	}
}
