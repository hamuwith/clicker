using UnityEngine;
using DG.Tweening;

public class AnimetorMoveBoss : AnimetorMoveScript
{
    [SerializeField] float cooltime;//クールタイム
    Vector2 vector2;
    [SerializeField] bool relative;//相対的か
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
        x = 0;
        y = 0;
        //スキル発動前にボスの向きをセット
        GameManager.boss.SetRight();
        //ボスのアニメーション開始時処理
        GameManager.boss.AnimationStart(stateInfo.shortNameHash, skillId, cooltime, condition, invincibleTime, animator);
        //ボス本体なら
        if (GameManager.boss.animator == animator)
        {
            MoveX(GameManager.boss.transform, animator, !GameManager.boss.right);
            MoveY(GameManager.boss.transform);
        }
        //残像なら
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
        //ボス本体なら当たり判定をセット
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.left = !GameManager.boss.right;
            if (relative)
            {
                //プレイヤーに対しての距離
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
