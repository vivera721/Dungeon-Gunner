using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnableObject<T>
{
    // 생성 기회값 범위 -- 그 범위 안에서 스폰될수 있는 개체가 그 수 만큼 스폰될것임
    private struct chanceBoundaries
    {
        public T spawnableObject;
        public int lowBoundaryValue;
        public int highBoundaryValue;
    }

    private int ratioValueTotal = 0;
    [NonReorderable]
    private List<chanceBoundaries> chanceBoundariesList = new List<chanceBoundaries>();
    [NonReorderable]
    private List<SpawnableObjectsByLevel<T>> spawnableObjectsByLevelList;

    /// <summary>
    /// 생성자
    /// </summary>
    public RandomSpawnableObject(List<SpawnableObjectsByLevel<T>> spawnableObjectsByLevelList)
    {
        this.spawnableObjectsByLevelList = spawnableObjectsByLevelList;
    }


    public T GetItem()
    {
        int upperBoundary = -1;
        ratioValueTotal = 0;
        chanceBoundariesList.Clear();
        // 객체의 유형에 따라 기본값이 다르기 때문에 (일부는 값이 없기 때문)
        // default 방식은 무언가를 미리 알지 못할때 일정시간동안 그 유형을 디폴트값으로 설정하는 방식 -- 일부 개체의 경우 null 이거나 0 일수 있다
        // 따라서 기본적으로 이런 spawnableObject (스폰 개체) 를 기본값으로 설정
        T spawnableObject = default(T);

        foreach (SpawnableObjectsByLevel<T> spawnableObjectsByLevel in spawnableObjectsByLevelList)
        {
            // check for current level
            if (spawnableObjectsByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                foreach (SpawnableObjectRatio<T> spawnableObjectRatio in spawnableObjectsByLevel.spawnableObjectRatioList)
                {
                    int lowerBoundary = upperBoundary + 1;

                    upperBoundary = lowerBoundary + spawnableObjectRatio.ratio - 1;

                    ratioValueTotal += spawnableObjectRatio.ratio;

                    // Add spawnable object to list
                    chanceBoundariesList.Add(new chanceBoundaries()
                    {
                        spawnableObject = spawnableObjectRatio.dungeonObject,
                        lowBoundaryValue = lowerBoundary,
                        highBoundaryValue = upperBoundary
                    });

                }
            }

        }

        if (chanceBoundariesList.Count == 0) return default(T);

        int lookUpValue = Random.Range(0, ratioValueTotal);

        // loop through list to get selected random spawnable object details
        foreach (chanceBoundaries spawnChance in chanceBoundariesList)
        {
            if (lookUpValue >= spawnChance.lowBoundaryValue && lookUpValue <= spawnChance.highBoundaryValue)
            {
                spawnableObject = spawnChance.spawnableObject;
                break;
            }
        }

        return spawnableObject;

    }


}
