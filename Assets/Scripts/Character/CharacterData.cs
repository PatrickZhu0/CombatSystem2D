using UnityEngine;

/// <summary>
/// 角色配置 ScriptableObject，供策划配置。
/// 参考：原始 STCharacterScript (dump.cs line 350091)
/// </summary>
[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/Character")]
public class CharacterData : ScriptableObject
{
    [Header("基础属性")]
    public int maxHP = 1000;
    public int maxMP = 100;
    public int attack = 100;
    public int defense = 50;
    public float moveSpeed = 3f;
    public float attackSpeed = 1f;
    public float jumpForce = 8f;

    [Header("暴击")]
    [Range(0f, 1f)]
    public float criticalRate = 0.05f;
    public float criticalDamageRate = 1.5f;

    [Header("碰撞体")]
    public Vector2 bodyBoxSize = new Vector2(0.8f, 1.5f);
    public Vector2 bodyBoxOffset = new Vector2(0f, 0.75f);

    [Header("移动")]
    public Vector2 groundCheckOffset = new Vector2(0f, -0.05f);
    public float groundCheckDistance = 0.3f;
    public float zMin = -2f;
    public float zMax = 2f;

    /// <summary>
    /// 应用配置到 CharacterStats
    /// </summary>
    public void ApplyTo(CharacterStats stats)
    {
        stats.baseHP = maxHP;
        stats.baseMP = maxMP;
        stats.baseAttack = attack;
        stats.baseDefense = defense;
        stats.baseMoveSpeed = moveSpeed;
        stats.baseAttackSpeed = attackSpeed;
        stats.baseJumpForce = jumpForce;
        stats.criticalRate = criticalRate;
        stats.criticalDamageRate = criticalDamageRate;
    }
}
