using UnityEngine;
using System.Collections.Generic;

public class ExpManager : MonoBehaviour
{
    List<Exp> exps;//�o���l�\�����X�g
    [SerializeField] Exp expObject;//�o���l�\�����\�[�X
    int count;//�ė��p�p
    //������
    public void Start0()
    {
        count = 0;
        exps = new List<Exp>();
        exps.Add(Instantiate(expObject, transform));
        exps[exps.Count - 1].Start0(0);
    }
    //�o���l�\���̃Z�b�g
    public void SetExp(int exp, Transform transform, bool right)
    {
        if (!exps[exps[count].nextNum].SetText(exp, transform, right))
        {
            //�g�p���Ȃ�쐬
            exps.Add(Instantiate(expObject, this.transform));
            exps[exps.Count - 1].Start0(exps[count].nextNum);
            exps[exps.Count - 1].SetText(exp, transform, right);
            exps[count].nextNum = exps.Count - 1;
        }
        count = exps[count].nextNum;
    }
}
