using UnityEngine;
using DG.Tweening;

public class AnimetorMoveBoss : AnimetorMoveScript
{
    [SerializeField] float cooltime;//クールタイム
    [SerializeField] bool relative;//相対的か
    [SerializeField] float cycleOffset;//アニメーションのオフセット
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
        x = 0;
        y = 0;
        //ボス本体なら
        if (GameManager.boss.animator == animator)
        {
            //スキル発動前にボスの向きをセット
            GameManager.boss.SetRight(0f);
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
        //ボスのアニメーション開始時処理
        GameManager.boss.AnimationStart(stateInfo.shortNameHash, skillId, cooltime, condition, invincibleTime, animator, cycleOffset);
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
        if (GameManager.boss.animator != animator) return;
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.attackCollision?.End(attackCollision.dependence);
        }
        GameManager.boss.ParticlesStopSkill(skillId);
    }
    void OnDestroy()
    {
        //オブジェクト破壊時処理
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
    }
}
