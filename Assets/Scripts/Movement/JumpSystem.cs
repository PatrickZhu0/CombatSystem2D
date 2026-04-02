using UnityEngine;

/// <summary>
/// 跳跃系统 - 重力模拟
/// 参考：原始 moveZ + Z_ACCEL_TYPE_GRAVITY (dump.cs line 156020, 179569)
/// Y 轴为垂直跳跃轴，使用重力加速度控制起跳和下落
/// </summary>
[System.Serializable]
public class JumpSystem
{
    // ==================== 配置 ====================
    public float gravity = -20f;          // 重力加速度（参考 IRDActiveObject.gravityAccel_ line 409439）
    public float groundCheckDistance = 0.3f;  // 地面检测距离
    public LayerMask groundLayer;          // 地面层级

    // ==================== 状态 ====================
    public bool IsGrounded { get; private set; }
    public bool IsJumping { get; private set; }
    public float VerticalVelocity { get; private set; }  // Y 轴速度
    public float Height { get; private set; }            // 当前跳跃高度（相对地面的 Y 偏移）

    // ==================== 内部 ====================
    private float _baseY;   // 地面 Y 坐标
    private bool _hasBase;

    /// <summary>
    /// 初始化跳跃系统
    /// </summary>
    public void Init(LayerMask layer)
    {
        groundLayer = layer;
        IsGrounded = true;
        IsJumping = false;
        VerticalVelocity = 0f;
        Height = 0f;
        _hasBase = false;
    }

    /// <summary>
    /// 设置地面基准 Y 坐标
    /// </summary>
    public void SetGroundY(float y)
    {
        _baseY = y;
        _hasBase = true;
    }

    /// <summary>
    /// 尝试跳跃
    /// </summary>
    /// <param name="jumpForce">跳跃力（来自 Stats.JumpForce）</param>
    /// <returns>是否成功起跳</returns>
    public bool Jump(float jumpForce)
    {
        if (!IsGrounded || IsJumping) return false;

        VerticalVelocity = jumpForce;
        IsGrounded = false;
        IsJumping = true;
        Height = 0f;
        return true;
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    /// <param name="deltaTime">帧间隔</param>
    /// <param name="position">角色当前位置</param>
    /// <returns>更新后的位置</returns>
    public Vector3 Tick(float deltaTime, Vector3 position)
    {
        if (IsGrounded) return position;

        // 应用重力（参考 Z_ACCEL_TYPE_GRAVITY_WORLD）
        VerticalVelocity += gravity * deltaTime;

        // 更新 Y 坐标
        float newY = position.y + VerticalVelocity * deltaTime;

        // 更新跳跃高度
        Height += VerticalVelocity * deltaTime;

        // 检测落地
        if (VerticalVelocity <= 0f && _hasBase && newY <= _baseY)
        {
            Land(position);
            newY = _baseY;
        }

        position.y = newY;
        return position;
    }

    /// <summary>
    /// 检测地面（通过射线检测）
    /// </summary>
    public void CheckGround(Vector2 position, Vector2 offset)
    {
        Vector2 origin = position + offset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

        if (hit && !IsGrounded && VerticalVelocity <= 0f)
        {
            Land(new Vector3(position.x, position.y, 0f));
        }
    }

    /// <summary>
    /// 强制落地
    /// </summary>
    private void Land(Vector3 position)
    {
        IsGrounded = true;
        IsJumping = false;
        VerticalVelocity = 0f;
        Height = 0f;
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        IsGrounded = true;
        IsJumping = false;
        VerticalVelocity = 0f;
        Height = 0f;
    }
}