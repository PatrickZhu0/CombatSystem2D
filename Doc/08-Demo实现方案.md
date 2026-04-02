# 2D 战斗系统 Demo 实现方案

## 一、Demo 目标

实现一个类似 DNF 的 2.5D 横版格斗 Demo，包含：
- 1 个可操控角色（枪手）
- 1 个简化 AI 怪物
- 基础战斗循环（移动 → 攻击 → 伤害 → 击退）
- 触摸/键盘操控

---

## 二、架构设计

### 文件结构

```
Assets/
├── Scripts/
│   ├── Core/                    # 核心框架
│   │   ├── GameLoop.cs          # 游戏主循环
│   │   ├── ObjectPool.cs        # 对象池
│   │   └── Singleton.cs         # 单例基类
│   │
│   ├── Character/               # 角色系统
│   │   ├── CharacterBase.cs     # 角色基类
│   │   ├── CharacterState.cs    # 状态枚举与状态机
│   │   ├── PlayerController.cs  # 玩家输入控制
│   │   └── CharacterStats.cs    # 属性数据（Add+Rate 双值设计）
│   │
│   ├── Attribute/               # 属性系统
│   │   ├── StatType.cs          # 属性类型枚举
│   │   ├── StatChange.cs        # 属性变更请求
│   │   └── AttributeHelper.cs   # 属性计算工具
│   │
│   ├── Buff/                    # Buff 系统
│   │   ├── BuffBase.cs          # Buff 基类（生命周期管理）
│   │   ├── BuffManager.cs       # Buff 管理器（叠加/覆盖/移除）
│   │   ├── BuffFactory.cs       # Buff 工厂（创建/类型映射）
│   │   └── Buffs/               # 具体 Buff 实现
│   │       ├── StatChangeBuff.cs    # 属性变更 Buff
│   │       ├── HealOverTimeBuff.cs  # 持续治疗 Buff
│   │       ├── StunBuff.cs          # 眩晕 Buff
│   │       └── InvincibleBuff.cs    # 无敌 Buff
│   │
│   ├── Movement/                # 移动系统
│   │   ├── MovementSystem.cs    # 移动逻辑
│   │   └── JumpSystem.cs        # 跳跃逻辑
│   │
│   ├── Combat/                  # 战斗系统
│   │   ├── AttackInfo.cs        # 攻击数据
│   │   ├── DamageInfo.cs        # 伤害数据
│   │   ├── DamageCalculator.cs  # 伤害计算（含属性/Buff 集成）
│   │   ├── Bullet.cs            # 子弹基类
│   │   └── HitBox.cs            # 碰撞盒
│   │
│   ├── Collision/               # 碰撞系统
│   │   ├── CollisionBox.cs      # AABB 碰撞盒
│   │   ├── CollisionSystem.cs   # 碰撞检测
│   │   └── GridPartition.cs     # 网格空间分区
│   │
│   ├── Map/                     # 地图系统
│   │   ├── MapManager.cs        # 地图管理
│   │   ├── PassageGrid.cs       # 通行网格
│   │   └── MapBoundary.cs       # 边界限制
│   │
│   ├── Skill/                   # 技能系统
│   │   ├── SkillBase.cs         # 技能基类（生命周期 + 控制类型）
│   │   ├── SkillManager.cs      # 技能管理器（冷却/可用性检查）
│   │   ├── SkillControlType.cs  # 技能控制类型枚举
│   │   ├── NormalAttack.cs      # 普通攻击
│   │   └── SkillGatling.cs      # 加特林技能示例
│   │
│   ├── AI/                      # AI 系统
│   │   ├── MonsterAI.cs         # 怪物 AI
│   │   └── SimpleStateMachine.cs # 简化状态机
│   │
│   └── UI/                      # UI 系统
│       ├── VirtualJoystick.cs   # 虚拟摇杆
│       ├── SkillButton.cs       # 技能按钮
│       ├── HealthBar.cs         # 血条
│       ├── DamageNumber.cs      # 伤害数字
│       └── BuffIcon.cs          # Buff 图标显示
│
├── Prefabs/
│   ├── Player.prefab
│   ├── Monster.prefab
│   ├── Bullet.prefab
│   └── UI/
│       ├── Joystick.prefab
│       ├── SkillButton.prefab
│       └── BuffIcon.prefab
│
└── Data/
    ├── CharacterData.asset      # 角色配置
    ├── SkillData.asset          # 技能配置
    ├── BuffData.asset           # Buff 配置
    └── MapData.asset            # 地图配置
```

---

## 三、核心系统实现

### 3.1 角色系统

#### CharacterBase.cs（约 300 行）

```csharp
public abstract class CharacterBase : MonoBehaviour
{
    // 核心组件
    public CharacterStats Stats { get; protected set; }
    public CharacterState State { get; protected set; }
    public MovementSystem Movement { get; protected set; }
    public SkillManager SkillManager { get; protected set; }
    public BuffManager BuffManager { get; protected set; }

    // 碰撞
    public CollisionBox BodyBox { get; protected set; }      // 受击框
    public CollisionBox AttackBox { get; protected set; }    // 攻击框（攻击时激活）

    // 基础字段
    public int TeamId;                    // 阵营（0=玩家, 1=敌人）
    public bool IsFacingRight;            // 朝向
    public bool IsStunned;                // 眩晕状态
    public bool IsDead => Stats.CurrentHP <= 0;

    // 生命周期
    protected virtual void Awake();
    protected virtual void Update();
    protected virtual void FixedUpdate();

    // 状态机
    public void ChangeState(CharacterState newState);
    protected virtual void OnStateEnter(CharacterState state);
    protected virtual void OnStateExit(CharacterState state);

    // 战斗
    public virtual void TakeDamage(DamageInfo damage);
    public virtual void OnAttackHit(CharacterBase target);
    public virtual void Heal(int amount);
}
```

#### CharacterState.cs（约 100 行）

```csharp
public enum CharacterState
{
    None,
    Idle,           // 站立
    Move,           // 移动
    Jump,           // 跳跃
    JumpAttack,     // 空中攻击
    Attack,         // 攻击
    Skill,          // 技能
    Hurt,           // 受伤
    Down,           // 倒地
    Dead            // 死亡
}

// 状态约束
public static class StateConstraints
{
    public static bool CanMove(CharacterState state) =>
        state == CharacterState.Idle || state == CharacterState.Move;

    public static bool CanAttack(CharacterState state) =>
        state == CharacterState.Idle || state == CharacterState.Move || state == CharacterState.Jump;

    public static bool CanChangeDirection(CharacterState state) =>
        state != CharacterState.Attack && state != CharacterState.Skill;
}
```

#### CharacterStats.cs（约 150 行）

> 基于 Add+Rate 双值设计（参考 09-属性系统.md），公式：`最终值 = (基础值 + Σ加值) × (1 + Σ倍率)`

```csharp
[Serializable]
public class CharacterStats
{
    // ========== 基础值 ==========
    public int BaseHP = 1000;
    public int BaseMP = 100;
    public int BaseAttack = 100;
    public int BaseDefense = 50;
    public float BaseMoveSpeed = 3f;
    public float BaseAttackSpeed = 1f;
    public float BaseJumpForce = 8f;

    // ========== 加值（来自 Buff/装备） ==========
    public int HPAdd;
    public int MPAdd;
    public int AttackAdd;
    public int DefenseAdd;
    public float MoveSpeedAdd;
    public float AttackSpeedAdd;

    // ========== 倍率（来自 Buff/装备） ==========
    public float HPRate = 1f;
    public float MPRate = 1f;
    public float AttackRate = 1f;
    public float DefenseRate = 1f;
    public float MoveSpeedRate = 1f;
    public float AttackSpeedRate = 1f;

    // ========== 暴击 ==========
    public float CriticalRate = 0.05f;
    public float CriticalDamage = 1.5f;

    // ========== 上下限保护 ==========
    public const float MIN_SPEED_RATE = -0.5f;  // 最减速 50%
    public const float MAX_SPEED_RATE = 2f;     // 最大加速 200%

    // ========== 当前 HP/MP ==========
    public int CurrentHP;
    public int CurrentMP;

    // ========== 最终值计算 ==========
    public int MaxHP => Math.Max(1, (int)((BaseHP + HPAdd) * HPRate));
    public int MaxMP => Math.Max(1, (int)((BaseMP + MPAdd) * MPRate));
    public int Attack => Math.Max(0, (int)((BaseAttack + AttackAdd) * AttackRate));
    public int Defense => Math.Max(0, (int)((BaseDefense + DefenseAdd) * DefenseRate));

    public float MoveSpeed
    {
        get
        {
            float rate = Math.Clamp(MoveSpeedRate, 1f + MIN_SPEED_RATE, 1f + MAX_SPEED_RATE);
            return Math.Max(0.5f, (BaseMoveSpeed + MoveSpeedAdd) * rate);
        }
    }

    public float AttackSpeed
    {
        get
        {
            float rate = Math.Clamp(AttackSpeedRate, 1f + MIN_SPEED_RATE, 1f + MAX_SPEED_RATE);
            return Math.Max(0.5f, (BaseAttackSpeed + AttackSpeedAdd) * rate);
        }
    }

    // 初始化
    public void Init()
    {
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
    }

    // 伤害计算（含属性/Buff 集成）
    public int CalculateDamage(AttackInfo attack, CharacterStats defender);
}
```

---

### 3.2 移动系统

#### MovementSystem.cs（约 200 行）

```csharp
public class MovementSystem : MonoBehaviour
{
    [Header("Components")]
    private CharacterBase _character;
    private Rigidbody2D _rb;

    [Header("Config")]
    public float MoveSpeed = 3f;
    public float JumpForce = 8f;
    public float Gravity = -20f;

    [Header("State")]
    public bool IsGrounded;
    public bool IsMoving;
    public Vector2 Velocity;

    // 2.5D: Z 轴深度（屏幕上下）
    public float ZPosition;        // 当前深度位置
    public float ZVelocity;        // 深度方向速度
    public float ZMin = -2f;       // 深度下限
    public float ZMax = 2f;        // 深度上限

    // 方法
    public void Move(float direction);       // 水平移动
    public void MoveZ(float direction);      // 深度移动（上下行）
    public void Jump();                       // 跳跃
    public void ApplyKnockback(Vector2 force, float duration);  // 击退

    // 每帧更新
    public void Tick(float deltaTime);
}
```

#### 关键点：2.5D 实现

```
屏幕坐标系：
  Y↑
   │
   │    角色
   │
   └────────→ X

Z 轴（深度）映射到屏幕 Y 的偏移：
  - Z = 0: 标准位置
  - Z > 0: 屏幕上移（远景）
  - Z < 0: 屏幕下移（近景）

碰撞判定：XY 平面重叠 + Z 范围重叠
```

---

### 3.3 碰撞系统

#### CollisionBox.cs（约 100 行）

```csharp
public struct CollisionBox
{
    public Vector2 Center;      // 中心点（相对于角色）
    public Vector2 Size;        // 尺寸
    public float ZMin;          // Z 深度范围下限
    public float ZMax;          // Z 深度范围上限

    // 获取世界空间 AABB
    public Rect GetWorldRect(Vector2 position, bool facingRight);

    // 碰撞检测
    public static bool Overlaps(
        CollisionBox a, Vector2 posA, bool faceRightA,
        CollisionBox b, Vector2 posB, bool faceRightB);
}
```

#### CollisionSystem.cs（约 150 行）

```csharp
public class CollisionSystem : Singleton<CollisionSystem>
{
    // 空间分区（网格加速）
    private Dictionary<GridPos, List<CharacterBase>> _gridCharacters;
    private float _cellSize = 2f;

    // 注册/注销
    public void Register(CharacterBase character);
    public void Unregister(CharacterBase character);

    // 查询
    public List<CharacterBase> GetNearbyCharacters(Vector2 position, float radius);

    // 碰撞检测
    public CharacterBase CheckHit(
        CollisionBox attackBox,
        Vector2 attackerPos,
        bool attackerFacingRight,
        int attackerTeam,
        float attackerZ);
}
```

---

### 3.4 战斗系统

#### AttackInfo.cs（约 80 行）

```csharp
public class AttackInfo
{
    public int Power = 100;                 // 基础伤害
    public float CriticalRate = 0.05f;      // 暴击率
    public int KnockbackForce = 5;          // 击退力度
    public int KnockbackType = 1;           // 0=无, 1=水平, 2=击飞
    public float StunDuration = 0.2f;       // 硬直时间
    public int PierceCount = 1;             // 穿透数量（枪手特色）

    // Buff 触发（参考 AttackInfo.active_statuses）
    public int TriggerBuffId = -1;          // 命中后触发的 Buff，-1 = 无
    public float TriggerBuffDuration;       // 触发 Buff 持续时间

    // 碰撞盒
    public CollisionBox HitBox;

    // 来源
    public CharacterBase Attacker;
}
```

#### DamageCalculator.cs（约 100 行）

```csharp
// 伤害计算器（集成属性/Buff 系统）
public static class DamageCalculator
{
    public static DamageInfo Calculate(AttackInfo attack, CharacterBase defender)
    {
        var attacker = attack.Attacker;
        var attackerStats = attacker.Stats;
        var defenderStats = defender.Stats;

        // 1. 无敌检查（参考 HasInvincibleAppendage）
        if (defender.BuffManager.IsInvincible())
            return DamageInfo.Create(0, false);

        // 2. 基础伤害（使用 Add+Rate 计算后的攻击力）
        int baseDamage = (int)(attackerStats.Attack * attack.Power / 100f);

        // 3. 暴击判定
        bool isCritical = UnityEngine.Random.value < attackerStats.CriticalRate;
        if (isCritical)
            baseDamage = (int)(baseDamage * attackerStats.CriticalDamage);

        // 4. 防御减免
        int defense = defenderStats.Defense;
        int finalDamage = Math.Max(1, baseDamage - defense);

        // 5. 构建伤害信息
        return DamageInfo.Create(finalDamage, isCritical);
    }
}
```

#### Bullet.cs（约 120 行）

```csharp
public class Bullet : MonoBehaviour, IPooledObject
{
    public AttackInfo AttackData;
    public Vector2 Direction;
    public float Speed = 10f;
    public float MaxDistance = 10f;
    public int PierceRemaining = 1;         // 剩余穿透次数

    private float _traveledDistance;
    private List<CharacterBase> _hitTargets = new List<CharacterBase>();

    // 每帧更新
    public void Tick(float deltaTime)
    {
        // 移动
        transform.position += (Vector3)(Direction * Speed * deltaTime);
        _traveledDistance += Speed * deltaTime;

        // 碰撞检测
        CheckCollision();

        // 超出距离或穿透耗尽
        if (_traveledDistance >= MaxDistance || PierceRemaining <= 0)
            ReturnToPool();
    }

    private void CheckCollision()
    {
        var hits = CollisionSystem.Instance.CheckHit(...);
        foreach (var target in hits)
        {
            if (_hitTargets.Contains(target)) continue;

            // 造成伤害
            target.TakeDamage(CreateDamageInfo());
            _hitTargets.Add(target);
            PierceRemaining--;

            // 特效
            SpawnHitEffect(target.transform.position);
        }
    }
}
```

---

### 3.5 技能系统

> 参考 11-技能系统.md，简化实现五层架构中的核心三层。

#### SkillControlType.cs（约 30 行）

```csharp
public enum SkillControlType
{
    Tap,            // 单击
    TapTap,         // 双击（追加攻击/终结攻击）
    OnPress,        // 长按（蓄力/持续）
    Max,
}

public enum UsableResult
{
    Success,
    CoolTime,           // 冷却中
    LackMp,             // MP 不足
    StateFail,          // 状态不可执行
}
```

#### SkillBase.cs（约 200 行）

```csharp
public abstract class SkillBase
{
    // 基础信息
    public int SkillId;
    public string SkillName;
    public float Cooldown = 1f;
    public int MpCost = 0;
    public int Level = 1;

    // 控制属性
    public SkillControlType ControlType = SkillControlType.Tap;
    public bool IsChargable;          // 可蓄力
    public bool IsSustainable;        // 可持续（长按）
    public bool IsFinishableAttack;   // 可终结连击

    // 运行时状态
    protected float _currentCooldown;
    protected bool _isUsing;
    protected bool _isFinished;
    protected CharacterBase _owner;

    public bool IsReady => _currentCooldown <= 0 && Level > 0;
    public bool IsUsing => _isUsing;

    // === 可用性检查（参考 Skill.isUsable） ===
    public virtual UsableResult IsUsable()
    {
        if (Level <= 0) return UsableResult.StateFail;
        if (_currentCooldown > 0) return UsableResult.CoolTime;
        if (_owner.Stats.CurrentMP < MpCost) return UsableResult.LackMp;
        return UsableResult.Success;
    }

    // === 生命周期（参考 Skill 基类） ===
    // 开始 → 运行 → 结束
    public virtual bool TryUse(CharacterBase owner)
    {
        _owner = owner;
        if (IsUsable() != UsableResult.Success) return false;

        _isUsing = true;
        _isFinished = false;
        _owner.Stats.CurrentMP -= MpCost;
        OnStart();
        return true;
    }

    public virtual void OnStart() { }              // 技能开始（播放动画、创建特效）
    public virtual void OnUpdate(float dt) { }     // 每帧更新（移动子弹、检测碰撞）
    public virtual void OnEnterFrame(int frame) { } // 动画帧事件（创建攻击框、生成子弹）
    public virtual void OnEnd()                    // 技能结束（清理资源、开始冷却）
    {
        _isUsing = false;
        _isFinished = true;
        _currentCooldown = Cooldown;
    }

    // 冷却
    public void TickCooldown(float dt)
    {
        if (_currentCooldown > 0) _currentCooldown -= dt;
    }
}
```

#### SkillManager.cs（约 100 行）

```csharp
public class SkillManager
{
    private List<SkillBase> _skills = new List<SkillBase>();
    private CharacterBase _owner;

    public void Init(CharacterBase owner) => _owner = owner;

    public void AddSkill(SkillBase skill)
    {
        skill.Init(_owner);
        _skills.Add(skill);
    }

    // 使用技能（参考 IRDCharacter.useSkill）
    public bool UseSkill(int skillId)
    {
        var skill = _skills.Find(s => s.SkillId == skillId);
        if (skill == null) return false;
        return skill.TryUse(_owner);
    }

    // 每帧更新所有技能
    public void Update(float deltaTime)
    {
        foreach (var skill in _skills)
        {
            skill.TickCooldown(deltaTime);
            if (skill.IsUsing)
            {
                skill.OnUpdate(deltaTime);
                if (skill.IsFinished)
                    skill.OnEnd();
            }
        }
    }

    public SkillBase GetSkill(int skillId) => _skills.Find(s => s.SkillId == skillId);
}
```

#### NormalAttack.cs（约 80 行）

```csharp
public class NormalAttack : SkillBase
{
    public AttackInfo AttackData;
    public GameObject BulletPrefab;

    public override void OnStart()
    {
        // 创建子弹
        var bullet = ObjectPool.Instance.Spawn<Bullet>(BulletPrefab);
        bullet.AttackData = AttackData;
        bullet.AttackData.Attacker = _owner;
        bullet.Direction = _owner.IsFacingRight ? Vector2.right : Vector2.left;
        bullet.transform.position = _owner.transform.position;

        // 单次攻击直接结束
        OnEnd();
    }
}
```

#### SkillGatling.cs（约 120 行）

```csharp
// 加特林技能：持续射击示例（参考 11-技能系统.md 中的 ShootSkill）
public class SkillGatling : SkillBase
{
    public int BulletCount = 10;
    public float ShootInterval = 0.1f;
    public int Damage = 50;
    public float BulletSpeed = 12f;
    public GameObject BulletPrefab;

    private float _shootTimer;
    private int _shotCount;

    public SkillGatling()
    {
        ControlType = SkillControlType.OnPress;
        IsSustainable = true;
        Cooldown = 5f;
        MpCost = 30;
    }

    public override void OnStart()
    {
        _shootTimer = 0f;
        _shotCount = 0;
    }

    public override void OnUpdate(float deltaTime)
    {
        _shootTimer += deltaTime;

        if (_shootTimer >= ShootInterval && _shotCount < BulletCount)
        {
            Shoot();
            _shootTimer = 0f;
            _shotCount++;
        }

        if (_shotCount >= BulletCount)
            OnEnd();
    }

    private void Shoot()
    {
        var bullet = ObjectPool.Instance.Spawn<Bullet>(BulletPrefab);
        bullet.AttackData = new AttackInfo { Attacker = _owner, Power = Damage * Level };
        bullet.Direction = _owner.IsFacingRight ? Vector2.right : Vector2.left;
        bullet.Speed = BulletSpeed;
        bullet.transform.position = _owner.transform.position;
    }
}
```

---

### 3.6 属性系统

> 参考 09-属性系统.md，使用 Add+Rate 双值设计。

#### StatType.cs（约 30 行）

```csharp
public enum StatType
{
    HP, MP,
    Attack, Defense,
    MoveSpeed, AttackSpeed,
    PhysicalCriticalRate, PhysicalCriticalDamage,
}

public enum ChangeKind
{
    Add,    // 加值变更
    Rate    // 倍率变更
}
```

#### StatChange.cs（约 30 行）

```csharp
public struct StatChange
{
    public StatType Type;
    public ChangeKind Kind;
    public float Value;
    public int SourceId;  // 来源 ID（用于撤销追踪）
}
```

#### AttributeHelper.cs（约 80 行）

```csharp
// 属性变更应用工具（参考 ActiveStaticInfo.updateActiveStaticInfo）
public static class AttributeHelper
{
    // 应用属性变更
    public static void ApplyChange(CharacterStats stats, StatChange change, bool add)
    {
        float value = add ? change.Value : -change.Value;

        switch (change.Type)
        {
            case StatType.Attack:
                if (change.Kind == ChangeKind.Add)
                    stats.AttackAdd += (int)value;
                else
                    stats.AttackRate += value;
                break;

            case StatType.Defense:
                if (change.Kind == ChangeKind.Add)
                    stats.DefenseAdd += (int)value;
                else
                    stats.DefenseRate += value;
                break;

            case StatType.MoveSpeed:
                if (change.Kind == ChangeKind.Add)
                    stats.MoveSpeedAdd += value;
                else
                    stats.MoveSpeedRate += value;
                break;

            case StatType.AttackSpeed:
                if (change.Kind == ChangeKind.Add)
                    stats.AttackSpeedAdd += value;
                else
                    stats.AttackSpeedRate += value;
                break;

            case StatType.HP:
                if (change.Kind == ChangeKind.Add)
                    stats.HPAdd += (int)value;
                else
                    stats.HPRate += value;
                break;
        }
    }
}
```

---

### 3.7 Buff 系统

> 参考 10-Buff系统.md，实现四层架构的简化版。

#### BuffBase.cs（约 120 行）

```csharp
public enum BuffType
{
    None,
    StatChange,     // 属性变更（对应 CNChangeStatus）
    HealOverTime,   // 持续治疗（对应 ChangeHp）
    DamageOverTime, // 持续伤害
    Invincible,     // 无敌
    Stun,           // 眩晕
    Slow,           // 减速
}

public class BuffBase
{
    // 身份
    public int BuffId;
    public string Name;
    public BuffType Type;

    // 时间（参考 IRDAppendage.endTimer_）
    public float Duration = -1f;     // -1 = 永久
    public float RemainingTime;
    public bool IsValid = true;

    // Buff/Debuff
    public bool IsBuff = true;

    // 叠加（参考 CNRDAppendageManager overlapMaxCount）
    public int MaxStack = 1;
    public int CurrentStack = 1;

    // 来源追踪
    public int SourceSkillId;
    public CharacterBase Source;
    public CharacterBase Target;

    // 属性变更数据
    public List<StatChange> Changes = new List<StatChange>();

    // 特效
    public GameObject StartEffect;
    public GameObject LoopEffect;
    public GameObject EndEffect;

    // 生命周期（参考 IRDAppendage）
    public virtual void OnStart()
    {
        RemainingTime = Duration;
        // 应用属性变更
        foreach (var change in Changes)
            AttributeHelper.ApplyChange(Target.Stats, change, true);

        if (StartEffect != null)
            SpawnEffect(StartEffect);
    }

    public virtual void OnUpdate(float deltaTime)
    {
        if (!IsValid) return;

        if (Duration > 0)
        {
            RemainingTime -= deltaTime;
            if (RemainingTime <= 0)
                IsValid = false;
        }
    }

    public virtual void OnEnd()
    {
        // 撤销属性变更（参考 procStatusInvalidAppendage）
        foreach (var change in Changes)
            AttributeHelper.ApplyChange(Target.Stats, change, false);

        if (EndEffect != null)
            SpawnEffect(EndEffect);
    }

    private void SpawnEffect(GameObject prefab)
    {
        if (Target != null && prefab != null)
            GameObject.Instantiate(prefab, Target.transform.position, Quaternion.identity);
    }
}
```

#### BuffManager.cs（约 150 行）

```csharp
// Buff 管理器（参考 CNRDAppendageManager）
public class BuffManager
{
    private List<BuffBase> _buffs = new List<BuffBase>();
    private CharacterBase _owner;

    public BuffManager(CharacterBase owner) => _owner = owner;

    // 添加 Buff（参考 addAppendage + ValidationOverlapNormal）
    public BuffBase AddBuff(BuffBase template)
    {
        var existing = _buffs.Find(b => b.BuffId == template.BuffId);

        if (existing != null)
        {
            if (existing.CurrentStack < existing.MaxStack)
            {
                // 叠加
                existing.CurrentStack++;
                existing.RemainingTime = existing.Duration;
                return existing;
            }
            else
            {
                // 覆盖旧的
                RemoveBuff(existing.BuffId);
            }
        }

        // 创建新 Buff
        var newBuff = CloneBuff(template);
        newBuff.Target = _owner;
        newBuff.OnStart();
        _buffs.Add(newBuff);
        return newBuff;
    }

    // 移除 Buff（参考 RemoveAppendageFromID）
    public void RemoveBuff(int buffId)
    {
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            if (_buffs[i].BuffId == buffId)
            {
                _buffs[i].OnEnd();
                _buffs.RemoveAt(i);
            }
        }
    }

    // 移除所有 Debuff（参考 RemoveAllDebuffs）
    public void RemoveAllDebuffs()
    {
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            if (!_buffs[i].IsBuff)
            {
                _buffs[i].OnEnd();
                _buffs.RemoveAt(i);
            }
        }
    }

    // 清空所有 Buff（参考 RemoveAllAppedages）
    public void ClearAllBuffs()
    {
        foreach (var buff in _buffs) buff.OnEnd();
        _buffs.Clear();
    }

    // 每帧更新（参考 mainProc：倒序遍历 + 时间到期检测）
    public void Update(float deltaTime)
    {
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            var buff = _buffs[i];
            buff.OnUpdate(deltaTime);

            if (!buff.IsValid)
            {
                buff.OnEnd();
                _buffs.RemoveAt(i);
            }
        }
    }

    // 查询（参考 getImmuneType / HasInvincibleAppendage）
    public bool HasBuff(int buffId) => _buffs.Exists(b => b.BuffId == buffId);
    public BuffBase GetBuff(int buffId) => _buffs.Find(b => b.BuffId == buffId);
    public List<BuffBase> GetAllBuffs() => _buffs;
    public bool IsInvincible() => _buffs.Exists(b => b.Type == BuffType.Invincible && b.IsValid);
    public bool IsStunned() => _buffs.Exists(b => b.Type == BuffType.Stun && b.IsValid);

    private BuffBase CloneBuff(BuffBase template)
    {
        return new BuffBase
        {
            BuffId = template.BuffId,
            Name = template.Name,
            Type = template.Type,
            Duration = template.Duration,
            IsBuff = template.IsBuff,
            MaxStack = template.MaxStack,
            Changes = new List<StatChange>(template.Changes),
            StartEffect = template.StartEffect,
            LoopEffect = template.LoopEffect,
            EndEffect = template.EndEffect,
        };
    }
}
```

#### 具体 Buff 实现

```csharp
// 属性变更 Buff（参考 CNChangeStatus）
public class StatChangeBuff : BuffBase
{
    public StatChangeBuff() { Type = BuffType.StatChange; }

    // 可通过 Changes 列表配置多种属性变更
    // 例如：攻击力+100、移动速度+20%
}

// 持续治疗 Buff（参考 ChangeHp EFFECT_TYPE_SLOW_HEAL）
public class HealOverTimeBuff : BuffBase
{
    public float HealPerSecond;
    private float _accumulatedTime;

    public HealOverTimeBuff() { Type = BuffType.HealOverTime; }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        if (!IsValid) return;

        _accumulatedTime += deltaTime;
        if (_accumulatedTime >= 1f)
        {
            Target.Heal((int)(HealPerSecond * _accumulatedTime * CurrentStack));
            _accumulatedTime = 0f;
        }
    }
}

// 眩晕 Buff
public class StunBuff : BuffBase
{
    public StunBuff() { Type = BuffType.Stun; IsBuff = false; }

    public override void OnStart()
    {
        base.OnStart();
        Target.IsStunned = true;
    }

    public override void OnEnd()
    {
        Target.IsStunned = false;
        base.OnEnd();
    }
}
```

---

### 3.8 AI 系统

#### MonsterAI.cs（约 150 行）

```csharp
public class MonsterAI : MonoBehaviour
{
    private CharacterBase _character;
    private CharacterBase _target;

    [Header("Config")]
    public float DetectRange = 5f;
    public float AttackRange = 1.5f;
    public float AttackCooldown = 1f;

    private float _attackTimer;
    private enum AIState { Idle, Chase, Attack, Return }
    private AIState _state;

    public void Tick(float deltaTime)
    {
        if (_character.IsDead) return;

        FindTarget();

        switch (_state)
        {
            case AIState.Idle:
                if (_target != null && GetDistanceToTarget() < DetectRange)
                    _state = AIState.Chase;
                break;

            case AIState.Chase:
                if (GetDistanceToTarget() <= AttackRange)
                    _state = AIState.Attack;
                else
                    MoveTowardTarget();
                break;

            case AIState.Attack:
                if (_attackTimer <= 0)
                {
                    DoAttack();
                    _attackTimer = AttackCooldown;
                }
                else
                {
                    _attackTimer -= deltaTime;
                }

                if (GetDistanceToTarget() > AttackRange * 1.5f)
                    _state = AIState.Chase;
                break;
        }
    }
}
```

---

### 3.9 UI 系统

#### VirtualJoystick.cs（约 100 行）

```csharp
public class VirtualJoystick : MonoBehaviour
{
    public RectTransform Background;
    public RectTransform Handle;
    public float MaxRadius = 50f;

    public Vector2 Direction { get; private set; }
    public bool IsPressed { get; private set; }

    // 事件
    public event Action<Vector2> OnDirectionChanged;

    // 触摸处理
    public void OnPointerDown(PointerEventData eventData);
    public void OnDrag(PointerEventData eventData);
    public void OnPointerUp(PointerEventData eventData);
}
```

#### SkillButton.cs（约 80 行）

```csharp
public class SkillButton : MonoBehaviour, IPointerDownHandler
{
    public SkillBase Skill;
    public CharacterBase Owner;
    public Image CooldownOverlay;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Skill.IsReady && Owner != null)
            Skill.OnUse(Owner);
    }

    public void UpdateCooldown()
    {
        CooldownOverlay.fillAmount = Skill._currentCooldown / Skill.Cooldown;
    }
}
```

#### DamageNumber.cs（约 60 行）

```csharp
public class DamageNumber : MonoBehaviour, IPooledObject
{
    public Text Text;
    public float FloatSpeed = 1f;
    public float FadeDuration = 0.5f;

    public void Show(int damage, Vector3 position, bool isCritical)
    {
        Text.text = damage.ToString();
        Text.color = isCritical ? Color.red : Color.white;
        transform.position = position;

        // 动画：上浮 + 淡出
        StartCoroutine(FloatAndFade());
    }
}
```

---

### 3.10 地图系统

#### PassageGrid.cs（约 100 行）

```csharp
public class PassageGrid : MonoBehaviour
{
    [Header("Config")]
    public int Width = 20;
    public int Height = 15;
    public float CellSize = 1f;

    // 通行数据：true = 可通行
    private bool[,] _passable;

    public void Init()
    {
        _passable = new bool[Width, Height];
        // 默认全部可通行
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _passable[x, y] = true;
    }

    public bool IsPassable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return _passable[x, y];
    }

    public void SetPassable(int x, int y, bool passable)
        => _passable[x, y] = passable;

    // 世界坐标转网格坐标
    public Vector2Int WorldToGrid(Vector2 worldPos);
    public Vector2 GridToWorld(int x, int y);
}
```

---

## 四、实现顺序与依赖

### Phase 1：基础框架（Day 1-2）

```
Core/
  ├── Singleton.cs          [无依赖]
  ├── ObjectPool.cs         [Singleton]
  └── GameLoop.cs           [ObjectPool]

Character/
  ├── CharacterStats.cs     [无依赖] ← Add+Rate 双值设计
  ├── CharacterState.cs     [无依赖]
  └── CharacterBase.cs      [Stats, State]
```

### Phase 2：属性与 Buff（Day 2-3）

```
Attribute/
  ├── StatType.cs           [无依赖]
  ├── StatChange.cs         [无依赖]
  └── AttributeHelper.cs    [CharacterStats]

Buff/
  ├── BuffBase.cs           [StatChange, AttributeHelper]
  ├── BuffFactory.cs        [BuffBase]
  └── BuffManager.cs        [BuffBase]
```

### Phase 3：移动与碰撞（Day 3-4）

```
Movement/
  ├── MovementSystem.cs     [CharacterBase]

Collision/
  ├── CollisionBox.cs       [无依赖]
  ├── GridPartition.cs      [无依赖]
  └── CollisionSystem.cs    [CollisionBox, GridPartition]

Map/
  └── PassageGrid.cs        [无依赖]
```

### Phase 4：战斗核心（Day 4-5）

```
Combat/
  ├── AttackInfo.cs         [CollisionBox]
  ├── DamageInfo.cs         [无依赖]
  ├── DamageCalculator.cs   [AttackInfo, CharacterStats, BuffManager]
  ├── HitBox.cs             [CollisionBox]
  └── Bullet.cs             [AttackInfo, DamageCalculator, CollisionSystem, ObjectPool]
```

### Phase 5：技能与 AI（Day 5-7）

```
Skill/
  ├── SkillControlType.cs   [无依赖]
  ├── SkillBase.cs          [CharacterBase, AttackInfo]
  ├── SkillManager.cs       [SkillBase]
  ├── NormalAttack.cs       [SkillBase, Bullet]
  └── SkillGatling.cs       [SkillBase, Bullet]

AI/
  └── MonsterAI.cs          [CharacterBase]
```

### Phase 6：UI 与整合（Day 7-8）

```
UI/
  ├── VirtualJoystick.cs    [无依赖]
  ├── SkillButton.cs        [SkillBase]
  ├── HealthBar.cs          [CharacterStats]
  ├── DamageNumber.cs       [ObjectPool]
  └── BuffIcon.cs           [BuffManager]
```

---

## 五、数据配置

### CharacterData（ScriptableObject）

```csharp
[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/Character")]
public class CharacterData : ScriptableObject
{
    public int MaxHP = 1000;
    public int PhysicalAttack = 100;
    public int PhysicalDefense = 50;
    public float MoveSpeed = 3f;
    public float JumpForce = 8f;
    public float AttackSpeed = 1f;

    // 碰撞盒尺寸
    public Vector2 BodyBoxSize = new Vector2(1f, 1.5f);
    public Vector2 AttackBoxSize = new Vector2(1.5f, 1f);
}
```

### SkillData（ScriptableObject）

```csharp
[CreateAssetMenu(fileName = "SkillData", menuName = "Data/Skill")]
public class SkillData : ScriptableObject
{
    public int SkillId;
    public string SkillName;
    public float Cooldown = 1f;
    public int Power = 100;
    public int PierceCount = 1;
    public float KnockbackForce = 5f;
    public GameObject BulletPrefab;
}
```

---

## 六、Demo 场景配置

### 场景层级

```
Scene: DemoBattle
├── MapRoot
│   ├── Background          # 背景
│   ├── Ground              # 地面碰撞
│   └── PassageGrid         # 通行网格（不可见）
│
├── Characters
│   ├── Player              # 玩家
│   └── MonsterSpawner      # 怪物生成点
│
├── BulletPool              # 子弹对象池容器
│
└── UIRoot
    ├── Canvas
    │   ├── VirtualJoystick
    │   ├── SkillButtons
    │   ├── PlayerHPBar
    │   └── DamageNumbers
    └── EventSystem
```

---

## 七、代码量估算

| 系统 | 文件数 | 预估行数 |
|------|--------|---------|
| Core | 3 | 150 |
| Character | 4 | 600 |
| Attribute | 3 | 200 |
| Buff | 5 | 450 |
| Movement | 2 | 300 |
| Collision | 3 | 350 |
| Combat | 5 | 500 |
| Skill | 5 | 530 |
| AI | 2 | 200 |
| UI | 5 | 400 |
| Map | 2 | 150 |
| **总计** | **39** | **~3830** |

---

## 八、后续扩展点

完成 Demo 后可逐步添加：

1. **更多技能** — 参考文档中的 146+ 技能设计，增加滑动连招（SlideSkills）
2. **更多 Buff 类型** — 光环（CNAuraMaster）、抓取（Hold）、无敌（Unbeatable）等
3. **元素属性系统** — 火/水/光/暗元素攻击与抗性
4. **异常状态系统** — 中毒、冰冻、石化、睡眠等完整异常状态
5. **多职业** — 扩展 CharacterBase 子类（剑士、格斗家、魔法师、牧师）
6. **A* 寻路** — 替换简化 AI
7. **副本迷宫** — MazeScript 拓扑
8. **网络同步** — MovementInfo 结构 + DamageSyncInfo
9. **粒子特效** — 命中、技能释放
10. **音效系统** — 攻击、受击、技能

---

## 九、关键设计决策

| 决策点 | Demo 方案 | 完整版方案 |
|--------|----------|-----------|
| 状态机 | 简化枚举 + switch | 层级状态机 (HSM) |
| 碰撞 | Unity 2D 物理 + 自定义 Z 检测 | 完全自定义 AABB |
| 对象池 | Unity Instantiate + 缓存 | 自定义 Pool + 预分配 |
| AI | 简化状态枚举 | AIStateManager + AIState 类 |
| 技能 | 类实例 + 生命周期方法 | 运行时 Skill 实例 + SkillScript 配置 + 热重载 |
| 技能控制 | Tap + OnPress | SkillControlType 8 种 + 子类型 |
| 属性 | Add + Rate 双值 | ActiveStaticInfo 多层倍率叠加 + 对象池 |
| Buff | 列表 + 叠加/覆盖 | AppendageFactory + AppendageManager + 光环系统 |
| 伤害 | 攻防相减 + 暴击 | 含属性/Buff/元素/异常状态完整公式 |
| 地图 | 单场景 | 多场景加载 + MazeScript |

---

**开始实现建议**：从 Phase 1 开始，每完成一个 Phase 进行单元测试，确保基础功能正常后再进入下一阶段。
