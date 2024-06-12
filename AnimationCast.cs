using UnityEngine;

public class AnimationCast : StateMachineBehaviour
{
    [SerializeField] Material castMaterial; //キャストゲージのマテリアル
    readonly int castSpeed = Shader.PropertyToID("_rate");  //キャストゲージの%
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.playerManagerSub.castObject.SetActive(true); //キャストゲージの表示
        GameManager.playerManagerSub.Move(); //キャスト位置への移動
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        castMaterial.SetFloat(castSpeed, stateInfo.normalizedTime); //キャストゲージの%セット
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.playerManagerSub.castObject.SetActive(false); //キャストゲージの非表示
    }
}
