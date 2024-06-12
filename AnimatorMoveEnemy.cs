using UnityEngine;
using DG.Tweening;

public class AnimetorMoveEnemy : AnimetorMoveScript
{
    Enemy enemy;
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
        //移動カウント
        x = 0;
        y = 0;        
        //アニメーションに合わせた移動
        MoveX(animator.transform, animator, GameManager.playerManager.transform.position.x > animator.transform.position.x);
        MoveY(animator.transform);
        if (attackCollisions == null || attackCollisions.Length <= 0) return;
        if (enemy == null) enemy = animator.GetComponent<Enemy>();
        //遠距離攻撃エフェクト
        enemy.AttackParticle();
        //当たり判定のセット
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.attackCollision = GameManager.attackCollisionManagerBoss.SetCollision(attackCollision, animator.transform.position, null);
        }
    }
    new public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //移動のキャンセル
        ((StateMachineBehaviour)this).OnStateExit(animator, stateInfo, layerIndex);
        if (enemy != null) enemy.AttackParticlesStop();
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
    }
    void OnDestroy()
    {
        //オブジェクト破壊時処理
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
    }
}
