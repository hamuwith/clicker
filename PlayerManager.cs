using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class PlayerManager : MonoBehaviour
{
    public Animator animator { get; private set; }
    [SerializeField] ParticleSystem slashParticle;//通常攻撃エフェクト
    [SerializeField] ParticleSystem slashSkillParticle;//通常攻撃スキルエフェクト
    [SerializeField] ParticleSystem slashSkillParticle3;//通常攻撃スキルエフェクト3
    [SerializeField] protected ParticleSystem slashSkillParticleBuff;//通常攻撃スキル発動中エフェクト
    [SerializeField] protected ParticleSystemSkill[] particleSystemSkills;//スキルエフェクト
    [SerializeField] ParticleSystem guardParticle;//ガードエフェクト
    protected Vector2 vector2;//一時利用
    public readonly int attackHash = Animator.StringToHash("attack");
    public readonly int dashHash = Animator.StringToHash("dash");
    public readonly int skillHash = Animator.StringToHash("skill");
    public readonly int skillIdHash = Animator.StringToHash("skillId");
    public readonly int atkbuffHash = Animator.StringToHash("atkbuff");
    public readonly int knockHash = Animator.StringToHash("knock");
    public readonly int knockEndHash = Animator.StringToHash("knockEnd");
    public readonly int downHash = Animator.StringToHash("down");
    public readonly int downEndHash = Animator.StringToHash("downEnd");
    public readonly int idleHash = Animator.StringToHash("idle");
    public readonly int idleStartHash = Animator.StringToHash("idleStart");
    public readonly int cancelHash = Animator.StringToHash("cancel");
    public readonly int guardHash = Animator.StringToHash("guard");
    public readonly int resuscitationHash = Animator.StringToHash("resuscitation");
    public readonly int castHash = Animator.StringToHash("cast");
    public readonly int deathHash = Animator.StringToHash("death");
    float hitStopCount;//ヒットストップ時間カウント
    protected float atkBuffTime;//通常攻撃バフ時間
    protected float atkBuff;//通常攻撃バフ時間カウント
    public Thunder thunder;//雷スキル
    protected InputAction attackAction;//デバッグ用
    protected InputAction[] skillAction;//デバッグ用
    [SerializeField] protected PlayerInput playerInput;//InputSystem
    [SerializeField] Afterimage[] afterimages;//残像
    [SerializeField] float maxhp;//最大HP
    public float hp { get; private set; }//現在のHP
    float down;//ダウン値
    Tweener tweenerX;//ダウンとノックバック
    Tweener tweenerY;//ダウンとノックバック
    float holdout0;//ノックバック時間
    float holdout
    {
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
    protected Vector3 vector3;//一時利用
    Afterimage.Condition condition;//状態
    public bool skill { get; protected set; }//スキル可能かどうか
    bool guard;//ガード成功
    bool guardAcceptable;//ガード可能かどうか
    bool invincible;//無敵かどうか
    bool avoidance;//回避かどうか
    protected float[] coolCounts;//クールタイムカウント
    protected GameManager.Character character;//ステータス
    float autoClickCount;//自動クリックカウント
    [SerializeField] protected Material mouse;//クリック可能かどうか表示用マテリアル
    [SerializeField] protected Material mouseClick;//クリック入力表示マテリアル
    [SerializeField] SpriteRenderer spriteRenderer;//クリック入力表示
    bool canAttack;//攻撃入力可能か
    bool skillClick0;
    protected IEnumerator enumerator;//進化スキルクリック停止用
    int materialClick0;
    int materialSkill0;
    protected int skillId;//使用スキルID
    protected ParticleSystem scaleSkillParticle;//スケールスキルエフェクト
    protected ParticleSystem scaleSkillParticle2;//スケールスキルエフェクト
    [SerializeField] ParticleSystemRenderer particleSystemRenderer;//進化スキルクリック時各スキルエフェクトのレンダラー
    [SerializeField] Material[] addMaterials;//進化スキルクリック時各スキルエフェクトマテリアル
    Vector3 vector3ScaleSkillParticle;//進化スケールスキルクリック時スケール
    [SerializeField] protected ParticleSystem skillEffect;//進化スキルクリック時各スキルエフェクト
    [SerializeField] ParticleSystem skillEffect1;//進化スキルクリック時マウスエフェクト
    public Gradient[] skillGradient;//スキルのモチーフカラー
    [SerializeField] Transform transformOnigiriEffect;//進化スキルの追加エフェクト
    [SerializeField] AttackCollisionValue attackCollisionValueOnigiriEvo;//進化スキルの追加ダメージ
    [SerializeField] Transform transformIaiEffect;//進化スキルの追加エフェクト
    [SerializeField] AttackCollisionValue attackCollisionValueIaiEvo;//進化スキルの追加ダメージ
    [SerializeField] ParticleSystem moonEffect;//進化スキルの追加エフェクト
    [SerializeField] AttackCollisionValue attackCollisionValueMoonEvo;//進化スキルの追加ダメージ
    readonly Vector3 offsetSkill2 = new Vector3(8f, 2.7f, 0f);//進化スキルの位置
    readonly public Vector2 inversionVector2 = new Vector2(-1f, 1f);
    public float scale = 3f;//デフォルトスケール
    public Transform popTextTransform;//吹き出しの座標
    [SerializeField] protected Color autoAttackColor;//通常攻撃の吹き出しの色
    [SerializeField] protected Color skillSpeechBubbleColor;//スキルの吹き出しの色
    [SerializeField] protected Color damageSpeechBubbleColor;//ダメージを受けたときの吹き出しの色
    [SerializeField] protected Color anotherSpeechBubbleColor;//それ以外の吹き出しの色
    public bool death;//倒れたか
    public float distance;//進んだ距離
    public float prePositionX;//進んだ距離カウント用
    [SerializeField] CapsuleCollider2D capsuleCollider2;//自身のコライダー
    PopText popText0;//現在の吹き出し
    [SerializeField] int tutorialId;
    public PopText popTextEvo { 
        get 
        {
            return popText0;
        }
        set 
        {
            if (popText0 != null) popText0.End();
            popText0 = value;
        }
    }
    public enum SpeechBubbleColor
    {
        Auto,
        Skill,
        Another
    }
    public bool materialClick
    {
        get
        {
            return materialClick0 == 1;
        }
        set
        {
            materialClick0 = value ? 1: 0;
            spriteRenderer.enabled = value;
            mouseClick.SetInt(GameManager.gameManager.clickHash, materialClick0);
        }
    }
    public bool materialSkill
    {
        get
        {
            return materialSkill0 == 1;
        }
        set
        {
            materialSkill0 = value ? 1 : 0;
            spriteRenderer.enabled = value;
            mouse.SetInt(GameManager.gameManager.skillHash, materialSkill0);
            mouseClick.SetInt(GameManager.gameManager.clickHash, materialSkill0);
        }
    }
    public bool left { get; protected set; }//プレイヤーの向き
    public bool skillClick //進化スキルのクリック
    {
        get
        {
            bool skillClick = skillClick0;
            skillClick0 = false;
            return skillClick;
        }
        set
        {
            if (canSkillClick && value)
            {
                skillClick0 = true;
                Click();
                popTextEvo?.UpdateText();
            }
        }
    }
    bool canSkillClick;//進化スキルクリック可能か
    Tweener tweener;//マウスクリック表示エフェクト
    public enum State
    {
        ATTACK,
        DASH,
        SKILL,
        NULL
    }
    //初期化
    public virtual void Start0()
    {
        animator = GetComponent<Animator>();
        prePositionX = transform.position.x;
        hitStopCount = 0f;
        atkBuff = 0f;
        atkBuffTime = 10f;
        skillAction = new InputAction[6];
        attackAction = playerInput.actions.FindAction("Attack");
        for (int i = 0; i < 6; i++)
        {
            skillAction[i] = playerInput.actions.FindAction($"Skill{i}");
        }
        foreach(var afterimage in afterimages)
        {
            afterimage.Init(this);
        }
        holdout = 0f;
        coolCounts = new float[6];
        character = GameManager.gameManager.character;
        materialSkill = false;
        scaleSkillParticle = particleSystemSkills[1].particleDelays[0].particleSystem;
        vector3ScaleSkillParticle = scaleSkillParticle.transform.localPosition;
        if (particleSystemSkills[1].particleDelays.Length > 1) scaleSkillParticle2 = particleSystemSkills[1].particleDelays[1].particleSystem;
        hp = maxhp;
    }
    //吹き出しの色の取得
    public Color GetSpeechBubbleColor(SpeechBubbleColor speechBubbleColor)
    {
        if (speechBubbleColor == SpeechBubbleColor.Auto) return autoAttackColor;
        else if (speechBubbleColor == SpeechBubbleColor.Skill) return skillSpeechBubbleColor;
        else return anotherSpeechBubbleColor;
    }
    //キャスト時間のセット
    public void SetCastSpeed(float perSpeed)
    {
        animator.SetFloat(castHash, 100 / (100 - perSpeed));
    }
    //プレイヤーをアクティブにした時の処理
    public virtual void SetAvtive(bool b)
    {
        skill = !b;
    }
    public void Update0()
    {
        distance += Mathf.Max(0f, transform.position.x - prePositionX);
        prePositionX = transform.position.x;
        if (hitStopCount > 0f)
        {
            hitStopCount -= Time.unscaledDeltaTime;
            if (hitStopCount <= 0f)
            {
                Time.timeScale = 1f;
            }
        }
        for(int i = 0; i < coolCounts.Length; i++)
        {
            coolCounts[i] -= Time.deltaTime;
            GameManager.gameManager.SetButton(this, i , coolCounts[i], skill);
        }
        GameManager.gameManager.SetBackstepColor(skill);
        if (atkBuff > 0)
        {
            atkBuff -= Time.deltaTime;
            if (atkBuff <= 0)
            {
                animator.SetBool(atkbuffHash, false);
                foreach (var afterimage in afterimages)
                {
                    StartCoroutine(afterimage.SetBool(atkbuffHash, false));
                }
                slashSkillParticleBuff.gameObject.SetActive(false);
            }
        }
        float autoClickTime = GameManager.gameManager.AutoClickTime(this);
        if (autoClickTime > 0) 
        {
            autoClickCount += Time.deltaTime;
            if(autoClickCount >= autoClickTime)
            {
                autoClickCount -= autoClickTime;
                Attack(true);
            }
        }
        EvoSkill();
        if (skill) return;
        SetRight(2.5f);
        transform.localScale = (left ? inversionVector2 : Vector2.one) * scale;
        popTextTransform.localScale = (left ? inversionVector2 : Vector2.one) / scale;
        for (int i = character.skills.Length - 1; i >= 0; i--)
        {
            if (character.skills[i].auto)
            {
                Skill(GameManager.gameManager.ChangeNumOut(i, this));
            }
        }
    }
    //向きの設定
    public void SetRight(float offset)
    {
        if (offset <= GameManager.boss?.transform.position.x - transform.position.x)
        {
            left = false;
        }
        else if (-offset > GameManager.boss?.transform.position.x - transform.position.x)
        {
            left = true;
        }
    }
    public virtual void Resuscitation()
    {

    }
    //蘇生アニメーションの開始
    public void ResuscitationStart()
    {
        animator.SetTrigger(resuscitationHash);
        skill = true;
        death = false;
        foreach (var afterimage in afterimages)
        {
            afterimage.SetTrigger(resuscitationHash);
        }
    }
    //プレイヤーの重心の取得
    public Vector3 GetPlayerCenter()
    {
        return transform.position + 2.5f * Vector3.up;
    }
    //蘇生完了
    public void ResuscitationEnd()
    {
        capsuleCollider2.enabled = true;
        skill = false;
        StartCoroutine(Invincible(1f));
        foreach (var afterimage in afterimages)
        {
            afterimage.SetCondition(Afterimage.Condition.InvinciblePlus, 1f);
        }
    }
    //蘇生完了(アニメーションより使用)
    void ResuscitationComplete()
    {
        hp = maxhp;
    }
    //進化スキルクリック時
    protected virtual void EvoSkill()
    {
        if (skillId == 6 && skillClick)
        {
            SkillEffectPlay(0, transformOnigiriEffect); 
            GameManager.attackCollisionManager.SetCollision(attackCollisionValueOnigiriEvo, transformOnigiriEffect.position, null);
        }
        else if (skillId == 2 && skillClick)
        {
            float rotation = UnityEngine.Random.Range(-Mathf.PI / 6, Mathf.PI / 6);
            float offsetX = UnityEngine.Random.Range(-1.5f, 1.5f);
            SkillEffectPlay(1, transformIaiEffect, rotation: rotation, offsetX : offsetX);
            attackCollisionValueIaiEvo.rotation = rotation * 180f / Mathf.PI;
            GameManager.attackCollisionManager.SetCollision(attackCollisionValueIaiEvo, transformIaiEffect.position + offsetX * Vector3.right, null);
        }
        else if (skillId == 3 && skillClick)
        {
            SetStar(transform.position);
        }
    }
    //進化スキルクリックエフェクト
    public void SkillEffectPlayClick(int skillIdSkillParticle, Transform transformSkillParticle, float offsetX)
    {
        var color = skillEffect1.colorOverLifetime;
        color.color = skillGradient[skillIdSkillParticle];
        var shape = skillEffect1.shape;
        shape.position = GameManager.gameManager.mousePoint.position - transformSkillParticle.position + offsetX * Vector3.right;
        skillEffect1.transform.position = transformSkillParticle.position;
        skillEffect1.Play();
    }
    //進化スキルエフェクト
    public void SkillEffectPlay(int skillIdSkillParticle, Transform transformSkillParticle, float size = 1f, float rotation = 0f, float offsetX = 0f)
    {
        SkillEffectPlayClick(skillIdSkillParticle, transformSkillParticle, offsetX);
        var main = skillEffect.main;
        main.customSimulationSpace = transformSkillParticle;
        main.startRotation = rotation;
        SetAddEffect(ref main, skillIdSkillParticle, transformSkillParticle, size, offsetX);
        particleSystemRenderer.sharedMaterial = addMaterials[skillIdSkillParticle];
        skillEffect.Play(); 
    }
    //進化スキルエフェクト
    protected virtual void SetAddEffect(ref ParticleSystem.MainModule main, int skillIdSkillParticle, Transform transformSkillParticle, float size, float offsetX)
    {
        main.simulationSpace = ParticleSystemSimulationSpace.Custom;
        main.startSize3D = false;
        if (skillIdSkillParticle == 0)
        {
            main.startSize = size * 13.5f;
            skillEffect.transform.position = transformSkillParticle.position;
        }
        else if(skillIdSkillParticle == 1)
        {
            main.startSize3D = true;
            main.startSizeX = size * 90f;
            main.startSizeY = size * 22.5f;
            skillEffect.transform.position = transformSkillParticle.position + offsetX * Vector3.right;
        }
        else if (skillIdSkillParticle == 2)
        {
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSize = size * 5f;
            skillEffect.transform.position = transformSkillParticle.position;
        }
        else if (skillIdSkillParticle == 3)
        {
            main.startRotation = UnityEngine.Random.Range(0f, Mathf.PI * 2);
            main.startSize = size * 8f;
            skillEffect.transform.position = transformSkillParticle.position;
        }
    }
    //スキル
    public bool Skill(int i)
    {
        if (skill) return false;
        if (coolCounts[i] > 0f) return false;
        if (Time.timeScale <= 0f) return false;
        skill = true;
        EvoSkill(ref i);
        GameManager.tutorialManager?.SetActive(1, false);
        GameManager.tutorialManager?.SetActive(3, true);
        animator.SetTrigger(skillHash);
        animator.SetInteger(skillIdHash, i);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetSkill(skillHash, skillIdHash, i);
        }
        SetSkillSpecial(i);
        return true;
    }
    //バックステップ
    public bool BackStep()
    {
        if (skill) return false;
        skill = true;
        GameManager.tutorialManager?.SetActive(3, false);
        animator.SetTrigger(skillHash);
        animator.SetInteger(skillIdHash, -2);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetSkill(skillHash, skillIdHash, -2);
        }
        return true;
    }
    //進化スキル
    protected virtual void EvoSkill(ref int i)
    {
        if(i == 0 && GameManager.gameManager.GetEvo(this, 0))
        {
            i = 6;
        }
    }
    //クリック時
    public void Attack(bool can)
    {
        skillClick = true;
        if (!can) return;
        if (!GetCanAttack()) return;
        //clickParticle.Play();
        Click();
        if (guardAcceptable)
        {
            guard = true;
            animator.SetTrigger(guardHash);
            foreach (var afterimage in afterimages)
            {
                afterimage.SetTrigger(guardHash);
            }
        }
        else
        {
            GameManager.tutorialManager?.SetActive(tutorialId, false);
            animator.SetTrigger(attackHash);
            foreach (var afterimage in afterimages)
            {
                afterimage.SetTrigger(attackHash);
            }
        }
    }
    //クリックエフェクト
    void Click()
    {
        tweener?.Complete();
        tweener = DOTween.To(() => 1f, (x) => mouseClick.SetFloat(GameManager.gameManager.scaleHash, x), 0.9f, 0.1f)
            .SetLoops(2, LoopType.Yoyo);
    }
    //特殊スキル処理
    protected virtual void SetSkillSpecial(int num)
    {
        if (num == 5)
        {
            animator.SetBool(atkbuffHash, true);
            atkBuff = atkBuffTime;
            slashSkillParticleBuff.gameObject.SetActive(true);
            foreach (var afterimage in afterimages)
            {
                StartCoroutine(afterimage.SetBool(atkbuffHash, true));
            }
        }
        else if (num == 2)
        {
            vector3 = offsetSkill2;
            if (left) vector3.x = -vector3.x;
            transformIaiEffect.position = transform.position + vector3;
        }
    }
    //進化スキル処理
    void SetStar(Vector3 vector3)
    {
        float add = UnityEngine.Random.Range(Mathf.PI / 2, Mathf.PI);
        float addX = UnityEngine.Random.Range(6f, 10f);
        float addY = addX * Mathf.Sin(add);
        addX = addX * Mathf.Cos(add);
        moonEffect.transform.position = vector3 + new Vector3(addX, addY, 0f);
        SkillEffectPlay(2, moonEffect.transform);
        moonEffect.transform.localScale = left ? inversionVector2 : Vector2.one;
        moonEffect.Play();
        GameManager.attackCollisionManager.SetCollision(attackCollisionValueMoonEvo, vector3 + new Vector3(addX, addY, 0f), null);
    }
    //各アニメーション開始時
    public virtual void AnimationStart(Animator animator, Afterimage.Condition condition, int hash, float invincibleTime, int skillId, float clickDelay, float clickDuration)
    {
        if (this.animator == animator)
        {
            this.skillId = skillId;
            if (skillId >= 0)
            {
                if (skillId < coolCounts.Length)
                {
                    coolCounts[skillId] = GameManager.gameManager.GetCoolTime(skillId, false);
                    GameManager.gameManager.SetCutIn(skillId, this);
                }
                ParticlePlaySkill(skillId);
                //SetText(skillId, addSkill);
            }
            if (clickDuration > 0f && (GameManager.gameManager.GetEvo(this, skillId) || skillId == 6))
            {
                if (enumerator != null) StopCoroutine(enumerator);
                enumerator = SkillClick(clickDuration, clickDelay);
                StartCoroutine(enumerator);
            }
        }
        foreach (var afterimage in afterimages)
        {
            if (afterimage.animator == animator) afterimage.SetCondition(condition, invincibleTime);
        }
        if (this.animator == animator)
        {
            if (condition == Afterimage.Condition.InvinciblePlus)
            {
                StartCoroutine(Invincible(invincibleTime));
            }
            else if(condition == Afterimage.Condition.AvoidancePlus)
            {
                StartCoroutine(AvoidancePlus(invincibleTime));
            }
            this.condition = condition;
            if(hash == cancelHash)
            {
                ResetDamage();
                skill = false;
                down = 0f;
            }
            else if (hash == idleHash || hash == idleStartHash)
            {
                skill = false;
                down = 0f;
            }
            canAttack = hash == downEndHash || hash == knockHash;
            materialClick = GetCanAttack();
        }
    }
    //進化スキルクリック受付時間処理
    protected IEnumerator SkillClick(float time, float delay)
    {
        if(delay > 0f) yield return new WaitForSeconds(delay);
        canSkillClick = true;
        materialSkill = true;
        //mouse.SetInt(GameManager.gameManager.clickHash, 1);
        //mouse.SetInt(GameManager.gameManager.skillHash, 1);
        yield return new WaitForSeconds(time);
        canSkillClick = false;
        materialSkill = false;
        popTextEvo = null;
        //if (skillId == 1) SetScale();
        //mouse.SetInt(GameManager.gameManager.clickHash, 0);
        //mouse.SetInt(GameManager.gameManager.skillHash, 0);
    }
    //ガードとノックバックキャンセル可能取得
    public bool GetCanAttack()
    {
        return !skill || canAttack;
    }
    //無敵時間のセット
    IEnumerator Invincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }
    //回避時間のセット
    IEnumerator AvoidancePlus(float time)
    {
        avoidance = true;
        yield return Invincible(time);
        avoidance = false;
    }
    //通常攻撃エフェクト位置(アニメーションより使用)
    protected virtual void SlashParticlePlay(int num)
    {
        var set = SetParticle(num); 
        var main = slashParticle.main;
        main.startRotationX = set.rotationX;
        main.startRotationY = set.rotationY;
        main.startRotationZ = set.rotationZ;
        vector2.x = set.positionX;
        vector2.y = set.positionY;
        slashParticle.transform.localPosition = vector2;
        slashParticle.Play();
    }
    //アニメーションより使用
    void SlashSkillParticlePlay(int num)
    {
        var set = SetSkillParticle(num);
        var main1 = slashSkillParticle.main;
        main1.startRotationX = set.rotationX;
        main1.startRotationY = set.rotationY;
        main1.startRotationZ = set.rotationZ;
        vector2.x = set.positionX;
        vector2.y = set.positionY;
        slashSkillParticle.transform.localPosition = vector2;
        slashSkillParticle.Play();
        if (num == 3) slashSkillParticle3.Play();
    }
    //アニメーションより使用
    void GuardParticlePlay()
    {
        guardParticle.Play();
    }
    //スキルエフェクト
    public void ParticlePlaySkill(int num)
    {
        SpecialParticlePlaySkill(ref num);
        foreach (var particleSystem in particleSystemSkills[num].particleDelays)
        {
            if (particleSystem.particleSystem == scaleSkillParticle)
            {
                scaleSkillParticle.transform.localScale = Vector3.one;
                scaleSkillParticle.transform.localPosition = vector3ScaleSkillParticle;
            }
            particleSystem.enumerator = SkillParticlePlay(particleSystem, num);
            StartCoroutine(particleSystem.enumerator);
        }
    }
    //特殊スキルエフェクト
    protected virtual void SpecialParticlePlaySkill(ref int num)
    {
    }
    protected IEnumerator SkillParticlePlay(ParticleDelay particleDelay, int num)
    {
        yield return new WaitForSeconds(particleDelay.delay);
        if (!particleDelay.dependence) particleDelay.particleSystem.transform.localScale = left ? inversionVector2 : Vector2.one;
        particleDelay.particleSystem.Play();
        SpecialSkillParticlePlay(num);
    }
    public void ParticlesStopSkill(int num)
    {
        if (num < 0 || particleSystemSkills.Length <= num) return;
        foreach (var particleSystem in particleSystemSkills[num].particleDelays)
        {
            if(particleSystem.enumerator != null) StopCoroutine(particleSystem.enumerator);
            if(particleSystem.dependence && particleSystem.particleSystem.IsAlive()) particleSystem.particleSystem.Stop();
        }
    }
    //スケールスキル処理
    public virtual void LastParticleScaleAdd(float plus)
    {
        scaleSkillParticle.transform.localScale += Vector3.one * plus;
        vector3 = scaleSkillParticle.transform.localPosition;
        vector3.x -= 0.21f;
        vector3.y += 0.08f;
        scaleSkillParticle.transform.localPosition = vector3;
        scaleSkillParticle2.transform.localScale = scaleSkillParticle.transform.localScale;
    }
    //スケール取得
    public Vector3 GetScale()
    {
        return scaleSkillParticle2.transform.localScale;
    }
    protected virtual void SpecialSkillParticlePlay(int num)
    {

    }
    //通常攻撃のエフェクトのデータ
    protected virtual (float positionX, float positionY, float rotationX, float rotationY, float rotationZ) SetParticle(int num)
    {
        if (num == 0) return (0f, 1.06f, 0.58835f, -0.094f, -1.2486f);
        else if (num == 1) return (-0.4f, 1f, -0.967f, 0.653f, 1.864f);
        else if (num == 2) return (0f, 1.46f, 2.2375f, -3.197f, 2.5028f);
        else return (0.07f, 1.3f, 0.54786f, -3.4383f, 3.616f);
    }
    //通常バフ攻撃のエフェクトのデータ
    (float positionX, float positionY, float rotationX, float rotationY, float rotationZ) SetSkillParticle(int num)
    {
        if (num == 0) return (0f, 1.14f, -1.09f, 0.5f, -0.53f);
        else if (num == 1) return (-0.07f, 1.13f, -1.02f, 0.31f, 1.71f);
        else if (num == 2) return (0.68f, 1.15f, -0.75f, 0.925f, -0.74f);
        else return (0.15f, 1.1f, 0f, 2.61f, -2.3f);
    }
    //ヒットストップ
    public void HitStop(float time, float scale,bool zoom, in Vector2 worldPoint = default)
    {
        if (time <= hitStopCount) return;
        hitStopCount = time;
        if(zoom) GameManager.cameraManager.ZoomCamera(hitStopCount, worldPoint);
        Time.timeScale = scale;
    }
    //当たり判定処理
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("EnemyWeapon")) return;
        (AttackCollisionValue attackCollisionValue0, _)  = collision.gameObject.GetComponent<AttackCollision>().GetAttackCollisionValue();
        StartCoroutine(Damage(attackCollisionValue0, collision));
    }
    //ダメージを受けたときの処理
    public IEnumerator Damage(AttackCollisionValue attackCollisionValue, Collider2D collider2D)
    {
        if (condition != Afterimage.Condition.Null || invincible || avoidance)
        {
            foreach (var afterimage in afterimages)
            {
                afterimage.SetScale();
            }
        }
        if (avoidance)
        {
            HitStop(0.45f, 0.15f, true, collider2D.ClosestPoint(transform.position));
            GameManager.popTextManager.SetPopText("回避", this, anotherSpeechBubbleColor, PopTextManager.Kind.Avoidance);
            yield break;
        }
        else if (condition == Afterimage.Condition.Invincible || invincible)
        {
            GameManager.popTextManager.SetPopText("無敵", this, anotherSpeechBubbleColor, PopTextManager.Kind.Invincible);
            yield break;
        }
        else if (condition == Afterimage.Condition.Armor)
        {
            GameManager.popTextManager.SetPopText("アーマー", this, anotherSpeechBubbleColor, PopTextManager.Kind.Armor);
            hp -= attackCollisionValue.damageRate;
            GameManager.damageManager.SetDamage((int)attackCollisionValue.damageRate, transform, false);
            yield break;
        }
        if (tweenerX == null)
        {
            guardAcceptable = true;
            guard = false;
        }
        yield return new WaitForSeconds(0.02f);
        guardAcceptable = false;
        if (guard)
        {
            HitStop(0.45f, 0.15f, true, collider2D.ClosestPoint(transform.position));
            GameManager.popTextManager.SetPopText("ガード", this, anotherSpeechBubbleColor, PopTextManager.Kind.Guard);
            yield break;
        }
        down += attackCollisionValue.down;
        hp -= attackCollisionValue.damageRate;
        if (hp <= 0f) down += 1f;
        GameManager.damageManager.SetDamage((int)attackCollisionValue.damageRate, transform, false);
        if (down >= 1f)
        {
            var duration = attackCollisionValue.force.y / 12;
            GameManager.cameraManager.ShakeCamera(duration * 2f);
            ResetForce(true);
            animator.SetTrigger(downHash);
            skill = true;
            foreach (var afterimage in afterimages)
            {
                afterimage.SetTrigger(downHash);
            }
            vector3.x = transform.position.x + attackCollisionValue.force.x * (left ? -1 : 1);
            vector3.y = transform.position.y + attackCollisionValue.force.y;
            tweenerX = transform.DOMoveX(vector3.x, duration)
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    tweenerX = null;
                });
            tweenerY = transform.DOMoveY(vector3.y, duration / 2)
                .OnComplete(() =>
                {
                    Down();
                });
            GameManager.popTextManager.SetPopText("ぐはっ！", this, damageSpeechBubbleColor, PopTextManager.Kind.Another);
        }
        else
        {
            var duration = (Mathf.Abs(attackCollisionValue.force.x) + attackCollisionValue.force.y) / 24;
            if (duration <= 0f) yield break;
            GameManager.cameraManager.ShakeCamera(duration);
            ResetForce(duration);
            skill = true;
            vector3 = transform.position + attackCollisionValue.force / 5 * (left ? -1 : 1);
            tweenerX = transform.DOMoveX(vector3.x, holdout)
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    tweenerX = null;
                    holdout = 0f;
                    animator.SetTrigger(knockEndHash);
                    foreach(var afterimage in afterimages)
                    {
                        afterimage.SetTrigger(knockEndHash);
                    }
                });
            GameManager.popTextManager.SetPopText("うっ！", this, damageSpeechBubbleColor);
        }
    }
    //ノックバックの更新
    void ResetForce(float duration)
    {
        holdout = duration + (holdout - (tweenerX != null ? tweenerX.Elapsed() : 0f)) / 2;
        ResetForce(false);
    }
    //ノックバックダウンの移動の更新
    void ResetForce(bool resetHold)
    {
        if (resetHold) holdout = 0f;
        tweenerX?.Kill();
        tweenerY?.Kill();
    }
    //ダウンの更新
    void ResetDamage()
    {
        holdout = 0f;
        tweenerX?.Kill();
        tweenerX = null;
    }
    //ダウン処理
    void Down()
    {
        tweenerY = transform.DOLocalMoveY(-5.5f, (transform.localPosition.y + 5.5f) / 18 / down)
            .OnComplete(() =>
            {
                if (hp <= 0)
                {
                    condition = Afterimage.Condition.Invincible;
                    animator.SetTrigger(deathHash);
                    foreach (var afterimage in afterimages)
                    {
                        afterimage.SetTrigger(deathHash);
                    }
                    Death();
                }
                else
                {
                    animator.SetTrigger(downEndHash);
                    foreach (var afterimage in afterimages)
                    {
                        afterimage.SetTrigger(downEndHash);
                    }
                }
            })
            .SetEase(Ease.InQuad);
    }
    //倒れたときの処理
    void Death()
    {
        death = true; 
        capsuleCollider2.enabled = false;
        StartCoroutine(GameManager.gameManager.Transition());
    }
    //ステータスアップ時のHPの更新
    public void SetHP(int maxHp)
    {
        maxhp = maxHp;
        if(hp > 0) hp = maxHp;
    }
}
//アニメーションによる移動
[System.Serializable]
public class AnimatorMove
{
    public Ease ease;
    public float delay = 0f;
    public float duration = 1f;
    public float move = 0f;
    public bool relative;
    public bool yoyo;
}
[System.Serializable]
public class Particle
{
    public ParticleSystem[] particleSystems;
}
[System.Serializable]
public class ParticleDelay
{
    public ParticleSystem particleSystem;
    public float delay;
    public IEnumerator enumerator;
    public bool dependence;
}
[System.Serializable]
public class ParticleSystemSkill
{
    public ParticleDelay[] particleDelays;
}
