using Sandbox;

public class Faction
{
	public GameObject Leader { get; set; }
	public string Name { get; set; }
    public string Password { get; set; }
    public Dictionary<string, GameObject> Members { get; set; } = new Dictionary<string, GameObject>();
    public string Color { get; set; }
}

public sealed class FactionComponent : Component
{
	[Sync] public Dictionary<string, Faction> FactionTable { get; set; } = new Dictionary<string, Faction>();

	protected override void OnUpdate()
	{

	}

	public Faction createFaction(GameObject player, string factionName, string password, string color)
	{
		if (FactionTable.ContainsKey(factionName))
		{
			Log.Info($"Faction with name {factionName} already exists");
			return null;
		}

		Faction faction = new Faction();
		faction.Leader = player;
		faction.Name = factionName;
		faction.Password = password;
		faction.Color = color;
		faction.Members.Add(player.Name, player);
		
		FactionTable.Add(factionName, faction);

		var playerComponent = player.Components.Get<PlayerComponent>();
		playerComponent?.UpdateFaction(faction);

		Log.Info($"Faction created: {factionName} by {player.Name}");
		return faction;
	}

	public Faction getFaction(string factionName)
	{
		if (FactionTable.TryGetValue(factionName, out Faction faction))
		{
			return faction;
		}
		return null;
	}

	public bool JoinFaction(GameObject player, string factionName, string password = "")
	{
		var faction = getFaction(factionName);
		if (faction == null)
		{
			Log.Info($"Faction {factionName} does not exist");
			return false;
		}

		if (!string.IsNullOrEmpty(faction.Password) && faction.Password != password)
		{
			Log.Info($"Wrong password for faction {factionName}");
			return false;
		}

		LeaveFaction(player);

		faction.Members[player.Name] = player;

		var playerComponent = player.Components.Get<PlayerComponent>();
		playerComponent?.UpdateFaction(faction);

		Log.Info($"{player.Name} joined faction {factionName}");
		return true;
	}

	public bool LeaveFaction(GameObject player, bool disbandIfLeader = false)
	{
		var currentFaction = GetPlayerFaction(player);
		if (currentFaction == null) return false;

		var isLeader = IsPlayerLeader(player, currentFaction);

		if (isLeader && disbandIfLeader)
		{
			return DisbandFaction(currentFaction.Name);
		}
		else if (isLeader && !disbandIfLeader)
		{
			Log.Info($"Leader {player.Name} cannot leave faction {currentFaction.Name} without disbanding");
			return false;
		}

		currentFaction.Members.Remove(player.Name);

		var playerComponent = player.Components.Get<PlayerComponent>();
		playerComponent?.ClearFaction();

		if (currentFaction.Members.Count == 0)
		{
			DisbandFaction(currentFaction.Name);
		}

		Log.Info($"{player.Name} left faction {currentFaction.Name}");
		return true;
	}

	public bool DisbandFaction(string factionName)
	{
		var faction = getFaction(factionName);
		if (faction == null) return false;

		var membersList = faction.Members.Values.ToList();

		foreach (var member in membersList)
		{
			var playerComponent = member?.Components?.Get<PlayerComponent>();
			if (playerComponent != null)
			{
				playerComponent.ClearFaction();
				Log.Info($"{member.Name} removed from disbanded faction {factionName}");
			}
		}

		faction.Members.Clear();
		faction.Leader = null;

		FactionTable.Remove(factionName);
		Log.Info($"Faction {factionName} has been disbanded");

		return true;
	}

	public Faction GetPlayerFaction(GameObject player)
	{
		foreach (var faction in FactionTable.Values)
		{
			if (faction.Members.ContainsValue(player))
			{
				return faction;
			}
		}
		return null;
	}

	public bool IsPlayerInFaction(GameObject player, string factionName = null)
	{
		var faction = GetPlayerFaction(player);
		
		if (string.IsNullOrEmpty(factionName))
		{
			return faction != null;
		}
		
		return faction != null && faction.Name == factionName;
	}

	public bool IsPlayerLeader(GameObject player, Faction faction = null)
	{
		faction ??= GetPlayerFaction(player);
		if (faction == null) return false;
		
		return faction.Leader == player;
	}

	public List<GameObject> GetFactionMembers(string factionName)
	{
		var faction = getFaction(factionName);
		if (faction == null) return new List<GameObject>();
		
		return faction.Members.Values.Where(p => p.IsValid()).ToList();
	}

	public bool ChangePassword(GameObject player, string newPassword)
	{
		var faction = GetPlayerFaction(player);
		if (faction == null) return false;
		
		if (!IsPlayerLeader(player, faction))
		{
			Log.Info($"{player.Name} is not leader, cannot change password");
			return false;
		}
		
		faction.Password = newPassword;
		Log.Info($"Password changed for faction {faction.Name}");
		return true;
	}

	public bool FactionExists(string factionName)
	{
		return FactionTable.ContainsKey(factionName);
	}

	public Dictionary<string, Faction> GetAllFactions()
	{
		return new Dictionary<string, Faction>(FactionTable);
	}

	public int GetFactionCount()
	{
		return FactionTable.Count;
	}


	public void CleanupPlayer(GameObject player)
	{
		var faction = GetPlayerFaction(player);
		if (faction != null)
		{
			if (IsPlayerLeader(player, faction))
			{
				DisbandFaction(faction.Name);
			}
			else
			{
				LeaveFaction(player);
			}
		}
	}
}
