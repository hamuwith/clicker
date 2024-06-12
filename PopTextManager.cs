using UnityEngine;
using System.Collections.Generic;

public class PopTextManager : MonoBehaviour
{
    public List<PopText> popTexts;//吹き出しの管理
    [SerializeField] PopText popText;//リソース
    int count;//次の吹き出し配列番号
    [SerializeField] Sprite[] sprites;//吹き出しのスプライト
    //初期化
    public void Start0()
    {
        count = 0;
        popTexts = new List<PopText>();
        popTexts.Add(Instantiate(popText, transform));
        popTexts[popTexts.Count - 1].Initialization(count);
    }
    public enum Kind
    {
        Armor,
        Invincible,
        Guard,
        Avoidance,
        Another
    }
    //吹き出しのセット
    public PopText SetPopText(in string firsttext, PlayerManager playerManager, in Color color, Kind kind = Kind.Another, in string betweentext = null, in string endtext = null, float duration = 0f, float delay = 0f)
    {
        PopText popText0 = popTexts[popTexts[count].nextNum].SetPopText(firsttext, playerManager, color, sprites[(int)kind], betweentext, endtext, this, duration, delay);
        if (popText0 == null)
        {
            popTexts.Add(Instantiate(popText, transform));
            popTexts[popTexts.Count - 1].Initialization(popTexts[count].nextNum);
            popText0 = popTexts[popTexts.Count - 1].SetPopText(firsttext, playerManager, color, sprites[(int)kind], betweentext, endtext, this, duration, delay);
            popTexts[count].nextNum = popTexts.Count - 1;
        }
        count = popTexts[count].nextNum;
        return popText0;
    }
}
