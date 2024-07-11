using UnityEngine;
using DG.Tweening;

public class AnimetorMoveBoss : AnimetorMoveScript
{
    [SerializeField] float cooltime;//クールタイム
    [SerializeField] bool relative;//相対的か
    [SerializeField] float cycleOffset;//アニメーションのオフセット
    Boss boss;
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
        x = 0;
        y = 0;
        boss = sub ? GameManager.bossSub : GameManager.boss;
        if (boss == null) return;
        Debug.Log($"{sub}, {boss}");
        //ボスのアニメーション開始時処理
        boss.AnimationStart(stateInfo.shortNameHash, skillId, cooltime, condition, invincibleTime, animator, cycleOffset);
        //ボス本体なら
        if (boss.animator == animator)
        {
            MoveX(boss.transform, animator, !boss.right ^ boss.inversion);
            MoveY(boss.transform);
            //遠距離攻撃エフェクト
            if(autoAttack) boss.AttackParticle();
        }
        //残像なら
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
        //ボス本体なら当たり判定をセット
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.left = !boss.right ^ boss.inversion;
            if (relative)
            {
                //プレイヤーに対しての距離
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
        //オブジェクト破壊時処理
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
    }
}
