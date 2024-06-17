using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] EnemySet[] enemySets;//�X�e�[�W�̓G�̏��
    List<Enemy> enemies;//�G���X�g
    float time;//�G�ǉ����ԃJ�E���g
    public bool stopSpown;//�{�X���X�|���̒�~
    [SerializeField] Transform spawnPosition;//�X�|���ʒu
    [SerializeField] Transform deathPosition;//�f�X�ʒu
    readonly float enemySpawn = 1f;//�G�ǉ�����
    Action<float> onGroupSpawn;
    int groupNum;
    int groupCount;
    //������
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
                        enemySet.count++;
                        enemies.Add(Instantiate(enemySet.enemy, transform));
                        if (enemies[enemies.Count - 1].groupSize > 0f) groupNum = enemySet.num;
                        else time = 0f;
                        enemies[enemies.Count - 1].Start0(spawnPosition, deathPosition);
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
    //�G�̍폜
    public bool DestroyEnemy(bool all)
    {
        if (enemies.Count > 0) return false;
        if (all)
        {
            Destroy(enemies[enemies.Count - 1].gameObject);
            enemies.RemoveAt(enemies.Count - 1);
            return enemies.Count > 0;
        }
        else
        {
            Destroy(enemies[enemies.Count - 2].gameObject);
            enemies.RemoveAt(enemies.Count - 2);
            return enemies.Count > 1;
        }
    }
    //�v���C���[���|���ꂽ�Ƃ��A���߂���
    public void ClearStage()
    {
        if (enemies.Count <= 0)
        {
            //�X�e�[�W����������
            foreach (var enemySet in enemySets)
            {
                enemySet.count = 0;
            }
            stopSpown = false;
        }
    }
    [System.Serializable]
    public class EnemySet
    {
        public Enemy enemy;//�G�̃��\�[�X
        public float distance;//�v���C���[�̂����񂾋����ɑ΂���G��������
        public int num;//�G������
        public int count { get; set; }//�G�����J�E���g
        public bool boss;//�{�X�t���O
    }
}
