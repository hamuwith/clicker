using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Afterimage : MonoBehaviour
{
    public Animator animator { get; private set; }//���g�̃A�j���[�V����
    [SerializeField] Material material;//���g�̃}�e���A��
    [SerializeField] float offsetTime = 0.5f;//�{�̂Ƃ̂���
    [SerializeField] Color colorArmor;//�A�[�}�[���̐F
    [SerializeField] Color colorInvincible;//���G���̐F
    Tweener tweener;//�q�b�g���X�P�[���A�b�v�p
    Condition condition;//���
    PlayerManager playerManager;//�Ώ�
    Boss boss;//�Ώ�
    bool invincible;//���G
    bool armor;//�n�C�p�[�A�[�}�[
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
    //�������{�X�p
    public void Init(Boss boss)
    {
        Init();
        this.boss = boss;
    }
    //�������v���C���[�p
    public void Init(PlayerManager playerManager)
    {
        Init();
        this.playerManager = playerManager;
    }
    //������
    void Init()
    {
        animator = GetComponentInChildren<Animator>();
        material.color = Color.clear;
    }
    //�v���C���[���{�X�ƃf�B���C�������ē����A�j���[�V���������s����
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
    //�c���̃Z�b�g
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
    //�c�����_���[�W����󂯂��ہA�X�P�[����ω�������
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
