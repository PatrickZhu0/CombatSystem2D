using System;
using UnityEngine;

/// <summary>
/// 游戏主循环，以固定时间步长驱动游戏逻辑更新。
/// 参考：原始 DnfObject.mainProc (dump.cs line 408534)
/// </summary>
public class GameLoop : Singleton<GameLoop>
{
    public event Action<float> OnTick;

    [Header("Settings")]
    public float tickInterval = 1f / 60f;

    private float _tickTimer;

    protected override void Awake()
    {
        base.Awake();
        _tickTimer = 0f;
    }

    protected virtual void Update()
    {
        _tickTimer += Time.deltaTime;

        if (_tickTimer >= tickInterval)
        {
            _tickTimer -= tickInterval;
            OnTick?.Invoke(tickInterval);
        }
    }
}
