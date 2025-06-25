using Sandbox;
using System;

/// <summary>
/// Player Level System - Implements the exact BaseWars logic for level and XP management
/// Based on the original BaseWars Lua module
/// </summary>
public static class PlayerLevelSystem
{
	public const int MAX_LEVEL = 5000;
	public const int XP_MULTIPLIER = 250;
	
	/// <summary>
	/// Calculate XP required for next level
	/// Formula: (currentLevel + 1) * 250
	/// </summary>
	public static int GetXPNextLevel(int currentLevel)
	{
		return (currentLevel + 1) * XP_MULTIPLIER;
	}
	
	/// <summary>
	/// Check if player has reached specified level
	/// </summary>
	public static bool HasLevel(int playerLevel, int requiredLevel)
	{
		return playerLevel >= requiredLevel;
	}
	
	/// <summary>
	/// Set level with validation (0-5000 range)
	/// </summary>
	public static int SetLevel(int level)
	{
		if (level < 0) level = 0;
		if (level > MAX_LEVEL) level = MAX_LEVEL;
		
		return (int)Math.Round((double)level);
	}
	
	/// <summary>
	/// Set XP with validation (non-negative)
	/// </summary>
	public static int SetXP(int xp)
	{
		if (xp < 0) xp = 0;
		
		return (int)Math.Round((double)xp);
	}
	
	/// <summary>
	/// Add level to current level
	/// </summary>
	public static int AddLevel(int currentLevel, int amount)
	{
		return SetLevel(currentLevel + amount);
	}
	
	/// <summary>
	/// Add XP to current XP
	/// </summary>
	public static int AddXP(int currentXP, int amount)
	{
		return SetXP(currentXP + amount);
	}
	
	/// <summary>
	/// Check and process level ups - Exact BaseWars logic with multiple level ups
	/// Returns updated PlayerData with new level/XP values
	/// </summary>
	public static (int newLevel, int newXP, bool leveledUp) CheckLevels(int currentLevel, int currentXP)
	{
		int newLevel = currentLevel;
		int newXP = currentXP;
		bool leveledUp = false;
		
		// Keep leveling up while we have enough XP (handles massive XP gains)
		while (newXP >= GetXPNextLevel(newLevel))
		{
			// Check if already at max level
			if (newLevel >= MAX_LEVEL)
			{
				newLevel = MAX_LEVEL;
				newXP = 0;
				leveledUp = true;
				break;
			}
			
			// Calculate XP needed for next level
			int neededXP = GetXPNextLevel(newLevel);
			
			// Level up and subtract used XP (exact Lua logic)
			newLevel++;
			newXP -= neededXP;
			leveledUp = true;
			
			Log.Info($"Level up! New level: {newLevel}, Remaining XP: {newXP}");
		}
		
		return (newLevel, newXP, leveledUp);
	}
	
	/// <summary>
	/// Get total XP accumulated across all levels + current XP
	/// Useful for statistics
	/// </summary>
	public static int GetTotalXPEarned(int level, int currentXP)
	{
		int totalXP = currentXP;
		
		for (int i = 1; i < level; i++)
		{
			totalXP += GetXPNextLevel(i);
		}
		
		return totalXP;
	}
	
	/// <summary>
	/// Get level progress as percentage (0-100)
	/// </summary>
	public static float GetLevelProgress(int level, int currentXP)
	{
		if (level >= MAX_LEVEL) return 100f;
		
		int xpNeeded = GetXPNextLevel(level);
		return (float)currentXP / xpNeeded * 100f;
	}
	
	/// <summary>
	/// Get levels until max level
	/// </summary>
	public static int GetLevelsUntilMax(int currentLevel)
	{
		return Math.Max(0, MAX_LEVEL - currentLevel);
	}
} 