using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Enemy : MonoBehaviour
{
    protected bool skill;//スキル使用可能か
    [SerializeField] float maxhp;//最大HP
    [SerializeField] float deffence;//防御力(未実装)
    [SerializeField] float exp;//倒したときの経験値
    float down;//ダウン値
    [SerializeField] float forceRate;//吹っ飛び率
    [SerializeField] float downRate;//ダウン値率
    [SerializeField] float knockResist;//ノックバック軽減
    Tweener tweenerX;//ノックバック・ダウン用
    Tweener tweenerY;//ノックバック・ダウン用
    AttackCollisionValue attackCollisionValue;//ヒットした当たり判定の値
    Vector3 vector3;//一時利用
    float hp;//現在のHP
    [SerializeField] CapsuleCollider2D capsuleCollider;//死亡時当たり判定無くす
    [SerializeField] public Animator animator;
    [SerializeField] float attackArea;//攻撃範囲
    [SerializeField] float damage;//攻撃力
    [SerializeField] protected float attackSpeed;//攻撃速度
    [SerializeField] float velocity;//移動速度
    readonly protected int idleHash = Animator.StringToHash("idle");
    readonly protected int moveHash = Animator.StringToHash("move");
    readonly int attackHash = Animator.StringToHash("attack");
    readonly int downHash = Animator.StringToHash("down");
    readonly int standupHash = Animator.StringToHash("standup");
    readonly int knockHash = Animator.StringToHash("knock");
    readonly int knockEndHash = Animator.StringToHash("knockEnd");
    readonly int starHash = Animator.StringToHash("star");
    readonly int respawnHash = Animator.StringToHash("respawn");   
    public static Vector2 inversionVector2;//反転時スケール
    public float scale { get; protected set; }//初期スケール
    protected float attackCount;//攻撃速度カウント
    float[] effectCounts;//状態の持続時間カウント
    [SerializeField] ParticleSystem[] effects;//状態のエフェクト
    [SerializeField] ParticleSystem kandenDamage;//感電追加攻撃のエフェクト
    [SerializeField] Armor armor;//ハイパーアーマーの設定
    [SerializeField] Kind kind;//敵の種類
    Material material;//スプライトのマテリアル、ハイパーアーマー時赤くする
    bool armorbool;//ハイパーアーマー状態か
    [SerializeField] ParticleDelay particleSystemAttack;
    enum Armor
    {
        Null,
        Attack,
        All
    }
    enum Kind
    {
        Normal,
        Fly
    }
    //ノックバック時間
    float holdout { 
        get 
        {
            return holdout0;
        }
        set
        {
            if (holdout0 > 0f)
            {
                holdout0 = value;
            }
            else
            {
                holdout0 = value;
                if (holdout0 > 0f)
                {
                    animator.SetTrigger(knockHash);
                    foreach (var afterimage in afterimages)
                    {
                        afterimage.SetTrigger(knockHash);
                    }
                }
            }
        }
    }
    float holdout0;
    [SerializeField] Transform transformDamage; //ダメージ表示位置
    [SerializeField] Rigidbody2D _rigidbody2D; //自身のリジットボディ
    bool death;//倒されたか
    float hurimukiCount;//振りむき可能時間カウント
    public bool right { get; protected set; }//どっち向きか
    public Afterimage[] afterimages;//残像のセット
    protected Afterimage.Condition condition;//状態
    protected bool invincible;//無敵か
    Vector2 deathPoint;//吹っ飛びエフェクト用
    Transform spawnPoint;//スポーン位置
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //プレイヤーの武器のみトリガー
        if (!collision.CompareTag("PlayerWeapon")) return;
        //残像表示時、視覚でわかるように残像の拡大
        if (condition != Afterimage.Condition.Null || invincible)
        {
            foreach (var afterimage in afterimages)
            {
                afterimage.SetScale();
            }
        }
        //コリジョンの情報を取得/ダメージ等
        AttackCollision attackCollision = collision.gameObject.GetComponent<AttackCollision>();
        (AttackCollisionValue attackCollisionValue0, SingleHit singleHit) = attackCollision.GetAttackCollisionValue();
        //アイス攻撃なら関数を起動
        if (attackCollisionValue0.special == AttackCollisionValue.Special.Ice)
        {
            GameManager.playerManagerSub.Ice(this, attackCollisionValue0);
            return;
        }
        //ダメージ、ノックバック、ダウンの処理
        Damage(attackCollisionValue0, transform.position.x - (attackCollision.transform.position.x + attackCollisionValue0.velocity.x * attackCollisionValue0.interval));
        //毒攻撃なら関数起動
        if (attackCollisionValue0.special == AttackCollisionValue.Special.Poison)
        {
            GameManager.playerManagerSub.Poison(attackCollision, attackCollisionValue0);
        }
        //感電状態なら追加ダメージとエフェクト
        if (effects[(int)AttackCollisionValue.Effect.Kanden - 1].IsAlive(true) && attackCollisionValue.special != AttackCollisionValue.Special.Kanden)
        {
            Damage(GameManager.effect[(int)AttackCollisionValue.Effect.Kanden - 1].attackCollisionValue, default);
            kandenDamage.transform.position = collision.ClosestPoint(transform.position);
            kandenDamage.Play();
        }
        //サブの通常攻撃なら攻撃判定とエフェクトの削除とエンドエフェクトのプレイ
        if (singleHit != null)
        {
            singleHit.Stop(attackCollision.transform, attackCollisionValue0);
            attackCollision.End(true);
        }
        //状態異常の処理
        if ((int)attackCollisionValue0.effect > (int)AttackCollisionValue.Effect.Null)
        {
            effectCounts[(int)attackCollisionValue0.effect - 1] = GameManager.effect[(int)attackCollisionValue0.effect - 1].effectInterval;
            effects[(int)attackCollisionValue0.effect - 1].Play();
        }
    }
    //ノックバックタイムの設定
    void ResetForce(float duration)
    {
        holdout = duration + (holdout - (tweenerX != null ? tweenerX.Elapsed() : 0f)) / 2;
        ResetForce(false);
    }
    //ダウン時ノックバックタイムのクリア
    void ResetForce(bool resetHold)
    {
        if (resetHold) holdout = 0f;
        tweenerX?.Kill();
        tweenerY?.Kill();
    }
    protected virtual void SetDamage()
    {

    }
    //遠距離攻撃のエフェクト
    public void AttackParticle()
    {
        particleSystemAttack.enumerator = ParticlePlay(particleSystemAttack);
        StartCoroutine(particleSystemAttack.enumerator);
    }
    IEnumerator ParticlePlay(ParticleDelay particleDelay)
    {
        yield return new WaitForSeconds(particleDelay.delay);
        particleDelay.particleSystem.Play();
        particleDelay.enumerator = null;
    }
    //エフェクトの停止
    public void AttackParticlesStop()
    {
        if (particleSystemAttack.enumerator != null) StopCoroutine(particleSystemAttack.enumerator);
        if (particleSystemAttack.dependence && particleSystemAttack.particleSystem.IsAlive()) particleSystemAttack.particleSystem.Stop();
    }
    public void Damage(AttackCollisionValue attackCollisionValue, float rightRate)
    {
        //残像表示時、視覚でわかるように残像の拡大
        if (condition != Afterimage.Condition.Null || invincible)
        {
            foreach (var afterimage in afterimages)
            {
                afterimage.SetScale();
            }
        }
        //無敵ならここまで
        if (condition == Afterimage.Condition.Invincible || invincible) return;
        //ダメージのセット
        PlayerManager playerManager = attackCollisionValue.sub ? GameManager.playerManagerSub : GameManager.playerManager;
        float damage = GameManager.gameManager.GetDamage(attackCollisionValue);
        this.attackCollisionValue = attackCollisionValue;
        hp -= damage;
        SetDamage();
        //ヒットした対象に雷を落とすときの処理
        Special(attackCollisionValue);
        //ダメージの表示
        //GameManager.damageManagers[attackCollisionValue.sub ? 1 : 0].SetDamage((int)damage, transformDamage, true);
        if (hp > 0f) GameManager.expManagers[attackCollisionValue.sub ? 1 : 0].SetExp((int)damage, transformDamage, !playerManager.left);
        //ハイパーアーマーならここまで
        if (condition == Afterimage.Condition.Armor || armorbool) return;
        //ダウン値のセット
        down += attackCollisionValue.down * downRate;
        //空中でヒットしたなら即ダウン
        if (transform.position.y > 0f && down < 1f && kind == Kind.Normal) down = 1f;
        if (hp > 0f)
        {
            if (down >= 1f)
            {
                //ダウン処理
                var duration = attackCollisionValue.force.y * forceRate / 12 / down;
                ResetForce(true);
                vector3.x = transform.position.x + attackCollisionValue.force.x * forceRate * (attackCollisionValue.suction ? rightRate : 1) * (!playerManager.left ? 1 : -1);
                right = !playerManager.left;
                vector3.y = transform.position.y + attackCollisionValue.force.y * forceRate;
                tweenerX = transform.DOMoveX(vector3.x, duration)
                    .SetEase(kind == Kind.Normal ? Ease.OutSine : Ease.Linear)
                    .OnComplete(() =>
                    {
                        tweenerX = null;
                    });
                tweenerY = transform.DOMoveY(vector3.y, duration / 2)
                    .OnComplete(() =>
                    {
                        Down();
                    });
                animator.SetTrigger(downHash); 
                foreach (var afterimage in afterimages)
                {
                    afterimage.SetTrigger(downHash);
                }
            }
            else
            {
                //ノックバック
                Knockback(rightRate, playerManager);
            }
        }
        else
        {
            //吹っ飛び
            vector3.x = attackCollisionValue.force.normalized.x * 35f * (right ? 1 : -1);
            vector3.y = attackCollisionValue.force.normalized.y * 35f;
            vector3 += transform.position;
            var duration = 0.3f;
            ResetForce(true);
            tweenerX = transform.DOMove(vector3, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => 
                {
                    tweenerX = null;
                    Spawn();
                })
                .OnStart(() =>
                {
                    DeathHitStop();
                })
                .OnUpdate(() => 
                {
                    if (!death && !GameManager.gameManager.CheckInScreen(transform))
                    {
                        death = true;
                        deathPoint = attackCollisionValue.force.normalized;
                        if (!right) deathPoint.x *= -1;
                        GameManager.gameManager.KillEffect(this, deathPoint);
                    }
                });
            capsuleCollider.enabled = false;
            animator.SetTrigger(downHash);
            foreach (var afterimage in afterimages)
            {
                afterimage.SetTrigger(downHash);
            }
            //経験値のセット
            GameManager.expManagers[attackCollisionValue.sub ? 1 : 0].SetExp((int)(damage + exp), transformDamage, !playerManager.left);
        }
    }
    public float GetHpRate()
    {
        return hp / maxhp;
    }
    protected virtual void DeathHitStop()
    {
        //if (!attackCollisionValue.autoAtk) GameManager.playerManager.HitStop(0.3f);
        //else GameManager.playerManager.HitStop(0.08f);
    }
    //飛ばられた後の処理
    void Down()
    {
        if (kind == Kind.Normal)
        {
            tweenerY = transform.DOLocalMoveY(0f, transform.localPosition.y /** forceRate*/ / 18 / down)
                .OnComplete(() =>
                {
                    ReMove();
                })
                .SetEase(Ease.InQuad);
        }
        else
        {
            ReMove();
        }
    }
    //ノックバック処理
    void Knockback(float rightRate, PlayerManager playerManager)
    {
        float forceX = Mathf.Abs(attackCollisionValue.force.x) - knockResist;
        var duration = forceX > 0f ? (forceX + attackCollisionValue.force.y / attackCollisionValue.force.x * forceX) /** forceRate*/ / 24 : 0f;
        if (duration <= 0f) return;
        ResetForce(duration);
        vector3 = transform.position + attackCollisionValue.force /** forceRate*/ / 5 * (attackCollisionValue.suction ? rightRate : 1) * (!playerManager.left ? 1 : -1);
        right = !playerManager.left;
        tweenerX = transform.DOMoveX(vector3.x, holdout)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                holdout = 0f; 
                animator.SetTrigger(knockEndHash);
                foreach (var afterimage in afterimages)
                {
                    afterimage.SetTrigger(knockEndHash);
                }
                tweenerX = null;
            });
    }
    //未使用
    void StandUp()
    {
        animator.SetTrigger(standupHash);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetTrigger(standupHash);
        }
    }
    //リスポン
    void Spawn()
    {
        if (GameManager.enemyManager.stopSpown)
        {
            gameObject.SetActive(false);
            return;
        }
        animator.SetTrigger(respawnHash);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetTrigger(respawnHash);
        }
        SetSpawn();
        capsuleCollider.enabled = true;
    }
    //雷処理
    void Special(AttackCollisionValue attackCollisionValue)
    {
        if (attackCollisionValue.special == AttackCollisionValue.Special.Thunder)
        {
            GameManager.playerManager.thunder.Set(transform);
        }
        else if (attackCollisionValue.special == AttackCollisionValue.Special.ThunderAll && GameManager.gameManager.GetEvo(GameManager.playerManager, attackCollisionValue))
        {
            GameManager.playerManager.thunder.SetAllOk(transform);
        }
    }
    //初期化
    public virtual void Start0(Transform _transform)
    {
        scale = transform.localScale.x;
        effectCounts = new float[(int)AttackCollisionValue.Effect.Length - 1];
        spawnPoint = _transform;
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        material = spriteRenderers[0].material;
        foreach(var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sharedMaterial = material;
        }
        if (armor == Armor.All)
        {
            armorbool = true;
            material.SetColor(GameManager.gameManager.colorHash, Color.red);
        }
        SetSpawn();
    }
    //スポン時の値の設定
    void SetSpawn()
    {
        holdout = 0f;
        hp = maxhp;
        attackCount = 0f;
        if(kind == Kind.Normal) transform.position = spawnPoint.transform.position + Random.Range(0f, 2f) * Vector3.right;
        else transform.position = (Vector2)spawnPoint.transform.position + Random.Range(-4f, 0f) * inversionVector2 + Vector2.up * 12f;
        death = false;
    }
    //向きの設定
    public void SetRight()
    {
        if (right != GameManager.playerManager.transform.position.x <= transform.position.x)
        {
            right = !right;
            hurimukiCount = 1f;
        }
    }
    public virtual void Update0()
    {
        int currectAnime = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (GameManager.boss != this)
        {
            (float _left, float _right) = GameManager.gameManager.LeftRight();
            //カメラの範囲外に行ったときリスポン
            if (((!GameManager.playerManager.left && transform.position.x <= _left) || (GameManager.playerManager.left && transform.position.x >= _right)) && moveHash == currectAnime)
            {
                SetSpawn();
            }
            //攻撃時アーマーなら
            if(Armor.Attack == armor)
            {
                if(armorbool == (currectAnime != attackHash))
                {
                    armorbool = !armorbool;
                    material.SetColor(GameManager.gameManager.colorHash, armorbool ? Color.red : Color.black);
                }
            } 
        }
        //振りむきのセット
        hurimukiCount -= Time.deltaTime;
        if (hurimukiCount <= 0 && !skill)
        {
            SetRight();
        }
        //移動と攻撃
        if (kind == Kind.Normal)
        {
            Attack(currectAnime);
            if (currectAnime == moveHash)
            {
                transform.position += Time.deltaTime * velocity * (right ? Vector3.left : Vector3.right);
            }
        }
        else
        {
            AttackFly(currectAnime);
            if (currectAnime == moveHash)
            {
                transform.position += Time.deltaTime * velocity * (GameManager.playerManager.GetPlayerCenter() - transform.position).normalized;
            }
        }
        //向き
        transform.localScale = scale * (right ? Vector2.one : inversionVector2);
        //アタックカウント
        if (attackCount > 0f)
        {
            attackCount -= Time.deltaTime;
        }
        //状態異常
        for (int i = 0; i < (int)AttackCollisionValue.Effect.Length - 1; i++)
        {
            if (!effects[i].IsAlive(true)) continue;
            if (effectCounts[i] >= 0)
            {
                effectCounts[i] -= Time.deltaTime;
            }
            else
            {
                Effect((AttackCollisionValue.Effect)i + 1, default);
                effectCounts[i] += GameManager.effect[i].effectInterval;
            }
        }
        animator.SetBool(starHash, effects[1].IsAlive(true));
    }
    //起き上がり
    protected virtual void ReMove()
    {
        animator.SetTrigger(standupHash);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetTrigger(standupHash);
        }
        down = 0f;
    }
    //攻撃
    public virtual void Attack(int currectAnime)
    {
        if (currectAnime == idleHash || currectAnime == moveHash)
        {
            if (Mathf.Abs(GameManager.playerManager.transform.position.x - transform.position.x) <= attackArea)
            {
                AttackIdle();
            }
            else
            {
                Move();
            }
        }
    }
    //攻撃範囲内処理
    void AttackIdle()
    {
        if (attackCount > 0f)
        {
            animator.SetBool(moveHash, false);
            foreach (var afterimage in afterimages)
            {
                StartCoroutine(afterimage.SetBool(moveHash, false));
            }
        }
        else
        {
            animator.SetTrigger(attackHash);
            attackCount = attackSpeed;
            foreach (var afterimage in afterimages)
            {
                afterimage.SetTrigger(attackHash);
            }
        }
    }
    //移動
    void Move()
    {
        animator.SetBool(moveHash, true);
        foreach (var afterimage in afterimages)
        {
            StartCoroutine(afterimage.SetBool(moveHash, true));
        }
    }
    //飛行モンスターの攻撃
    public virtual void AttackFly(int currectAnime)
    {
        if (currectAnime == idleHash || currectAnime == moveHash)
        {
            if ((GameManager.playerManager.transform.position - transform.position).sqrMagnitude <= attackArea * attackArea)
            {
                AttackIdle();
            }
            else
            {
                Move();
            }
        }
    }
    //状態異常の処理
    public void Effect(AttackCollisionValue.Effect effect, float value)
    {
        if (effect == AttackCollisionValue.Effect.Poison) Damage(GameManager.effect[(int)effect - 1].attackCollisionValue, default);
    }
    protected virtual void OnDestroy()
    {
        tweenerX?.Kill();
        tweenerY?.Kill();
    }
}
