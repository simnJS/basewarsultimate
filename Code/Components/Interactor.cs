
using static Sandbox.Component;

public sealed class Interactor : Component, IPressable
{
	public bool Press( IPressable.Event e )
	{
		Log.Info("I was pressed!");
		
		return true;
	}

	protected override void OnUpdate()
	{
		
	}
}
