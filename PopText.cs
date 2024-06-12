using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class PopText : MonoBehaviour
{
    [SerializeField] TextMeshPro textMeshPro;//吹き出しコメント
    [SerializeField] SpriteRenderer spriteRenderer;//吹き出し画像
    bool exist0;//再利用用
    readonly float oneSize = 0.8f;//一文字当たりのサイズ
    readonly float baseSize = 2f;//基本サイズ
    Tweener tweener;//追加コメント時完了用
    bool exist 
    {
        get
        {
            return exist0;
        }
        set
        {
            exist0 = value;
            textMeshPro.enabled = exist0;
            spriteRenderer.enabled = exist0;
        } 
    }
    public int nextNum { get; set; }
    PlayerManager playerManager;//プレイヤー
    Vector2 vector2;//一時利用
    string first;//進化スキルの文頭・吹き出しの本文
    string between;//中間追加文字
    string end;//文末
    string str;//追加文
    PopTextManager popTextManager;//親
    IEnumerator enumerator;//キャンセル用
    //初期化
    public void Initialization(int nextNum)
    {
        this.nextNum = nextNum;
        exist = false;
        vector2 = spriteRenderer.size;
    }
    //吹き出しのセットディレイ
    public PopText SetPopText(in string firsttext, PlayerManager playerManager, in Color color, Sprite sprite, in string betweentext, in string endtext, PopTextManager popTextManager, float duration, float delay)
    {
        if (exist) return null;
        exist = true;
        if(delay > 0f)
        {
            enumerator = SetPopTextDelay(firsttext, playerManager, color, sprite, betweentext, endtext, popTextManager, duration, delay);
            StartCoroutine(enumerator);
        }
        else
        {
            SetPopTextReal(firsttext, playerManager, color, sprite, betweentext, endtext, popTextManager, duration);
        }
        return this;
    }
    //吹き出しセット
    void SetPopTextReal(in string firsttext, PlayerManager playerManager, in Color color, Sprite sprite, in string betweentext, in string endtext, PopTextManager popTextManager, float duration)
    {
        this.playerManager = playerManager;
        spriteRenderer.sprite = sprite;
        transform.parent = playerManager.popTextTransform;
        transform.localPosition = (playerManager.left ? 2f : -2f) * Vector3.right;
        spriteRenderer.color = color;
        textMeshPro.color = Color.black;
        textMeshPro.sortingOrder = 91;
        spriteRenderer.sortingOrder = 90;
        this.popTextManager = popTextManager;
        if (betweentext == null || betweentext == "")
        {
            textMeshPro.text = firsttext;
            vector2.x = textMeshPro.text.Length * oneSize + baseSize;
            spriteRenderer.size = vector2;
            End(duration);
        }
        else
        {
            this.playerManager.popTextEvo = this;
            first = firsttext;
            end = endtext;
            between = betweentext;
            textMeshPro.text = $"{firsttext}{endtext}";
            vector2.x = textMeshPro.text.Length * oneSize + baseSize;
            spriteRenderer.size = vector2;
            str = "";
        }
    }
    //吹き出しエンド処理
    public void End(float delay = 0f)
    {
        if (delay == 0f)
        {
            transform.parent = popTextManager.transform;
            transform.DOMoveX(transform.position.x + (playerManager.left ? 2f : -2f), 1f)
                .SetEase(Ease.Linear);
            transform.DOMoveY(transform.position.y - 3f, 1f)
                .SetEase(Ease.InBack);
            spriteRenderer.DOFade(0f, 1f)
                .SetEase(Ease.InCubic);
            textMeshPro.DOFade(0f, 1f)
                .SetEase(Ease.InCubic)
                .OnComplete(() => 
                {
                    exist = false;
                });
            DOTween.To(() => 90, (x) =>
            {
                textMeshPro.sortingOrder = x + 1;
                spriteRenderer.sortingOrder = x;
            }, 70, 1f);
        }
        else
        {
            transform.parent = popTextManager.transform;
            transform.DOMoveX(transform.position.x + (playerManager.left ? 2f : -2f), 1f)
                .SetEase(Ease.Linear)
                .SetDelay(delay);
            transform.DOMoveY(transform.position.y - 3f, 1f)
                .SetEase(Ease.InBack)
                .SetDelay(delay);
            spriteRenderer.DOFade(0f, 1f)
                .SetEase(Ease.InCubic)
                .SetDelay(delay);
            textMeshPro.DOFade(0f, 1f)
                .SetEase(Ease.InCubic)
                .OnComplete(() => exist = false)
                .SetDelay(delay);
            DOTween.To(() => 90, (x) =>
            {
                textMeshPro.sortingOrder = x + 1;
                spriteRenderer.sortingOrder = x;
            }, 70, 1f)
                .SetDelay(delay);
        }
    }
    //吹き出し追加処理
    public void UpdateText()
    {
        str += between;
        textMeshPro.text = $"{first}{str}{end}";
        vector2.x = textMeshPro.text.Length * oneSize + baseSize;
        spriteRenderer.size = vector2;
        tweener?.Complete();
        tweener = transform.DOScale(Vector3.one * 1.2f, 0.08f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }
    //吹き出しのディレイ
    IEnumerator SetPopTextDelay(string firsttext, PlayerManager playerManager, Color color, Sprite sprite, string betweentext, string endtext, PopTextManager popTextManager, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetPopTextReal(firsttext, playerManager, color, sprite, betweentext, endtext, popTextManager, duration);
        enumerator = null;
    }
    //吹き出しのキャンセル
    public void StopDisplay()
    {
        if (enumerator == null) return;
        StopCoroutine(enumerator);
        exist = false;
    }
}
