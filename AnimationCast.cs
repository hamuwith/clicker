using UnityEngine;

public class AnimationCast : StateMachineBehaviour
{
    [SerializeField] Material castMaterial; //�L���X�g�Q�[�W�̃}�e���A��
    readonly int castSpeed = Shader.PropertyToID("_rate");  //�L���X�g�Q�[�W��%
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.playerManagerSub.castObject.SetActive(true); //�L���X�g�Q�[�W�̕\��
        GameManager.playerManagerSub.Move(); //�L���X�g�ʒu�ւ̈ړ�
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        castMaterial.SetFloat(castSpeed, stateInfo.normalizedTime); //�L���X�g�Q�[�W��%�Z�b�g
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.playerManagerSub.castObject.SetActive(false); //�L���X�g�Q�[�W�̔�\��
    }
}
