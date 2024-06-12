using UnityEngine;
using System.Collections.Generic;

public class ExpManager : MonoBehaviour
{
    List<Exp> exps;//経験値表示リスト
    [SerializeField] Exp expObject;//経験値表示リソース
    int count;//再利用用
    //初期化
    public void Start0()
    {
        count = 0;
        exps = new List<Exp>();
        exps.Add(Instantiate(expObject, transform));
        exps[exps.Count - 1].Start0(0);
    }
    //経験値表示のセット
    public void SetExp(int exp, Transform transform, bool right)
    {
        if (!exps[exps[count].nextNum].SetText(exp, transform, right))
        {
            //使用中なら作成
            exps.Add(Instantiate(expObject, this.transform));
            exps[exps.Count - 1].Start0(exps[count].nextNum);
            exps[exps.Count - 1].SetText(exp, transform, right);
            exps[count].nextNum = exps.Count - 1;
        }
        count = exps[count].nextNum;
    }
}
