using UnityEngine;
using System.Collections;

public class Boss1 : Boss
{
    [SerializeField] ParticleSystem particleSystemCounter;//カウンターエフェクト
    [SerializeField] AttackCollisionValue attackCollisionValueCounter;//カウンター攻撃
    protected override bool Counter()
    {
        if (particleSystemSkills[2].particleDelays[0].particleSystem.IsAlive())
        {
            particleSystemCounter.Play();
            GameManager.attackCollisionManagerBoss.SetCollision(attackCollisionValueCounter, transform.position, null);
            return true;
        }
        return false;
    } 
    //固有スキル
    protected override void Special(int skillId)
    {
        if (skillId == 3)
        {
            StartCoroutine(Special3(right));
        }
    }
    IEnumerator Special3(bool right0)
    {
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < 13; i++)
        {
            (float left, float right) = GameManager.gameManager.LeftRight();
            (float top, float down) = GameManager.gameManager.TopDown();
            vector2.x = this.right ? right + 6f : left - 6f;
            vector2.y = Random.Range(top, down + 6f);
            particleSystemSpecial4.transform.position = vector2;
            particleSystemSpecial4.transform.localScale = right0 ? Vector2.one : inversionVector2;
            particleSystemSpecial4.Play();
            AttackCollisionValueSpecial4.left = !right0;
            GameManager.attackCollisionManagerBoss.SetCollision(AttackCollisionValueSpecial4, vector2, null);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
