using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;

public class AttackCollision : MonoBehaviour
{
    [SerializeField] CapsuleCollider2D capsuleCollider2;//�����蔻��
    [SerializeField] AttackCollisionValue attackCollisionValue;//�����蔻��̐ݒ�
    IEnumerator enumeratorInterval;//�����蔻��L�����Z����Kill�p
    bool exist;//�ė��p
    Vector2 vector2;//�ꎞ���p
    Vector2 vector2side;//�ꎞ���p
    public int nextNum { get; set; }//���̓����蔻��̔z��
    SingleHit singleHit;//�V���O���q�b�g
    Tweener tweener;//�����蔻��L�����Z����Kill�p
    PlayerManager playerManager;//�����蔻��̔�����
    IEnumerator enumerator;//�����蔻��L�����Z����Kill�p
    public AttackCollision SetCollision(AttackCollisionValue attackCollisionValue, Vector2 position, SingleHit singleHit)
    {
        if (exist) return null;
        exist = true;
        if (attackCollisionValue.delay > 0)
        {
            //�f�B���C����̎��f�B���C�㓖���蔻��̐ݒ�
            enumerator = SetCollisionDelay(attackCollisionValue, position, singleHit);
            StartCoroutine(enumerator);
        }
        else
        {
            //�f�B���C�Ȃ��̎������蔻��̐ݒ�
            SetCollisionReal(attackCollisionValue, position, singleHit);
        }
        return this;
    }
    //�f�B���C�����蔻��
    IEnumerator SetCollisionDelay(AttackCollisionValue attackCollisionValue, Vector2 position, SingleHit singleHit)
    {
        yield return new WaitForSeconds(attackCollisionValue.delay);
        SetCollisionReal(attackCollisionValue, position, singleHit);
        enumerator = null;
    }
    //�����蔻��̐ݒ�
    void SetCollisionReal(AttackCollisionValue attackCollisionValue, Vector2 position, SingleHit singleHit)
    {
        playerManager = attackCollisionValue.sub ? GameManager.playerManagerSub : GameManager.playerManager;
        this.attackCollisionValue = attackCollisionValue;
        this.singleHit = singleHit;
        int side = attackCollisionValue.left ? -1 : 1;
        tweener = DOTween.To(() => 0f, (x) =>
        {
            //�����蔻��̈ړ�
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
                //�����ݒ�
                capsuleCollider2.direction = attackCollisionValue.direction;
                transform.position = position + attackCollisionValue.position * side;
                capsuleCollider2.size = attackCollisionValue.size;
                //�i���X�L���N���b�N���X�P�[���̏����ݒ�
                if (attackCollisionValue.special == AttackCollisionValue.Special.ScaleStart) transform.localScale = GameManager.playerManager.GetScale();
                //��x�q�b�g
                if (attackCollisionValue.interval <= 0f) capsuleCollider2.enabled = true;
                //�i���X�L���A�C�X�̂Ƃ��N���b�N���_���[�W����
                else if (attackCollisionValue.special == AttackCollisionValue.Special.Ice)
                {
                    enumeratorInterval = Interval();
                    StartCoroutine(enumeratorInterval);
                }
                //�i���X�L�����̂Ƃ��N���b�N���_���[�W����
                else if (attackCollisionValue.special == AttackCollisionValue.Special.WindAdd)
                {
                    enumeratorInterval = IntervalWindAdd();
                    StartCoroutine(enumeratorInterval);
                }
                //����ȊO�̊Ԋu�_���[�W�̎�
                else
                {
                    enumeratorInterval = IntervalNormal();
                    StartCoroutine(enumeratorInterval);
                }

            })
            .OnUpdate(() =>
            {
                //�i���X�L���N���b�N���X�P�[���̃N���b�N������
                if (attackCollisionValue.special == AttackCollisionValue.Special.Scale && playerManager.skillClick)
                {
                    transform.localScale += Vector3.one * 0.1f;
                    playerManager.LastParticleScaleAdd(0.1f);
                    playerManager.SkillEffectPlay(attackCollisionValue.skillId, transform, transform.localScale.x);
                };
            })
            .OnComplete(() =>
            {
                //�����蔻���~
                Stop();
                //�v���C���[�ƓƗ����Ă��Ȃ��Ȃ�
                if (!attackCollisionValue.dependence) exist = false; 
            })
            .SetEase(attackCollisionValue.ease);
    }
    //�Ԋu�_���[�W����
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
    //�i���X�L�����p
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
    }//�i���X�L���A�C�X�p
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
    //�C���^�[�o������
    IEnumerator Interval0()
    {
        yield return new WaitForSeconds(attackCollisionValue.interval);
        yield return true;
    }
    //������
    public void Initialization(int nextNum)
    {
        this.nextNum = nextNum;
        Destroy();
    }
    //�_���[�W�̎󂯓n��
    public (AttackCollisionValue, SingleHit) GetAttackCollisionValue()
    {
        return (attackCollisionValue, singleHit);
    }
    //�A�j���[�V�����̒�~�ɂ�铖���蔻��̒�~
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
    //��~
    private void Stop()
    {
        Destroy();
        if (enumeratorInterval != null) StopCoroutine(enumeratorInterval);
    }
    //�����蔻��̍폜
    void Destroy()
    {
        transform.localScale = Vector3.one;
        capsuleCollider2.enabled = false;
    }
}
[System.Serializable]
public class AttackCollisionValue
{
    public float duration;//�ێ�����
    public Vector2 size;//�T�C�Y
    public Vector2 position;//�ʒu
    public Vector2 velocity;//���x
    public Vector2 expantion;//�g��
    public CapsuleDirection2D direction;//����
    public float delay;//�f�B���C����
    public float rotation;//��]
    public float rotationVelocity;//��]���x
    public float damageRate;//�_���[�W��
    public float interval;//�_���[�W�Ԋu
    public Vector3 force;//�������
    public float down;//�_�E����
    public Ease ease;//�ړ����@
    public bool autoAtk;//�ʏ�U����
    public Special special;//����U��
    public Effect effect;//��Ԉُ�
    public bool suction;//�z������
    public int skillId = -1;//�X�L��ID
    public bool sub { get; set; }//�T�u�v���C���[��
    public bool left { get; set; }//�����蔻��̌���
    public AttackCollision attackCollision { get; set; }//�����蔻���~�p
    public bool dependence;//�v���C���[�ƓƗ����Ă��邩
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
