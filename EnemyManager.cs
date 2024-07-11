using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] StageEnemySet[] stageEnemySets;//ステージの敵の情報
    List<Enemy> enemies;//敵リスト
    float time;//敵追加時間カウント
    public bool stopSpown;//ボス時スポンの停止
    [SerializeField] Transform spawnPosition;//スポン位置
    [SerializeField] Transform deathPosition;//デス位置
    readonly float enemySpawn = 1f;//敵追加時間
    Action<float> onGroupSpawn;//グループスポン
    int groupNum;//グループで生成される敵
    int groupCount;//グループで生成される敵のカウント
    bool boss;
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
                foreach(var enemySet in stageEnemySets[GameManager.stage].enemySets)
                {
                    if(enemySet.distance <= GameManager.playerManager.distance && enemySet.count < enemySet.num)
                    {
                        enemySet.count++;
                        enemies.Add(Instantiate(enemySet.enemy, transform));
                        if (enemies[enemies.Count - 1].groupSize > 0f) groupNum = enemySet.num;
                        else time = 0f;
                        enemies[enemies.Count - 1].Start0(spawnPosition, deathPosition);
                        if (enemySet.boss) boss = true;
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
    public void SetGroupSpawn(Action<float, int> action)
    {
        int group = groupCount;
        onGroupSpawn += (f) => action(f, group);
        groupCount++;
        if (groupNum > groupCount) return;
        onGroupSpawn?.Invoke(GetRandom());
        groupCount = 0;
        onGroupSpawn = null;
    }
    public float GetRandom()
    {
        return UnityEngine.Random.Range(0f, 2f);
    }
    //敵の削除
    public bool DestroyEnemy(bool all)
    {
        if (all)
        {
            if (enemies.Count <= 0) return false;
            Destroy(enemies[enemies.Count - 1].gameObject);
            enemies.RemoveAt(enemies.Count - 1);
            GameManager.boss = null;
            GameManager.bossSub = null;
            return enemies.Count > 0;
        }
        else
        {
            if (enemies.Count <= 0) return false;
            int num = 1;
            while (true)
            {
                if (enemies[enemies.Count - num] == GameManager.boss || GameManager.bossSub)
                {
                    num++;
                    if (enemies.Count < num) return false;
                }
                else
                {
                    Destroy(enemies[enemies.Count - num].gameObject);
                    enemies.RemoveAt(enemies.Count - num);
                    return enemies.Count - num + 1 > 0;
                }
            }            
        }
    }
    //クリア判定
    public void BossDeath(Boss boss)
    {
        if(GameManager.boss == boss)
        {
            GameManager.boss = null;
        }
        else if (GameManager.bossSub == boss)
        {
            GameManager.bossSub = null;
        }
        if(GameManager.boss == null && GameManager.bossSub == null )
        {
            if (this.boss) GameManager.gameManager.Clear();
            else stopSpown = false;
        }
    }
    //プレイヤーが倒されたとき、初めから
    public void ClearStage()
    {
        if (enemies.Count <= 0)
        {
            //ステージ初期化処理
            foreach (var enemySet in stageEnemySets[GameManager.stage].enemySets)
            {
                enemySet.count = 0;
            }
            stopSpown = false;
            boss = false;
        }
    }
    [System.Serializable]
    public class StageEnemySet
    {
        public EnemySet[] enemySets;//ステージの敵の情報
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
