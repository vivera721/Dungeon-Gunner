using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PoolManager : SingletoneMonobehaviour<PoolManager>
{
    #region Tooltip
    [Tooltip("Populate this array with prefabs that you want to add to the pool, and specify the number of gameobjects to be created for each")]
    #endregion
    [SerializeField] private Pool[] poolArray = null;
    private Transform objectPoolTransform;
    private Dictionary<int, Queue<Component>> poolDictionary = new Dictionary<int, Queue<Component>>();

    [System.Serializable]
    public struct Pool
    {
        public int poolSize;
        public GameObject prefab;
        public string componentType;
    }


    private void Start()
    {
        // This singleton gameobject will be the object pool parent
        objectPoolTransform = this.gameObject.transform;

        // Create object pools on start
        for (int i = 0; i < poolArray.Length; i++)
        {
            CreatePool(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType);
        }

    }

    /// <summary>
    /// Create the object pool with the specified prefabs and the specified pool size for each
    /// </summary>
    private void CreatePool(GameObject prefab, int poolSize, string componentType)
    {
        // prefab 에서 인스턴스 ID 를 얻고 저장
        int poolKey = prefab.GetInstanceID();

        // prefab name 저장
        string prefabName = prefab.name;

        // create parent gameobject to parent the child objects to
        // 자식의 부모가 될수 있게 부모 gameobject 생성
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");

        parentGameObject.transform.SetParent(objectPoolTransform);
        
        // dictionary 에 이미 모든 키가 포함되어있는지 확인
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<Component>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;

                newObject.SetActive(false);

                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));

            }
        }

    }

    /// <summary>
    /// Reuse a gameobject component in the pool. 
    /// 'prefab' is the prefab gameobject containing the component. 
    /// 'position' is the world position for the gameobject where it is should appear when enable
    /// 'rotation' should be set if the gameobject needs to be rotated
    /// 오브젝트 풀링 방식에서는 게임오브젝트 컴포넌트를 재사용 한다
    /// 프리팹은 컴포넌트를 포함하고 있는 프리팹 게임오브젝트이고,
    /// 위치는 이 게임오브젝트가 활성화될때 나타나야 할 위치, 회전값은 게임오브젝트가 회전값이 필요하다면 설정해야한다
    /// </summary>
    public Component ReuseComponent(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            // Get object from pool queue
            Component componentToReuse = GetComponentFromPool(poolKey);

            ResetObject(position, rotation, componentToReuse, prefab);

            return componentToReuse;
        }
        else
        {
            Debug.Log("No object pool for " + prefab);
            return null;
        }


    }

    /// <summary>
    /// Get a gameobject component from the pool using the 'poolKey'
    /// pool 에서 poolKey를 사용하고 있는 게임오브젝트 컴포넌트를 반환
    /// </summary>
    private Component GetComponentFromPool(int poolKey)
    {
        Component componentToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(componentToReuse);

        if (componentToReuse.gameObject.activeSelf == true)
        {
            componentToReuse.gameObject.SetActive(false);
        }

        return componentToReuse;
    }

    /// <summary>
    /// Reset the gameobject
    /// </summary>>
    private void ResetObject(Vector3 position, Quaternion rotation, Component componentToReuse, GameObject prefab)
    {
        // 재사용하는 컴포넌트가 과거의 상태와 같게 함
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.gameObject.transform.localScale = prefab.transform.localScale;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(poolArray), poolArray);
    }
#endif
    #endregion

}
