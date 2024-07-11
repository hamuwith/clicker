using UnityEngine;
using DG.Tweening;

public class AnimetorMoveBoss : AnimetorMoveScript
{
    [SerializeField] float cooltime;//�N�[���^�C��
    [SerializeField] bool relative;//���ΓI��
    [SerializeField] float cycleOffset;//�A�j���[�V�����̃I�t�Z�b�g
    Boss boss;
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
        x = 0;
        y = 0;
        boss = sub ? GameManager.bossSub : GameManager.boss;
        if (boss == null) return;
        Debug.Log($"{sub}, {boss}");
        //�{�X�̃A�j���[�V�����J�n������
        boss.AnimationStart(stateInfo.shortNameHash, skillId, cooltime, condition, invincibleTime, animator, cycleOffset);
        //�{�X�{�̂Ȃ�
        if (boss.animator == animator)
        {
            MoveX(boss.transform, animator, !boss.right ^ boss.inversion);
            MoveY(boss.transform);
            //�������U���G�t�F�N�g
            if(autoAttack) boss.AttackParticle();
        }
        //�c���Ȃ�
        else
        {
            foreach(var afterimage in boss.afterimages)
            {
                if (afterimage.animator == animator)
                {
                    MoveX(afterimage.transform, animator, !boss.right ^ boss.inversion);
                    MoveY(afterimage.transform);
                    break;
                }
            }
        }
        if (boss.animator != animator) return;
        //�{�X�{�̂Ȃ瓖���蔻����Z�b�g
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.left = !boss.right ^ boss.inversion;
            if (relative)
            {
                //�v���C���[�ɑ΂��Ă̋���
                vector2.x = GameManager.playerManager.transform.position.x;
                vector2.y = boss.transform.position.y;
                attackCollision.attackCollision = GameManager.attackCollisionManagerBoss.SetCollision(attackCollision, vector2, null);
            }
            else attackCollision.attackCollision = GameManager.attackCollisionManagerBoss.SetCollision(attackCollision, boss.transform.position, null);
        }
    }
    new public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateExit(animator, stateInfo, layerIndex);
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
        if (boss == null || boss.animator != animator) return;
        if (autoAttack) boss.AttackParticlesStop();
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.attackCollision?.End(attackCollision.dependence);
        }
        boss.ParticlesStopSkill(skillId);
    }
    void OnDestroy()
    {
        //�I�u�W�F�N�g�j�󎞏���
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
    }
}
