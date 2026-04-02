using UnityEngine;

/// <summary>
/// 子弹飞行体，用于枪手等远程攻击。
/// 参考：CNGunnerBullet (dump.cs line 446815)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    // ==================== 配置 ====================
    public float lifetime = 3f;

    // ==================== 运行时 ====================
    public Vector2 Direction { get; private set; }
    public float Speed { get; private set; }
    public int Damage { get; private set; }
    public int OwnerTeamId { get; private set; }
    public float MaxRange { get; private set; }

    private Vector2 _startPos;
    private bool _initialized;

    /// <summary>
    /// 初始化子弹
    /// </summary>
    /// <param name="direction">飞行方向</param>
    /// <param name="speed">飞行速度</param>
    /// <param name="damage">伤害值</param>
    /// <param name="teamId">所属队伍</param>
    /// <param name="range">最大射程</param>
    public void Init(Vector2 direction, float speed, int damage, int teamId, float range)
    {
        Direction = direction.normalized;
        Speed = speed;
        Damage = damage;
        OwnerTeamId = teamId;
        MaxRange = range;
        _startPos = transform.position;
        _initialized = true;

        // 朝向飞行方向
        if (direction.x < 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!_initialized) return;

        // 移动
        transform.Translate(Direction * Speed * Time.deltaTime);

        // 超出射程销毁
        float dist = Vector2.Distance(_startPos, transform.position);
        if (dist >= MaxRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测命中角色
        var character = other.GetComponent<CharacterBase>();
        if (character != null)
        {
            // 不打队友，不打自己
            if (character.TeamId == OwnerTeamId) return;
            if (character.IsDead) return;

            character.TakeDamage(Damage);
        }

        // 命中任何有效目标后销毁
        if (character != null)
            Destroy(gameObject);
    }
}
