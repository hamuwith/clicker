using UnityEngine;
using System.Collections.Generic;

public class AttackCollisionManager : MonoBehaviour
{
    public List<AttackCollision> attackCollisions;//�����蔻��o�^���X�g
    [SerializeField] AttackCollision attackCollision;//�����蔻��̃��\�[�X
    int count;//���Ɏg�p����z��ԍ�
    public void Start0()
    {
        //������
        count = 0;
        attackCollisions = new List<AttackCollision>();
        attackCollisions.Add(Instantiate(attackCollision, transform));
        attackCollisions[attackCollisions.Count - 1].Initialization(count);
    }
    public AttackCollision SetCollision(AttackCollisionValue attackCollisionValue, Vector2 position, SingleHit singleHit)
    {
        //�����蔻��̃Z�b�g
        AttackCollision attackCollision0 = attackCollisions[attackCollisions[count].nextNum].SetCollision(attackCollisionValue, position, singleHit);
        //�����蔻����g�p���Ȃ�V���������蔻���ǉ�
        if (attackCollision0 == null)
        {
            attackCollisions.Add(Instantiate(attackCollision, transform));
            attackCollisions[attackCollisions.Count - 1].Initialization(attackCollisions[count].nextNum);
            attackCollision0 = attackCollisions[attackCollisions.Count - 1].SetCollision(attackCollisionValue, position, singleHit);
            attackCollisions[count].nextNum = attackCollisions.Count - 1;
        }
        count = attackCollisions[count].nextNum;
        return attackCollision0;
    }
}
