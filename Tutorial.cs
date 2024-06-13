using UnityEngine;
using DG.Tweening;

public class Tutorial : MonoBehaviour
{
    public bool complete { get; private set; }//�������ǂ���
    Tweener tweener;
    [SerializeField] float moveValue;//�h��̑傫��
    [SerializeField] bool diretionHorizontal;//����
    //�N��
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
    //����
    public void SetComplete()
    {
        tweener.Kill();
        complete = true;
    }
}
