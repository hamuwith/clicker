using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Afterimage : MonoBehaviour
{
    public Animator animator { get; private set; }//自身のアニメーション
    [SerializeField] Material material;//自身のマテリアル
    [SerializeField] float offsetTime = 0.5f;//本体とのずれ
    [SerializeField] Color colorArmor;//アーマー時の色
    [SerializeField] Color colorInvincible;//無敵時の色
    Tweener tweener;//ヒット時スケールアップ用
    Condition condition;//状態
    PlayerManager playerManager;//対象
    Boss boss;//対象
    bool invincible;//無敵
    bool armor;//ハイパーアーマー
    public enum Condition
    {
        Null,
        Armor,
        ArmorPlus,
        Invincible,
        InvinciblePlus,
        AvoidancePlus,
        ArmorBreak
    }
    //初期化ボス用
    public void Init(Boss boss)
    {
        Init();
        this.boss = boss;
    }
    //初期化プレイヤー用
    public void Init(PlayerManager playerManager)
    {
        Init();
        this.playerManager = playerManager;
    }
    //初期化
    void Init()
    {
        animator = GetComponentInChildren<Animator>();
        material.color = Color.clear;
    }
    //プレイヤーかボスとディレイをかけて同じアニメーションを実行する
    public void SetSkill(int hash ,int skillHash, int id, int distanceHash = default, float distance = default)
    {
        if(distanceHash != default) animator.SetFloat(distanceHash, distance);
        animator.SetInteger(skillHash, id);
        SetTrigger(hash);
    }
    void ResuscitationComplete()
    {
    }
    public IEnumerator SetBool(int hash, bool bool0)
    {
        yield return new WaitForSeconds(offsetTime);
        animator.SetBool(hash, bool0);
    }
    public void SetTrigger(int hash)
    {
        StartCoroutine(SetStart(hash));
    }
    public void SetHprate(int hash, float hprate)
    {
        animator.SetFloat(hash, hprate);
    }
    IEnumerator Invincible(float time)
    {
        condition = Condition.Null;
        invincible = true; 
        material.color = colorInvincible;
        SetScaleYoyo();
        yield return new WaitForSeconds(time);
        invincible = false;
        SetCondition(condition, 0f);
    }
    IEnumerator Armor(float time)
    {
        condition = Condition.Null;
        armor = true;
        material.color = colorArmor;
        SetScaleYoyo();
        yield return new WaitForSeconds(time);
        armor = false;
        SetCondition(condition, 0f);
    }
    IEnumerator SetStart(int hash)
    {
        if (playerManager != null)
        {
            transform.position = playerManager.transform.position;
            transform.localScale = (playerManager.left ? playerManager.inversionVector2 : Vector2.one) * playerManager.scale;
        }
        else
        {
            transform.position = boss.transform.position;
            transform.localScale = (boss.right ^ boss.inversion ? Vector2.one : Enemy.inversionVector2) * boss.scale;
        }
        yield return new WaitForSeconds(offsetTime);
        animator.SetTrigger(hash);
    }
    //残像のセット
    public void SetCondition(Condition condition, float time)
    {
        if(condition == Condition.ArmorBreak) armor = false;
        this.condition = condition;
        if (invincible) return;
        if (armor) return;
        if (condition == Condition.Armor) material.color = colorArmor;
        else if (condition == Condition.Invincible) material.color = colorInvincible;
        else if (condition == Condition.InvinciblePlus) StartCoroutine(Invincible(time));
        else if (condition == Condition.AvoidancePlus) StartCoroutine(Invincible(time));
        else if (condition == Condition.ArmorPlus) StartCoroutine(Armor(time));
        else material.color = Color.clear;
    }
    //残像時ダメージ判定受けた際、スケールを変化させる
    public void SetScale()
    {
        StartCoroutine(SetScaleStart());
    }
    IEnumerator SetScaleStart()
    {
        yield return new WaitForSeconds(offsetTime);
        SetScaleYoyo();
    }
    void SetScaleYoyo()
    {
        tweener?.Complete();
        tweener = transform.DOScale(transform.localScale * 1.3f, 0.05f)
            .SetLoops(2, LoopType.Yoyo);
    }
    void SlashParticlePlay(int num)
    {
    }
    void SlashSkillParticlePlay(int num)
    {
    }
    void GuardParticlePlay()
    {
    }
    void OnDestroy()
    {
        tweener.Kill();
    }
}
