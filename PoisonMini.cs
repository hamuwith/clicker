using UnityEngine;
//進化毒スキルクリック時の小さな毒
public class PoisonMini : SingleHit
{
    [SerializeField] AttackCollisionValue attackCollisionValue;
    public override void Stop(Transform transform, AttackCollisionValue attackCollisionValue)
    {
        particleSystem0.gameObject.SetActive(false);
    }
    public override bool Set()
    {
        if (!base.Set()) return false;
        GameManager.attackCollisionManager.SetCollision(attackCollisionValue, transform.position, this);
        return true;
    }
}
