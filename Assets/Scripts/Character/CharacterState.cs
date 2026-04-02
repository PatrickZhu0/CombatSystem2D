using UnityEngine;

/// <summary>
/// 角色状态枚举，参考原始 ENUM_ACTIVE_OBJECT_STATE (dump.cs line 505222)
/// </summary>
public enum CharacterState
{
    None,
    Idle,           // STATE_STAND - 站立
    Move,           // STATE_MOVE - 移动
    Jump,           // STATE_JUMP_UP - 跳跃上升
    Falling,        // STATE_JUMP_DOWN - 下落
    Land,           // STATE_JUMP_STAND - 落地
    Attack,         // STATE_ATTACK - 攻击
    JumpAttack,     // STATE_JUMP_ATTACK - 空中攻击
    Skill,          // STATE_ATTACK + Skill
    Hurt,           // STATE_DAMAGE - 受伤(硬直)
    Down,           // STATE_DOWN - 倒地
    Dead            // STATE_DIE - 死亡
}

/// <summary>
/// 状态约束，参考原始 isMovableState/isExcutableState (dump.cs line 409828/409720)
/// </summary>
public static class StateConstraints
{
    public static bool CanMove(CharacterState state) =>
        state == CharacterState.Idle || state == CharacterState.Move || state == CharacterState.Jump;

    public static bool CanAttack(CharacterState state) =>
        state == CharacterState.Idle || state == CharacterState.Move ||
        state == CharacterState.Jump || state == CharacterState.Falling;

    public static bool CanJump(CharacterState state) =>
        state == CharacterState.Idle || state == CharacterState.Move;

    public static bool CanChangeDirection(CharacterState state) =>
        state != CharacterState.Attack && state != CharacterState.Skill &&
        state != CharacterState.Hurt;

    public static bool IsJumpState(CharacterState state) =>
        state == CharacterState.Jump || state == CharacterState.Falling ||
        state == CharacterState.JumpAttack;

    public static bool CanBeInterrupted(CharacterState state) =>
        state != CharacterState.Down && state != CharacterState.Dead;

    public static bool IsControllable(CharacterState state) =>
        state == CharacterState.Idle || state == CharacterState.Move ||
        state == CharacterState.Jump || state == CharacterState.Falling;
}
