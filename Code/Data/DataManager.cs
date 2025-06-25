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
		// Initialize or load data here
		Log.Info("DataManager has started.");
		
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
				Log.Info($"Player data loaded successfully. Level: {currentPlayerData.Level}, Money: {currentPlayerData.Money}");
			}
			else
			{
				Log.Info("No save file found, will create new player data.");
			}
		}
		catch (System.Exception ex)
		{
			Log.Error($"Failed to load player data: {ex.Message}");
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
				Log.Info("Player data saved successfully.");
			}
		}
		catch (System.Exception ex)
		{
			Log.Error($"Failed to save player data: {ex.Message}");
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
		
		Log.Info($"New player data created. Money: {currentPlayerData.Money}, Level: {currentPlayerData.Level}");
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
			Log.Info($"Added {amount} money. Total: {currentPlayerData.Money}");
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
			Log.Info($"Spent {amount} money. Remaining: {currentPlayerData.Money}");
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
			
			Log.Info($"Added {exp} experience. Total: {currentPlayerData.ExperiencePoints}");
			
			if (leveledUp)
			{
				Log.Info($"🎉 Level up! New level: {currentPlayerData.Level}");
			}
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
			Log.Info($"Level set to: {currentPlayerData.Level}");
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
				Log.Info($"🎉 Level up after XP adjustment! New level: {currentPlayerData.Level}");
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
			Log.Info($"Rebirth added! Total rebirths: {currentPlayerData.Rebirth}");
		}
	}
	
	public void ResetRebirth()
	{
		if (currentPlayerData != null)
		{
			currentPlayerData.Rebirth = 0;
			Log.Info("Rebirth count has been reset.");
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
			
			Log.Info($"🔄 Rebirth completed! Now at rebirth {currentPlayerData.Rebirth}");
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
		Log.Info("Player data has been reset.");
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
