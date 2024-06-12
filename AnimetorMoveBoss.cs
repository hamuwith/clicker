using UnityEngine;
using DG.Tweening;

public class AnimetorMoveBoss : AnimetorMoveScript
{
    [SerializeField] float cooltime;//�N�[���^�C��
    Vector2 vector2;
    [SerializeField] bool relative;//���ΓI��
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
        x = 0;
        y = 0;
        //�X�L�������O�Ƀ{�X�̌������Z�b�g
        GameManager.boss.SetRight();
        //�{�X�̃A�j���[�V�����J�n������
        GameManager.boss.AnimationStart(stateInfo.shortNameHash, skillId, cooltime, condition, invincibleTime, animator);
        //�{�X�{�̂Ȃ�
        if (GameManager.boss.animator == animator)
        {
            MoveX(GameManager.boss.transform, animator, !GameManager.boss.right);
            MoveY(GameManager.boss.transform);
        }
        //�c���Ȃ�
        else
        {
            foreach(var afterimage in GameManager.boss.afterimages)
            {
                if (afterimage.animator == animator)
                {
                    MoveX(afterimage.transform, animator, !GameManager.boss.right);
                    MoveY(afterimage.transform);
                    break;
                }
            }
        }
        if (GameManager.boss.animator != animator) return;
        //�{�X�{�̂Ȃ瓖���蔻����Z�b�g
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.left = !GameManager.boss.right;
            if (relative)
            {
                //�v���C���[�ɑ΂��Ă̋���
                vector2.x = GameManager.playerManager.transform.position.x;
                vector2.y = GameManager.boss.transform.position.y;
                attackCollision.attackCollision = GameManager.attackCollisionManagerBoss.SetCollision(attackCollision, vector2, null);
            }
            else attackCollision.attackCollision = GameManager.attackCollisionManagerBoss.SetCollision(attackCollision, GameManager.boss.transform.position, null);
        }
    }
    new public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateExit(animator, stateInfo, layerIndex);
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.attackCollision?.End(attackCollision.dependence);
        }
        GameManager.boss.ParticlesStopSkill(skillId);
    }
}
