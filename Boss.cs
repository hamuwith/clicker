using UnityEngine;
using System.Collections;

public class Boss : Enemy
{
    [SerializeField] SkillSet[] skillSets;//スキルの設定
    readonly int skillHash = Animator.StringToHash("skill");
    readonly int skillIdHash = Animator.StringToHash("skillId");
    [SerializeField] float attackArea2;//近距離攻撃の範囲
    readonly int attackkinHash = Animator.StringToHash("attackkin");
    readonly int distanceHash = Animator.StringToHash("distance");
    readonly int hprateHash = Animator.StringToHash("hprate");
    [SerializeField] Particle[] skillParticles;
    [SerializeField] ParticleSystemSkill[] particleSystemSkills;//スキルのエフェクト設定
    [SerializeField] Afterimage[] AfterimageObjects;//残像のゲームオブジェクト
    [SerializeField] ParticleSystem particleSystemSpecial4;//特殊エフェクト
    [SerializeField] AttackCollisionValue AttackCollisionValueSpecial4;//特殊当たり判定
    Vector2 vector2;//一時利用
    IEnumerator enumerator;//無敵時間処理
    readonly float armorTime = 1.5f; //起き上がりアーマー時間
    public bool Skill(int i)
    {
        if (skill) return false;//別行動中
        if (skillSets[i].activeHprate < GetHpRate()) return false;//使用可能HP
        if (skillSets[i].coolCount > 0f) return false;//クールタイム
        float distance = Mathf.Abs(GameManager.playerManager.transform.position.x - transform.position.x);
        if (distance > skillSets[i].activeArea) return false;//使用可能距離
        skill = true;
        animator.SetTrigger(skillHash);
        animator.SetInteger(skillIdHash, i);
        animator.SetFloat(distanceHash, distance);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetSkill(skillHash, skillIdHash, i, distanceHash, distance);
        }
        return true;
    }
    //クリア
    protected override void Death()
    {
        GameManager.gameManager.Clear();
    }
    //ダメージによるアニメーションのブレンド用
    protected override void SetDamage()
    {
        animator.SetFloat(hprateHash, 0f);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetHprate(hprateHash, 0f);
        }
    }
    //倒されたときヒットストップ
    protected override void DeathHitStop()
    {
        GameManager.playerManager.HitStop(0.6f);
    }
    public override void Update0()
    {
        for (int i = 0; i < skillSets.Length; i++)
        {
            skillSets[i].coolCount -= Time.deltaTime;
        }
        base.Update0(); 
        if (skill) return;
        for (int i = skillSets.Length - 1; i >= 0; i--)
        {
            Skill(i);
        }
    }
    //起き上がり処理
    protected override void ReMove()
    {
        base.ReMove();
        condition = Afterimage.Condition.ArmorPlus;
        if (enumerator != null) StopCoroutine(enumerator);
        enumerator = Armor(armorTime);
        StartCoroutine(enumerator);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetCondition(condition, armorTime);
        }
    }
    //攻撃処理
    public override void Attack(int currectAnime)
    {
        if (skill) return;
        if (currectAnime == idleHash || currectAnime == moveHash)
        {
            //近距離攻撃
            if (Mathf.Abs(GameManager.playerManager.transform.position.x - transform.position.x) <= attackArea2)
            {
                if (attackCount > 0f)
                {
                }
                else
                {
                    animator.SetTrigger(attackkinHash);
                    attackCount = attackSpeed; 
                    foreach (var afterimage in afterimages)
                    {
                        afterimage.SetTrigger(attackkinHash);
                    }
                }
            }
            else
            {
                base.Attack(currectAnime);
            }
        }
    }
    //初期化
    public override void Start0(Transform _transform)
    {
        base.Start0(_transform);
        GameManager.boss = this;
        //残像のセット
        afterimages = new Afterimage[AfterimageObjects.Length];
        StartCoroutine(AfterimageInit());
        transform.position += Vector3.right * 20f;
        animator.SetTrigger("first");
        GameManager.cameraManager.Zoom(transform);
    }
    IEnumerator AfterimageInit()
    {
        afterimages[0] = Instantiate(AfterimageObjects[0]);
        afterimages[0].Init(this);
        //yield return new WaitForSeconds(0.1f);
        afterimages[1] = Instantiate(AfterimageObjects[1]);
        afterimages[1].Init(this);
        //yield return new WaitForSeconds(0.1f);
        afterimages[2] = Instantiate(AfterimageObjects[2]);
        afterimages[2].Init(this);
        yield return null;
    }
    //無敵
    IEnumerator Invincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
        enumerator = null;
    }
    //ハイパーアーマー
    IEnumerator Armor(float time)
    {
        armorbool = true;
        yield return new WaitForSeconds(time);
        armorbool = false;
        enumerator = null;
    }
    //アニメーション開始時
    public void AnimationStart(int hash, int skillId, float cooltime, Afterimage.Condition condition, float invincibleTime, Animator animator)
    {
        if(this.animator == animator)
        {
            if (skillId >= 0 && skillId < skillSets.Length)
            {
                skillSets[skillId].coolCount = cooltime; //クールタイムのセット
                ParticlePlaySkill(skillId);//エフェクトのセット
            }
            //無敵時間指定なら
            if (condition == Afterimage.Condition.InvinciblePlus)
            {
                if(enumerator != null) StopCoroutine(enumerator);
                enumerator = Invincible(invincibleTime);
                StartCoroutine(enumerator);
            }
            else if (condition == Afterimage.Condition.ArmorPlus)
            {
                if (enumerator != null) StopCoroutine(enumerator);
                enumerator = Armor(invincibleTime);
                StartCoroutine(enumerator);
            }
            this.condition = condition;
            //行動制限
            skill = hash != idleHash && hash != moveHash;
        }
        //残像の状態のセット
        foreach (var afterimage in afterimages)
        {
            if (afterimage.animator == animator) afterimage.SetCondition(condition, invincibleTime);
        }
    }
    //固有スキル
    void Special(int skillId)
    {
        if(skillId == 4)
        {
            StartCoroutine(Special4());
        }
    }
    IEnumerator Special4()
    {
        yield return new WaitForSeconds(2f);
        for(int i = 0; i < (GetHpRate() < 0.1f ? 20 :10); i++)
        {
            (float left, float right) = GameManager.gameManager.LeftRight();
            (float top, _) = GameManager.gameManager.TopDown();
            vector2.x = Random.Range(left, right);
            vector2.y = top + 2f;
            particleSystemSpecial4.transform.position = vector2;
            particleSystemSpecial4.Play();
            GameManager.attackCollisionManagerBoss.SetCollision(AttackCollisionValueSpecial4, vector2, null);
            yield return new WaitForSeconds(0.2f);
        }
    }
    //スキルエフェクトのセット
    public virtual void ParticlePlaySkill(int num)
    {
        foreach (var particleSystem in particleSystemSkills[num].particleDelays)
        {
            particleSystem.enumerator = SkillParticlePlay(particleSystem, num);
            StartCoroutine(particleSystem.enumerator);
        }
    }
    //スキルエフェクトのディレイ
    protected virtual IEnumerator SkillParticlePlay(ParticleDelay particleDelay, int num)
    {
        yield return new WaitForSeconds(particleDelay.delay);
        particleDelay.particleSystem.Play();
        Special(num);
    }
    //スキルエフェクトの停止
    public void ParticlesStopSkill(int num)
    {
        if (num < 0 || particleSystemSkills.Length <= num) return;
        foreach (var particleSystem in particleSystemSkills[num].particleDelays)
        {
            if(particleSystem.enumerator != null) StopCoroutine(particleSystem.enumerator);
            if (particleSystem.dependence && particleSystem.particleSystem.IsAlive()) particleSystem.particleSystem.Stop();
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.boss = null; 
        foreach(var afterimage in afterimages)
        {
            Destroy(afterimage.gameObject);
        }
    }
    [System.Serializable]
    class SkillSet 
    {
        public float coolCount;//クールタイムのカウント
        public float activeArea;//攻撃開始範囲
        public float activeHprate = 1f;//攻撃可能HP
    }
}
