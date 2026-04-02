using System;
using UnityEngine;

/// <summary>
/// 角色基类，简化版 IRDActiveObject + IRDCharacter
/// 参考：IRDActiveObject (dump.cs line 409310), IRDCharacter (line 412442)
/// </summary>
public abstract class CharacterBase : MonoBehaviour
{
    // ==================== 核心组件 ====================
    public CharacterStats Stats { get; private set; }
    public MovementSystem Movement { get; private set; }

    // ==================== 状态 ====================
    public CharacterState CurrentState { get; protected set; } = CharacterState.None;
    public int TeamId { get; set; } = 0;
    public bool IsFacingRight { get; set; } = true;
    public bool IsStunned { get; set; }
    public bool IsDead => Stats.IsDead;
    public bool IsGrounded { get; protected set; } = true;

    // ==================== 状态机 ====================
    protected float _stateTimer;
    protected CharacterState _prevState;

    // ==================== 无敌 ====================
    protected float _invincibleTimer;
    public bool IsInvincible => _invincibleTimer > 0;

    // ==================== 事件 ====================
    public event Action<CharacterBase, CharacterState, CharacterState> OnStateChanged;
    public event Action<CharacterBase, int> OnDamaged;
    public event Action<CharacterBase> OnDead;
    public event Action<CharacterBase> OnHealed;

    // ==================== 生命周期 ====================
    protected virtual void Awake()
    {
        Stats = new CharacterStats();
        CurrentState = CharacterState.None;

        // 自动获取或添加 MovementSystem
        Movement = GetComponent<MovementSystem>();
        if (Movement == null)
            Movement = gameObject.AddComponent<MovementSystem>();
    }

    protected virtual void Start()
    {
        Stats.Init();
        Movement.Init(this);
        ChangeState(CharacterState.Idle);
    }

    protected virtual void Update()
    {
        // 始终驱动移动系统
        Movement.Tick(Time.deltaTime);
        UpdateTimers();
    }

    // ==================== 计时器更新 ====================
    protected void UpdateTimers()
    {
        float dt = Time.deltaTime;

        if (_stateTimer > 0f)
        {
            _stateTimer -= dt;
            if (_stateTimer <= 0f)
            {
                _stateTimer = 0f;
                OnStateTimeout();
            }
        }

        if (_invincibleTimer > 0f)
            _invincibleTimer -= dt;
    }

    // ==================== 状态机 ====================
    public void ChangeState(CharacterState newState)
    {
        if (newState == CurrentState) return;

        _prevState = CurrentState;
        OnStateExit(CurrentState);
        CurrentState = newState;
        _stateTimer = 0f;
        OnStateEnter(newState);
        OnStateChanged?.Invoke(this, newState, _prevState);
    }

    protected virtual void OnStateEnter(CharacterState state)
    {
        if (state == CharacterState.Dead)
            OnDead?.Invoke(this);
    }

    protected virtual void OnStateExit(CharacterState state) { }

    protected virtual void OnStateTimeout() { }

    // ==================== 战斗 ====================
    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;
        if (IsInvincible) return;

        int actualDamage = Stats.TakeDamage(damage);
        OnDamaged?.Invoke(this, actualDamage);

        SetInvincible(0.3f);

        if (StateConstraints.CanBeInterrupted(CurrentState))
        {
            ChangeState(CharacterState.Hurt);
        }
    }

    public virtual void Heal(int amount)
    {
        Stats.Heal(amount);
        OnHealed?.Invoke(this);
    }

    public void SetInvincible(float duration)
    {
        _invincibleTimer = Mathf.Max(_invincibleTimer, duration);
    }

    // ==================== 朝向 ====================
    public void FaceDirection(bool facingRight)
    {
        if (!StateConstraints.CanChangeDirection(CurrentState)) return;
        IsFacingRight = facingRight;

        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void FaceToward(Vector3 target)
    {
        FaceDirection(target.x > transform.position.x);
    }

    // ==================== 工具 ====================
    public float GetDistanceTo(CharacterBase other)
    {
        return Vector3.Distance(transform.position, other.transform.position);
    }
}
