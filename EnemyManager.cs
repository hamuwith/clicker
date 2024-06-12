using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] EnemySet[] enemySets;//ステージの敵の情報
    List<Enemy> enemies;//敵リスト
    float time;//敵追加時間カウント
    public bool stopSpown;//ボス時スポンの停止
    [SerializeField] Transform spawnPosition;//スポン位置
    readonly float enemySpawn = 1f;//敵追加時間
    //初期化
    public void Start0()
    {
        enemies = new List<Enemy>();
        time = 0f;
        Enemy.inversionVector2 = new Vector2(-1f,1f);
    }
    public void Update0()
    {
        if (!stopSpown)
        {
            time += Time.deltaTime;
            if (time >= enemySpawn)
            {
                foreach(var enemySet in enemySets)
                {
                    if(enemySet.distance <= GameManager.playerManager.distance && enemySet.count < enemySet.num)
                    {
                        time = 0f;
                        enemySet.count++;
                        enemies.Add(Instantiate(enemySet.enemy, transform));
                        enemies[enemies.Count - 1].Start0(spawnPosition);
                        if (enemySet.boss) stopSpown = true;
                        break;
                    }
                }
            }
        }
        foreach(var enemy in enemies)
        {
            enemy.Update0();
        }
    }
    //プレイヤーが倒されたときの敵の削除
    public bool DestroyEnemy()
    {
        if (enemies.Count <= 0)
        {
            foreach (var enemySet in enemySets)
            {
                enemySet.count = 0;
            }
            return false;
        }
        Destroy(enemies[enemies.Count - 1].gameObject);
        enemies.RemoveAt(enemies.Count - 1);
        return true;
    }
    [System.Serializable]
    public class EnemySet
    {
        public Enemy enemy;//敵のリソース
        public float distance;//プレイヤーのすすんだ距離に対する敵発生距離
        public int num;//敵発生数
        public int count { get; set; }//敵発生カウント
        public bool boss;//ボスフラグ
    }
}
