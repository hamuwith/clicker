using UnityEngine;
using System.Collections.Generic;

public class SingleHitManager : MonoBehaviour
{
    List<SingleHit> singleHits;//管理
    [SerializeField] SingleHit singleHit;//リソース
    int count;//次の配列番号
    //初期化
    public void Start0()
    {
        count = 0;
        singleHits = new List<SingleHit>();
        singleHits.Add(Instantiate(singleHit, transform));
        singleHits[singleHits.Count - 1].Init(0);
    }
    //セット
    public SingleHit SetParticle(Vector2 vector2, bool left)
    {
        if (!singleHits[singleHits[count].nextNum].Set(vector2, left))
        {
            //使用中なら作成
            singleHits.Add(Instantiate(singleHit, transform));
            singleHits[singleHits.Count - 1].Init(singleHits[count].nextNum);
            singleHits[singleHits.Count - 1].Set(vector2, left);
            singleHits[count].nextNum = singleHits.Count - 1;
        }
        SingleHit singleHit0 = singleHits[singleHits[count].nextNum];
        count = singleHits[count].nextNum;
        return singleHit0;
    }
}
