using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AnimetorMoveScript : StateMachineBehaviour
{
	[SerializeField] protected AnimatorMove[] animatorMovesX;//X���ւ̈ړ�
    [SerializeField] AnimatorMove[] animatorMovesY;//Y���ւ̈ړ�
    [SerializeField] protected AttackCollisionValue[] attackCollisions;//�A�j���[�V�����̓����蔻��
    [SerializeField] bool cameraMove;//�ړ��ɑ΂��ăJ�����̌Œ肷�邩
    [SerializeField] protected bool sub;//�T�u�v���C���[��
    protected Tweener tweenerMoveX;//�A�j���[�V�����L�����Z�����L�����邽��
    protected Tweener tweenerMoveY;//�A�j���[�V�����L�����Z�����L�����邽��
    protected int x;//X�ړ��C���f�b�N�X�p
    protected int y;//Y�ړ��C���f�b�N�X�p
    [SerializeField] protected Afterimage.Condition condition;//�A�j���[�V�����̏��
    [SerializeField] protected float invincibleTime = 0.7f;//���G���Ԃ̃Z�b�g
    [SerializeField] protected int skillId = -1;//�X�L��ID
    [SerializeField] float clickDuration = -1f;//�i���X�L���p�A�ŉ\����
    [SerializeField] float clickDelay;//�i���X�L���p�A�ŉ\���ԃf�B���C
    [SerializeField] ParticleSystemDelay[] particleSystemDelays;//�A�j���[�V�����̃G�t�F�N�g
    protected Vector2 vector2;//�ꎞ
    [SerializeField] SpeechBubble[] speechBubbles;//�����o���̐ݒ�
    [SerializeField] SpeechBubbleEvo[] speechBubbleEvos;//�i���X�L�����̐����o���̐ݒ�
    [SerializeField] protected bool autoAttack;
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        PlayerManager playerManager = sub ? GameManager.playerManagerSub : GameManager.playerManager;
        base.OnStateEnter(animator, stateInfo, layerIndex);
        x = 0;
        y = 0;
        //�J�����Œ�̐ݒ�
        if (GameManager.playerManager.animator == animator) CameraManager.fix = !cameraMove;
        //�A�j���[�V�����X�^�[�g
        playerManager.AnimationStart(animator, condition, stateInfo.shortNameHash, invincibleTime, skillId, clickDelay, clickDuration);
        //�����̐ݒ�
        MoveX(animator.transform, animator, playerManager.left);
        MoveY(animator.transform);
        if (GameManager.playerManager.animator != animator && !sub) return;
        //�����o���̐ݒ�
        if (skillId >= 0 && GameManager.gameManager.GetEvo(playerManager, skillId) && speechBubbleEvos.Length > 0)
        {
            //�i�����A�N���b�N���ɉ����ăR�����g��ǉ��p�̐ݒ�
            foreach (var speechBubble in speechBubbleEvos)
            {
                if(speechBubble.between == null || speechBubble.between == "") speechBubble.popText = GameManager.popTextManager.SetPopText(speechBubble.str, playerManager, playerManager.GetSpeechBubbleColor(speechBubble.speechBubbleColor), duration: 1f, delay: speechBubble.delay);
                speechBubble.popText = GameManager.popTextManager.SetPopText(speechBubble.str, playerManager, playerManager.GetSpeechBubbleColor(speechBubble.speechBubbleColor), betweentext: speechBubble.between, endtext: speechBubble.end, delay: speechBubble.delay);
            }
        }
        else
        {
            //�����o���̐ݒ�
            foreach (var speechBubble in speechBubbles)
            {
                speechBubble.popText = GameManager.popTextManager.SetPopText(speechBubble.str, playerManager, playerManager.GetSpeechBubbleColor(speechBubble.speechBubbleColor), duration : skillId >= 0 ? 1f : 0f, delay: speechBubble.delay);
            }
        }
        //�����蔻��̐ݒ�
        foreach (var attackCollision in attackCollisions)   
        {
            if (attackCollision.special == AttackCollisionValue.Special.WindAdd && !GameManager.gameManager.GetEvo(GameManager.playerManagerSub, attackCollision)) return;
            attackCollision.sub = sub;
            attackCollision.left = playerManager.left;
            //�V���O���q�b�g�U���̏���
            SingleHit singleHitObject = null;
            if(attackCollision.special == AttackCollisionValue.Special.Fire)
            {
                vector2 = attackCollision.position + Vector2.left;
                if(playerManager.left) vector2.x = -vector2.x;
                vector2 += (Vector2)GameManager.playerManagerSub.transform.position;
                singleHitObject = GameManager.singleHitManager.SetParticle(vector2, playerManager.left);
            }
            //�e�����蔻��̐ݒ�
            attackCollision.attackCollision = GameManager.attackCollisionManager.SetCollision(attackCollision, animator.transform.position, singleHitObject);
            //���U����1��̍U���ɑ΂���1�x�̂�
            if (attackCollision.special == AttackCollisionValue.Special.Thunder && GameManager.gameManager.GetEvo(GameManager.playerManager, attackCollision)) Thunder.flag = true;
            //�����X�L�����g�p���A�ʏ�U������
            if (attackCollision.special == AttackCollisionValue.Special.Fire && !GameManager.playerManagerSub.GetSummon()) return;
        }
    }
    new public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        tweenerMoveX?.Kill();
        tweenerMoveY?.Kill();
        PlayerManager playerManager = sub ? GameManager.playerManagerSub : GameManager.playerManager;
        if (GameManager.playerManager.animator != animator && !sub) return;
        //�G�t�F�N�g�̒�~
        playerManager.ParticlesStopSkill(skillId);
        //�����蔻��̒�~
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.attackCollision?.End(attackCollision.dependence);
        }
        //�����o���̒�~
        if (skillId >= 0 && GameManager.gameManager.GetEvo(playerManager, skillId) && speechBubbleEvos.Length > 0)
        {
            foreach (var speechBubble in speechBubbleEvos)
            {
                speechBubble.popText?.StopDisplay();
            }
        }
        else
        {
            foreach (var speechBubble in speechBubbles)
            {
                speechBubble.popText?.StopDisplay();
            }
        }
    }
    protected void MoveX(Transform transform, Animator animator, bool changeSide)
    {
        if (animatorMovesX.Length <= x) 
        {
            //X�ړ��I�����J�����̌Œ����
            if (GameManager.playerManager.animator == animator) CameraManager.fix = false;
            return;
        }
        //X�ړ�
        if (animatorMovesX[x].move != 0f || animatorMovesX[x].relative)
        {
            float rate = 1f;
            if (autoAttack && GameManager.boss != null && Mathf.Abs(transform.position.x - GameManager.boss.transform.position.x) < 4f) rate = 0.1f;
            float movex = (animatorMovesX[x].relative ? GameManager.playerManager.transform.position.x : transform.position.x) + animatorMovesX[x].move * (changeSide ? -rate : rate);            
            tweenerMoveX = transform.DOMoveX(movex, animatorMovesX[x].duration)
                .SetDelay(animatorMovesX[x].delay)
                .OnComplete(() =>
                {
                    x++;
                    MoveX(transform, animator, changeSide);
                })
                .SetLoops(animatorMovesX[x].yoyo ? 2: 1, LoopType.Yoyo)
                .SetEase(animatorMovesX[x].ease);            
        }
    }
    protected void MoveY(Transform transform)
    {
        if (animatorMovesY.Length <= y) return;
        //Y�ړ��AX�ړ��Ɠ���
        float movey = (animatorMovesY[y].relative ? GameManager.playerManager.transform.position.y : -5.5f) + animatorMovesY[y].move;
        tweenerMoveY = transform.DOMoveY(movey, animatorMovesY[y].duration)
            .SetDelay(animatorMovesY[y].delay)
            .OnComplete(() =>
            {
                y++;
                MoveY(transform);
            })
            .SetLoops(animatorMovesY[y].yoyo ? 2 : 1, LoopType.Yoyo)
            .SetEase(animatorMovesY[y].ease);
    }
}
[System.Serializable]
public class ParticleSystemDelay
{
    public ParticleSystem particleSystem;
    public float delay;//�G�t�F�N�g�̃f�B���C����
}
[System.Serializable]
public class SpeechBubble
{
    public string str;//���e
    public float delay;//�f�B���C����
    public PlayerManager.SpeechBubbleColor speechBubbleColor;//�����o���J���[
    public PopText popText { get; set; }//�����o���̒ǉ����~�p
}
[System.Serializable]
public class SpeechBubbleEvo : SpeechBubble
{
    public string between;//�i���X�L�����̒ǉ�����
    public string end;//�i���X�L���̖����e
}