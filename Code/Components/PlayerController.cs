using System;
using Sandbox.Citizen;

public sealed class PlayerController : Component
{
	[RequireComponent] CharacterController characterController { get; set; }

	public Faction CurrentFaction { get; set; } = null; 


	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		var composants = Scene.GetAllComponents<FactionComponent>();
		var factionComponent = composants.FirstOrDefault();
		factionComponent.CleanupPlayer(GameObject);
	}

	public bool IsInFaction()
	{
		return CurrentFaction != null;
	}

	public bool IsInFaction(string factionName)
	{
		return CurrentFaction != null && CurrentFaction.Name == factionName;
	}

	public bool IsLeader()
	{
		return CurrentFaction != null && CurrentFaction.Leader == GameObject;
	}

	public string GetFactionName()
	{
		return CurrentFaction?.Name ?? "";
	}

	public void UpdateFaction(Faction newFaction)
	{
		CurrentFaction = newFaction;
	}

	public void ClearFaction()
	{
		if (CurrentFaction != null)
		{
			var oldFactionName = CurrentFaction.Name;
			CurrentFaction = null;
			Log.Info($"Player {GameObject.Name} faction cleared (was: {oldFactionName})");
		}
	}
}