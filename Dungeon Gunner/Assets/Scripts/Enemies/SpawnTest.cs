using System.Collections.Generic;
using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    [NonReorderable]
    private List<SpawnableObjectsByLevel<EnemyDetailsSO>> testLevelSpawnList;
    [NonReorderable]
    private RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass;
    [NonReorderable]
    private List<GameObject> instantiatedEnemyList = new List<GameObject>();

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        // Destroy any spawned enemies
        // 방이 바뀔때 스폰된 모든 적 파괴
        if (instantiatedEnemyList != null && instantiatedEnemyList.Count > 0)
        {
            foreach (GameObject enemy in instantiatedEnemyList)
            {
                Destroy(enemy);
            }
        }

        RoomTemplateSO roomTemplate = DungeonBuilder.Instance.GetRoomTemplate(roomChangedEventArgs.room.templateID);

        if (roomTemplate != null)
        {
            testLevelSpawnList = roomTemplate.enemiesByLevelList;

            // Create RandomSpawnableObject helper class
            randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(testLevelSpawnList);
        }

    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            EnemyDetailsSO enemyDetails = randomEnemyHelperClass.GetItem();

            if (enemyDetails != null)
            {
                instantiatedEnemyList.Add(Instantiate(enemyDetails.enemyPrefab, HelperUtilities.GetSpawnPositionNearestToPlayer
                    (HelperUtilities.GetMouseWorldPosition()), Quaternion.identity));
            }


        }
    }


}
