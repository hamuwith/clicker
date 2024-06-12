using UnityEngine;

public class AnimationSosei : AnimationCast
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        //�h���p�G�t�F�N�g�̍Đ�
        GameManager.playerManagerSub.resuscitationParticle.Play();
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        //�T�u�v���C���[���A�����b�N�̎��A�T�u�v���C���[���A�N�e�B�u��
        GameManager.gameManager.characterSub.unlock = GameManager.gameManager.characterSub.unlock;
        //�h���p�G�t�F�N�g�̒�~
        GameManager.playerManagerSub.resuscitationParticle.Stop(); 
        //HP�Ɠ����蔻������Z�b�g
        GameManager.playerManager.ResuscitationEnd();
    }
}
