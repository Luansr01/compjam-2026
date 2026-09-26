using Godot;

[GlobalClass]
public partial class MusicPlayer : AudioStreamPlayer
{
	public override void _Ready()
	{
		EnableLoop();
		Play();
	}

	void EnableLoop()
	{
		switch (Stream)
		{
			case AudioStreamOggVorbis ogg:
				ogg.Loop = true;
				break;
			case AudioStreamMP3 mp3:
				mp3.Loop = true;
				break;
		}
	}
}
