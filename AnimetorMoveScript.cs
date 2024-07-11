using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AnimetorMoveScript : StateMachineBehaviour
{
	[SerializeField] protected AnimatorMove[] animatorMovesX;//X軸への移動
    [SerializeField] AnimatorMove[] animatorMovesY;//Y軸への移動
    [SerializeField] protected AttackCollisionValue[] attackCollisions;//アニメーションの当たり判定
    [SerializeField] bool cameraMove;//移動に対してカメラの固定するか
    [SerializeField] protected bool sub;//サブプレイヤーか
    protected Tweener tweenerMoveX;//アニメーションキャンセル時キルするため
    protected Tweener tweenerMoveY;//アニメーションキャンセル時キルするため
    protected int x;//X移動インデックス用
    protected int y;//Y移動インデックス用
    [SerializeField] protected Afterimage.Condition condition;//アニメーションの状態
    [SerializeField] protected float invincibleTime = 0.7f;//無敵時間のセット
    [SerializeField] protected int skillId = -1;//スキルID
    [SerializeField] float clickDuration = -1f;//進化スキル用連打可能時間
    [SerializeField] float clickDelay;//進化スキル用連打可能時間ディレイ
    [SerializeField] ParticleSystemDelay[] particleSystemDelays;//アニメーションのエフェクト
    protected Vector2 vector2;//一時
    [SerializeField] SpeechBubble[] speechBubbles;//吹き出しの設定
    [SerializeField] SpeechBubbleEvo[] speechBubbleEvos;//進化スキル時の吹き出しの設定
    [SerializeField] protected bool autoAttack;
    new public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        PlayerManager playerManager = sub ? GameManager.playerManagerSub : GameManager.playerManager;
        base.OnStateEnter(animator, stateInfo, layerIndex);
        x = 0;
        y = 0;
        //カメラ固定の設定
        if (GameManager.playerManager.animator == animator) CameraManager.fix = !cameraMove;
        //アニメーションスタート
        playerManager.AnimationStart(animator, condition, stateInfo.shortNameHash, invincibleTime, skillId, clickDelay, clickDuration);
        //動きの設定
        MoveX(animator.transform, animator, playerManager.left);
        MoveY(animator.transform);
        if (GameManager.playerManager.animator != animator && !sub) return;
        //吹き出しの設定
        if (skillId >= 0 && GameManager.gameManager.GetEvo(playerManager, skillId) && speechBubbleEvos.Length > 0)
        {
            //進化時、クリック数に応じてコメントを追加用の設定
            foreach (var speechBubble in speechBubbleEvos)
            {
                if(speechBubble.between == null || speechBubble.between == "") speechBubble.popText = GameManager.popTextManager.SetPopText(speechBubble.str, playerManager, playerManager.GetSpeechBubbleColor(speechBubble.speechBubbleColor), duration: 1f, delay: speechBubble.delay);
                speechBubble.popText = GameManager.popTextManager.SetPopText(speechBubble.str, playerManager, playerManager.GetSpeechBubbleColor(speechBubble.speechBubbleColor), betweentext: speechBubble.between, endtext: speechBubble.end, delay: speechBubble.delay);
            }
        }
        else
        {
            //吹き出しの設定
            foreach (var speechBubble in speechBubbles)
            {
                speechBubble.popText = GameManager.popTextManager.SetPopText(speechBubble.str, playerManager, playerManager.GetSpeechBubbleColor(speechBubble.speechBubbleColor), duration : skillId >= 0 ? 1f : 0f, delay: speechBubble.delay);
            }
        }
        //当たり判定の設定
        foreach (var attackCollision in attackCollisions)   
        {
            if (attackCollision.special == AttackCollisionValue.Special.WindAdd && !GameManager.gameManager.GetEvo(GameManager.playerManagerSub, attackCollision)) return;
            attackCollision.sub = sub;
            attackCollision.left = playerManager.left;
            //シングルヒット攻撃の処理
            SingleHit singleHitObject = null;
            if(attackCollision.special == AttackCollisionValue.Special.Fire)
            {
                vector2 = attackCollision.position + Vector2.left;
                if(playerManager.left) vector2.x = -vector2.x;
                vector2 += (Vector2)GameManager.playerManagerSub.transform.position;
                singleHitObject = GameManager.singleHitManager.SetParticle(vector2, playerManager.left);
            }
            //各当たり判定の設定
            attackCollision.attackCollision = GameManager.attackCollisionManager.SetCollision(attackCollision, animator.transform.position, singleHitObject);
            //雷攻撃は1回の攻撃に対して1度のみ
            if (attackCollision.special == AttackCollisionValue.Special.Thunder && GameManager.gameManager.GetEvo(GameManager.playerManager, attackCollision)) Thunder.flag = true;
            //狛犬スキルを使用時、通常攻撃増加
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
        //エフェクトの停止
        playerManager.ParticlesStopSkill(skillId);
        //当たり判定の停止
        foreach (var attackCollision in attackCollisions)
        {
            attackCollision.attackCollision?.End(attackCollision.dependence);
        }
        //吹き出しの停止
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
            //X移動終了時カメラの固定解除
            if (GameManager.playerManager.animator == animator) CameraManager.fix = false;
            return;
        }
        //X移動
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
        //Y移動、X移動と同じ
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
    public float delay;//エフェクトのディレイ時間
}
[System.Serializable]
public class SpeechBubble
{
    public string str;//内容
    public float delay;//ディレイ時間
    public PlayerManager.SpeechBubbleColor speechBubbleColor;//吹き出しカラー
    public PopText popText { get; set; }//吹き出しの追加や停止用
}
[System.Serializable]
public class SpeechBubbleEvo : SpeechBubble
{
    public string between;//進化スキル時の追加文字
    public string end;//進化スキルの末内容
}