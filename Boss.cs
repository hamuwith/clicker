using UnityEngine;
using System.Collections;

public class Boss : Enemy
{
    [SerializeField] SkillSet[] skillSets;//�X�L���̐ݒ�
    readonly int skillHash = Animator.StringToHash("skill");
    readonly int skillIdHash = Animator.StringToHash("skillId");
    [SerializeField] float attackArea2;//�ߋ����U���͈̔�
    readonly int attackkinHash = Animator.StringToHash("attackkin");
    readonly int distanceHash = Animator.StringToHash("distance");
    readonly int hprateHash = Animator.StringToHash("hprate");
    [SerializeField] Particle[] skillParticles;
    [SerializeField] ParticleSystemSkill[] particleSystemSkills;//�X�L���̃G�t�F�N�g�ݒ�
    [SerializeField] Afterimage[] AfterimageObjects;//�c���̃Q�[���I�u�W�F�N�g
    [SerializeField] ParticleSystem particleSystemSpecial4;//����G�t�F�N�g
    [SerializeField] AttackCollisionValue AttackCollisionValueSpecial4;//���ꓖ���蔻��
    Vector2 vector2;//�ꎞ���p
    IEnumerator enumerator;//���G���ԏ���
    readonly float armorTime = 1.5f; //�N���オ��A�[�}�[����
    public bool Skill(int i)
    {
        if (skill) return false;//�ʍs����
        if (skillSets[i].activeHprate < GetHpRate()) return false;//�g�p�\HP
        if (skillSets[i].coolCount > 0f) return false;//�N�[���^�C��
        float distance = Mathf.Abs(GameManager.playerManager.transform.position.x - transform.position.x);
        if (distance > skillSets[i].activeArea) return false;//�g�p�\����
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
    //�N���A
    protected override void Death()
    {
        GameManager.gameManager.Clear();
    }
    //�_���[�W�ɂ��A�j���[�V�����̃u�����h�p
    protected override void SetDamage()
    {
        animator.SetFloat(hprateHash, 0f);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetHprate(hprateHash, 0f);
        }
    }
    //�|���ꂽ�Ƃ��q�b�g�X�g�b�v
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
    //�N���オ�菈��
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
    //�U������
    public override void Attack(int currectAnime)
    {
        if (skill) return;
        if (currectAnime == idleHash || currectAnime == moveHash)
        {
            //�ߋ����U��
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
    //������
    public override void Start0(Transform _transform)
    {
        base.Start0(_transform);
        GameManager.boss = this;
        //�c���̃Z�b�g
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
    //���G
    IEnumerator Invincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
        enumerator = null;
    }
    //�n�C�p�[�A�[�}�[
    IEnumerator Armor(float time)
    {
        armorbool = true;
        yield return new WaitForSeconds(time);
        armorbool = false;
        enumerator = null;
    }
    //�A�j���[�V�����J�n��
    public void AnimationStart(int hash, int skillId, float cooltime, Afterimage.Condition condition, float invincibleTime, Animator animator)
    {
        if(this.animator == animator)
        {
            if (skillId >= 0 && skillId < skillSets.Length)
            {
                skillSets[skillId].coolCount = cooltime; //�N�[���^�C���̃Z�b�g
                ParticlePlaySkill(skillId);//�G�t�F�N�g�̃Z�b�g
            }
            //���G���Ԏw��Ȃ�
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
            //�s������
            skill = hash != idleHash && hash != moveHash;
        }
        //�c���̏�Ԃ̃Z�b�g
        foreach (var afterimage in afterimages)
        {
            if (afterimage.animator == animator) afterimage.SetCondition(condition, invincibleTime);
        }
    }
    //�ŗL�X�L��
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
    //�X�L���G�t�F�N�g�̃Z�b�g
    public virtual void ParticlePlaySkill(int num)
    {
        foreach (var particleSystem in particleSystemSkills[num].particleDelays)
        {
            particleSystem.enumerator = SkillParticlePlay(particleSystem, num);
            StartCoroutine(particleSystem.enumerator);
        }
    }
    //�X�L���G�t�F�N�g�̃f�B���C
    protected virtual IEnumerator SkillParticlePlay(ParticleDelay particleDelay, int num)
    {
        yield return new WaitForSeconds(particleDelay.delay);
        particleDelay.particleSystem.Play();
        Special(num);
    }
    //�X�L���G�t�F�N�g�̒�~
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
        public float coolCount;//�N�[���^�C���̃J�E���g
        public float activeArea;//�U���J�n�͈�
        public float activeHprate = 1f;//�U���\HP
    }
}
