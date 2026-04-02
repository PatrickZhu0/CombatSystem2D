using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单对象池，用于复用 GameObject，减少实例化开销。
/// 参考：原始 SelfObjectPool&lt;T&gt; (dump.cs)
/// </summary>
public class ObjectPool : Singleton<ObjectPool>
{
    private readonly Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();
    private readonly Dictionary<int, GameObject> _prefabs = new Dictionary<int, GameObject>();
    private Transform _poolRoot;

    public int defaultPoolSize = 10;

    protected override void Awake()
    {
        base.Awake();
        _poolRoot = new GameObject("PoolRoot").transform;
        _poolRoot.SetParent(transform);
    }

    /// <summary>
    /// 注册预制体到对象池
    /// </summary>
    public void Register(GameObject prefab, int initialSize = -1)
    {
        if (initialSize <= 0) initialSize = defaultPoolSize;

        int poolId = prefab.GetInstanceID();
        _prefabs[poolId] = prefab;

        if (!_pools.ContainsKey(poolId))
            _pools[poolId] = new Queue<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(_poolRoot);
            _pools[poolId].Enqueue(obj);
        }
    }

    /// <summary>
    /// 从对象池取出一个实例
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolId = prefab.GetInstanceID();

        if (!_pools.ContainsKey(poolId))
            Register(prefab);

        GameObject obj;
        if (_pools[poolId].Count > 0)
        {
            obj = _pools[poolId].Dequeue();
        }
        else
        {
            obj = Instantiate(prefab);
        }

        obj.transform.SetParent(null);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 归还一个实例到对象池
    /// </summary>
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(_poolRoot);

        // 找到对应的预制体 poolId
        foreach (var kvp in _prefabs)
        {
            if (kvp.Value.name == obj.name.Replace("(Clone)", ""))
            {
                _pools[kvp.Key].Enqueue(obj);
                return;
            }
        }

        // 没找到对应预制体，直接销毁
        Destroy(obj);
    }

    /// <summary>
    /// 清空所有池
    /// </summary>
    public void ClearAll()
    {
        foreach (var queue in _pools.Values)
        {
            foreach (var obj in queue)
                Destroy(obj);
        }
        _pools.Clear();
        _prefabs.Clear();
    }
}
