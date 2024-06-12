using UnityEngine;
using System.Collections;

public class Thunder : MonoBehaviour
{
    [SerializeField] ParticleSystem particleSystem0;//エフェクト
    [SerializeField] AttackCollisionValue attackCollisionValue;//パラメータ
    static public bool flag;//攻撃一回につき一度のみ
    Vector2 vector2;
    //セット
    public void Set(Transform transform)
    {
        if (!flag) return;
        StartCoroutine(SetDelay(transform));
        flag = false;
    }
    //攻撃一回につきなんどでも
    public void SetAllOk(Transform transform)
    {
        StartCoroutine(SetDelay(transform));
    }
    //セット
    IEnumerator SetDelay(Transform transform)
    {
        yield return new WaitForSeconds(0.2f);
        vector2 = transform.position;
        vector2.y = this.transform.position.y;
        this.transform.position = vector2;
        particleSystem0.Play();
        GameManager.attackCollisionManager.SetCollision(attackCollisionValue, vector2, null);
    }
}
