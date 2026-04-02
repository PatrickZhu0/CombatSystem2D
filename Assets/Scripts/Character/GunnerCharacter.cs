using UnityEngine;

/// <summary>
/// 枪手角色，继承 CharacterBase。
/// 参考：CNGunner (dump.cs line 368706), CNGunnerDef (line 175296)
/// Demo 阶段实现基础射击攻击，后续扩展技能系统。
/// </summary>
public class GunnerCharacter : CharacterBase
{
    // ==================== 射击配置 ====================
    [Header("射击参数")]
    public float attackRange = 5f;             // 普攻射程
    public float attackInterval = 0.3f;        // 普攻间隔（秒）
    public int attackDamage = 80;              // 每发子弹伤害
    public float bulletSpeed = 15f;            // 子弹飞行速度
    public int maxAmmo = 6;                    // 弹夹容量（参考 BULLET_VIRTUAL_SIZES）
    public float reloadTime = 1.2f;            // 装弹时间（参考 ATTACK_1_RELOAD_DELAY）

    [Header("子弹")]
    public GameObject bulletPrefab;            // 子弹预制体
    public Transform firePoint;                // 发射点

    // ==================== 运行时状态 ====================
    public int CurrentAmmo { get; private set; }
    public bool IsReloading { get; private set; }
    public float AttackCooldown { get; private set; }

    // ==================== 内部 ====================
    private float _attackTimer;
    private float _reloadTimer;
    private MovementSystem _movement;

    // ==================== 生命周期 ====================
    protected override void Awake()
    {
        base.Awake();
        CurrentAmmo = maxAmmo;
        _attackTimer = 0f;
        _reloadTimer = 0f;
    }

    protected override void Start()
    {
        base.Start();
        _movement = GetComponent<MovementSystem>();
    }

    protected override void Update()
    {
        Debug.Log($"[GunnerCharacter] State: {CurrentState}, Ammo: {CurrentAmmo}/{maxAmmo}, Reloading: {IsReloading}, AttackCooldown: {AttackCooldown:F2}");
        base.Update();
        UpdateAttackCooldown();
        UpdateReload();
    }

    // ==================== 射击 ====================

    /// <summary>
    /// 尝试射击。返回是否成功开火。
    /// </summary>
    public bool Shoot()
    {
        if (IsDead || IsStunned) return false;
        if (IsReloading) return false;
        if (_attackTimer > 0f) return false;
        if (CurrentAmmo <= 0)
        {
            StartReload();
            return false;
        }
        if (!StateConstraints.CanAttack(CurrentState)) return false;

        // 消耗弹药
        CurrentAmmo--;
        _attackTimer = attackInterval;

        // 切换攻击状态
        if (StateConstraints.IsJumpState(CurrentState))
            ChangeState(CharacterState.JumpAttack);
        else
            ChangeState(CharacterState.Attack);

        // 发射子弹
        FireBullet();

        // 弹夹打空自动装弹
        if (CurrentAmmo <= 0)
            StartReload();

        return true;
    }

    /// <summary>
    /// 手动装弹
    /// </summary>
    public void StartReload()
    {
        if (IsReloading || CurrentAmmo >= maxAmmo) return;
        IsReloading = true;
        _reloadTimer = reloadTime;
    }

    // ==================== 内部方法 ====================

    private void FireBullet()
    {
        if (bulletPrefab == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector3 dir = IsFacingRight ? Vector3.right : Vector3.left;

        // 简单实例化，后续接入对象池
        GameObject bulletObj = Instantiate(bulletPrefab, origin, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            int dmg = Mathf.Max(1, Stats.Attack + attackDamage - 0); // 防御由命中目标计算
            bullet.Init(dir, bulletSpeed, dmg, TeamId, attackRange);
        }
    }

    private void UpdateAttackCooldown()
    {
        if (_attackTimer > 0f)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackTimer = 0f;
                AttackCooldown = 0f;

                // 攻击结束自动回到待机
                if (CurrentState == CharacterState.Attack || CurrentState == CharacterState.JumpAttack)
                {
                    if (StateConstraints.IsJumpState(CurrentState))
                        ChangeState(CharacterState.Falling);
                    else
                        ChangeState(CharacterState.Idle);
                }
            }
        }
    }

    private void UpdateReload()
    {
        if (!IsReloading) return;

        _reloadTimer -= Time.deltaTime;
        if (_reloadTimer <= 0f)
        {
            _reloadTimer = 0f;
            IsReloading = false;
            CurrentAmmo = maxAmmo;
        }
    }

    // ==================== 状态回调 ====================
    protected override void OnStateEnter(CharacterState state)
    {
        base.OnStateEnter(state);
    }
}
