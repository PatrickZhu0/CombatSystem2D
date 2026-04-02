using UnityEngine;

/// <summary>
/// 2.5D 移动系统
/// 参考：原始 DNFObject 的三轴位置系统 (dump.cs line 408283)
/// X 轴 = 水平移动, Z 轴 = 深度（屏幕上下行）, Y 轴由 JumpSystem 管理
/// </summary>
public class MovementSystem : MonoBehaviour
{
    // ==================== 配置 ====================
    [Header("移动参数")]
    public float zMin = -2f;
    public float zMax = 2f;
    public float zSpeedScale = 0.7f;
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.15f;

    [Header("地面检测")]
    public float groundCheckDistance = 0.3f;
    public Vector2 groundCheckOffset = new Vector2(0f, -0.75f);
    public LayerMask groundLayer;

    [Header("排序")]
    public int sortOrderBase = 0;
    public int sortOrderFactor = 100;

    // ==================== 运行时状态 ====================
    public Vector2 InputDirection { get; set; }
    public float CurrentSpeedX { get; private set; }
    public float CurrentSpeedZ { get; private set; }
    public float DepthZ { get; private set; }
    public bool IsGrounded => _jumpSystem != null && _jumpSystem.IsGrounded;

    // ==================== 组件引用 ====================
    private CharacterBase _character;
    private CharacterStats _stats;
    private JumpSystem _jumpSystem;

    // ==================== 目标速度 ====================
    private float _targetSpeedX;
    private float _targetSpeedZ;

    // ==================== 击退 ====================
    private Vector2 _knockbackForce;
    private float _knockbackDuration;
    private float _knockbackTimer;

    // ==================== 初始 Y（地面基准） ====================
    private float _initialY;

    // ==================== 初始化 ====================
    public void Init(CharacterBase character)
    {
        _character = character;
        _stats = character.Stats;

        _jumpSystem = new JumpSystem();
        _jumpSystem.Init(groundLayer);

        _initialY = transform.position.y;
        _jumpSystem.SetGroundY(_initialY);

        DepthZ = 0f;
        InputDirection = Vector2.zero;
        _knockbackTimer = 0f;
    }

    // ==================== 外部接口 ====================

    /// <summary>
    /// 设置移动输入方向。参考：原始 onMoveInput (dump.cs line 409808)
    /// </summary>
    public void Move(float x, float z)
    {
        if (_character.IsStunned || _character.IsDead) return;
        if (!StateConstraints.CanMove(_character.CurrentState)) return;

        InputDirection = new Vector2(x, z);
    }

    /// <summary>
    /// 尝试跳跃
    /// </summary>
    public bool Jump()
    {
        if (_character.IsStunned || _character.IsDead) return false;
        if (!StateConstraints.CanJump(_character.CurrentState)) return false;

        float jumpForce = _stats.JumpForce;
        if (_jumpSystem.Jump(jumpForce))
        {
            _character.ChangeState(CharacterState.Jump);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 停止移动。参考：原始 StopMove (dump.cs line 409812)
    /// </summary>
    public void StopMove()
    {
        InputDirection = Vector2.zero;
    }

    /// <summary>
    /// 应用击退力
    /// </summary>
    public void ApplyKnockback(Vector2 force, float duration)
    {
        _knockbackForce = force;
        _knockbackDuration = duration;
        _knockbackTimer = duration;
    }

    // ==================== 每帧更新 ====================

    /// <summary>
    /// 驱动移动物理更新（在 Update 中由 PlayerController 驱动）
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (_character == null)
        {
            Debug.LogError("[MovementSystem] Tick called but _character is null! Init() was not called.");
            return;
        }
        if (_character.IsDead) return;

        float moveSpeed = _stats.MoveSpeed;

        // --- 击退处理 ---
        UpdateKnockback(deltaTime);

        // --- X 轴移动（水平） ---
        if (InputDirection.x != 0f && StateConstraints.CanMove(_character.CurrentState))
        {
            _targetSpeedX = InputDirection.x * moveSpeed;
            _character.FaceDirection(InputDirection.x > 0f);
        }
        else
        {
            _targetSpeedX = 0f;
        }

        // --- Z 轴移动（深度/屏幕上下） ---
        if (InputDirection.y != 0f && StateConstraints.CanMove(_character.CurrentState))
        {
            _targetSpeedZ = InputDirection.y * moveSpeed * zSpeedScale;
        }
        else
        {
            _targetSpeedZ = 0f;
        }

        // --- 平滑加减速 ---
        CurrentSpeedX = SmoothSpeed(CurrentSpeedX, _targetSpeedX, deltaTime);
        CurrentSpeedZ = SmoothSpeed(CurrentSpeedZ, _targetSpeedZ, deltaTime);

        // --- 应用 X 位移 ---
        float newX = transform.position.x + CurrentSpeedX * deltaTime;

        // --- 应用 Z 位移（深度 → 映射到屏幕 Y） ---
        DepthZ += CurrentSpeedZ * deltaTime;
        DepthZ = Mathf.Clamp(DepthZ, zMin, zMax);

        // 当前深度的地面 Y
        float effectiveGroundY = _initialY + DepthZ;

        // 更新 JumpSystem 的落地基准（跳跃中移动 Z 时落地目标同步更新）
        _jumpSystem.SetGroundY(effectiveGroundY);

        // --- 应用位置 ---
        Vector3 pos = transform.position;
        pos.x = newX;

        // 跳跃/重力 由 JumpSystem 管理 Y
        pos = _jumpSystem.Tick(deltaTime, pos);

        // 地面时：Y 直接由深度决定
        if (_jumpSystem.IsGrounded)
        {
            pos.y = effectiveGroundY;
        }

        // --- 设置最终位置 ---
        transform.position = pos;

        // --- 更新 Sprite 排序（Z 深度越大越远） ---
        UpdateSortOrder();

        // --- 地面检测 ---
        _jumpSystem.CheckGround(transform.position, groundCheckOffset);

        // --- 更新角色状态 ---
        UpdateCharacterState();
    }

    // ==================== 平滑速度 ====================
    private float SmoothSpeed(float current, float target, float dt)
    {
        if (Mathf.Abs(target - current) < 0.01f) return target;

        float time = target != 0f ? accelerationTime : decelerationTime;
        if (time <= 0f) time = 0.1f;
        return Mathf.Lerp(current, target, dt / time);
    }

    // ==================== 击退更新 ====================
    private void UpdateKnockback(float deltaTime)
    {
        if (_knockbackTimer <= 0f) return;

        _knockbackTimer -= deltaTime;
        if (_knockbackTimer <= 0f)
        {
            _knockbackTimer = 0f;
            return;
        }

        transform.position += (Vector3)_knockbackForce * deltaTime;
    }

    // ==================== 排序更新 ====================
    private void UpdateSortOrder()
    {
        // Z 值越大（越远） → sortOrder 越小（绘制在后面）
        int order = sortOrderBase + Mathf.RoundToInt(-DepthZ * sortOrderFactor);
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sortingOrder = order;
    }

    // ==================== 角色状态更新 ====================
    private void UpdateCharacterState()
    {
        if (_character.IsDead) return;

        // 跳跃状态由 JumpSystem 管理
        if (!_jumpSystem.IsGrounded)
        {
            if (_jumpSystem.VerticalVelocity > 0f)
            {
                if (_character.CurrentState != CharacterState.Jump)
                    _character.ChangeState(CharacterState.Jump);
            }
            else
            {
                if (_character.CurrentState != CharacterState.Falling)
                    _character.ChangeState(CharacterState.Falling);
            }
            return;
        }

        // 落地检测
        if (_character.CurrentState == CharacterState.Jump ||
            _character.CurrentState == CharacterState.Falling)
        {
            _character.ChangeState(CharacterState.Land);
            return;
        }

        // 地面移动状态
        bool isMoving = Mathf.Abs(CurrentSpeedX) > 0.01f || Mathf.Abs(CurrentSpeedZ) > 0.01f;
        CharacterState targetState = isMoving ? CharacterState.Move : CharacterState.Idle;

        if (_character.CurrentState != targetState)
        {
            _character.ChangeState(targetState);
        }
    }

    // ==================== 编辑器工具 ====================
#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (_jumpSystem == null) return;

        var pos = transform.position;

        // 深度范围
        Gizmos.color = Color.yellow;
        float zLeft = pos.x - 0.5f;
        float zRight = pos.x + 0.5f;
        Gizmos.DrawLine(new Vector3(zLeft, pos.y + zMax), new Vector3(zRight, pos.y + zMax));
        Gizmos.DrawLine(new Vector3(zLeft, pos.y + zMin), new Vector3(zRight, pos.y + zMin));

        // 当前深度位置
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(pos.x, pos.y + DepthZ, pos.z), 0.1f);

        // 地面检测射线
        Gizmos.color = _jumpSystem.IsGrounded ? Color.green : Color.red;
        Vector2 origin = (Vector2)pos + groundCheckOffset;
        Gizmos.DrawLine(origin, origin + Vector2.down * groundCheckDistance);
    }
#endif
}
