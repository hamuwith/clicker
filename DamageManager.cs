using UnityEngine;
using System.Collections.Generic;

public class DamageManager : MonoBehaviour
{
    List<Damage> damages;//ダメージ表示
    [SerializeField] Damage damageObject;//ダメ―ジ時表示リソース
    int count;//次のダメージ表示配列
    //初期化
    public void Start0()
    {
        count = 0;
        damages = new List<Damage>();
        damages.Add(Instantiate(damageObject, transform));
        damages[damages.Count - 1].Start0(0);
    }
    //ダメージ表示のセット
    public void SetDamage(int damage, Transform transform, bool right)
    {
        //再利用
        if (!damages[damages[count].nextNum].SetText(damage, transform, right))
        {
            //使用中なら新規で作成
            damages.Add(Instantiate(damageObject, this.transform));
            damages[damages.Count - 1].Start0(damages[count].nextNum);
            damages[damages.Count - 1].SetText(damage, transform, right);
            damages[count].nextNum = damages.Count - 1;
        }
        count = damages[count].nextNum;
    }
}
