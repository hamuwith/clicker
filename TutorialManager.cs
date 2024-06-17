using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] Tutorial[] tutorials;//チュートリアル
    [SerializeField] bool notActive;//チュ―トリアルか
    //初期化
    public void Start0()
    {
        foreach(var tutorial in tutorials)
        {
            tutorial.gameObject.SetActive(false);
        }
    }
    //チュートリアルの表示と終了
    public void SetActive(int id, bool active)
    {
        if (notActive) return;
        if (tutorials.Length <= id || tutorials[id].complete) return;
        if (active && !tutorials[id].gameObject.activeSelf)
        {
            tutorials[id].gameObject.SetActive(active);
            tutorials[id].SetMove();
        }
        else if (!active && tutorials[id].gameObject.activeSelf)
        {
            tutorials[id].transform.SetParent(null);
            tutorials[id].SetComplete();
            if (id == 0) SetActive(1, true);
            else if (id == 2) SetActive(5, true);
        }
    }
}
