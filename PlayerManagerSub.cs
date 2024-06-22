using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerManagerSub : PlayerManager
{
    [SerializeField] ParticleSystem fireEnd;//�ʏ�U���̃G���h�G�t�F�N�g
    [SerializeField] Material material;//�X�v���C�g�̃}�e���A��
    [SerializeField] Ice ice;//�A�C�X�X�L��
    [SerializeField] Poison poison;//�ŃX�L��
    public ParticleSystem resuscitationParticle;//�h���G�t�F�N�g
    Tweener tweener;//�ړ��p
    IceParameter iceParameter;//�A�C�X�X�L���̊Ǘ��p
    readonly int timeHash = Shader.PropertyToID("_time");
    readonly int vector3Hash = Shader.PropertyToID("_Vector3");
    [SerializeField] GameObject[] slashSkillParticleBuffChildren;//�ʏ�U���o�t�I�u�W�F�N�g
    int fireCount;//�ʏ�U���o�t�J�E���g
    [SerializeField] Transform transformPoisonEffect;//�ŃX�L���G�t�F�N�g���W
    public GameObject castObject;//�L���X�g�\��
    //������
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
    //�i���X�L���N���b�N����
    
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
    //�i���X�P�[���X�L���X�P�[������
    public override void LastParticleScaleAdd(float plus)
    {
        scaleSkillParticle.transform.localScale += Vector3.one * plus;
        var velocityOverLifetime = scaleSkillParticle.velocityOverLifetime;
        velocityOverLifetime.speedModifierMultiplier = 1f / scaleSkillParticle.transform.localScale.x;
    }
    //�ʏ�U���G�t�F�N�g�ʒu
    protected override void SlashParticlePlay(int num)
    {
        (vector2.x, vector2.y) = SetParticlSub(num);
        if (left) vector2.x = -vector2.x;
        GameManager.singleHitManager.SetParticle((Vector2)transform.position + vector2, left);
    }
    //�ʏ�U���G�t�F�N�g�ʒu�f�[�^
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
    //�ʏ�U���G���h�G�t�F�N�g
    public void FireParticlePlay(Transform _transform)
    {
        fireEnd.transform.position = _transform.position;
        fireEnd.Play();
    }
    //�X�L���G�t�F�N�g
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
    //�ŃX�L���G�t�F�N�g
    IEnumerator PoisonParticlePlay(ParticleDelay particleDelay)
    {
        yield return new WaitForSeconds(particleDelay.delay);
        poison.Set();
    }
    //�ŃX�L���G�t�F�N�g�I���
    public void Poison(AttackCollision attackCollision, AttackCollisionValue attackCollisionValue)
    {
        poison.Stop(attackCollision.transform, attackCollisionValue);
        attackCollision.End(true);
    }
    //����X�L���G�t�F�N�g
    protected override void SpecialSkillParticlePlay(int num)
    {
        if(num == 0)
        {
            StartCoroutine(IcePlay());
        }
    }
    //�i���X�L���G�t�F�N�g
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
    //����X�L��
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
    //�v���C���[���A�N�e�B�u�ɂ������̏���
    public override void SetAvtive(bool b)
    {
        base.SetAvtive(b);
        transform.position = GameManager.playerManager.transform.position + (GameManager.playerManager.left ? Vector3.right : Vector3.left) * 100f;
    }
    //�h������
    public override void Resuscitation()
    {
        if (GameManager.playerManager.death)
        {
            GameManager.popTextManager.SetPopText("���A�����邩��I�I", this, anotherSpeechBubbleColor, duration:1f);
            animator.SetTrigger(resuscitationHash);
            skill = true;
            Debug.Log("inin");
            GameManager.playerManager.ResuscitationStart();
        }
    }
    //�������̎擾
    public bool GetSummon()
    {
        fireCount++;
        return slashSkillParticleBuffChildren[0].activeSelf || fireCount < 2;
    }
    //�A�C�X�X�L��
    public IEnumerator IcePlay()
    {
        ice.Play(); 
        yield return new WaitWhile(() => particleSystemSkills[0].particleDelays[0].particleSystem.IsAlive());
        ice.Stop();
    }
    //�A�C�X�U���ΏۃZ�b�g
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
    //�A�C�X�̏�����
    public void IceInit()
    {
        iceParameter.Start();
    }
    //�i���A�C�X�X�L���N���b�N�Z�b�g
    public bool SetIceClick(bool b)
    {
        iceParameter.click = b;
        return b;
    }
    //�A�C�X�̃_���[�W����
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
    //�e�A�j���[�V�����̊J�n����
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
    //�v���C���[�Ɨ���Ă�����u�Ԉړ�
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
    //�A�C�X�f�[�^
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
