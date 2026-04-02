using System;
using UnityEngine;

/// <summary>
/// 角色属性数据，采用 Add+Rate 双值设计。
/// 参考：原始 ActiveStaticInfo (dump.cs line 356298)
/// 核心公式：最终值 = (基础值 + 加值) * (1 + 倍率)
/// </summary>
[Serializable]
public class CharacterStats
{
    // ==================== 基础值 ====================
    public int baseHP = 1000;
    public int baseMP = 100;
    public int baseAttack = 100;
    public int baseDefense = 50;
    public float baseMoveSpeed = 3f;
    public float baseAttackSpeed = 1f;
    public float baseJumpForce = 8f;

    // ==================== 加值（来自 Buff/装备） ====================
    public int hpAdd;
    public int mpAdd;
    public int attackAdd;
    public int defenseAdd;
    public float moveSpeedAdd;
    public float attackSpeedAdd;
    public float jumpForceAdd;

    // ==================== 倍率（来自 Buff/装备） ====================
    public float hpRate;
    public float mpRate;
    public float attackRate;
    public float defenseRate;
    public float moveSpeedRate;
    public float attackSpeedRate;

    // ==================== 暴击属性 ====================
    [Range(0f, 1f)]
    public float criticalRate = 0.05f;
    public float criticalDamageRate = 1.5f;

    // ==================== 速度上下限保护 ====================
    public const float MIN_SPEED_RATE = -0.5f;
    public const float MAX_SPEED_RATE = 2f;
    public const float MIN_SPEED = 0.5f;

    // ==================== 当前 HP/MP ====================
    public int currentHP;
    public int currentMP;

    // ==================== 最终值计算 ====================
    public int MaxHP => Mathf.Max(1, (int)((baseHP + hpAdd) * (1f + hpRate)));
    public int MaxMP => Mathf.Max(1, (int)((baseMP + mpAdd) * (1f + mpRate)));
    public int Attack => Mathf.Max(0, (int)((baseAttack + attackAdd) * (1f + attackRate)));
    public int Defense => Mathf.Max(0, (int)((baseDefense + defenseAdd) * (1f + defenseRate)));

    public float MoveSpeed
    {
        get
        {
            float rate = Mathf.Clamp(moveSpeedRate, MIN_SPEED_RATE, MAX_SPEED_RATE);
            return Mathf.Max(MIN_SPEED, (baseMoveSpeed + moveSpeedAdd) * (1f + rate));
        }
    }

    public float AttackSpeed
    {
        get
        {
            float rate = Mathf.Clamp(attackSpeedRate, MIN_SPEED_RATE, MAX_SPEED_RATE);
            return Mathf.Max(MIN_SPEED, (baseAttackSpeed + attackSpeedAdd) * (1f + rate));
        }
    }

    public float JumpForce
    {
        get
        {
            return baseJumpForce + jumpForceAdd;
        }
    }

    // ==================== 存活状态 ====================
    public bool IsDead => currentHP <= 0;
    public float HpPercent => MaxHP > 0 ? (float)currentHP / MaxHP : 0f;

    // ==================== 初始化 ====================
    public void Init()
    {
        ResetAddRate();
        currentHP = MaxHP;
        currentMP=MaxMP;
    }

    /// <summary>
    /// 重置所有加值和倍率到默认状态
    /// </summary>
    public void ResetAddRate()
    {
        hpAdd = 0; mpAdd = 0; attackAdd = 0; defenseAdd = 0;
        moveSpeedAdd = 0f; attackSpeedAdd = 0f; jumpForceAdd = 0f;

        hpRate = 0f; mpRate = 0f; attackRate = 0f; defenseRate = 0f;
        moveSpeedRate = 0f; attackSpeedRate = 0f;
    }

    // ==================== 战斗方法 ====================
    public int TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage - Defense);
        currentHP = Mathf.Max(0, currentHP - actualDamage);
        return actualDamage;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(MaxHP, currentHP + amount);
    }

    public bool ConsumeMP(int amount)
    {
        if (currentMP < amount) return false;
        currentMP -= amount;
        return true;
    }
}
