using UnityEngine;
using DG.Tweening;

public class Tutorial : MonoBehaviour
{
    public bool complete { get; private set; }//Š®—¹‚©‚Ç‚¤‚©
    Tweener tweener;
    [SerializeField] float moveValue;//—h‚ê‚Ì‘å‚«‚³
    [SerializeField] bool diretionHorizontal;//•ûŒü
    //‹N“®
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
    //Š®—¹
    public void SetComplete()
    {
        tweener.Kill();
        complete = true;
    }
}
