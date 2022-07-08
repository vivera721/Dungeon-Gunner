using UnityEngine;

[System.Serializable]
public class RoomEnemySpawnParameters
{
    #region Tooltip
    [Tooltip("Defines the dungeon level for this room with regard to how many enemies in total be spawned")]
    #endregion
    public DungeonLevelSO dungeonLevel;
    #region Tooltip
    [Tooltip("The minimum number of enemies to spawn in this room for this dungeon level. The actual number will be a random value between the minimum and maximum values")]
    #endregion
    public int minTotalEnemiesToSpawn;
    #region Tooltip
    [Tooltip("The maximum number of enemies to spawn in this room for this dungeon level. The actual number will be a random value between the minimum and maximum values")]
    #endregion
    public int maxTotalEnemiesToSpawn;
    #region Tooltip
    [Tooltip("The minimum number of concurrent enemies to spawn in this room for this dungeon level. The actual number will be a random value between the minimum and maximum values")]
    #endregion
    public int minConcurrentEnemies;
    #region Tooltip
    [Tooltip("The maximum number of concurrent enemies to spawn in this room for this dungeon level. The actual number will be a random value between the minimum and maximum values")]
    #endregion
    public int maxConcurrentEnemies;
    #region Tooltip
    [Tooltip("The minimum number of interval in seconds for enemies in this room for this dungeon level. The actual number will be a random value between the minimum and maximum values")]
    #endregion
    public int minSpawnInterval;
    #region Tooltip
    [Tooltip("The maximum number of interval in seconds for enemies in this room for this dungeon level. The actual number will be a random value between the minimum and maximum values")]
    #endregion
    public int maxSpawnInterval;




}
