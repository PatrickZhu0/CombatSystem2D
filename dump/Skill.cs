// Dll : Assembly-CSharp.dll
// Namespace: 
public abstract class Skill
{
	// Fields
	private Boolean <IsAdditionalControlable>k__BackingField; 
	private Boolean <IsChargable>k__BackingField; 
	private Boolean <IsComsumeable>k__BackingField; 
	private Boolean <IsIncreasable>k__BackingField; 
	private Boolean <IsSustainable>k__BackingField; 
	private Boolean <IsFinishableAttack>k__BackingField; 
	private Boolean <IsDistanceAdjustable>k__BackingField; 
	private Int32 <SwitchableSkillIndex>k__BackingField; 
	private Boolean <CanBeSlideToNeutral>k__BackingField; 
	private SlidableDirectionTypes <SlidableDirectionType>k__BackingField; 
	private Vector3 <SkillDirectionGuideOffset>k__BackingField; 
	private DIRECTION_FLAGS <SlidableDirections>k__BackingField; 
	private List<T> <SlidingSkills>k__BackingField; 
	private Boolean <IsUseButtonJoystick>k__BackingField; 
	private Boolean <ShowJoystickNotiGuide>k__BackingField; 
	private Boolean <UseJumpBurst>k__BackingField; 
	private Boolean <UseJumpCancel>k__BackingField; 
	private Boolean <UseReuseCancel>k__BackingField; 
	private Boolean <UseBackstepSwitchDirection>k__BackingField; 
	protected Boolean isFinishableAttackUsed; 
	private Boolean isAppliedFirstOrderSkillControl; 
	private Int32 <currentSkillControlOrder>k__BackingField; 
	private readonly List<T> currentOrderSkillControlList; 
	private Int32 RemainAdditionalControlCount_; 
	private SkillGaugeState _skillGaugeState; 
	public readonly SkillViewController ViewController; 
	public static Int32 DEFINE_ALL; 
	public const Single float0_1; 
	public const Single float0_01; 
	public const Single float0_001; 
	public const Int32 NO_ATTACK_BONUS_RATE; 
	protected Int32 skillIndex_; 
	public readonly List<T> skillObjects_; 
	private Ref parent; 
	private Skill replaceSkill_; 
	protected ENUM_CHARACTERJOB job_; 
	private Int32 Cached_GrowType; 
	private Int32 Cached_MaxLevel; 
	private Boolean Cached_IsPreventCubeSkill; 
	private Boolean Cached_HasUseItems; 
	private Boolean Cached_HasDualUseItems; 
	private Boolean Cached_UseItemModule; 
	private Int32 Cached_CurrentLevel; 
	private Int32 Cached_SpendMP; 
	private Int32 Cached_DualSpendMP; 
	private Single Cached_SkillMPRate; 
	private Boolean CachedIsInCooltime; 
	private Boolean CachedIsInDualCooltime; 
	protected HashSet<T> skillStates_; 
	protected STSkillScript skillScript_; 
	private readonly Int32[] levels_; 
	protected Int32 subState_; 
	protected Boolean isPlayWeaponSound_; 
	protected List<T> preLoadPrefabs_; 
	protected List<T> preLoadMonsterObjs_; 
	protected List<T> preLoadAnis_; 
	protected List<T> preLoadSounds_; 
	protected List<T> preLoadParticles_; 
	protected List<T> preLoadImgs_; 
	protected List<T> preLoadAppendages_; 
	protected Boolean isInit_; 
	protected Boolean isMapContinuity_; 
	public Boolean IsUsablePassive; 
	private Single currentCoolTime_; 
	private Single currentCoolEndTime_; 
	protected Boolean isUseDuelCoolTime_; 
	private Single currentDuelCoolTime_; 
	private Single currentDuelCoolEndTime_; 
	private readonly List<T> coolTimes_; 
	private readonly Dictionary<TKey, TValua> calcedEquCoolTimes_; 
	private readonly List<T> coolTimeOnStart_; 
	private Boolean isUsingCoolTimeOnStart; 
	private readonly Dictionary<TKey, TValua> calcedEquSpendMps_; 
	private readonly List<T> castTimes_; 
	private readonly Dictionary<TKey, TValua> skillVariableFactor_; 
	private readonly Dictionary<TKey, TValua> skillVariableSubindexFactor_; 
	private static readonly Dictionary<TKey, TValua> PreloadedSkillList; 
	protected GetSkillReasonFail failReason_; 
	public Int32 skillUseCount; 
	public Func<TKey, TValua> cbCheckDuelSkillActivateCondition_; 
	private Boolean isDualSkill_; 
	protected Boolean isDuelSkillActivated_; 
	public Boolean IsSkillUsing; 
	public Boolean IsCancelState; 
	public Boolean IsAIAutoPressSkillKey; 
	private Boolean isSkillAnimationPause; 
	private Boolean IsAlreadyLoggingActiveStaticInfoError; 
	public ENUM_DIRECTION_MOBILE skillDirection_; 
	public Vector2 skillPanelPos_; 
	public Boolean isPress_; 
	public Boolean isDoubleClick; 
	private Boolean isFinished_; 
	private Boolean reservedInput; 
	private Single reservedInputAvailableCheckStartTime; 
	public Boolean ApplyOptimizeUsableCheck; 
	protected Boolean CompleteCachingConstData; 
	protected readonly List<T> customAniIndex_; 
	private IRDCollisionObject targetDbgAtkInfo; 
	protected Boolean isInCustomCastAni_; 
	private Int32 shotAnimation_; 
	private static Action<TKey, TValua> <>f__mg$cache0; 
	private static Action<TKey, TValua> <>f__mg$cache1; 
	private static Action<TKey, TValua> <>f__mg$cache2; 
	private static Action<TKey, TValua> <>f__mg$cache3; 
	private static Action<TKey, TValua> <>f__mg$cache4; 
	private static Action<TKey, TValua> <>f__mg$cache5; 
	private static Action<TKey, TValua> <>f__mg$cache6; 
	private static Action<TKey, TValua> <>f__mg$cache7; 
	private static Predicate<T> <>f__am$cache0; 
	private static Predicate<T> <>f__am$cache1; 
	private static Action<TKey, TValua> <>f__mg$cache8; 
	private static Action<TKey, TValua> <>f__mg$cache9; 
	private static Action<TKey, TValua> <>f__mg$cacheA; 
	private static Action<TKey, TValua> <>f__mg$cacheB; 

	// Properties
	public Boolean IsAdditionalControlable { get; set; }
	public Boolean IsChargable { get; set; }
	public Boolean IsComsumeable { get; set; }
	public Boolean IsIncreasable { get; set; }
	public Boolean IsSustainable { get; set; }
	public Boolean IsFinishableAttack { get; set; }
	public Boolean IsDistanceAdjustable { get; set; }
	public Int32 SwitchableSkillIndex { get; set; }
	public Boolean CanBeSlideToNeutral { get; set; }
	public SlidableDirectionTypes SlidableDirectionType { get; set; }
	public Vector3 SkillDirectionGuideOffset { get; set; }
	public DIRECTION_FLAGS SlidableDirections { get; set; }
	public List<T> SlidingSkills { get; set; }
	public Boolean IsUseButtonJoystick { get; set; }
	public Boolean ShowJoystickNotiGuide { get; set; }
	public Boolean UseJumpBurst { get; set; }
	public Boolean UseJumpCancel { get; set; }
	public Boolean UseReuseCancel { get; set; }
	public Boolean UseBackstepSwitchDirection { get; set; }
	public Boolean IsFinishableAttackPossible { get; }
	public Int32 currentSkillControlOrder { get; set; }
	public Int32 RemainAdditionalControlCount { get; set; }
	public virtual Boolean IsAddAttackTypeDecreaseCountOnSkillStart { get; }
	public virtual Boolean IsAddAttackTypeEndTimeCustom { get; }
	public virtual Boolean IsFinishAttackTypeEndTimeCustom { get; }
	public virtual Boolean IsGhostMode { get; }
	public Int32 SkillIndex { get; set; }
	public virtual Boolean IsSDSkill { get; }
	protected IRDActiveObject parent_ { get; set; }
	private Boolean IsGrow { get; }
	protected Int32 GrowType { get; }
	protected Int32 SecondGrowType { get; }
	public Int32 SubState { get; set; }
	public Boolean IsMapContinuity { get; }
	protected virtual ENUM_ACTIVE_OBJECT_STATE StartState { get; }
	public virtual Int32 CastAnimationIndex { get; }
	public virtual EventFireTiming btnTriggerType { get; }
	public Boolean IsDualSkill { get; }
	public Boolean isFinished { get; set; }
	public ENUM_ACTIVE_OBJECT_STATE ParentState { get; }
	private Boolean isNoCoolTime { get; }

	// Methods
	
	protected Void Skill() { }
	
	public Boolean get_IsAdditionalControlable() { }
	
	private Void set_IsAdditionalControlable(Boolean value) { }
	
	public Boolean get_IsChargable() { }
	
	private Void set_IsChargable(Boolean value) { }
	
	public Boolean get_IsComsumeable() { }
	
	private Void set_IsComsumeable(Boolean value) { }
	
	public Boolean get_IsIncreasable() { }
	
	private Void set_IsIncreasable(Boolean value) { }
	
	public Boolean get_IsSustainable() { }
	
	private Void set_IsSustainable(Boolean value) { }
	
	public Boolean get_IsFinishableAttack() { }
	
	private Void set_IsFinishableAttack(Boolean value) { }
	
	public Boolean get_IsDistanceAdjustable() { }
	
	private Void set_IsDistanceAdjustable(Boolean value) { }
	
	public Int32 get_SwitchableSkillIndex() { }
	
	private Void set_SwitchableSkillIndex(Int32 value) { }
	
	public Boolean get_CanBeSlideToNeutral() { }
	
	private Void set_CanBeSlideToNeutral(Boolean value) { }
	
	public SlidableDirectionTypes get_SlidableDirectionType() { }
	
	private Void set_SlidableDirectionType(SlidableDirectionTypes value) { }
	
	public Vector3 get_SkillDirectionGuideOffset() { }
	
	private Void set_SkillDirectionGuideOffset(Vector3 value) { }
	
	public DIRECTION_FLAGS get_SlidableDirections() { }
	
	private Void set_SlidableDirections(DIRECTION_FLAGS value) { }
	
	public List<T> get_SlidingSkills() { }
	
	private Void set_SlidingSkills(List<T> value) { }
	
	public Boolean get_IsUseButtonJoystick() { }
	
	private Void set_IsUseButtonJoystick(Boolean value) { }
	
	public Boolean get_ShowJoystickNotiGuide() { }
	
	private Void set_ShowJoystickNotiGuide(Boolean value) { }
	
	public Boolean get_UseJumpBurst() { }
	
	private Void set_UseJumpBurst(Boolean value) { }
	
	public Boolean get_UseJumpCancel() { }
	
	private Void set_UseJumpCancel(Boolean value) { }
	
	public Boolean get_UseReuseCancel() { }
	
	private Void set_UseReuseCancel(Boolean value) { }
	
	public Boolean get_UseBackstepSwitchDirection() { }
	
	private Void set_UseBackstepSwitchDirection(Boolean value) { }
	
	public Boolean get_IsFinishableAttackPossible() { }
	
	private Void ClearSkillControlProperties() { }
	
	public Int32 get_currentSkillControlOrder() { }
	
	private Void set_currentSkillControlOrder(Int32 value) { }
	
	public Boolean ResetSkillControlOrder() { }
	
	public Int32 GetUsableControlRequireCount() { }
	
	public Boolean UsableControllInfo() { }
	
	private Boolean isHideUISkill() { }
	
	private Void OnEndBeforeSkillControlInfo() { }
	
	public String GetSkillIconAtlasPath() { }
	
	public SkillControlType GetSkillControlType() { }
	
	public virtual Boolean ApplySkillControlInfo() { }
	
	public Boolean TryApplyFirstOrderSkillControl() { }
	
	public Boolean IsExistNextOrderControlInfo() { }
	
	private Boolean CheckExtraSkillOptionExists(ExtraSkillOptions flag, ExtraSkillOptions value) { }
	
	private Boolean checkUsableControlInfo(List<T> requireList) { }
	
	public Boolean CheckOrderSkillControlEnd(ControlEndTriggerType type) { }
	
	public virtual Int32 GetTotalAdditionalControlCount() { }
	
	public Int32 get_RemainAdditionalControlCount() { }
	
	protected Void set_RemainAdditionalControlCount(Int32 value) { }
	
	public virtual Void DecreaseRemainAdditionalControlCount() { }
	
	public virtual Single GetRangeSkillMaxDistance() { }
	
	public virtual Single GetRangeSkillMaxTime() { }
	
	public virtual Boolean get_IsAddAttackTypeDecreaseCountOnSkillStart() { }
	
	public virtual Boolean get_IsAddAttackTypeEndTimeCustom() { }
	
	public virtual Boolean get_IsFinishAttackTypeEndTimeCustom() { }
	
	protected virtual Boolean CheckRequireCodeCustom(Int32 index) { }
	
	public virtual List<T> GetRelativeCodeCustomSkills() { }
	
	public Void DoEndFinishAttackControl() { }
	
	public Void DoEndAddAttackControl() { }
	
	private Void NotifyViewCallNextCombo() { }
	
	public Void ForceResetAdditionalControlCount() { }
	
	public Void SetSlidableDirections(DIRECTION_FLAGS flags) { }
	
	public Void SetSkillGaugeState(SkillGaugeState state) { }
	
	public Boolean IsSkillGaugeState(SkillGaugeState state) { }
	
	public Boolean IsOnPressControlTypeSkill() { }
	
	public Void OnSkillControlButtonDoubleDown() { }
	
	public Void OnSkillControlButtonDown() { }
	
	public Void OnSkillControlButtonUp() { }
	
	protected virtual Void ReleaseSkillGaugeUiByButtonUp() { }
	
	private Void ApplySkillControlNextOrderByButtonUp() { }
	
	public Int32 GetSlidingSkillIndex(Int32 slideDirectionIndex) { }
	
	public virtual Boolean get_IsGhostMode() { }
	
	public Int32 get_SkillIndex() { }
	
	public Void set_SkillIndex(Int32 value) { }
	
	public virtual Boolean get_IsSDSkill() { }
	
	protected IRDActiveObject get_parent_() { }
	
	protected Void set_parent_(IRDActiveObject value) { }
	
	public ICharacterMediator getCharacter() { }
	
	public ICharacterMediator getCharacter(IRDActiveObject parentObj) { }
	
	public Nullable<T> GetCharacterUniqID() { }
	
	private Boolean get_IsGrow() { }
	
	protected Int32 get_GrowType() { }
	
	protected Int32 get_SecondGrowType() { }
	
	public Int32 get_SubState() { }
	
	public Void set_SubState(Int32 value) { }
	
	public Boolean get_IsMapContinuity() { }
	
	protected virtual Boolean isOnReplaceSkill() { }
	
	public Boolean TryToGetFailReason(out String stringKey) { }
	
	protected virtual ENUM_ACTIVE_OBJECT_STATE get_StartState() { }
	
	public virtual Int32 get_CastAnimationIndex() { }
	
	public Void RefreshScript() { }
	
	private Void RefreshCoolTime() { }
	
	public virtual EventFireTiming get_btnTriggerType() { }
	
	public Boolean get_IsDualSkill() { }
	
	public Boolean IsIgnoreHistory() { }
	
	public Void SetSkillDirection(ENUM_DIRECTION_MOBILE skillDirection) { }
	
	public Void addSkillState(ENUM_ACTIVE_OBJECT_STATE state) { }
	
	public Void setLevel(Int32 index, Int32 level) { }
	
	public Int32 getLevel(Int32 index) { }
	
	public Int32 increaseLevel(Int32 index, Int32 value) { }
	
	public Void OnChangeLevel(Int32 index, Int32 level) { }
	
	public Boolean get_isFinished() { }
	
	public Void set_isFinished(Boolean value) { }
	
	public STSkillScript GetScript() { }
	
	private Void ResetReservedInput() { }
	
	private Void TryReservedInput() { }
	
	public Boolean IsOnReservedInput() { }
	
	public virtual Void Init(Int32 skillIndex, ENUM_CHARACTERJOB job, IRDActiveObject _parent) { }
	
	protected Void Init(Int32 skillIndex, ENUM_CHARACTERJOB job) { }
	
	protected Void CachingConstData() { }
	
	protected Void CachingDynamicData(Int32 level) { }
	
	public Void OnChangeSkillMPRate(Single rate) { }
	
	public Void Release() { }
	
	protected Int32 getScriptCustomAniIndex(Int32 index) { }
	
	public Int32 getScriptPassiveIndex(Int32 index) { }
	
	protected Int32 getScriptMonsterIndex(Int32 index) { }
	
	public String getScriptSoundTag(Int32 index) { }
	
	public String getScriptEffectAni(Int32 index) { }
	
	protected AttackInfo getScriptAttackInfo(Int32 index) { }
	
	public String getScriptParticleName(Int32 index) { }
	
	public virtual Void InitAnimation() { }
	
	public virtual Void RegistPreloadResoruce() { }
	
	public Boolean TryUseSkill() { }
	
	public virtual UsableResult isUsable(Boolean isDualSkill, Boolean withoutStateCheck) { }
	
	protected UsableResult isUsable2(Boolean isDualSkill, Boolean withoutStateCheck) { }
	
	public ENUM_ACTIVE_OBJECT_STATE get_ParentState() { }
	
	public virtual Single UsableCoolTime() { }
	
	public Boolean isUsableMP(Boolean isDualSkill) { }
	
	public Int32 CheckUseItem(Boolean isDualSkill) { }
	
	public virtual Boolean isUsableAnotherSkill(Int32 skillIndex) { }
	
	public virtual Boolean isExcutableState() { }
	
	protected virtual Boolean isExcutableStateBurst() { }
	
	public virtual BurstState GetBurstState() { }
	
	public virtual Boolean isExecutableAnotherSkill(Int32 skillIndex) { }
	
	public virtual Void onStartSkill(ENUM_DIRECTION_MOBILE inputDirection, Boolean isDualSkill) { }
	
	public virtual Void onEndSkill() { }
	
	public virtual Void OnStartCinematic(Boolean isIgnoreObject) { }
	
	private Void NotifyViewSkillStart() { }
	
	private Void NotifyViewSkillEnd() { }
	
	public virtual Void OnStartMap() { }
	
	public virtual Void OnEndMap() { }
	
	public Void StartSkillGague(Boolean resume) { }
	
	public Boolean hasState(ENUM_ACTIVE_OBJECT_STATE state) { }
	
	public virtual Boolean isValidState(ENUM_ACTIVE_OBJECT_STATE state) { }
	
	public virtual Boolean isValidStateAnotherSkill(ENUM_ACTIVE_OBJECT_STATE state) { }
	
	public virtual Void preProc(Single _LogicDeltaTime) { }
	
	public virtual Void preProcBeforeEvade() { }
	
	public virtual Void mainProc(Single procDeltaTime) { }
	
	public virtual Void preParentStateChange(ENUM_ACTIVE_OBJECT_STATE newState, List<T> datas) { }
	
	public virtual Void onEndState(ENUM_ACTIVE_OBJECT_STATE oldState, ENUM_ACTIVE_OBJECT_STATE newState) { }
	
	public virtual Void onAfterSetState(ENUM_ACTIVE_OBJECT_STATE state, List<T> datas) { }
	
	public virtual Void postParentStateChange(ENUM_ACTIVE_OBJECT_STATE newState, ENUM_ACTIVE_OBJECT_STATE oldState, List<T> datas) { }
	
	protected virtual Void setState(ENUM_ACTIVE_OBJECT_STATE newState, List<T> datas) { }
	
	public virtual Void constantProc() { }
	
	public virtual Void onParentAnimationEnd() { }
	
	public virtual Boolean changeThrowState(ENUM_THROW_STATE throwState) { }
	
	public virtual Void onBuffCastEnd(IRDActiveObject parentObj) { }
	
	public virtual Void onAnimationOffset(Ref fromAni, String offsetName, Single offsetX, Single offsetY, Single offsetZ) { }
	
	public virtual Void onAnimationAdditionalTag(Ref fromAni, KeyFrameAdditionalTag tag) { }
	
	public virtual Void onParentDestroyObject() { }
	
	public virtual Void prepareDraw() { }
	
	public static Void ClearLoadedSkillList() { }
	
	public static Boolean HasLoadedSkill() { }
	
	private static Boolean IsLoadedSkill(ENUM_CHARACTERJOB job, Int32 skillIndex) { }
	
	private static Void RegistLoadedSkill(ENUM_CHARACTERJOB job, Int32 skillIndex) { }
	
	public Void PreLoadSkill() { }
	
	protected virtual Void preLoad() { }
	
	public virtual Void OnBeforeAttack(IRDCollisionObject damager, Boolean isStuck) { }
	
	public virtual Void OnAfterAttack(IRDCollisionObject damager, Boolean isStuck) { }
	
	public virtual Void onBeforeDamage(IRDCollisionObject attacker, Boolean isStuck) { }
	
	public virtual Boolean onAttack(IRDCollisionObject damager, IRDCollisionObject attacker, Boolean isStuck) { }
	
	public virtual Void onDamage(IRDCollisionObject attacker, ref Boolean isCritical, Boolean isStuck) { }
	
	public virtual ENUM_IMMUNE_TYPE getImmuneType(ref Int32 damageRate, IRDCollisionObject attacker) { }
	
	public virtual Boolean checkSkillOn() { }
	
	public Int32 getIntData(Int32 index) { }
	
	public Int32 getIntDataForUI(Int32 index, Boolean pIsPVPStat) { }
	
	private Single addSkillLevelInfo(Single data, Int32 index) { }
	
	private Single addSkillLevelInfoForUI(Single data, Int32 index, Boolean pIsPVPStat) { }
	
	private Int32 addSkillStaticData(Int32 data, Int32 index) { }
	
	public Skill getFeatureSkill() { }
	
	public Skill getFeatureSkillForUI(Boolean pIsPVPStat) { }
	
	private Dictionary<TKey, TValua> getLevelDatasForUI(Boolean pIsPVPStat) { }
	
	private Boolean getLevelDatasForUI(Boolean pIsPVPStat, Int32 index, out List<T> datas) { }
	
	private Dictionary<TKey, TValua> getLevelDatas() { }
	
	private Boolean getLevelDatas(Int32 index, out List<T> datas) { }
	
	public List<T> getCustomIntDatas() { }
	
	public List<T> getCustomIntDatasForUI(Boolean pIsPVPStat) { }
	
	public Single getLevelData(Int32 index, IRDActiveObject destObject, ENUM_CONVERT_TABLE_TYPE convertTableType) { }
	
	public Single getLevelDataWithRate(Int32 index, Single rate) { }
	
	public Single getLevelData(Int32 index, Int32 level) { }
	
	public Single getLevelDataForUI(Boolean pIsPVPStat, Int32 index, Int32 level) { }
	
	public Int32 GetMaxLevel() { }
	
	public Int32 GetPureMaxLevel() { }
	
	protected Single getLevelRevisionData(Int32 index, Int32 skillLevel) { }
	
	protected Single getLevelRevisionDataForUI(Boolean pIsPVPStat, Int32 index, Int32 skillLevel) { }
	
	public Int32 getAttackBonusRate(Int32 levelRevisionDataIndex, Single bonusRate, Int32 skillLevel, Int32 addRawAttackBonusRate) { }
	
	public Int32 getRawAddAttackBonusRate(Int32 levelRevisionDataIndex, Int32 skillLevel) { }
	
	public Single getActiveStatusPower(Int32 levelRevisionDataIndex, ENUM_DAMAGE_TYPE damageType, Int32 level) { }
	
	public Single getActiveStatusPowerForUI(Int32 levelRevisionDataIndex, ENUM_DAMAGE_TYPE damageType, Boolean pIsPVPStat, Int32 level) { }
	
	public Int32 getPower(Int32 levelRevisionDataIndex, Int32 attackBonusRate, IRDCollisionObject destObject, Single powerBonusRate, Int32 level, IRDCharacter srcObject, Int32 addRawPowerValue) { }
	
	public Int32 getRawAddPowerValue(Int32 levelRevisionDataIndex, Int32 level) { }
	
	public Int32 getPowerForUI(Int32 levelRevisionDataIndex, Boolean pIsPVPStat, Int32 attackBonusRate, IRDCollisionObject destObject, Single powerBonusRate, Int32 level, IRDCharacter srcObject) { }
	
	private static Single GetSeperateAttackPower(IRDCharacter character) { }
	
	private static Single GetSeperateAttackPowerForUI(IRDCharacter character, Boolean pIsPVPStat) { }
	
	public static Int32 getPower(Single power, Int32 attackBonusRate, IRDCollisionObject destObject, Single powerBonusRate, Single skillPowerRate, ICharacterMediator srcObject, Int32 skillIndex, Skill targetSkill) { }
	
	public static Int32 getPowerForUI(Single power, Int32 attackBonusRate, IRDCollisionObject destObject, Single powerBonusRate, Single skillPowerRate, ICharacterMediator srcObject, Int32 skillIndex, Boolean pIsPVPStat) { }
	
	protected Single GetSkillMaxChargeTime(SkillDataType dataType, Int32 index, Int32 targetSkillIndex) { }
	
	protected Single GetSkillMaxConsumeTime(SkillDataType dataType, Int32 index, Int32 targetSkillIndex) { }
	
	public Single getCoolTime() { }
	
	public Single getCoolTime(Int32 level, Boolean isDualSkill) { }
	
	public Int32 getCoolTimeApplyType() { }
	
	public virtual Single GetRemainCoolTime(Boolean isDualSkill) { }
	
	public Single getCoolTimeOnStart(Int32 level) { }
	
	private Boolean get_isNoCoolTime() { }
	
	public Boolean isInCoolTimeWithCache(Boolean isDualSkill) { }
	
	public Boolean isInStartCoolTime(Boolean isDualSkill) { }
	
	public Boolean isInCoolTime(Boolean isDualSkill) { }
	
	public virtual Single getSkillCoolTimeInRateRevision(Single coolTime, Int32 skillIndex, Int32 skillLevel) { }
	
	public Void getCameraInterpolationValues(ref Int32[] values) { }
	
	public virtual Void AwakeSkillButtonEvent() { }
	
	public Boolean isPassiveSkill(Boolean isSd) { }
	
	public Boolean isPressSkillkey() { }
	
	public Boolean isDirectionForceSkill() { }
	
	public virtual Void setCurrentCoolTime(Single coolTime, Boolean isDualSkill) { }
	
	public Void startCoolTime() { }
	
	public virtual Void startCoolTime(Int32 level, Single rate, Single specificCoolTime, Boolean isHighSkillInitialCoolTime, Boolean isDualSkill) { }
	
	public Void ResetCurrentCoolTime() { }
	
	public Boolean isMainBuff() { }
	
	public Int32 getSpendMp(Int32 level, Boolean isDualSkill, Boolean useCached) { }
	
	public virtual Int64 GetSpendHP() { }
	
	public Single getMaintainMp(Int32 level) { }
	
	private Single getOriginalCastTime(Int32 level) { }
	
	public Single getCastTime(Int32 level, Boolean castSpeedApply) { }
	
	protected ENUM_CASTING_PROTECTION_TYPE GetCastingProtectionType() { }
	
	protected AttackInfo getCurrentAttackInfo() { }
	
	public DbgAttackInfo getDbgAttackInfo() { }
	
	public Void setDbgAttackInfoTarget(IRDCollisionObject target) { }
	
	protected Void setAttackInfoBySkillScript(Int32 index) { }
	
	protected AttackInfo setCurrentAttackInfo(Int32 customAttackIndex) { }
	
	protected AttackInfo getCustomAttackInfo(Int32 index) { }
	
	protected List<T> getAttackAni(Int32 attackAniIndex) { }
	
	protected List<T> getStateAni(Int32 stateAniIndex) { }
	
	protected List<T> getCustomAni(Int32 customAniIndex) { }
	
	protected Void setCurrentAnimation(Int32 customAniIndex, Single animationSpeedRate) { }
	
	protected Void setCurrentCastAnimation(Int32 chargeAni, Int32 shotAni, Boolean useSelectTarget, Int32 range) { }
	
	protected Void setCurrentAnimation(List<T> aniInfos, Single animationSpeedRate) { }
	
	protected Void setAllAnimationSpeed(Single animationSpeed) { }
	
	protected Void setAllAnimationFrameDelay(Int32 frame, Single delay, Boolean isLayerAlso, Boolean isAutoLayerAlso) { }
	
	protected DNFAnimator getCurrentAnimator() { }
	
	protected Boolean IsKeyFrameFlag(UInt16 flag) { }
	
	protected Void ClearKeyFrameFlag() { }
	
	public virtual Void shakeScreen(Boolean shakeOnlyOwner, Int32 customTime) { }
	
	protected Void stopShakeScreen() { }
	
	public virtual Void onFinishSkillChangeState() { }
	
	protected Ref getCurrentBaseAnimation() { }
	
	protected Ref getCurrentAnimation() { }
	
	public Void getScrollBasisPos(ref Vector3 scrollBasisPos) { }
	
	protected Void SetCameraSkillBasisPos(Nullable<T> targetID, ENUM_SKILL_SCROLL_BASIS_TYPE skillScrollBasisType, Single scrollBasisPosX, Single scrollBasisPosY) { }
	
	protected Void SetCameraSkillBasisMovablePos(Nullable<T> targetID, ENUM_SKILL_SCROLL_BASIS_TYPE skillScrollBasisType, ref Single scrollBasisPosX, ref Single scrollBasisPosY) { }
	
	public virtual Void onUserInputAttack() { }
	
	public virtual Void onUserInputJump() { }
	
	public virtual Void onMoveCollision(Int32 axisIndex, IRDCollisionObject collisionedObject) { }
	
	public virtual Void onUseSkill(Int32 skillIndex, ENUM_DIRECTION_MOBILE inputDirection) { }
	
	protected virtual Boolean onSkillButtonDown(ENUM_DIRECTION_MOBILE inputDirection) { }
	
	public Boolean trySkillButtonDown(ENUM_DIRECTION_MOBILE inputDirection) { }
	
	public virtual Void OnPointerUpHandler(ENUM_DIRECTION_MOBILE skillDirection) { }
	
	private Void OnTapTapControSuccess() { }
	
	protected virtual Boolean isIgnoreSkillAttackBtnAddControl() { }
	
	public virtual Boolean isJumpButtonCancelableState() { }
	
	public virtual Boolean isStackableSkill() { }
	
	public virtual Void DoJumpButtonCancel() { }
	
	public virtual Boolean IsBigBodyFollowSkill() { }
	
	public Boolean isValidStateAttackBtnAddControl() { }
	
	public List<T> getUseItems(Boolean isDualSkill) { }
	
	public Int32 onUseCubeSkill(Boolean isDualSkill) { }
	
	public Void ConsumeSkillUseItems(Boolean isDualSkill) { }
	
	public Void clearCalcEquDatas() { }
	
	protected Void addSetState(ENUM_ACTIVE_OBJECT_STATE state, SetStateData datas, ENUM_STATE_PRIORITY priority) { }
	
	protected Void resetHitObjectList() { }
	
	protected Void disableLayerAnimation(Ref animation) { }
	
	protected Void removeLayerAnimation(Clip animation) { }
	
	protected Void removeLayerAnimation(Ref animation) { }
	
	protected Void setStaticSpeedInfo(ENUM_SPEED_TYPE moveSpeedType, ENUM_SPEED_TYPE animationSpeedType, Int32 moveSpeedValue, Int32 animationSpeedValue, Single moveSpeedRate, Single animationSpeedRate, Int32 minmumMoveSpeed, Int32 minmumAnimationSpeed) { }
	
	protected Void setMoveDirection(ENUM_DIRECTION_MOBILE xDirection, ENUM_DIRECTION_MOBILE yDirection, Boolean changeDirection) { }
	
	protected Void setStaticMoveInfo(Int32 axisIndex, Single normalMoveVelocity, Int32 dummy_slantMoveVelocity, Boolean isUserControl, Single moveAccel, Boolean isAccelToStop) { }
	
	protected Void addAnimationFromFile(ENUM_ACTIVE_OBJECT_STATE state, String path) { }
	
	protected Void addLayerAnimation(Int32 layerIndex, Clip animInstance, Single xPos, Single yPos, LAYER_OPTION flag, Single resize_rate) { }
	
	protected Void addLayerAnimation(IRDActiveObject parent, Int32 layerIndex, Clip animInstance, Single xPos, Single yPos, LAYER_OPTION flag, Single resize_rate) { }
	
	protected Ref addLayerAnimation(Int32 layerIndex, String aniFileName, Single xPos, Single yPos, LAYER_OPTION flag, Single resize_rate) { }
	
	protected Ref addLayerAnimation(Int32 layerIndex, Ref animation, Single xPos, Single yPos, LAYER_OPTION flag, Single resize_rate) { }
	
	protected Void playSound(String soundTag, Int32 soundID, Boolean playOnlyOwner, Boolean isSameAudioIgnore) { }
	
	protected Void playSound(IRDActiveObject parentObj, String soundTag, Int32 soundID, Boolean playOnlyOwner, Boolean isSameAudioIgnore) { }
	
	protected T getTeam() { }
	
	protected Void stopSound(Int32 soundID) { }
	
	protected Void stopSound(String soundTag) { }
	
	protected Int32 Rand_r() { }
	
	public Boolean isIncreaseSkillDamageAutoStart() { }
	
	public Void setReplaceSkill(Skill target) { }
	
	public Void setIncreaseSkillDamage(Boolean isIncrease, Int32 count) { }
	
	public ENUM_ACTIVE_OBJECT_STATE getState() { }
	
	public virtual Void getSubState(ref Byte subState) { }
	
	public Void clearAllItemSkillVariableFactor() { }
	
	public Void setSkillVariableFactor(ENUM_EQUIPMENTTYPE pEquipType, ENUM_EQUIPMENT_SKILL_DATA_EXPANSION pDataupType, Boolean pIsRate, Single pValue) { }
	
	public Void removeSkillVariableFactor(ENUM_EQUIPMENTTYPE pEquipType, ENUM_EQUIPMENT_SKILL_DATA_EXPANSION pDataupType, Boolean pIsRate, Single pValue) { }
	
	public Void setSkillVariableFactorBySubindex(ENUM_EQUIPMENTTYPE pEquipType, ENUM_EQUIPMENT_SKILL_DATA_EXPANSION pType, Int32 pSubIndex, Boolean pIsRate, Single pValue) { }
	
	public Void removeSkillVariableFactorBySubindex(ENUM_EQUIPMENTTYPE pEquipType, ENUM_EQUIPMENT_SKILL_DATA_EXPANSION pType, Int32 pSubIndex, Boolean pIsRate, Single pValue) { }
	
	private Single getReferenceCastTime(Int32 level) { }
	
	private Single getReferenceCoolTime(Int32 level) { }
	
	private Int32 getReferenceSpendMP(Int32 level) { }
	
	private Int32 getReferenceMaintainMP(Int32 level) { }
	
	private Int32 getDualSpendMP(Int32 level) { }
	
	private Single getDualCoolTime(Int32 level) { }
	
	public Void getIconImage(Int32 idx, DNFSprite_UI iconSprite, Boolean isDualSkill) { }
	
	public Void setUseDualSkill(Boolean isUse) { }
	
	public Boolean IsDualSkillActivated() { }
	
	public ENUM_HOLDTYPE getHoldType() { }
	
	public Boolean getCheckIsGrabable() { }
	
	protected CNInvincible setInvincible(Single validTime) { }
	
	protected Void SetAnimationPause(Boolean pause) { }
	
	protected Single getPassiveFeatureData(Int32 index, Single data, Boolean isLevelData, RATE_OR_CONSTANT rate_or_const) { }
	
	protected Single getPassiveFeatureDataForUI(Boolean pIsPVPStat, Int32 index, Single data, Boolean isLevelData, RATE_OR_CONSTANT rate_or_const) { }
	
	public static Color ALPHA(Int32 alpha) { }
	
	protected Color NRGBA(Int32 r, Int32 g, Int32 b, Int32 a) { }
	
	protected Void BeginBackgroundEffect() { }
	
	protected Void EndBackgroundEffect() { }
	
	private Void WriteCoolTimeLog() { }
	
	protected virtual Void StartCutScene() { }
	
	public virtual Int32 getLimitCount(Int32 level) { }
	
	public virtual Void appendageProc(IRDAppendage appendage, Single procDeltaTime) { }
	
	public virtual Void appendageOnStart(IRDAppendage appendage) { }
	
	public virtual Void appendageOnEnd(IRDAppendage appendage, Boolean onReleaseParent) { }
	
	public virtual Void appendageDrawAppend(IRDAppendage appendage, Boolean isOver, Single _deltaTime) { }
	
	protected ENUM_DIRECTION_MOBILE GetSkillDirection(Int32 axis) { }
	
	public virtual Void PostCheckHitCollision() { }
	
	public virtual Void OnBeforeDamage_OriginProcedure(IRDCollisionObject attacker, Boolean isStuck) { }
	
	public virtual Void OnAfterDamage_OriginProcedure(IRDCollisionObject attacker, Boolean isStuck) { }
	
	private static Void Skill() { }
	
	private static Boolean <isUsable>m__0(SkillUseItem e) { }
	
	private static Boolean <CheckUseItem>m__1(SkillUseItem e) { }
}