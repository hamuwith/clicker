using UnityEngine;
using System.Collections.Generic;

public class AttackCollisionManager : MonoBehaviour
{
    public List<AttackCollision> attackCollisions;//当たり判定登録リスト
    [SerializeField] AttackCollision attackCollision;//当たり判定のリソース
    int count;//次に使用する配列番号
    public void Start0()
    {
        //初期化
        count = 0;
        attackCollisions = new List<AttackCollision>();
        attackCollisions.Add(Instantiate(attackCollision, transform));
        attackCollisions[attackCollisions.Count - 1].Initialization(count);
    }
    public AttackCollision SetCollision(AttackCollisionValue attackCollisionValue, Vector2 position, SingleHit singleHit)
    {
        //当たり判定のセット
        AttackCollision attackCollision0 = attackCollisions[attackCollisions[count].nextNum].SetCollision(attackCollisionValue, position, singleHit);
        //当たり判定を使用中なら新しい当たり判定を追加
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
