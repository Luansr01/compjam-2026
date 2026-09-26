using Godot;
using System;

public partial class Utils : Node
{
	static public void DEBUG(Action func){
		if(OS.IsDebugBuild()){
			func();
		}
	}
}
