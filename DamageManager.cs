using UnityEngine;
using System.Collections.Generic;

public class DamageManager : MonoBehaviour
{
    List<Damage> damages;//�_���[�W�\��
    [SerializeField] Damage damageObject;//�_���\�W���\�����\�[�X
    int count;//���̃_���[�W�\���z��
    //������
    public void Start0()
    {
        count = 0;
        damages = new List<Damage>();
        damages.Add(Instantiate(damageObject, transform));
        damages[damages.Count - 1].Start0(0);
    }
    //�_���[�W�\���̃Z�b�g
    public void SetDamage(int damage, Transform transform, bool right)
    {
        //�ė��p
        if (!damages[damages[count].nextNum].SetText(damage, transform, right))
        {
            //�g�p���Ȃ�V�K�ō쐬
            damages.Add(Instantiate(damageObject, this.transform));
            damages[damages.Count - 1].Start0(damages[count].nextNum);
            damages[damages.Count - 1].SetText(damage, transform, right);
            damages[count].nextNum = damages.Count - 1;
        }
        count = damages[count].nextNum;
    }
}
