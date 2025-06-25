using Sandbox;
using System.Text.Json;

public sealed class DataManager : Component
{
	[Property] private bool EnableTestData { get; set; } = false;
	
	[Property] private int InitialMoney { get; set; } = 100;
	[Property] private int InitialLevel { get; set; } = 1;
	[Property] public int MaxLevel { get; set; } = PlayerLevelSystem.MAX_LEVEL;
	
	private PlayerData currentPlayerData;
	private const string SAVE_FILE_NAME = "playerdata.json";
	
	public PlayerData CurrentPlayerData => currentPlayerData;
	
	protected override void OnUpdate()
	{

	}

	protected override void OnStart()
	{
		LoadPlayerData();
		
		if (currentPlayerData == null)
		{
			CreateNewPlayerData();
		}
	}
	
	/// <summary>
	/// Load player data from file
	/// </summary>
	private void LoadPlayerData()
	{
		try
		{
			if (FileSystem.Data.FileExists(SAVE_FILE_NAME))
			{
				var jsonString = FileSystem.Data.ReadAllText(SAVE_FILE_NAME);
				currentPlayerData = JsonSerializer.Deserialize<PlayerData>(jsonString);
			}
			else
			{
			}
		}
		catch (System.Exception)
		{
			currentPlayerData = null;
		}
	}
	
	/// <summary>
	/// Save current player data to file
	/// </summary>
	public void SavePlayerData()
	{
		try
		{
			if (currentPlayerData != null)
			{
				var jsonString = JsonSerializer.Serialize(currentPlayerData, new JsonSerializerOptions 
				{ 
					WriteIndented = true 
				});
				
				FileSystem.Data.WriteAllText(SAVE_FILE_NAME, jsonString);	
			}
		}
		catch (System.Exception)
		{
		}
	}
	
	/// <summary>
	/// Create new player data with initial values
	/// </summary>
	private void CreateNewPlayerData()
	{
		currentPlayerData = new PlayerData
		{
			Money = EnableTestData ? InitialMoney * 10 : InitialMoney,
			Level = InitialLevel,
			ExperiencePoints = 0,
			LastPlayTime = System.DateTime.Now
		};
		
		SavePlayerData(); // Auto-save new data
	}
	
	/// <summary>
	/// Add money to player data
	/// </summary>
	public void AddMoney(int amount)
	{
		if (currentPlayerData != null && amount > 0)
		{
			currentPlayerData.Money += amount;
		}
	}
	
	/// <summary>
	/// Spend money from player data
	/// </summary>
	public bool SpendMoney(int amount)
	{
		if (currentPlayerData != null && amount > 0 && currentPlayerData.Money >= amount)
		{
			currentPlayerData.Money -= amount;
			return true;
		}
		return false;
	}
	
	/// <summary>
	/// Add experience points and handle level up using BaseWars logic
	/// </summary>
	public void AddExperience(int exp)
	{
		if (currentPlayerData != null && exp > 0)
		{
			// Use PlayerLevelSystem to add XP with validation
			int newXP = PlayerLevelSystem.AddXP(currentPlayerData.ExperiencePoints, exp);
			currentPlayerData.ExperiencePoints = newXP;
			
			// Check for level up using BaseWars logic
			var (newLevel, finalXP, leveledUp) = PlayerLevelSystem.CheckLevels(
				currentPlayerData.Level, 
				currentPlayerData.ExperiencePoints
			);
			
			currentPlayerData.Level = newLevel;
			currentPlayerData.ExperiencePoints = finalXP;
			
		}
	}
	
	/// <summary>
	/// Set player level directly using BaseWars validation
	/// </summary>
	public void SetLevel(int level)
	{
		if (currentPlayerData != null)
		{
			currentPlayerData.Level = PlayerLevelSystem.SetLevel(level);
		}
	}
	
	/// <summary>
	/// Set player XP directly using BaseWars validation
	/// </summary>
	public void SetXP(int xp)
	{
		if (currentPlayerData != null)
		{
			currentPlayerData.ExperiencePoints = PlayerLevelSystem.SetXP(xp);
			
			// Check for level up after setting XP
			var (newLevel, finalXP, leveledUp) = PlayerLevelSystem.CheckLevels(
				currentPlayerData.Level, 
				currentPlayerData.ExperiencePoints
			);
			
			currentPlayerData.Level = newLevel;
			currentPlayerData.ExperiencePoints = finalXP;
			
			if (leveledUp)
			{
			}
		}
	}
	
	/// <summary>
	/// Check if player has reached specified level
	/// </summary>
	public bool HasLevel(int requiredLevel)
	{
		if (currentPlayerData == null) return false;
		return PlayerLevelSystem.HasLevel(currentPlayerData.Level, requiredLevel);
	}
	
	/// <summary>
	/// Get XP required for next level
	/// </summary>
	public int GetXPForNextLevel()
	{
		if (currentPlayerData == null) return 0;
		return PlayerLevelSystem.GetXPNextLevel(currentPlayerData.Level);
	}
	
	/// <summary>
	/// Get level progress as percentage
	/// </summary>
	public float GetLevelProgress()
	{
		if (currentPlayerData == null) return 0f;
		return PlayerLevelSystem.GetLevelProgress(currentPlayerData.Level, currentPlayerData.ExperiencePoints);
	}
	
	/// <summary>
	/// Add rebirth to player data
	/// </summary>
	public void AddRebirth(int amount = 1)
	{
		if (currentPlayerData != null && amount > 0)
		{
			currentPlayerData.Rebirth += amount;
		}
	}
	
	public void ResetRebirth()
	{
		if (currentPlayerData != null)
		{
			currentPlayerData.Rebirth = 0;
		}
	}
	
	/// <summary>
	/// Reset player for rebirth (keeps rebirth count)
	/// </summary>
	public void DoRebirth()
	{
		if (currentPlayerData != null)
		{
			int currentRebirths = currentPlayerData.Rebirth;
			currentPlayerData.Level = InitialLevel;
			currentPlayerData.ExperiencePoints = 0;
			currentPlayerData.Rebirth = currentRebirths + 1;
			
		}
	}
	
	/// <summary>
	/// Update last play time
	/// </summary>
	public void UpdateLastPlayTime()
	{
		if (currentPlayerData != null)
		{
			currentPlayerData.LastPlayTime = System.DateTime.Now;
		}
	}
	
	/// <summary>
	/// Reset all player data
	/// </summary>
	public void ResetPlayerData()
	{
		CreateNewPlayerData();
	}
	
	protected override void OnDestroy()
	{
		UpdateLastPlayTime();
		SavePlayerData();
	}
}

/// <summary>
/// Player data structure for serialization
/// </summary>
public class PlayerData
{
	public int Money { get; set; }
	public int Level { get; set; }
	public int ExperiencePoints { get; set; }
	public int Rebirth { get; set; } = 0;
	public System.DateTime LastPlayTime { get; set; }
	
}
