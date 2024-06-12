using UnityEngine;
using System.Collections.Generic;

public class PopTextManager : MonoBehaviour
{
    public List<PopText> popTexts;//�����o���̊Ǘ�
    [SerializeField] PopText popText;//���\�[�X
    int count;//���̐����o���z��ԍ�
    [SerializeField] Sprite[] sprites;//�����o���̃X�v���C�g
    //������
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
    //�����o���̃Z�b�g
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
