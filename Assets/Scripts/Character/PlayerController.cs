using UnityEngine;

/// <summary>
/// 玩家输入控制，将键盘/触摸输入转换为角色指令。
/// 移动物理由 CharacterBase.Update() 驱动，此处只处理输入。
/// 参考：原始 onMoveInput + onUserMove_Dungeon (dump.cs line 409808, 414343)
/// </summary>
[RequireComponent(typeof(CharacterBase))]
public class PlayerController : MonoBehaviour
{
    // ==================== 配置 ====================
    [Header("输入设置")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string jumpButton = "Jump";
    public string attackButton = "Fire1";

    [Header("输入范围")]
    public float deadZone = 0.1f;

    // ==================== 组件引用 ====================
    private CharacterBase _character;
    private GunnerCharacter _gunner;

    // ==================== 初始化 ====================
    private void Awake()
    {
        _character = GetComponent<CharacterBase>();
        _gunner = GetComponent<GunnerCharacter>();
    }

    // ==================== 每帧更新 ====================
    private void Update()
    {
        if (_character.IsDead) return;
        if (_character.IsStunned) return;
        if (!StateConstraints.IsControllable(_character.CurrentState)) return;

        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    // ==================== 移动处理 ====================
    private void HandleMovement()
    {
        float x = Input.GetAxisRaw(horizontalAxis);
        float z = Input.GetAxisRaw(verticalAxis);

        if (Mathf.Abs(x) < deadZone) x = 0f;
        if (Mathf.Abs(z) < deadZone) z = 0f;

        _character.Movement.Move(x, z);
    }

    // ==================== 跳跃处理 ====================
    private void HandleJump()
    {
        if (Input.GetButtonDown(jumpButton))
        {
            _character.Movement.Jump();
        }
    }

    // ==================== 攻击处理 ====================
    private void HandleAttack()
    {
        if (Input.GetButtonDown(attackButton))
        {
            if (_gunner != null)
                _gunner.Shoot();
        }
    }
}
