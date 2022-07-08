using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableObjectsByLevel<T>
{
    public DungeonLevelSO dungeonLevel;
    [NonReorderable]
    public List<SpawnableObjectRatio<T>> spawnableObjectRatioList;
}
