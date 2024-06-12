using UnityEngine;
using System.Collections.Generic;

public class SingleHitManager : MonoBehaviour
{
    List<SingleHit> singleHits;//�Ǘ�
    [SerializeField] SingleHit singleHit;//���\�[�X
    int count;//���̔z��ԍ�
    //������
    public void Start0()
    {
        count = 0;
        singleHits = new List<SingleHit>();
        singleHits.Add(Instantiate(singleHit, transform));
        singleHits[singleHits.Count - 1].Init(0);
    }
    //�Z�b�g
    public SingleHit SetParticle(Vector2 vector2, bool left)
    {
        if (!singleHits[singleHits[count].nextNum].Set(vector2, left))
        {
            //�g�p���Ȃ�쐬
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
