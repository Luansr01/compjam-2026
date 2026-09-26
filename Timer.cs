using System.Numerics;

public struct Timer<T>(T current, T time) where T : IFloatingPointIeee754<T> {
	public T time    = time;
	public T current = current;

	public void Tick(T delta) => current -= delta;	
	public readonly bool Elapsed => current <= T.Zero;
	public void Restart() => current = time;
}
