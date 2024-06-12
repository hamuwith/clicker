using UnityEngine;
using TMPro;
using DG.Tweening;

public class Exp : Damage
{
    //経験値表示のセット
    public override bool SetText(int value, Transform transform, bool right)
    {
        if (exist) return false;
        this.transform.position = transform.position;
        textMesh.text = value.ToString("#,#");
        exist = true;
        //経験値に応じたスケール
        if (value < 20) textMesh.transform.localScale = Vector3.one;
        else if (value < 100) textMesh.transform.localScale = Vector3.one * 1.2f;
        else if (value < 500) textMesh.transform.localScale = Vector3.one * 1.4f;
        else if (value < 2000) textMesh.transform.localScale = Vector3.one * 1.6f;
        else textMesh.transform.localScale = Vector3.one * 1.8f;
        //動き
        this.transform.DOLocalMoveX(0f, 0.7f)
            .OnComplete(() => 
            {
                exist = false;
                GameManager.gameManager.SetExp(value);
            })
            .SetEase(right ? Ease.OutQuad : Ease.InBack);
        this.transform.DOLocalMoveY(0f, 0.7f)
            .SetEase(Ease.InOutSine);
        textMesh.DOFade(1f, 0.35f)
            .SetLoops(2, LoopType.Yoyo);
        return true;
    }
}
