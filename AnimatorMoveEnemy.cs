using UnityEngine;
using DG.Tweening;

public class AnimetorMoveEnemy : AnimetorMoveScript
{
    Enemy enemy;
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
        //�ړ��J�E���g
        x = 0;
        y = 0;        
        //�A�j���[�V�����ɍ��킹���ړ�
        MoveX(animator.transform, animator, GameManager.playerManager.transform.position.x > animator.transform.position.x);
        MoveY(animator.transform);
        if (attackCollisions == null || attackCollisions.Length <= 0) return;
        if (enemy == null) enemy = animator.GetComponent<Enemy>();
        //�������U���G�t�F�N�g
        enemy.AttackParticle();
        //�����蔻��̃Z�b�g
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.attackCollision = GameManager.attackCollisionManagerBoss.SetCollision(attackCollision, animator.transform.position, null);
        }
    }
    new public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //�ړ��̃L�����Z��
        ((StateMachineBehaviour)this).OnStateExit(animator, stateInfo, layerIndex);
        if (enemy != null) enemy.AttackParticlesStop();
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
    }
    void OnDestroy()
    {
        //�I�u�W�F�N�g�j�󎞏���
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
    }
}
