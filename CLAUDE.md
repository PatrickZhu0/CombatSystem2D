# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 2D combat game project (similar to DNF/Dungeon Fighter Online) with decompiled code analysis. The main codebase exists as a large dump file (`dump/dump.cs` - 772K+ lines) containing decompiled IL code from `Assembly-CSharp.dll`.

**Unity Version:** 6000.3.11f1

**Project Structure:**
```
dump/
  ├── dump.cs          # Main decompiled codebase (772,578 lines)
  ├── Skill.cs         # Extracted Skill class definition
  └── Enum.cs          # Extracted enums
```

## Core Architecture

### 1. Character System

**Key Classes:**
- `CharacterSkill` - Main player skill class (inherits from `Skill`)
- `CharacterViewController` - UI interaction controller
- `CharacterInfo` / `CharacterHelper` - Data structures and utilities

**Speed Types:**
- `SPEED_TYPE_MOVE_SPEED` - Movement speed
- `SPEED_TYPE_ATTACK_SPEED` - Attack speed
- `SPEED_TYPE_CAST_SPEED` - Casting speed
- `SPEED_TYPE_EXCEPT_WEAPON_ATTACK_SPEED` - Non-weapon attack speed

**Direction System:**
- `ENUM_DIRECTION_NEUTRAL` - Neutral direction
- `ENUM_DIRECTION_UP` - Up direction
- `ENUM_DIRECTION_DOWN` - Down direction

### 2. Skill System

**Skill Base Class** (`dump/Skill.cs`):
The `Skill` abstract class is the foundation for all skills with these key capabilities:

**Controllability Flags:**
- `IsAdditionalControlable` - Can receive additional input
- `IsChargable` - Can be charged
- `IsSustainable` - Can be sustained (hold)
- `IsFinishableAttack` - Can finish combo
- `IsDistanceAdjustable` - Can adjust distance

**Skill Control Types** (`dump/Enum.cs`):
- `Tap` - Single tap
- `TapTap` - Double tap
- `Slide` - Slide gesture
- `SlideZ` - Z-axis slide
- `OnPress` - Press and hold
- `Joystick` - Joystick control
- `SlideSkills` - Slide between skills
- `SlideBuffs` - Slide between buffs

**Sliding/Direction System:**
- `SlidableDirectionType` - Type of sliding direction
- `SlidableDirections` - Available direction flags
- `SlidingSkills` - List of sliding skill connections

**Attack Information:**
`AttackInfo` class contains:
- `maxHitCount` - Maximum targets hit
- `power_` - Base attack power
- `attackTerm` - Attack interval
- `absoluteDamage_` - Fixed damage value
- `eType` - Attack type enum
- `criticalHitRate_` - Critical hit chance
- `stuckRate_` - Stun/stuck rate
- `knockBackType_` - Knockback behavior
- `weakness_rate_` - Weakness detection rate

### 3. AI System

**State Machine Architecture:**
```
AIState (base)
  ├── OnStart()
  ├── OnEnd()
  ├── Think() -> float
  ├── GetDesire() -> bool
  └── GetMoveState() / SetMoveState()
```

**AI State Manager:**
- `AIStateManager` manages state transitions
- `AICharacterBuffManager` handles AI buffs
- `RemainDamageList` tracks damage over time

**Monster Parameters:**
`MonsterBaseParameter` - Base stats:
- Level, HP/MP max
- Physical/Magical attack/defense
- Equipment stats
- Element tolerance/attack arrays
- Hit recovery, rigidity

`MonsterBalance` - Balance modifiers:
- Attack/move speed
- Damage/defense multipliers
- Vision range, war-likeness (aggression)
- Super armor level

### 4. Movement & Pathfinding

**A* Implementation:**
`AStarMoveController` manages pathfinding:
- `astarPathCheckPoint` - Queue of path checkpoints
- `startPos_`, `targetPos_` - Path endpoints
- `isDash` - Dash movement flag
- `isOnAutomaticMove_` - Auto-movement state
- `onPathComplete` - Path completion callback

**Grid System:**
`GridPos` - Grid position with object pooling:
- `x`, `y` coordinates
- Implements `IEquatable<GridPos>`
- Uses `SelfObjectPool<GridPos>` for performance

### 5. Damage System

**Damage Flow:**
```
AttackInfo (attacker)
    ↓
DamagedObject (receiver)
    ↓
DamageSyncInfo (network sync)
```

**Damage Synchronization:**
`DamageSyncInfo` for multiplayer:
- `partyIndex` - Party identifier
- `monsterIndexList` - Affected monsters
- `checkDamage` - Damage value
- `maxHp` - Max HP for percentage calc

**Damage Tracking:**
- `DamagedObject` - Tracks last damage time
- `DamageHistroy` - Historical damage records
- `PlayerDamageHistroyManager` - Player damage history

### 6. Map System

**Map Structure:**
- `MapTile` - Individual tile data
- `MapData` - Map index dictionary
- `MapArea` - Area management
- `GridScript` - Grid behavior

**Map Options:**
`MapDataOption` flags:
- `useExitTagForGridWhenSave` - Exit tag usage
- `useKoreanGreedTag` - Korean tag system
- `load_raw_tag` - Raw tag loading
- `load_raw_hellparty_info` - Hell party info

## Code Navigation Tips

**Finding Classes in dump.cs:**
Use grep/search with patterns:
```
public class ClassName
public sealed class ClassName
public abstract class ClassName
public interface InterfaceName
```

**Finding Enums:**
```
public enum EnumName
```

**Key Interfaces:**
- `IRDCharacter` - Character interface
- `IRDAICharacter` - AI character interface
- `IRDActiveObject` - Active object interface
- `IRDCollisionObject` - Collision object interface
- `ICharacterMediator` - Character mediator pattern

**Common Patterns:**
- Mediator pattern for character interactions
- State machine for AI behaviors
- Object pooling for GridPos and AttackInfo
- Delegate callbacks for pathfinding completion
- Skill chaining via sliding system

## Important Notes

1. **Decompiled Code**: The main codebase is decompiled IL, not original source. Variable names may be generic (e.g., `P0`, `P1` for parameters).

2. **No Build Commands**: This is an analysis project, not active development. Focus on understanding architecture.

3. **Large File**: `dump.cs` is 772K+ lines. Use targeted searches rather than reading entire file.

4. **Unity-Specific**: Code uses Unity types (Vector3, MonoBehaviour, etc.) and Unity's component architecture.

5. **Network Synchronization**: Damage and state systems include multiplayer sync mechanisms.

## Analysis Workflow

When analyzing systems:
1. Start with key classes (CharacterSkill, Skill, AIState)
2. Trace interfaces to find implementations
3. Look for manager classes (Singleton pattern common)
4. Check for data/script table classes for configuration
5. Identify related UI controller classes

**Useful Search Patterns:**
- Inheritance: `class.*:.*Skill`
- Managers: `class.*Manager`
- Factories: `class.*Factory`
- Data classes: `class.*Data$` (ends with Data)
- Info classes: `class.*Info$`
