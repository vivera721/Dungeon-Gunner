using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [System.Serializable]
    private struct RangeByLevel
    {
        public DungeonLevelSO dungeonLevel;
        [Range(0, 100)] public int min;
        [Range(0, 100)] public int max;
    }

    #region Header CHEST PREFAB
    [Space(10)]
    [Header("CHEST PREFAB")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the chest prefab")]
    #endregion
    [SerializeField] private GameObject chestPrefab;

    #region Header CHEST SPAWN CHANCE
    [Space(10)]
    [Header("CHEST SPAWN CHANCE")]
    #endregion
    #region Tooltip
    [Tooltip("The minimum probability for spawning a chest")]
    #endregion
    [SerializeField][Range(0, 100)] private int chestSpawnChangeMin;
    #region Tooltip
    [Tooltip("The maximum probability for spawning a chest")]
    #endregion
    [SerializeField][Range(0, 100)] private int chestSpawnChangeMax;
    #region Tooltip
    [Tooltip("You can override the chest spawn chanve by dungeon level")]
    #endregion
    [SerializeField] private List<RangeByLevel> chestSpawnChangeByLevelList;

    #region Header CHEST SPAWN DETAILS
    [Space(10)]
    [Header("CHEST SPAWN DETAILS")]
    #endregion
    [SerializeField] private ChestSpawnEvent chestSpawnEvent;
    [SerializeField] private ChestSpawnPosition chestSpawnPosition;
    #region Tooltip
    [Tooltip("The minimum number of items to spawn (note that a maximum of 1 of each type of ammo, health, and weapon will be spawned)")]
    #endregion
    [SerializeField][Range(0, 3)] private int numberOfItemsToSpawnMin;
    #region Tooltip
    [Tooltip("The maximum number of items to spawn (note that a maximum of 1 of each type of ammo, health, and weapon will be spawned)")]
    #endregion
    [SerializeField][Range(0, 3)] private int numberOfItemsToSpawnMax;

    #region Header CHEST CONTENT DETAILS
    [Space(10)]
    [Header("CHEST CONTENT DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("The weapons to spawn for each dungeon level and their spawn ratios")]
    #endregion
    [SerializeField] private List<SpawnableObjectsByLevel<WeaponDetailsSO>> weaponSpawnByLevelList;
    #region Tooltip
    [Tooltip("The range of health to spawn for each level")]
    #endregion
    [SerializeField] private List<RangeByLevel> healthSpawnByLevelList;
    #region Tooltip
    [Tooltip("The range of ammo to spawn for each level")]
    #endregion
    [SerializeField] private List<RangeByLevel> ammoSpawnByLevelList;

    private bool chestSpawned = false;
    private Room chestRoom;

    // Subscribe event
    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
    }

    // Unsubscribe event
    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        // Get the room the chest is in if we don't already have it
        if (chestRoom == null)
        {
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        // If the chest is spawned on room entry then spawn chest
        if (!chestSpawned && chestSpawnEvent == ChestSpawnEvent.onRoomEntry && chestRoom == roomChangedEventArgs.room)
        {
            SpawnChest();
        }
    }


    /// <summary>
    /// Handle room enemies defeated event
    /// </summary>
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        // Get the room the chest is in if we don't already have it
        if (chestRoom == null)
        {
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        // If the chest is spawned when enemies are defeated and the chest is in the room that the enemies have been defeated
        if (!chestSpawned && chestSpawnEvent == ChestSpawnEvent.onEnemiesDefeated && chestRoom == roomEnemiesDefeatedArgs.room)
        {
            SpawnChest();
        }
    }


    private void SpawnChest()
    {
        chestSpawned = true;

        // Should chest be spawned based on specified chance? if no return
        if (!RandomSpawnChest()) return;

        // Get number of ammo, health, and weapon items to spawn (max 1 of each)
        GetItemsToSpawn(out int ammoNum, out int healthNum, out int weaponNum);

        // instantiate chest
        GameObject chestGameObject = Instantiate(chestPrefab, this.transform);

        // Position chest
        // 생성위치가 지정된 경우라면 그냥 생성하고
        if (chestSpawnPosition == ChestSpawnPosition.atSpawnPosition)
        {
            chestGameObject.transform.position = this.transform.position;
        }
        // 생성위치가 플레이어 근처라면 새로 무작위 위치를 만들어서 플레이어 근처에 생성한다
        else if (chestSpawnPosition == ChestSpawnPosition.atPlayerPoition)
        {
            // Get nearest spawn position to player
            Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(GameManager.Instance.GetPlayer().transform.position);

            // Calculate some random variation
            Vector3 variation = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            chestGameObject.transform.position = spawnPosition + variation;
        }

        // Get chest component
        Chest chest = chestGameObject.GetComponent<Chest>();

        // Initialize chest
        if (chestSpawnEvent == ChestSpawnEvent.onRoomEntry)
        {
            // Don't use materialize effect
            chest.Initialize(false, GetHealthPercentToSpawn(healthNum), GetWeaponDetailsToSpawn(weaponNum), GetAmmoPercentToSpawn(ammoNum));
        }
        else
        {
            // use materialize effect
            chest.Initialize(true, GetHealthPercentToSpawn(healthNum), GetWeaponDetailsToSpawn(weaponNum), GetAmmoPercentToSpawn(ammoNum));
        }

    }

    /// <summary>
    /// Check if a chest should be spawned based on the chest spawn chance - returns true if chest should be spawned false otherwise
    /// </summary>
    /// <returns></returns>
    private bool RandomSpawnChest()
    {
        int chancePercent = Random.Range(chestSpawnChangeMin, chestSpawnChangeMax);

        // Check if an override chance percent has been set for the current level
        foreach (RangeByLevel rangeByLevel in chestSpawnChangeByLevelList)
        {
            if (rangeByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                chancePercent = Random.Range(rangeByLevel.min, rangeByLevel.max + 1);
                break;
            }
        }

        // get random value between 1 and 100
        int randomPercent = Random.Range(1, 100 + 1);

        if (randomPercent <= chancePercent)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// Get the number of items to spawn - max 1 of each - max 3 in total
    /// 얻을 아이템의 수 - 1 개씩 생성 - 총 3 개
    /// </summary>
    private void GetItemsToSpawn(out int ammo, out int health, out int weapons)
    {
        ammo = 0;
        health = 0;
        weapons = 0;

        int numberOfItemsToSpawn = Random.Range(numberOfItemsToSpawnMin, numberOfItemsToSpawnMax + 1);

        int choice;

        // 생성하고자 하는 아이템이 1 개일때
        if (numberOfItemsToSpawn == 1)
        {
            choice = Random.Range(0, 3);
            if (choice == 0) { weapons++; return; }
            if (choice == 1) { ammo++; return; }
            if (choice == 2) { health++; return; }
            return;
        }
        // 2 개일때
        else if (numberOfItemsToSpawn == 2)
        {
            choice = Random.Range(0, 3);
            if (choice == 0) { weapons++; ammo++; return; }
            if (choice == 1) { ammo++; health++; return; }
            if (choice == 2) { health++; weapons++; return; }
            return;
        }
        // 3 개이상일떄
        else if (numberOfItemsToSpawn >= 3)
        {
            choice = Random.Range(0, 3);
            weapons++; 
            ammo++; 
            health++; 
            return;
        }
    }

    /// <summary>
    /// Get ammo percent to spawn
    /// </summary>
    private int GetAmmoPercentToSpawn(int ammoNumber)
    {
        if (ammoNumber == 0) return 0;

        // Get ammo spawn percent range for level
        foreach (RangeByLevel spawnPercentByLevel in ammoSpawnByLevelList)
        {
            if (spawnPercentByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                return Random.Range(spawnPercentByLevel.min, spawnPercentByLevel.max);
            }
        }

        return 0;
    }
    
    /// <summary>
    /// Get health percent to spawn
    /// </summary>
    private int GetHealthPercentToSpawn(int healthNumber)
    {
        if (healthNumber == 0) return 0;

        // Get ammo spawn percent range for level
        foreach (RangeByLevel spawnPercentByLevel in healthSpawnByLevelList)
        {
            if (spawnPercentByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                return Random.Range(spawnPercentByLevel.min, spawnPercentByLevel.max);
            }
        }

        return 0;
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNumber)
    {
        if (weaponNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        // 클래스를 목록 항목을 지정된의 상대적인 '비율'을 기준으로 무작위 상품을 선택하는 데 사용된 인스턴스를 만든다
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByLevelList);

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();

        return weaponDetails;
    }


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestPrefab), chestPrefab);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(chestSpawnChangeMin), chestSpawnChangeMin, nameof(chestSpawnChangeMax), chestSpawnChangeMax, true);

        if (chestSpawnChangeByLevelList != null && chestSpawnChangeByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(chestSpawnChangeByLevelList), chestSpawnChangeByLevelList);

            foreach (RangeByLevel rangeByLevel in chestSpawnChangeByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }

        HelperUtilities.ValidateCheckPositiveRange(this, nameof(numberOfItemsToSpawnMin), numberOfItemsToSpawnMin, nameof(numberOfItemsToSpawnMax), numberOfItemsToSpawnMax, true);

        if (weaponSpawnByLevelList != null && weaponSpawnByLevelList.Count > 0)
        {
            foreach (SpawnableObjectsByLevel<WeaponDetailsSO> weaponDetailsByLevel in weaponSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(weaponDetailsByLevel.dungeonLevel), weaponDetailsByLevel.dungeonLevel);

                foreach (SpawnableObjectRatio<WeaponDetailsSO> weaponRatio in weaponDetailsByLevel.spawnableObjectRatioList)
                {
                    HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRatio.dungeonObject), weaponRatio.dungeonObject);

                    HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponRatio.ratio), weaponRatio.ratio, true);
                }
            }
        }

        if (healthSpawnByLevelList != null && healthSpawnByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(healthSpawnByLevelList), healthSpawnByLevelList);

            foreach (RangeByLevel rangeByLevel in healthSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }

        if (ammoSpawnByLevelList != null && ammoSpawnByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoSpawnByLevelList), ammoSpawnByLevelList);

            foreach (RangeByLevel rangeByLevel in ammoSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }
    }
#endif
    #endregion

}
