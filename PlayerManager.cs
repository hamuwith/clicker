using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class PlayerManager : MonoBehaviour
{
    public Animator animator { get; private set; }
    [SerializeField] ParticleSystem slashParticle;//�ʏ�U���G�t�F�N�g
    [SerializeField] ParticleSystem slashSkillParticle;//�ʏ�U���X�L���G�t�F�N�g
    [SerializeField] ParticleSystem slashSkillParticle3;//�ʏ�U���X�L���G�t�F�N�g3
    [SerializeField] protected ParticleSystem slashSkillParticleBuff;//�ʏ�U���X�L���������G�t�F�N�g
    [SerializeField] protected ParticleSystemSkill[] particleSystemSkills;//�X�L���G�t�F�N�g
    [SerializeField] ParticleSystem guardParticle;//�K�[�h�G�t�F�N�g
    protected Vector2 vector2;//�ꎞ���p
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
    float hitStopCount;//�q�b�g�X�g�b�v���ԃJ�E���g
    protected float atkBuffTime;//�ʏ�U���o�t����
    protected float atkBuff;//�ʏ�U���o�t���ԃJ�E���g
    public Thunder thunder;//���X�L��
    protected InputAction attackAction;//�f�o�b�O�p
    protected InputAction[] skillAction;//�f�o�b�O�p
    [SerializeField] protected PlayerInput playerInput;//InputSystem
    [SerializeField] Afterimage[] afterimages;//�c��
    [SerializeField] float maxhp;//�ő�HP
    public float hp { get; private set; }//���݂�HP
    float down;//�_�E���l
    Tweener tweenerX;//�_�E���ƃm�b�N�o�b�N
    Tweener tweenerY;//�_�E���ƃm�b�N�o�b�N
    float holdout0;//�m�b�N�o�b�N����
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
    protected Vector3 vector3;//�ꎞ���p
    Afterimage.Condition condition;//���
    public bool skill { get; protected set; }//�X�L���\���ǂ���
    bool guard;//�K�[�h����
    bool guardAcceptable;//�K�[�h�\���ǂ���
    bool invincible;//���G���ǂ���
    bool avoidance;//������ǂ���
    protected float[] coolCounts;//�N�[���^�C���J�E���g
    protected GameManager.Character character;//�X�e�[�^�X
    float autoClickCount;//�����N���b�N�J�E���g
    [SerializeField] protected Material mouse;//�N���b�N�\���ǂ����\���p�}�e���A��
    [SerializeField] protected Material mouseClick;//�N���b�N���͕\���}�e���A��
    [SerializeField] SpriteRenderer spriteRenderer;//�N���b�N���͕\��
    bool canAttack;//�U�����͉\��
    bool skillClick0;
    protected IEnumerator enumerator;//�i���X�L���N���b�N��~�p
    int materialClick0;
    int materialSkill0;
    protected int skillId;//�g�p�X�L��ID
    protected ParticleSystem scaleSkillParticle;//�X�P�[���X�L���G�t�F�N�g
    protected ParticleSystem scaleSkillParticle2;//�X�P�[���X�L���G�t�F�N�g
    [SerializeField] ParticleSystemRenderer particleSystemRenderer;//�i���X�L���N���b�N���e�X�L���G�t�F�N�g�̃����_���[
    [SerializeField] Material[] addMaterials;//�i���X�L���N���b�N���e�X�L���G�t�F�N�g�}�e���A��
    Vector3 vector3ScaleSkillParticle;//�i���X�P�[���X�L���N���b�N���X�P�[��
    [SerializeField] protected ParticleSystem skillEffect;//�i���X�L���N���b�N���e�X�L���G�t�F�N�g
    [SerializeField] ParticleSystem skillEffect1;//�i���X�L���N���b�N���}�E�X�G�t�F�N�g
    public Gradient[] skillGradient;//�X�L���̃��`�[�t�J���[
    [SerializeField] Transform transformOnigiriEffect;//�i���X�L���̒ǉ��G�t�F�N�g
    [SerializeField] AttackCollisionValue attackCollisionValueOnigiriEvo;//�i���X�L���̒ǉ��_���[�W
    [SerializeField] Transform transformIaiEffect;//�i���X�L���̒ǉ��G�t�F�N�g
    [SerializeField] AttackCollisionValue attackCollisionValueIaiEvo;//�i���X�L���̒ǉ��_���[�W
    [SerializeField] ParticleSystem moonEffect;//�i���X�L���̒ǉ��G�t�F�N�g
    [SerializeField] AttackCollisionValue attackCollisionValueMoonEvo;//�i���X�L���̒ǉ��_���[�W
    readonly Vector3 offsetSkill2 = new Vector3(8f, 2.7f, 0f);//�i���X�L���̈ʒu
    readonly public Vector2 inversionVector2 = new Vector2(-1f, 1f);
    public float scale = 3f;//�f�t�H���g�X�P�[��
    public Transform popTextTransform;//�����o���̍��W
    [SerializeField] protected Color autoAttackColor;//�ʏ�U���̐����o���̐F
    [SerializeField] protected Color skillSpeechBubbleColor;//�X�L���̐����o���̐F
    [SerializeField] protected Color damageSpeechBubbleColor;//�_���[�W���󂯂��Ƃ��̐����o���̐F
    [SerializeField] protected Color anotherSpeechBubbleColor;//����ȊO�̐����o���̐F
    public bool death;//�|�ꂽ��
    public float distance;//�i�񂾋���
    public float prePositionX;//�i�񂾋����J�E���g�p
    [SerializeField] CapsuleCollider2D capsuleCollider2;//���g�̃R���C�_�[
    PopText popText0;//���݂̐����o��
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
    public bool left { get; protected set; }//�v���C���[�̌���
    public bool skillClick //�i���X�L���̃N���b�N
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
    bool canSkillClick;//�i���X�L���N���b�N�\��
    Tweener tweener;//�}�E�X�N���b�N�\���G�t�F�N�g
    public enum State
    {
        ATTACK,
        DASH,
        SKILL,
        NULL
    }
    //������
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
    //�����o���̐F�̎擾
    public Color GetSpeechBubbleColor(SpeechBubbleColor speechBubbleColor)
    {
        if (speechBubbleColor == SpeechBubbleColor.Auto) return autoAttackColor;
        else if (speechBubbleColor == SpeechBubbleColor.Skill) return skillSpeechBubbleColor;
        else return anotherSpeechBubbleColor;
    }
    //�L���X�g���Ԃ̃Z�b�g
    public void SetCastSpeed(float perSpeed)
    {
        animator.SetFloat(castHash, 100 / (100 - perSpeed));
    }
    //�v���C���[���A�N�e�B�u�ɂ������̏���
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
    //�����̐ݒ�
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
    //�h���A�j���[�V�����̊J�n
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
    //�v���C���[�̏d�S�̎擾
    public Vector3 GetPlayerCenter()
    {
        return transform.position + 2.5f * Vector3.up;
    }
    //�h������
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
    //�h������(�A�j���[�V�������g�p)
    void ResuscitationComplete()
    {
        hp = maxhp;
    }
    //�i���X�L���N���b�N��
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
    //�i���X�L���N���b�N�G�t�F�N�g
    public void SkillEffectPlayClick(int skillIdSkillParticle, Transform transformSkillParticle, float offsetX)
    {
        var color = skillEffect1.colorOverLifetime;
        color.color = skillGradient[skillIdSkillParticle];
        var shape = skillEffect1.shape;
        shape.position = GameManager.gameManager.mousePoint.position - transformSkillParticle.position + offsetX * Vector3.right;
        skillEffect1.transform.position = transformSkillParticle.position;
        skillEffect1.Play();
    }
    //�i���X�L���G�t�F�N�g
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
    //�i���X�L���G�t�F�N�g
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
    //�X�L��
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
    //�o�b�N�X�e�b�v
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
    //�i���X�L��
    protected virtual void EvoSkill(ref int i)
    {
        if(i == 0 && GameManager.gameManager.GetEvo(this, 0))
        {
            i = 6;
        }
    }
    //�N���b�N��
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
    //�N���b�N�G�t�F�N�g
    void Click()
    {
        tweener?.Complete();
        tweener = DOTween.To(() => 1f, (x) => mouseClick.SetFloat(GameManager.gameManager.scaleHash, x), 0.9f, 0.1f)
            .SetLoops(2, LoopType.Yoyo);
    }
    //����X�L������
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
    //�i���X�L������
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
    //�e�A�j���[�V�����J�n��
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
    //�i���X�L���N���b�N��t���ԏ���
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
    //�K�[�h�ƃm�b�N�o�b�N�L�����Z���\�擾
    public bool GetCanAttack()
    {
        return !skill || canAttack;
    }
    //���G���Ԃ̃Z�b�g
    IEnumerator Invincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }
    //������Ԃ̃Z�b�g
    IEnumerator AvoidancePlus(float time)
    {
        avoidance = true;
        yield return Invincible(time);
        avoidance = false;
    }
    //�ʏ�U���G�t�F�N�g�ʒu(�A�j���[�V�������g�p)
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
    //�A�j���[�V�������g�p
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
    //�A�j���[�V�������g�p
    void GuardParticlePlay()
    {
        guardParticle.Play();
    }
    //�X�L���G�t�F�N�g
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
    //����X�L���G�t�F�N�g
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
    //�X�P�[���X�L������
    public virtual void LastParticleScaleAdd(float plus)
    {
        scaleSkillParticle.transform.localScale += Vector3.one * plus;
        vector3 = scaleSkillParticle.transform.localPosition;
        vector3.x -= 0.21f;
        vector3.y += 0.08f;
        scaleSkillParticle.transform.localPosition = vector3;
        scaleSkillParticle2.transform.localScale = scaleSkillParticle.transform.localScale;
    }
    //�X�P�[���擾
    public Vector3 GetScale()
    {
        return scaleSkillParticle2.transform.localScale;
    }
    protected virtual void SpecialSkillParticlePlay(int num)
    {

    }
    //�ʏ�U���̃G�t�F�N�g�̃f�[�^
    protected virtual (float positionX, float positionY, float rotationX, float rotationY, float rotationZ) SetParticle(int num)
    {
        if (num == 0) return (0f, 1.06f, 0.58835f, -0.094f, -1.2486f);
        else if (num == 1) return (-0.4f, 1f, -0.967f, 0.653f, 1.864f);
        else if (num == 2) return (0f, 1.46f, 2.2375f, -3.197f, 2.5028f);
        else return (0.07f, 1.3f, 0.54786f, -3.4383f, 3.616f);
    }
    //�ʏ�o�t�U���̃G�t�F�N�g�̃f�[�^
    (float positionX, float positionY, float rotationX, float rotationY, float rotationZ) SetSkillParticle(int num)
    {
        if (num == 0) return (0f, 1.14f, -1.09f, 0.5f, -0.53f);
        else if (num == 1) return (-0.07f, 1.13f, -1.02f, 0.31f, 1.71f);
        else if (num == 2) return (0.68f, 1.15f, -0.75f, 0.925f, -0.74f);
        else return (0.15f, 1.1f, 0f, 2.61f, -2.3f);
    }
    //�q�b�g�X�g�b�v
    public void HitStop(float time, float scale,bool zoom, in Vector2 worldPoint = default)
    {
        if (time <= hitStopCount) return;
        hitStopCount = time;
        if(zoom) GameManager.cameraManager.ZoomCamera(hitStopCount, worldPoint);
        Time.timeScale = scale;
    }
    //�����蔻�菈��
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("EnemyWeapon")) return;
        (AttackCollisionValue attackCollisionValue0, _)  = collision.gameObject.GetComponent<AttackCollision>().GetAttackCollisionValue();
        StartCoroutine(Damage(attackCollisionValue0, collision));
    }
    //�_���[�W���󂯂��Ƃ��̏���
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
            GameManager.popTextManager.SetPopText("���", this, anotherSpeechBubbleColor, PopTextManager.Kind.Avoidance);
            yield break;
        }
        else if (condition == Afterimage.Condition.Invincible || invincible)
        {
            GameManager.popTextManager.SetPopText("���G", this, anotherSpeechBubbleColor, PopTextManager.Kind.Invincible);
            yield break;
        }
        else if (condition == Afterimage.Condition.Armor)
        {
            GameManager.popTextManager.SetPopText("�A�[�}�[", this, anotherSpeechBubbleColor, PopTextManager.Kind.Armor);
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
            GameManager.popTextManager.SetPopText("�K�[�h", this, anotherSpeechBubbleColor, PopTextManager.Kind.Guard);
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
            GameManager.popTextManager.SetPopText("���͂��I", this, damageSpeechBubbleColor, PopTextManager.Kind.Another);
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
            GameManager.popTextManager.SetPopText("�����I", this, damageSpeechBubbleColor);
        }
    }
    //�m�b�N�o�b�N�̍X�V
    void ResetForce(float duration)
    {
        holdout = duration + (holdout - (tweenerX != null ? tweenerX.Elapsed() : 0f)) / 2;
        ResetForce(false);
    }
    //�m�b�N�o�b�N�_�E���̈ړ��̍X�V
    void ResetForce(bool resetHold)
    {
        if (resetHold) holdout = 0f;
        tweenerX?.Kill();
        tweenerY?.Kill();
    }
    //�_�E���̍X�V
    void ResetDamage()
    {
        holdout = 0f;
        tweenerX?.Kill();
        tweenerX = null;
    }
    //�_�E������
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
    //�|�ꂽ�Ƃ��̏���
    void Death()
    {
        death = true; 
        capsuleCollider2.enabled = false;
        StartCoroutine(GameManager.gameManager.Transition());
    }
    //�X�e�[�^�X�A�b�v����HP�̍X�V
    public void SetHP(int maxHp)
    {
        maxhp = maxHp;
        if(hp > 0) hp = maxHp;
    }
}
//�A�j���[�V�����ɂ��ړ�
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
