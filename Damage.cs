using UnityEngine;
using TMPro;
using DG.Tweening;

public class Damage : MonoBehaviour
{
    [SerializeField] protected TextMeshPro textMesh;//ダメージ量表示
    //再利用用
    public int nextNum { get; set; }
    protected bool exist;
    //初期化
    public void Start0(int nextNum)
    {
        this.nextNum = nextNum;
        exist = false;
    }
    //ダメージのセット
    public virtual bool SetText(int value, Transform transform,bool right)
    {
        if (exist) return false;
        this.transform.position = transform.position + Vector3.up * 2f;
        textMesh.text = value.ToString("#,#");
        if (value < 100) textMesh.transform.localScale = Vector3.one;
        else if (value < 500) textMesh.transform.localScale = Vector3.one * 1.2f;
        else if (value < 1000) textMesh.transform.localScale = Vector3.one * 1.4f;
        else if(value < 5000) textMesh.transform.localScale = Vector3.one * 1.6f;
        else textMesh.transform.localScale = Vector3.one * 1.8f;
        exist = true;
        this.transform.DOMoveY(this.transform.position.y + 1.5f, 0.5f)
            .SetLoops(2, LoopType.Yoyo);
        this.transform.DOMoveX(this.transform.position.x + (right ? 3f : -3f), 1f)
            .SetEase(Ease.Linear);
        textMesh.DOFade(1f, 0.5f)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() => exist = false);
        return true;
    }
}
