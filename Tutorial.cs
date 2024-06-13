using UnityEngine;
using DG.Tweening;

public class Tutorial : MonoBehaviour
{
    public bool complete { get; private set; }//完了かどうか
    Tweener tweener;
    [SerializeField] float moveValue;//揺れの大きさ
    [SerializeField] bool diretionHorizontal;//方向
    //起動
    public void SetMove()
    {
        if (diretionHorizontal)
        {
            tweener = transform.DOLocalMoveX(transform.localPosition.x + moveValue, 0.5f)
                       .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            tweener = transform.DOLocalMoveY(transform.localPosition.y + moveValue, 0.5f)
                       .SetLoops(-1, LoopType.Yoyo);
        }
    }
    //完了
    public void SetComplete()
    {
        tweener.Kill();
        complete = true;
    }
}
