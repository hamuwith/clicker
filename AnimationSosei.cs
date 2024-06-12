using UnityEngine;

public class AnimationSosei : AnimationCast
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        //蘇生用エフェクトの再生
        GameManager.playerManagerSub.resuscitationParticle.Play();
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        //サブプレイヤーがアンロックの時、サブプレイヤーを非アクティブに
        GameManager.gameManager.characterSub.unlock = GameManager.gameManager.characterSub.unlock;
        //蘇生用エフェクトの停止
        GameManager.playerManagerSub.resuscitationParticle.Stop(); 
        //HPと当たり判定をリセット
        GameManager.playerManager.ResuscitationEnd();
    }
}
