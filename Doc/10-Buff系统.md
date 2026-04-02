# Buff 系统实现分析

## 系统架构总览

```
┌─────────────────────────────────────────────────────────────────┐
│                      Buff 系统四层架构                            │
├─────────────────────────────────────────────────────────────────┤
│  第一层：AppendageScript                                         │
│         Buff 配置脚本（持续时间、特效、参数等）                     │
├─────────────────────────────────────────────────────────────────┤
│  第二层：AppendageFactory                                        │
│         Buff 工厂（创建、对象池、类型映射）                         │
├─────────────────────────────────────────────────────────────────┤
│  第三层：IRDAppendage 及子类                                      │
│         Buff 实体（生命周期、属性应用、回调事件）                    │
├─────────────────────────────────────────────────────────────────┤
│  第四层：CNRDAppendageManager                                    │
│         Buff 管理器（列表管理、覆盖规则、查询、移除）                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 一、Buff 基类：IRDAppendage（line 412114）

### 1.1 类定义

```csharp
public class IRDAppendage
{
    // 所有 Buff 的基类，提供完整的生命周期管理
}
```

### 1.2 核心字段

#### 身份标识

| 字段 | 类型 | 说明 |
|------|------|------|
| `ClassId` | ENUM_APPENDAGE_CLASS_ID | Buff 类型 ID |
| `id_` | ENUM_APPENDAGE_ID | Buff ID |
| `UniqueId` | Int32 | 唯一标识（用于撤销/覆盖） |
| `typeList_` | List | Buff 类型标签列表 |
| `skillUniqueId_` | Int32 | 来源技能 ID |
| `appendCauseItemIndex_` | Int32 | 来源物品索引 |

#### 状态标记

| 字段 | 类型 | 说明 |
|------|------|------|
| `isValid_` | Boolean | 是否有效 |
| `isBuff_` | Boolean | true=Buff, false=Debuff |
| `isPause_` | Boolean | 是否暂停 |
| `isShowBuffIcon_` | Boolean | 是否显示 Buff 图标 |
| `buffIconIndex_` | Int32 | Buff 图标索引 |
| `IsImmuneDisenchant` | Boolean | 是否免疫驱散 |
| `isAutoReleaseAfterSourceLeave` | Boolean | 来源离开后自动释放 |

#### 时间管理

| 字段 | 类型 | 说明 |
|------|------|------|
| `endTimer_` | Single | 剩余时间（-1 = 永久） |
| `callDelayedEnd_` | Boolean | 是否延迟结束 |
| `enableTimerEvent_` | Boolean | 是否启用定时事件 |
| `timeTimerEventStart_` | Single | 定时事件起始时间 |
| `timeTimerEventInterval_` | Single | 定时事件间隔 |

#### 特效资源

| 字段 | 类型 | 说明 |
|------|------|------|
| `appendageStartEffectAni_` | Clip | 开始特效动画 |
| `appendageEffectAni_` | Clip | 持续特效动画 |
| `appendageEndEffectAni_` | Clip | 结束特效动画 |
| `appendageBackEffectAni_` | Clip | 背面特效动画 |
| `effectAniHeightOffset_` | Int32 | 特效高度偏移 |
| `isFlipEffectAnimation_` | Boolean | 是否翻转特效 |
| `IsEffectVisibleTarget` | Boolean | 特效是否对目标可见 |

#### 回调委托

| 字段 | 类型 | 说明 |
|------|------|------|
| `appendageProc` | Action<IRDAppendage, Single> | 每帧回调 |
| `appendageOnStart` | Action<IRDAppendage> | 开始回调 |
| `appendageOnEnd` | Action<IRDAppendage, Boolean> | 结束回调 |
| `appendageDrawAppend` | Action<IRDAppendage, Boolean, Single> | 绘制回调 |

#### 验证与追踪

| 字段 | 类型 | 说明 |
|------|------|------|
| `ParentVerificationId` | UInt64 | 父对象验证 ID |
| `SourceVerificationId` | UInt64 | 来源对象验证 ID |
| `appendageInfo_` | AppendageInfo | Buff 配置信息 |

### 1.3 生命周期方法

```csharp
public class IRDAppendage
{
    // === 创建阶段 ===
    protected virtual void create() { }           // 初始化
    public virtual Boolean PreLoad() { }          // 预加载资源
    protected virtual void OnStart() { }          // 开始
    
    // === 运行阶段 ===
    public virtual void Proc(Single deltaTime)    // 每帧更新
    public virtual void PrepareDraw(Single dt)    // 绘制准备
    public virtual void DrawAppend(Boolean isOver, Single dt) // 绘制特效
    
    // === 结束阶段 ===
    public virtual void OnEnd(Boolean onReleaseParent) // 结束
    protected virtual void clear() { }            // 清理
    
    // === 属性应用 ===
    protected virtual Boolean procStatus(ActiveStaticInfo info) { } // 应用属性
    public virtual void procStatusInvalidAppendage() { } // 撤销属性
    
    // === 战斗事件钩子 ===
    public virtual void procAttack(AttackInfo attackInfo) { }
    public virtual void onAttackParent(IRDCollisionObject attacker, IRDCollisionObject damager, Boolean isStuck) { }
    public virtual void onDamageParent(IRDCollisionObject attacker, Boolean isStuck) { }
    public virtual void onApplyHpDamage(ref Int64 hpDamage, IRDCollisionObject attacker) { }
    public virtual void onSetHp(ref Int64 hp, IRDCollisionObject attacker) { }
    public virtual void onDieParent() { }
    public virtual void onSetState(ENUM_ACTIVE_OBJECT_STATE state, List datas) { }
    public virtual ENUM_IMMUNE_TYPE getImmuneType(ref Int32 damageRate, IRDCollisionObject attacker) { }
    public virtual void isEnemy(ref Boolean isEnemy, IRDCollisionObject target) { }
}
```

### 1.4 核心方法详解

#### Append 方法（添加到角色）

```csharp
// line 412315
public virtual Boolean Append(
    IRDActiveObject parent,           // 目标角色
    IRDCollisionObject source,        // 来源对象
    Boolean isBuff,                   // 是 Buff 还是 Debuff
    ENUM_APPENDAGE_ID id,             // Buff ID
    Int32 customOverlapMax,           // 自定义最大叠加数
    IRDAppendage master)              // 主 Buff（用于光环）
{
    // 1. 验证是否可添加
    if (!CheckScoreCondition()) return false;
    
    // 2. 处理覆盖规则
    ValidationOverlapNormal(parent, id, customOverlapMax);
    
    // 3. 设置基本信息
    this.parent = parent;
    this.source = source;
    this.isBuff_ = isBuff;
    this.id_ = id;
    
    // 4. 调用 OnStart
    OnStart();
    
    // 5. 注册到角色的 Buff 列表
    parent.GetAppendageManager().addAppendage(this, master, customOverlapMax);
    
    return true;
}
```

#### 时间管理方法

```csharp
// 获取剩余时间
public Single getTimer() { return endTimer_; }

// 时间信号（检查是否到期）
public Single Signal(Single term)
{
    if (endTimer_ < 0) return endTimer_;  // 永久 Buff
    endTimer_ -= term;
    return endTimer_;
}

// 设置有效时间
public void setValidTime(Single time)
{
    endTimer_ = time;
    if (time > 0) callDelayedEnd_ = true;
}

// 增加时间
public void addValidTime(Single time)
{
    if (endTimer_ > 0)
        endTimer_ += time;
}

// 设置自动销毁
public void setAutoDestroy(Boolean bSet) { /* ... */ }
```

#### 定时事件系统

```csharp
// 启用定时事件
public void enableTimerEvent(Boolean enable)
{
    enableTimerEvent_ = enable;
    if (enable)
        timeTimerEventStart_ = endTimer_;
}

// 设置定时事件间隔
public void setTimerEvent(Int32 damageTerm)
{
    timeTimerEventInterval_ = damageTerm;
    enableTimerEvent(true);
}

// 定时事件回调（子类重写）
public virtual void onTimerEvent() { }
```

---

## 二、Buff 工厂：AppendageFactory（line 356665）

### 2.1 类定义

```csharp
public sealed class AppendageFactory
{
    private static Int32 uniqueIdGenerator;              // ID 生成器
    private static readonly EventPair<OnCreateAppendageDelegate> OnCreateAppendageFunc;
    
    // 类型映射：根据 ENUM_APPENDAGE_CLASS_ID 创建对应的 IRDAppendage 子类
}
```

### 2.2 核心方法

#### 创建 Buff

```csharp
// 通过类型 ID 创建（line 356692）
public static IRDAppendage CreateAppendage(ENUM_APPENDAGE_CLASS_ID appendageId)
{
    // 1. 生成唯一 ID
    Int32 uniqueId = GenerateUniqueId();
    
    // 2. 获取对应的 Buff 类
    IRDAppendage appendage = GetAppendageClass(appendageId);
    
    // 3. 设置基本属性
    appendage.ClassId = appendageId;
    appendage.UniqueId = uniqueId;
    
    return appendage;
}

// 通过脚本创建（line 356694）
public static IRDAppendage CreateAppendageByScript(AppendageScript pScript)
{
    // 1. 根据 type_ 字段确定 Buff 类型
    ENUM_APPENDAGE_CLASS_ID classId = GetClassIdFromType(pScript.type_);
    
    // 2. 创建 Buff 实例
    IRDAppendage appendage = CreateAppendage(classId);
    
    // 3. 应用脚本配置
    appendage.setValidTime(pScript.duration_);
    appendage.isBuff_ = pScript.isBuff_;
    // ... 应用其他配置
    
    // 4. 应用特效
    ApplyEffectAnimation(pScript, appendage);
    
    return appendage;
}
```

#### 类型映射

```csharp
// 根据 classId 获取对应的 Buff 类（line 356680）
private static IRDAppendage GetAppendageClass(ENUM_APPENDAGE_CLASS_ID appendageClassId)
{
    switch (appendageClassId)
    {
        case ENUM_APPENDAGE_CLASS_ID.DUMMY:
            return createDummy();
            
        case ENUM_APPENDAGE_CLASS_ID.CHANGE_STATUS:
            return new CNChangeStatus();
            
        case ENUM_APPENDAGE_CLASS_ID.CHANGE_WEAPON_COLOR:
            return createChangeWeaponColor();
            
        case ENUM_APPENDAGE_CLASS_ID.AERIAL_GATLINGED:
            return new CNAerialGatlinged();
            
        // ... 更多类型映射
            
        default:
            return new IRDAppendage();
    }
}
```

### 2.3 Buff 类型枚举

#### ENUM_APPENDAGE_CLASS_ID（line 344067）

部分重要类型：

| 类型 | 说明 |
|------|------|
| DUMMY | 空 Buff（用于占位） |
| CHANGE_STATUS | 属性变更 Buff |
| CHANGE_WEAPON_COLOR | 武器颜色变更 |
| AERIAL_GATLINGED | 空中加特林状态 |
| AURA_MASTER | 光环主控 |
| CHANGE_HP | HP 变更（治疗/持续伤害） |

#### ENUM_APPENDAGE_TYPE（line 344240）

Buff 行为类型：

| 类型 | 说明 |
|------|------|
| AP_TYPE_NONE | 无特殊类型 |
| AP_TYPE_GAUGE | 量表型（能量条） |
| AP_TYPE_ACCMULATE_DAMAGE | 累积伤害型 |
| AP_TYPE_AFTER_TREATMENT | 后处理型 |
| AP_TYPE_ELEMENT_COUNT | 元素计数型 |
| AP_TYPE_MASTERY_CONTROL | 精通控制型 |
| AP_TYPE_ENCAHNT_WEAPON | 附魔武器型 |
| AP_TYPE_EQUIPMENT_ANIMATION_CHANGE | 装备动画变更型 |
| AP_TYPE_UNBEATABLE | 无敌型 |
| AP_TYPE_BETRAY | 背叛型（混乱） |
| AP_TYPE_HOLD_AND_DELAYED_DIE | 抓取延迟死亡型 |
| AP_TYPE_CHANGE_STATUS | 属性变更型 |
| AP_TYPE_VARIABLE_CHANGE_STATUS | 变动属性变更型 |

---

## 三、Buff 管理器：CNRDAppendageManager（line 403357）

### 3.1 类定义

```csharp
public class CNRDAppendageManager
{
    // Buff 列表
    private List<IRDAppendage> vectorAppendages_;
    
    // 主 Buff 映射（用于光环等）
    private Dictionary<ENUM_APPENDAGE_ID, IRDAppendage> appendageMasters_;
    
    // 无敌 Buff 计数
    private Int32 invincibleAppendageCnt;
    
    // 最大叠加数映射
    private static Dictionary<ENUM_APPENDAGE_ID, Int32> overlapMaxCountMap_;
}
```

### 3.2 核心方法

#### 添加 Buff

```csharp
// line 403386
public Boolean addAppendage(IRDAppendage appendage, IRDAppendage master, Int32 customOverlapCount)
{
    // 1. 检查是否可添加（叠加数限制）
    if (!isAppendable(appendage.id_, customOverlapCount))
        return false;
    
    // 2. 检查是否已存在
    if (isExistAppendage(appendage))
    {
        // 刷新时间（续Buff）
        appendage.addValidTime(appendage.endTimer_);
        return true;
    }
    
    // 3. 添加到列表
    vectorAppendages_.Add(appendage);
    
    // 4. 如果是主 Buff，注册到映射
    if (master == null)
        appendageMasters_[appendage.id_] = appendage;
    
    // 5. 如果是无敌 Buff，增加计数
    if (appendage.IsInvincible)
        invincibleAppendageCnt++;
    
    return true;
}
```

#### 移除 Buff

```csharp
// 通过索引移除（line 403412）
public void RemoveAppendage(Int32 index)
{
    if (index < 0 || index >= vectorAppendages_.Count) return;
    
    IRDAppendage appendage = vectorAppendages_[index];
    
    // 调用结束回调
    appendage.OnEnd(false);
    
    // 如果是无敌 Buff，减少计数
    if (appendage.IsInvincible)
        invincibleAppendageCnt--;
    
    // 从列表移除
    vectorAppendages_.RemoveAt(index);
    
    // 从主 Buff 映射移除
    appendageMasters_.Remove(appendage.id_);
}

// 通过 ID 移除（line 403418）
public void RemoveAppendageFromID(ENUM_APPENDAGE_ID id, IRDCollisionObject source)
{
    for (int i = vectorAppendages_.Count - 1; i >= 0; i--)
    {
        if (vectorAppendages_[i].id_ == id)
        {
            vectorAppendages_[i].OnEnd(false);
            vectorAppendages_.RemoveAt(i);
        }
    }
}

// 移除所有 Buff（line 403408）
public void RemoveAllAppedages(Boolean isRelease)
{
    foreach (var appendage in vectorAppendages_)
    {
        appendage.OnEnd(isRelease);
    }
    vectorAppendages_.Clear();
    appendageMasters_.Clear();
    invincibleAppendageCnt = 0;
}
```

#### 查询 Buff

```csharp
// 检查是否存在（line 403388）
public Boolean isExistAppendage(IRDAppendage appendage)
{
    return vectorAppendages_.Contains(appendage);
}

public Boolean isExistAppendage(ENUM_APPENDAGE_ID id)
{
    return appendageMasters_.ContainsKey(id);
}

// 获取 Buff 数量（line 403394）
public Int32 getAppendageCount(ENUM_APPENDAGE_ID id)
{
    int count = 0;
    foreach (var ap in vectorAppendages_)
    {
        if (ap.id_ == id) count++;
    }
    return count;
}

// 检查是否可添加（叠加数检查）（line 403392）
public Boolean isAppendable(ENUM_APPENDAGE_ID id, Int32 customOverlapCount)
{
    int maxOverlap = customOverlapCount > 0 ? customOverlapCount : getOverlapMaxCount(id);
    return getAppendageCount(id) < maxOverlap;
}

// 获取 Buff（line 403422）
public AppendageQuery GetAppendageFromID(ENUM_APPENDAGE_ID id)
{
    var query = AppendageQuery.Make();
    foreach (var ap in vectorAppendages_)
    {
        if (ap.id_ == id)
            query.Add(ap);
    }
    return query;
}

public IRDAppendage GetAppendageFromIDFirst(ENUM_APPENDAGE_ID id)
{
    return appendageMasters_.GetValueOrDefault(id);
}

// 通过技能索引获取（line 403422）
public AppendageQuery GetAppendageFromSkillIdx(Int32 skillIdx) { /* ... */ }

// 通过类型获取（line 403428）
public AppendageQuery getAppendageFromType(ENUM_APPENDAGE_TYPE type) { /* ... */ }
```

#### 更新处理

```csharp
// 预处理（line 403382）
public void preProc(Single procDeltaTime)
{
    foreach (var ap in vectorAppendages_)
    {
        if (ap.isValid_ && !ap.isPause_)
            ap.PrepareDraw(procDeltaTime);
    }
}

// 主处理（line 403384）
public void mainProc(Single procDeltaTime)
{
    for (int i = vectorAppendages_.Count - 1; i >= 0; i--)
    {
        var ap = vectorAppendages_[i];
        
        if (!ap.isValid_)
        {
            RemoveAppendage(i);
            continue;
        }
        
        if (!ap.isPause_)
        {
            ap.Proc(procDeltaTime);
            
            // 检查时间到期
            if (ap.Signal(procDeltaTime) <= 0 && ap.endTimer_ > 0)
            {
                RemoveAppendage(i);
            }
        }
    }
}

// 后处理（line 403380）
public void postProc(Single procDeltaTime)
{
    foreach (var ap in vectorAppendages_)
    {
        if (ap.isValid_ && !ap.isPause_)
            ap.DrawAppend(true, procDeltaTime);
    }
}
```

#### 属性应用

```csharp
// 应用所有 Buff 的属性（line 403438）
public void procStatus(ActiveStaticInfo activeStaticInfo, Boolean applyAfterDepth3)
{
    foreach (var ap in vectorAppendages_)
    {
        if (ap.isValid_ && !ap.isPause_)
        {
            if (applyAfterDepth3 == ap.isAfterActiveStaticDepth3())
            {
                ap.procStatus(activeStaticInfo);
            }
        }
    }
}
```

#### 战斗事件转发

```csharp
// 攻击时（line 403442）
public void onAttackParent(IRDCollisionObject realAttacker, IRDCollisionObject damager, Boolean isStuck)
{
    foreach (var ap in vectorAppendages_)
    {
        if (ap.isValid_)
            ap.onAttackParent(realAttacker, damager, isStuck);
    }
}

// 受伤时（line 403446）
public void onDamageParent(IRDCollisionObject attacker, Boolean isStuck)
{
    foreach (var ap in vectorAppendages_)
    {
        if (ap.isValid_)
            ap.onDamageParent(attacker, isStuck);
    }
}

// 死亡时（line 403456）
public void onDieParent()
{
    foreach (var ap in vectorAppendages_)
    {
        if (ap.isValid_)
            ap.onDieParent();
    }
}

// 获取免疫类型（line 403398）
public ENUM_IMMUNE_TYPE getImmuneType(ref Int32 damageRate, IRDCollisionObject attacker)
{
    foreach (var ap in vectorAppendages_)
    {
        if (ap.isValid_)
        {
            var immune = ap.getImmuneType(ref damageRate, attacker);
            if (immune != ENUM_IMMUNE_TYPE.NONE)
                return immune;
        }
    }
    return ENUM_IMMUNE_TYPE.NONE;
}

// 检查是否有无敌 Buff（line 403396）
public Boolean HasInvincibleAppendage()
{
    return invincibleAppendageCnt > 0;
}
```

---

## 四、Buff 配置脚本：AppendageScript（line 344277）

### 4.1 类定义

```csharp
public class AppendageScript : IBinaryReadWriteScript
{
    public String baseFolder;                  // 基础路径
    public String scriptFileName;              // 脚本文件名
    public Int32 appendageIndex_;              // Buff 索引
    public String type_;                       // 类型字符串
    public Int32 duration_;                    // 持续时间
    public Boolean isBuff_;                    // 是 Buff 还是 Debuff
    public Int32 maxOverlab_;                  // 最大叠加数
    public String effectAnimationFileName_;    // 特效动画文件名
    public String iconImageFileName_;          // 图标文件名
    public Int32 iconImageIndex_;              // 图标索引
    public Int32 remainDestroyHitCount_;       // 受击次数销毁
    public Int32 remainDestroyAttackCount_;    // 攻击次数销毁
    public Int32 isParty_;                     // 是否队伍共享
    public Int32 createLimitCount_;            // 创建数量限制
    public Boolean isShowBuffIcon_;            // 是否显示 Buff 图标
    public Boolean isRenewalAppendage_;        // 是否续 Buff
    public Int32 exceptionReason_;             // 例外原因
    public Boolean isFlipEffectAnimation_;     // 是否翻转特效
    public Boolean isEffectVisibleTarget_;     // 特效对目标可见
    public List<Int32> vecIntDatas_;           // 整数参数列表
    public List<Single> vecFloatDatas_;        // 浮点参数列表
    public List<String> vecStringDatas_;       // 字符串参数列表
    public Dictionary<String, AppendageEffectStateData> effectAnimationStateData; // 特效动画状态数据
}
```

### 4.2 脚本解析器

```csharp
public class Parser : CachingParser<AppendageScript>
{
    // 解析函数映射
    private static Dictionary<String, Action<String>> parsingFunctionMap;
    
    private void InitFunctionMap()
    {
        parsingFunctionMap["[TYPE]"] = Func_Type;
        parsingFunctionMap["[DURATION]"] = Func_Duration;
        parsingFunctionMap["[BUFF]"] = Func_Buff;
        parsingFunctionMap["[MAX_OVERLAP]"] = Func_MaxOverlap;
        parsingFunctionMap["[EFFECT_ANIMATION]"] = Func_EffectAnimation;
        parsingFunctionMap["[ICON_IMAGE]"] = Func_IconImage;
        parsingFunctionMap["[DESTROY_WHEN_ATTACK_COUNT]"] = Func_DestroyWhenAttackCount;
        parsingFunctionMap["[DESTROY_WHEN_HIT_COUNT]"] = Func_DestroyWhenHitCount;
        parsingFunctionMap["[INT_DATA]"] = Func_IntData;
        parsingFunctionMap["[FLOAT_DATA]"] = Func_FloatData;
        parsingFunctionMap["[STRING_DATA]"] = Func_StringData;
        parsingFunctionMap["[SHOW_BUFF_ICON]"] = Func_ShowBuffIcon;
        parsingFunctionMap["[RENEWAL_APPENDAGE]"] = Func_RenewalAppendage;
        parsingFunctionMap["[APPLY_FLIP_EFFECT_ANIMATION]"] = Func_EffectAnimationStateData;
        // ...
    }
}
```

### 4.3 脚本示例

```
[TYPE]
CHANGE_STATUS

[DURATION]
10000

[BUFF]
1

[MAX_OVERLAP]
1

[EFFECT_ANIMATION]
effect/buff/power_up.ani

[ICON_IMAGE]
icon/buff/power_up.png
0

[INT_DATA]
100    // 物理攻击加值
50      // 魔法攻击加值

[FLOAT_DATA]
0.2     // 攻击速度倍率
0.1     // 移动速度倍率

[SHOW_BUFF_ICON]
1
```

---

## 五、特殊 Buff 子类

### 5.1 属性变更 Buff：CNChangeStatus（line 358654）

详见 [09-属性系统.md](09-属性系统.md) 第三章。

### 5.2 HP 变更 Buff：ChangeHp（line 356724）

用于治疗或持续伤害：

```csharp
public sealed class ChangeHp : IRDAppendage
{
    private Single changeHp_;              // 变更量（正=治疗，负=伤害）
    private Single changeTime_;            // 总时间
    private Single changedHp_;             // 已变更量
    private ENUM_EFFECT_TYPE effectType_;  // 效果类型
    private EventTimer displayEventTimer_; // 显示计时器
    
    // 效果类型
    // EFFECT_TYPE_HEAL        - 立即治疗
    // EFFECT_TYPE_FAST_HEAL   - 快速治疗
    // EFFECT_TYPE_SLOW_HEAL   - 缓慢治疗
    // EFFECT_TYPE_DOT_AREA    - 区域持续伤害
    
    public override void Proc(Single procDeltaTime)
    {
        // 计算本次变更量
        Single changeAmount = (changeHp_ / changeTime_) * procDeltaTime;
        
        // 应用到角色
        if (changeHp_ > 0)
            parent.AddHP((Int64)changeAmount);
        else
            parent.SubtractHP((Int64)(-changeAmount));
        
        changedHp_ += changeAmount;
    }
    
    public void setParameter(Int32 changeHp, Single changeTime, 
                             Boolean isProcessPVPConvert, ENUM_EFFECT_TYPE effectType)
    {
        this.changeHp_ = changeHp;
        this.changeTime_ = changeTime;
        this.effectType_ = effectType;
    }
}
```

### 5.3 空中加特林状态：CNAerialGatlinged（line 356809）

```csharp
public sealed class CNAerialGatlinged : IRDAppendage
{
    private Boolean IsCloseAerialGatling;   // 是否近距离
    private IRDActiveObject Attacker;       // 攻击者
    private Vector3 destPos_;               // 目标位置
    private Vector3 closePos_;              // 近距离位置
    private Single hitTimer_;               // 命中计时器
    
    // 特殊行为：控制角色在空中的位置和状态
    public override void Proc(Single procDeltaTime)
    {
        // 持续跟踪攻击者位置
        if (Attacker != null)
        {
            // 更新目标位置
            destPos_ = Attacker.transform.position;
        }
    }
    
    public override void onDamageParent(IRDCollisionObject attacker, Boolean isStuck)
    {
        // 受伤时可能中断
    }
}
```

### 5.4 光环 Buff：CNAuraMaster（line 403634）

```csharp
public class CNAuraMaster : IRDAppendage, IAuraEventHandler
{
    private CNRDAuraModule auraModule_;              // 光环模块
    private Boolean atferAuraActiveStaticDepth3_;   // 属性应用顺序
    private AuraMasterData auraMasterData;          // 光环数据
    
    public CNRDAuraModule AuraModule => auraModule_;
    
    public override void OnStart()
    {
        auraModule_.onStart(parent);
    }
    
    public override void Proc(Single procDeltaTime)
    {
        auraModule_.proc();
    }
    
    public override void OnEnd(Boolean onReleaseParent)
    {
        auraModule_.onEnd();
    }
    
    // IAuraEventHandler 接口实现
    public void enableAuraEffect(IRDActiveObject target)
    {
        // 给目标添加光环效果
    }
    
    public void disableAuraEffect(IRDActiveObject target)
    {
        // 移除目标的光环效果
    }
    
    public Boolean isAuraTarget(IRDActiveObject obj)
    {
        // 判断是否是光环目标
        return obj.TeamId == parent.TeamId;
    }
}
```

### 5.5 光环模块：CNRDAuraModule（line 403520）

```csharp
public class CNRDAuraModule
{
    protected IRDCollisionObject parent_;            // 父对象
    protected IAuraEventHandler eventHandler_;       // 事件处理器
    protected Int32 auraType_;                       // 光环类型
    protected Single auraRange_;                     // 光环范围
    protected Boolean isUniqueApply_;                // 是否唯一应用
    protected EventTimer updateEventTimer_;          // 更新计时器
    protected List<IRDActiveObject> underAuraObject_;// 光环内对象列表
    
    // 光环类型常量
    // AURATYPE_INCLUDEMYSELF - 包含自己
    // AURATYPE_EXCEPTMYSELF - 排除自己
    // AURATYPE_RANGE - 范围型
    // AURATYPE_ALONE - 独立型
    
    public void proc()
    {
        // 更新光环内的对象
        updateAuraEffectiveObject();
        
        // 对光环内的对象应用效果
        foreach (var obj in underAuraObject_)
        {
            eventHandler_.refreshAuraEffect(obj);
        }
    }
    
    protected void updateAuraEffectiveObject()
    {
        // 查找范围内的对象
        var nearbyObjects = FindObjectsInRange(parent_.position, auraRange_);
        
        // 新进入光环的对象
        foreach (var obj in nearbyObjects)
        {
            if (!underAuraObject_.Contains(obj) && eventHandler_.isAuraTarget(obj))
            {
                underAuraObject_.Add(obj);
                eventHandler_.enableAuraEffect(obj);
            }
        }
        
        // 离开光环的对象
        for (int i = underAuraObject_.Count - 1; i >= 0; i--)
        {
            var obj = underAuraObject_[i];
            if (!nearbyObjects.Contains(obj) || !eventHandler_.isAuraTarget(obj))
            {
                eventHandler_.disableAuraEffect(obj);
                underAuraObject_.RemoveAt(i);
            }
        }
    }
}
```

---

## 六、Buff 查询结构：AppendageQuery（line 403467）

### 6.1 类定义

```csharp
public sealed class AppendageQuery : ISelfObject<AppendageQuery>
{
    private List<IRDAppendage> container;        // 结果容器
    private static SelfObjectPool<AppendageQuery> _pool; // 对象池
    
    public IRDAppendage this[Int32 index] => container[index];
    public Int32 Count => container.Count;
    
    // 对象池方法
    public static AppendageQuery Make() { /* ... */ }
    public void Free() { /* ... */ }
    
    // 操作方法
    public void Add(IRDAppendage item) { container.Add(item); }
    public List<IRDAppendage> Result() { return container; }
    public Enumerator GetEnumerator() { /* ... */ }
}
```

### 6.2 使用示例

```csharp
// 查询所有 ID 为某值的 Buff
AppendageQuery query = manager.GetAppendageFromID(ENUM_APPENDAGE_ID.POWER_UP);
foreach (var ap in query)
{
    // 处理每个 Buff
    ap.setValidTime(10f);  // 刷新时间
}
query.Free();  // 释放回对象池
```

---

## 七、完整 Buff 流程

### 7.1 Buff 创建与添加

```
技能释放/物品使用/装备穿戴
    ↓
加载 AppendageScript 配置
    ↓
AppendageFactory.CreateAppendageByScript(script)
    ↓
创建 IRDAppendage 子类实例
    ↓
设置参数（duration, isBuff, 特效等）
    ↓
IRDAppendage.Append(parent, source, isBuff, id, overlapMax, master)
    ↓
CNRDAppendageManager.addAppendage(appendage, master, overlapCount)
    ↓
检查叠加限制
    ↓
添加到 vectorAppendages_ 列表
    ↓
OnStart() 回调
    ↓
procStatus() 应用属性变更
```

### 7.2 Buff 更新循环

```
每帧更新：
    preProc(deltaTime)
        └── PrepareDraw() 绘制准备
    
    mainProc(deltaTime)
        └── Proc() 每帧逻辑
        └── Signal() 时间检查
        └── 到期则 RemoveAppendage()
    
    postProc(deltaTime)
        └── DrawAppend() 绘制特效
    
    procStatus(activeStaticInfo)
        └── 应用属性到 ActiveStaticInfo
```

### 7.3 Buff 移除

```
时间到期 / 手动移除 / 来源离开
    ↓
CNRDAppendageManager.RemoveAppendage(index)
    ↓
IRDAppendage.OnEnd(onReleaseParent)
    ↓
procStatusInvalidAppendage() 撤销属性变更
    ↓
播放结束特效
    ↓
clear() 清理资源
    ↓
从 vectorAppendages_ 移除
    ↓
从 appendageMasters_ 移除
```

---

## 八、Buff 覆盖规则

### 8.1 最大叠加数

```csharp
// 静态配置
private static Dictionary<ENUM_APPENDAGE_ID, Int32> overlapMaxCountMap_;

// 默认最大叠加数
public Int32 getOverlapMaxCount(ENUM_APPENDAGE_ID id)
{
    if (overlapMaxCountMap_.TryGetValue(id, out int max))
        return max;
    return 1;  // 默认不可叠加
}

// 自定义叠加数
public Boolean isAppendable(ENUM_APPENDAGE_ID id, Int32 customOverlapCount)
{
    int max = customOverlapCount > 0 ? customOverlapCount : getOverlapMaxCount(id);
    return getAppendageCount(id) < max;
}
```

### 8.2 续 Buff 机制

```csharp
// AppendageScript 中
public Boolean isRenewalAppendage_;  // 是否续 Buff

// 添加时检查
if (isExistAppendage(appendage) && pScript.isRenewalAppendage_)
{
    // 刷新时间
    existingAppendage.addValidTime(pScript.duration_);
    return true;  // 不创建新的
}
```

### 8.3 覆盖优先级

```csharp
// IRDAppendage 中的评分系统
public virtual Int32 GetScore(ENUM_CHARACTERJOB job) { return 0; }

// 评分条件检查
protected Boolean CheckScoreCondition()
{
    // 根据职业和 Buff 类型计算优先级
    // 高优先级可以覆盖低优先级
}
```

---

## 九、Demo 简化实现

### 9.1 简化 Buff 基类

```csharp
public enum BuffType
{
    None,
    StatChange,     // 属性变更
    Heal,           // 治疗
    Damage,         // 持续伤害
    Invincible,     // 无敌
    Stun,           // 眩晕
    Slow,           // 减速
}

public class SimpleBuff
{
    public int BuffId;
    public string Name;
    public BuffType Type;
    public float Duration = -1f;      // -1 = 永久
    public float RemainingTime;
    public bool IsBuff = true;        // true=Buff, false=Debuff
    public bool IsValid = true;
    public int MaxStack = 1;
    public int CurrentStack = 1;
    
    // 来源
    public int SourceSkillId;
    public CharacterBase Source;
    public CharacterBase Target;
    
    // 参数
    public List<float> FloatParams = new List<float>();
    public List<int> IntParams = new List<int>();
    
    // 特效
    public GameObject StartEffect;
    public GameObject LoopEffect;
    public GameObject EndEffect;
    
    // 回调
    public Action<SimpleBuff> OnStart;
    public Action<SimpleBuff, float> OnUpdate;
    public Action<SimpleBuff> OnEnd;
    
    // 生命周期
    public virtual void Start()
    {
        RemainingTime = Duration;
        OnStart?.Invoke(this);
        if (StartEffect != null)
            SpawnEffect(StartEffect);
    }
    
    public virtual void Update(float deltaTime)
    {
        if (!IsValid) return;
        
        OnUpdate?.Invoke(this, deltaTime);
        
        if (Duration > 0)
        {
            RemainingTime -= deltaTime;
            if (RemainingTime <= 0)
            {
                IsValid = false;
            }
        }
    }
    
    public virtual void End()
    {
        OnEnd?.Invoke(this);
        if (EndEffect != null)
            SpawnEffect(EndEffect);
    }
    
    private void SpawnEffect(GameObject prefab)
    {
        if (Target != null && prefab != null)
        {
            var effect = GameObject.Instantiate(prefab, Target.transform.position, Quaternion.identity);
            // ... 特效管理
        }
    }
}
```

### 9.2 简化 Buff 管理器

```csharp
public class SimpleBuffManager
{
    private List<SimpleBuff> _buffs = new List<SimpleBuff>();
    private CharacterBase _owner;
    
    public SimpleBuffManager(CharacterBase owner)
    {
        _owner = owner;
    }
    
    // 添加 Buff
    public SimpleBuff AddBuff(SimpleBuff buffTemplate)
    {
        // 检查是否已存在
        var existing = _buffs.Find(b => b.BuffId == buffTemplate.BuffId);
        
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
                // 刷新时间
                existing.RemainingTime = existing.Duration;
                return existing;
            }
        }
        
        // 创建新 Buff
        var newBuff = CloneBuff(buffTemplate);
        newBuff.Target = _owner;
        newBuff.Start();
        _buffs.Add(newBuff);
        
        return newBuff;
    }
    
    // 移除 Buff
    public void RemoveBuff(int buffId)
    {
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            if (_buffs[i].BuffId == buffId)
            {
                _buffs[i].End();
                _buffs.RemoveAt(i);
            }
        }
    }
    
    // 移除所有 Debuff
    public void RemoveAllDebuffs()
    {
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            if (!_buffs[i].IsBuff)
            {
                _buffs[i].End();
                _buffs.RemoveAt(i);
            }
        }
    }
    
    // 清空所有 Buff
    public void ClearAllBuffs()
    {
        foreach (var buff in _buffs)
        {
            buff.End();
        }
        _buffs.Clear();
    }
    
    // 每帧更新
    public void Update(float deltaTime)
    {
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            var buff = _buffs[i];
            buff.Update(deltaTime);
            
            if (!buff.IsValid)
            {
                buff.End();
                _buffs.RemoveAt(i);
            }
        }
    }
    
    // 查询
    public bool HasBuff(int buffId) => _buffs.Exists(b => b.BuffId == buffId);
    public SimpleBuff GetBuff(int buffId) => _buffs.Find(b => b.BuffId == buffId);
    public List<SimpleBuff> GetAllBuffs() => _buffs;
    public List<SimpleBuff> GetAllDebuffs() => _buffs.FindAll(b => !b.IsBuff);
    
    // 检查是否有无敌
    public bool IsInvincible() => _buffs.Exists(b => b.Type == BuffType.Invincible && b.IsValid);
    
    private SimpleBuff CloneBuff(SimpleBuff template)
    {
        // 克隆 Buff
        return new SimpleBuff
        {
            BuffId = template.BuffId,
            Name = template.Name,
            Type = template.Type,
            Duration = template.Duration,
            IsBuff = template.IsBuff,
            MaxStack = template.MaxStack,
            FloatParams = new List<float>(template.FloatParams),
            IntParams = new List<int>(template.IntParams),
            StartEffect = template.StartEffect,
            LoopEffect = template.LoopEffect,
            EndEffect = template.EndEffect,
        };
    }
}
```

### 9.3 具体 Buff 实现

```csharp
// 属性变更 Buff
public class StatChangeBuff : SimpleBuff
{
    public int AttackAdd;
    public float AttackRate;
    public int DefenseAdd;
    public float DefenseRate;
    public float MoveSpeedRate;
    
    public override void Start()
    {
        base.Start();
        // 应用属性
        var stats = Target.Stats;
        stats.AttackAdd += AttackAdd * CurrentStack;
        stats.AttackRate *= (1 + AttackRate * CurrentStack);
        stats.DefenseAdd += DefenseAdd * CurrentStack;
        stats.DefenseRate *= (1 + DefenseRate * CurrentStack);
        stats.MoveSpeedRate *= (1 + MoveSpeedRate * CurrentStack);
    }
    
    public override void End()
    {
        // 撤销属性
        var stats = Target.Stats;
        stats.AttackAdd -= AttackAdd * CurrentStack;
        stats.AttackRate /= (1 + AttackRate * CurrentStack);
        stats.DefenseAdd -= DefenseAdd * CurrentStack;
        stats.DefenseRate /= (1 + DefenseRate * CurrentStack);
        stats.MoveSpeedRate /= (1 + MoveSpeedRate * CurrentStack);
        
        base.End();
    }
}

// 持续治疗 Buff
public class HealOverTimeBuff : SimpleBuff
{
    public float HealPerSecond;
    private float _accumulatedTime;
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
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
public class StunBuff : SimpleBuff
{
    public override void Start()
    {
        base.Start();
        Target.IsStunned = true;
        Target.Animation.Pause();
    }
    
    public override void End()
    {
        Target.IsStunned = false;
        Target.Animation.Resume();
        base.End();
    }
}
```

---

## 十、设计要点总结

### 10.1 核心设计模式

| 模式 | 应用 |
|------|------|
| 工厂模式 | AppendageFactory 创建不同类型的 Buff |
| 对象池 | AppendageQuery 和 IRDAppendage 支持池化复用 |
| 观察者模式 | 回调委托（OnStart/OnEnd/OnDamageParent 等） |
| 组合模式 | CNAuraMaster 组合 CNRDAuraModule |
| 策略模式 | 不同 Buff 子类实现不同的 procStatus 策略 |

### 10.2 关键设计决策

| 决策点 | 设计选择 | 原因 |
|--------|----------|------|
| Buff 存储 | List + Dictionary 双结构 | List 便于遍历，Dictionary 便于查找 |
| 时间管理 | endTimer_ 倒计时 | 便于续 Buff 和时间显示 |
| 叠加控制 | maxOverlapCount + uniqueId | 支持多层叠加和精确移除 |
| 属性应用 | procStatus 抽象方法 | 子类灵活实现不同的属性修改逻辑 |
| 事件转发 | 管理器统一转发到各 Buff | 解耦 Buff 和角色逻辑 |

### 10.3 性能优化

- **对象池**：AppendageQuery 使用对象池减少 GC
- **分层更新**：preProc → mainProc → postProc 分阶段处理
- **延迟移除**：isValid_ 标记，统一在循环结束后移除
- **计数器**：invincibleAppendageCnt 避免遍历检查无敌状态

---

## 附录：源码位置索引

| 类/枚举 | 位置 |
|---------|------|
| IRDAppendage | line 412114 |
| AppendageFactory | line 356665 |
| CNRDAppendageManager | line 403357 |
| AppendageQuery | line 403467 |
| AppendageScript | line 344277 |
| CNChangeStatus | line 358654 |
| ChangeHp | line 356724 |
| CNAerialGatlinged | line 356809 |
| CNAuraMaster | line 403634 |
| CNRDAuraModule | line 403520 |
| ENUM_APPENDAGE_TYPE | line 344240 |
| ENUM_APPENDAGE_CLASS_ID | line 344067 |
| ENUM_APPENDAGE_ID | line 343857 |