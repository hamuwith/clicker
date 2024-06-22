using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerManagerSub : PlayerManager
{
    [SerializeField] ParticleSystem fireEnd;//通常攻撃のエンドエフェクト
    [SerializeField] Material material;//スプライトのマテリアル
    [SerializeField] Ice ice;//アイススキル
    [SerializeField] Poison poison;//毒スキル
    public ParticleSystem resuscitationParticle;//蘇生エフェクト
    Tweener tweener;//移動用
    IceParameter iceParameter;//アイススキルの管理用
    readonly int timeHash = Shader.PropertyToID("_time");
    readonly int vector3Hash = Shader.PropertyToID("_Vector3");
    [SerializeField] GameObject[] slashSkillParticleBuffChildren;//通常攻撃バフオブジェクト
    int fireCount;//通常攻撃バフカウント
    [SerializeField] Transform transformPoisonEffect;//毒スキルエフェクト座標
    public GameObject castObject;//キャスト表示
    //初期化
    public override void Start0()
    {
        scale = 1f;
        base.Start0();
        attackAction = playerInput.actions.FindAction("AttackSub");
        for (int i = 0; i < 6; i++)
        {
            skillAction[i] = playerInput.actions.FindAction($"SkillSub{i}");
        }
        iceParameter = new IceParameter();
        material.SetFloat(timeHash, 1f);
        vector3 = transform.position;
        character = GameManager.gameManager.characterSub;
        scaleSkillParticle = particleSystemSkills[1].particleDelays[0].particleSystem;
        skill = true;
        if(castObject.activeSelf) castObject.SetActive(false);
    }
    //進化スキルクリック処理
    
    protected override void EvoSkill()
    {
        if (skillId == 2 && skillClick)
        {
            float f = Random.Range(0f, 2f * Mathf.PI); 
            vector2.x = Random.Range(0f, 1f) * Mathf.Cos(f) + 2f;
            vector2.y = Random.Range(0f, 1.5f) * Mathf.Sin(f) + 2.5f;
            if (left) vector2 = -vector2;
            GameManager.poisonManager.SetParticle((Vector2)transform.position + vector2, left);
            SkillEffectPlay(2, transformPoisonEffect);
        }
    }
    //進化スケールスキルスケール処理
    public override void LastParticleScaleAdd(float plus)
    {
        scaleSkillParticle.transform.localScale += Vector3.one * plus;
        var velocityOverLifetime = scaleSkillParticle.velocityOverLifetime;
        velocityOverLifetime.speedModifierMultiplier = 1f / scaleSkillParticle.transform.localScale.x;
    }
    //通常攻撃エフェクト位置
    protected override void SlashParticlePlay(int num)
    {
        (vector2.x, vector2.y) = SetParticlSub(num);
        if (left) vector2.x = -vector2.x;
        GameManager.singleHitManager.SetParticle((Vector2)transform.position + vector2, left);
    }
    //通常攻撃エフェクト位置データ
    protected (float positionX, float positionY) SetParticlSub(int num)
    {
        if (num == 0) return (4.5f, 2.6f);
        else if (num == 1) return (2.62f, 1.98f);
        else if (num == 2) return (2.77f, 2.9f);
        else return (2.67f, 2.25f);
    }
    protected override void EvoSkill(ref int i)
    {

    }
    //通常攻撃エンドエフェクト
    public void FireParticlePlay(Transform _transform)
    {
        fireEnd.transform.position = _transform.position;
        fireEnd.Play();
    }
    //スキルエフェクト
    public override void ParticlePlaySkill(int num)
    {
        if (num == 2)
        {
            particleSystemSkills[num].particleDelays[0].enumerator = PoisonParticlePlay(particleSystemSkills[num].particleDelays[0]);
            StartCoroutine(particleSystemSkills[num].particleDelays[0].enumerator);
            particleSystemSkills[num].particleDelays[1].particleSystem.Play();
            return;
        }
        base.ParticlePlaySkill(num);
    }
    //毒スキルエフェクト
    IEnumerator PoisonParticlePlay(ParticleDelay particleDelay)
    {
        yield return new WaitForSeconds(particleDelay.delay);
        poison.Set();
    }
    //毒スキルエフェクト終わり
    public void Poison(AttackCollision attackCollision, AttackCollisionValue attackCollisionValue)
    {
        poison.Stop(attackCollision.transform, attackCollisionValue);
        attackCollision.End(true);
    }
    //特殊スキルエフェクト
    protected override void SpecialSkillParticlePlay(int num)
    {
        if(num == 0)
        {
            StartCoroutine(IcePlay());
        }
    }
    //進化スキルエフェクト
    protected override void SetAddEffect(ref ParticleSystem.MainModule main, int skillIdSkillParticle, Transform transformSkillParticle, float size, float offsetX)
    {
        main.simulationSpace = ParticleSystemSimulationSpace.Custom;
        main.startSize3D = false;
        if (skillIdSkillParticle == 0)
        {
            main.startSize = size * 8f;
            skillEffect.transform.position = transformSkillParticle.position + new Vector3(left ? 1 : -1, 0.4f, 0f);
        }
        else if (skillIdSkillParticle == 1)
        {
            main.startSize = size * 4f;
            skillEffect.transform.position = transformSkillParticle.position + (left ? Vector3.right : Vector3.left) * 0.6f;
        }
        else if (skillIdSkillParticle == 2)
        {
            main.startSize = size * 4f;
            skillEffect.transform.position = transformSkillParticle.position;
        }
        else
        {
            main.startSize = size * 12f;
            skillEffect.transform.position = transformSkillParticle.position;
        }
    }
    //特殊スキル
    protected override void SetSkillSpecial(int num)
    {
        if (num == 4)
        {
            animator.SetBool(atkbuffHash, true);
            atkBuff = atkBuffTime;
            slashSkillParticleBuff.gameObject.SetActive(true);
            foreach (var slashSkillParticleBuffChild in slashSkillParticleBuffChildren)
            {
                if(!slashSkillParticleBuffChild.activeSelf == GameManager.gameManager.GetEvo(this, num)) slashSkillParticleBuffChild.SetActive(GameManager.gameManager.GetEvo(this, num));
            }
        }
    }
    //プレイヤーをアクティブにした時の処理
    public override void SetAvtive(bool b)
    {
        base.SetAvtive(b);
        transform.position = GameManager.playerManager.transform.position + (GameManager.playerManager.left ? Vector3.right : Vector3.left) * 100f;
    }
    //蘇生処理
    public override void Resuscitation()
    {
        if (GameManager.playerManager.death)
        {
            GameManager.popTextManager.SetPopText("今、助けるから！！", this, anotherSpeechBubbleColor, duration:1f);
            animator.SetTrigger(resuscitationHash);
            skill = true;
            Debug.Log("inin");
            GameManager.playerManager.ResuscitationStart();
        }
    }
    //召喚数の取得
    public bool GetSummon()
    {
        fireCount++;
        return slashSkillParticleBuffChildren[0].activeSelf || fireCount < 2;
    }
    //アイススキル
    public IEnumerator IcePlay()
    {
        ice.Play(); 
        yield return new WaitWhile(() => particleSystemSkills[0].particleDelays[0].particleSystem.IsAlive());
        ice.Stop();
    }
    //アイス攻撃対象セット
    public void Ice(Enemy enemy, AttackCollisionValue attackCollisionValue, Collider2D collision)
    {
        float mag = (enemy.transform.position - transform.position).sqrMagnitude;
        if (iceParameter.sqrMag > mag)
        {
            iceParameter.targetIce = enemy;
            iceParameter.sqrMag = mag;
            iceParameter.iceAttackCollisionValue = attackCollisionValue;
            iceParameter.collider2D = collision;
        }
    }
    //アイスの初期化
    public void IceInit()
    {
        iceParameter.Start();
    }
    //進化アイススキルクリックセット
    public bool SetIceClick(bool b)
    {
        iceParameter.click = b;
        return b;
    }
    //アイスのダメージ処理
    public void FixedUpdate0()
    {
        if(iceParameter.sqrMag == Mathf.Infinity)
        {
            ice.SetSpriteSize(null, left); 
            return;
        }
        else
        {
            ice.SetSpriteSize(iceParameter.targetIce?.transform, left);
            iceParameter.targetIce.Damage(iceParameter.iceAttackCollisionValue, default, iceParameter.collider2D);
            if (iceParameter.clickDamage) SkillEffectPlay(1, ice.transform);
        }
    }
    //各アニメーションの開始処理
    public override void AnimationStart(Animator animator, Afterimage.Condition condition, int hash, float invincibleTime, int skillId, float clickDelay, float clickDuration)
    {
        Move();
        fireCount = 0;
        this.skillId = skillId;
        if (skillId >= 0)
        {
            coolCounts[skillId] = GameManager.gameManager.GetCoolTime(skillId, true); 
            ParticlePlaySkill(skillId);
            GameManager.gameManager.SetCutIn(skillId, this);
            //SetText(skillId, addSkill);
        }
        if (clickDuration > 0f && GameManager.gameManager.GetEvo(this, skillId))
        {
            Debug.Log("in");
            if (enumerator != null) StopCoroutine(enumerator);
            enumerator = SkillClick(clickDuration, clickDelay);
            StartCoroutine(enumerator);
        }
        if (hash == idleHash)
        {
            skill = false;
        }
        materialClick = GetCanAttack();
    }
    //プレイヤーと離れていたら瞬間移動
    public void Move()
    {
        left = GameManager.boss?.transform.position.x < transform.position.x;
        transform.localScale = (left ? hanten : Vector2.one) * scale;
        if (!left && (GameManager.playerManager.transform.position.x - transform.position.x > 8.5f || GameManager.playerManager.transform.position.x - transform.position.x < 1f))
        {
            tweener?.Kill();
            vector3.x = GameManager.playerManager.transform.position.x - 4f;
            material.SetVector(vector3Hash, transform.position - vector3);
            transform.position = vector3;
            tweener = DOTween.To(() => 0f, (x) => material.SetFloat(timeHash, x), 1f, 0.4f);
        }
        else if (left && (GameManager.playerManager.transform.position.x - transform.position.x < -8.5f || GameManager.playerManager.transform.position.x - transform.position.x > -1f))
        {
            tweener?.Kill();
            vector3.x = GameManager.playerManager.transform.position.x + 4f;
            material.SetVector(vector3Hash, transform.position - vector3);
            transform.position = vector3;
            tweener = DOTween.To(() => 0f, (x) => material.SetFloat(timeHash, x), 1f, 0.4f);
        }
    }
    //アイスデータ
    class IceParameter
    {
        public float sqrMag;
        public Enemy targetIce;
        public AttackCollisionValue iceAttackCollisionValue;
        public bool click;
        public bool clickDamage;
        public Collider2D collider2D;
        public IceParameter()
        {
            sqrMag = Mathf.Infinity;
        }
        public void Start()
        {
            sqrMag = Mathf.Infinity;
            clickDamage = click;
            click = false;
        }
    }
}
