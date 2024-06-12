using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;

public class AttackCollision : MonoBehaviour
{
    [SerializeField] CapsuleCollider2D capsuleCollider2;//当たり判定
    [SerializeField] AttackCollisionValue attackCollisionValue;//当たり判定の設定
    IEnumerator enumeratorInterval;//当たり判定キャンセル時Kill用
    bool exist;//再利用
    Vector2 vector2;//一時利用
    Vector2 vector2side;//一時利用
    public int nextNum { get; set; }//次の当たり判定の配列
    SingleHit singleHit;//シングルヒット
    Tweener tweener;//当たり判定キャンセル時Kill用
    PlayerManager playerManager;//当たり判定の発生元
    IEnumerator enumerator;//当たり判定キャンセル時Kill用
    public AttackCollision SetCollision(AttackCollisionValue attackCollisionValue, Vector2 position, SingleHit singleHit)
    {
        if (exist) return null;
        exist = true;
        if (attackCollisionValue.delay > 0)
        {
            //ディレイありの時ディレイ後当たり判定の設定
            enumerator = SetCollisionDelay(attackCollisionValue, position, singleHit);
            StartCoroutine(enumerator);
        }
        else
        {
            //ディレイなしの時当たり判定の設定
            SetCollisionReal(attackCollisionValue, position, singleHit);
        }
        return this;
    }
    //ディレイ当たり判定
    IEnumerator SetCollisionDelay(AttackCollisionValue attackCollisionValue, Vector2 position, SingleHit singleHit)
    {
        yield return new WaitForSeconds(attackCollisionValue.delay);
        SetCollisionReal(attackCollisionValue, position, singleHit);
        enumerator = null;
    }
    //当たり判定の設定
    void SetCollisionReal(AttackCollisionValue attackCollisionValue, Vector2 position, SingleHit singleHit)
    {
        playerManager = attackCollisionValue.sub ? GameManager.playerManagerSub : GameManager.playerManager;
        this.attackCollisionValue = attackCollisionValue;
        this.singleHit = singleHit;
        int side = attackCollisionValue.left ? -1 : 1;
        tweener = DOTween.To(() => 0f, (x) =>
        {
            //当たり判定の移動
            vector2.x = attackCollisionValue.expantion.x * Mathf.Cos(attackCollisionValue.rotation / 180f * Mathf.PI);
            vector2.y = attackCollisionValue.expantion.y * Mathf.Sin(attackCollisionValue.rotation / 180f * Mathf.PI);
            vector2side.x = position.x + (attackCollisionValue.position.x + attackCollisionValue.velocity.x * x + vector2.x * x / 2) * side;
            vector2side.y = position.y + attackCollisionValue.position.y + attackCollisionValue.velocity.y * x + vector2.y * x / 2;
            transform.position = vector2side;
            capsuleCollider2.size = attackCollisionValue.size + attackCollisionValue.expantion * x;
            transform.localEulerAngles = Vector3.forward * (attackCollisionValue.rotation + attackCollisionValue.rotationVelocity * x) * side;
        }, 1f, attackCollisionValue.duration)
            .OnStart(() =>
            {
                //初期設定
                capsuleCollider2.direction = attackCollisionValue.direction;
                transform.position = position + attackCollisionValue.position * side;
                capsuleCollider2.size = attackCollisionValue.size;
                //進化スキルクリック時スケールの初期設定
                if (attackCollisionValue.special == AttackCollisionValue.Special.ScaleStart) transform.localScale = GameManager.playerManager.GetScale();
                //一度ヒット
                if (attackCollisionValue.interval <= 0f) capsuleCollider2.enabled = true;
                //進化スキルアイスのときクリック時ダメージ判定
                else if (attackCollisionValue.special == AttackCollisionValue.Special.Ice)
                {
                    enumeratorInterval = Interval();
                    StartCoroutine(enumeratorInterval);
                }
                //進化スキル風のときクリック時ダメージ判定
                else if (attackCollisionValue.special == AttackCollisionValue.Special.WindAdd)
                {
                    enumeratorInterval = IntervalWindAdd();
                    StartCoroutine(enumeratorInterval);
                }
                //それ以外の間隔ダメージの時
                else
                {
                    enumeratorInterval = IntervalNormal();
                    StartCoroutine(enumeratorInterval);
                }

            })
            .OnUpdate(() =>
            {
                //進化スキルクリック時スケールのクリック時処理
                if (attackCollisionValue.special == AttackCollisionValue.Special.Scale && playerManager.skillClick)
                {
                    transform.localScale += Vector3.one * 0.1f;
                    playerManager.LastParticleScaleAdd(0.1f);
                    playerManager.SkillEffectPlay(attackCollisionValue.skillId, transform, transform.localScale.x);
                };
            })
            .OnComplete(() =>
            {
                //当たり判定停止
                Stop();
                //プレイヤーと独立していないなら
                if (!attackCollisionValue.dependence) exist = false; 
            })
            .SetEase(attackCollisionValue.ease);
    }
    //間隔ダメージ処理
    IEnumerator IntervalNormal()
    {
        while (true)
        {
            capsuleCollider2.enabled = true;
            yield return new WaitForFixedUpdate();
            capsuleCollider2.enabled = false;
            yield return new WaitForSeconds(attackCollisionValue.interval);
        }
    }
    //進化スキル風用
    IEnumerator IntervalWindAdd()
    {
        while (true)
        {
            yield return new WaitUntil(() => playerManager.skillClick);
            capsuleCollider2.enabled = true;
            playerManager.SkillEffectPlay(4, transform);
            yield return new WaitForFixedUpdate();
            capsuleCollider2.enabled = false;
        }
    }//進化スキルアイス用
    IEnumerator Interval()
    {
        while (true)
        {
            capsuleCollider2.enabled = true;
            GameManager.playerManagerSub.IceInit();
            yield return new WaitForFixedUpdate();
            GameManager.playerManagerSub.FixedUpdate0();
            capsuleCollider2.enabled = false;
            IEnumerator enumerator = Interval0();
            StartCoroutine(enumerator);
            yield return new WaitUntil(() => enumerator.Current is bool || GameManager.playerManagerSub.SetIceClick(playerManager.skillClick));
            StopCoroutine(enumerator);
        }
    }
    //インターバル時間
    IEnumerator Interval0()
    {
        yield return new WaitForSeconds(attackCollisionValue.interval);
        yield return true;
    }
    //初期化
    public void Initialization(int nextNum)
    {
        this.nextNum = nextNum;
        Destroy();
    }
    //ダメージの受け渡し
    public (AttackCollisionValue, SingleHit) GetAttackCollisionValue()
    {
        return (attackCollisionValue, singleHit);
    }
    //アニメーションの停止による当たり判定の停止
    public AttackCollision End(bool kill)
    {
        if (enumerator != null)
        {
            StopCoroutine(enumerator);
            exist = false;
            enumerator = null;
        }
        if (!kill) return this;
        exist = false;
        tweener.Kill();
        Stop();
        return this;
    }
    //停止
    private void Stop()
    {
        Destroy();
        if (enumeratorInterval != null) StopCoroutine(enumeratorInterval);
    }
    //当たり判定の削除
    void Destroy()
    {
        transform.localScale = Vector3.one;
        capsuleCollider2.enabled = false;
    }
}
[System.Serializable]
public class AttackCollisionValue
{
    public float duration;//維持時間
    public Vector2 size;//サイズ
    public Vector2 position;//位置
    public Vector2 velocity;//速度
    public Vector2 expantion;//拡大
    public CapsuleDirection2D direction;//向き
    public float delay;//ディレイ時間
    public float rotation;//回転
    public float rotationVelocity;//回転速度
    public float damageRate;//ダメージ率
    public float interval;//ダメージ間隔
    public Vector3 force;//吹っ飛び
    public float down;//ダウン率
    public Ease ease;//移動方法
    public bool autoAtk;//通常攻撃か
    public Special special;//特殊攻撃
    public Effect effect;//状態異常
    public bool suction;//吸い込み
    public int skillId = -1;//スキルID
    public bool sub { get; set; }//サブプレイヤーか
    public bool left { get; set; }//当たり判定の向き
    public AttackCollision attackCollision { get; set; }//当たり判定停止用
    public bool dependence;//プレイヤーと独立しているか
    public enum Effect    
    { 
        Null,
        Poison,
        Star,
        Kanden,
        Length
    }
    
    public enum Special
    {
        Null,
        Thunder,
        Ice,
        Poison,
        Fire,
        Kanden,
        Scale,
        WindAdd,
        ThunderAll,
        ScaleStart
    }
}
