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
│   │   └── CharacterStats.cs    # 属性数据
│   │
│   ├── Movement/                # 移动系统
│   │   ├── MovementSystem.cs    # 移动逻辑
│   │   └── JumpSystem.cs        # 跳跃逻辑
│   │
│   ├── Combat/                  # 战斗系统
│   │   ├── AttackInfo.cs        # 攻击数据
│   │   ├── DamageInfo.cs        # 伤害数据
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
│   │   ├── SkillBase.cs         # 技能基类
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
│       └── DamageNumber.cs      # 伤害数字
│
├── Prefabs/
│   ├── Player.prefab
│   ├── Monster.prefab
│   ├── Bullet.prefab
│   └── UI/
│       ├── Joystick.prefab
│       └── SkillButton.prefab
│
└── Data/
    ├── CharacterData.asset      # 角色配置
    ├── SkillData.asset          # 技能配置
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

    // 碰撞
    public CollisionBox BodyBox { get; protected set; }      // 受击框
    public CollisionBox AttackBox { get; protected set; }    // 攻击框（攻击时激活）

    // 基础字段
    public int TeamId;                    // 阵营（0=玩家, 1=敌人）
    public bool IsFacingRight;            // 朝向
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

#### CharacterStats.cs（约 80 行）

```csharp
[Serializable]
public class CharacterStats
{
    // 基础属性
    public int MaxHP = 1000;
    public int CurrentHP;
    public int PhysicalAttack = 100;
    public int PhysicalDefense = 50;

    // 速度
    public float MoveSpeed = 3f;
    public float JumpForce = 8f;

    // 战斗
    public float AttackSpeed = 1f;
    public float CriticalRate = 0.05f;
    public float CriticalDamage = 1.5f;

    // 初始化
    public void Init() => CurrentHP = MaxHP;

    // 伤害计算
    public int CalculateDamage(AttackInfo attack, bool isCritical);
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

    // 碰撞盒
    public CollisionBox HitBox;

    // 来源
    public CharacterBase Attacker;
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

#### SkillBase.cs（约 100 行）

```csharp
public abstract class SkillBase : ScriptableObject
{
    public int SkillId;
    public string SkillName;
    public float Cooldown = 1f;
    public int MpCost = 0;

    // 运行时状态
    protected float _currentCooldown;

    public bool IsReady => _currentCooldown <= 0;

    // 生命周期
    public virtual void OnUse(CharacterBase owner);
    public virtual void OnUpdate(float deltaTime);

    // 冷却
    protected void StartCooldown() => _currentCooldown = Cooldown;
    public void TickCooldown(float deltaTime) => _currentCooldown -= deltaTime;
}
```

#### NormalAttack.cs（约 80 行）

```csharp
[CreateAssetMenu(fileName = "NormalAttack", menuName = "Skills/Normal Attack")]
public class NormalAttack : SkillBase
{
    public AttackInfo AttackData;
    public GameObject BulletPrefab;

    public override void OnUse(CharacterBase owner)
    {
        if (!IsReady) return;

        // 创建子弹
        var bullet = ObjectPool.Instance.Spawn<Bullet>(BulletPrefab);
        bullet.AttackData = AttackData;
        bullet.AttackData.Attacker = owner;
        bullet.Direction = owner.IsFacingRight ? Vector2.right : Vector2.left;
        bullet.transform.position = owner.transform.position;

        StartCooldown();
    }
}
```

---

### 3.6 AI 系统

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

### 3.7 UI 系统

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

### 3.8 地图系统

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
  ├── CharacterStats.cs     [无依赖]
  ├── CharacterState.cs     [无依赖]
  └── CharacterBase.cs      [Stats, State]
```

### Phase 2：移动与碰撞（Day 2-3）

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

### Phase 3：战斗核心（Day 3-4）

```
Combat/
  ├── AttackInfo.cs         [CollisionBox]
  ├── DamageInfo.cs         [无依赖]
  ├── HitBox.cs             [CollisionBox]
  └── Bullet.cs             [AttackInfo, CollisionSystem, ObjectPool]
```

### Phase 4：技能与 AI（Day 4-5）

```
Skill/
  ├── SkillBase.cs          [CharacterBase]
  └── NormalAttack.cs       [SkillBase, Bullet]

AI/
  └── MonsterAI.cs          [CharacterBase]
```

### Phase 5：UI 与整合（Day 5-6）

```
UI/
  ├── VirtualJoystick.cs    [无依赖]
  ├── SkillButton.cs        [SkillBase]
  ├── HealthBar.cs          [CharacterStats]
  └── DamageNumber.cs       [ObjectPool]
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
| Character | 4 | 500 |
| Movement | 2 | 300 |
| Collision | 3 | 350 |
| Combat | 4 | 400 |
| Skill | 3 | 250 |
| AI | 2 | 200 |
| UI | 4 | 300 |
| Map | 2 | 150 |
| **总计** | **27** | **~2600** |

---

## 八、后续扩展点

完成 Demo 后可逐步添加：

1. **更多技能** — 参考文档中的 146+ 技能设计
2. **Buff 系统** — AttackInfo 中的 active_statuses
3. **多职业** — 扩展 CharacterBase 子类
4. **A* 寻路** — 替换简化 AI
5. **副本迷宫** — MazeScript 拓扑
6. **网络同步** — MovementInfo 结构
7. **粒子特效** — 命中、技能释放
8. **音效系统** — 攻击、受击、技能

---

## 九、关键设计决策

| 决策点 | Demo 方案 | 完整版方案 |
|--------|----------|-----------|
| 状态机 | 简化枚举 + switch | 层级状态机 (HSM) |
| 碰撞 | Unity 2D 物理 + 自定义 Z 检测 | 完全自定义 AABB |
| 对象池 | Unity Instantiate + 缓存 | 自定义 Pool + 预分配 |
| AI | 简化状态枚举 | AIStateManager + AIState 类 |
| 技能 | ScriptableObject | 运行时 Skill 实例 + 热重载 |
| 地图 | 单场景 | 多场景加载 + MazeScript |

---

**开始实现建议**：从 Phase 1 开始，每完成一个 Phase 进行单元测试，确保基础功能正常后再进入下一阶段。
