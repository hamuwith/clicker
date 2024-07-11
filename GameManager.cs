using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    /*すること
    空中で起き上がるバグ
    */
    public static GameManager gameManager;//this
    public static PlayerManager playerManager;//プレイヤー
    public static PlayerManagerSub playerManagerSub;//プレイヤー
    public static Boss boss;//ボス
    public static Boss bossSub;//サブボス
    public static AttackCollisionManager attackCollisionManager;//当たり判定プレイヤー用
    public static AttackCollisionManager attackCollisionManagerBoss;//当たり判定ボス用
    public static CameraManager cameraManager;//カメラ
    public static EnemyManager enemyManager;//敵マネージャー
    public static DamageManager damageManager;//プレイヤーのダメージ表示
    public static ExpManager[] expManagers;//経験値表示
    public static SingleHitManager singleHitManager;//サブプレイヤーの通常攻撃
    public static SingleHitManager poisonManager;//サブプレイヤーのシングルターゲットスキル
    public static Effect[] effect;//状態異常の設定
    public static PopTextManager popTextManager;//吹き出しマネージャー
    public static CutIn cutIn;//スキルカットイン
    public static TutorialManager tutorialManager;//チュートリアル
    public UIDocument uIDocument;//UIスキルボタン
    public UIDocument uIDocumentUpgrade;//UIレベルアップボタン
    bool skillPlayer;//スキルボタンクリック時true
    bool skillPlayerSub;//スキルボタンクリック時true
    readonly Color gray = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Character character;//プレイヤーのキャラクター設定
    public Character characterSub;//サブプレイヤーのキャラクター設定
    Label arrowLabel;//表示非表示ラベル
    Label goldLabel;//保有ゴールドラベル
    int gold0;
    public Texture2D texture2DAuto;//スキルの自動手動切替画像
    public Texture2D texture2DManual;//スキルの自動手動切替画像
    readonly public int clickHash = Shader.PropertyToID("_click");
    readonly public int skillHash = Shader.PropertyToID("_skill");
    readonly public int scaleHash = Shader.PropertyToID("_scale");
    readonly public int colorHash = Shader.PropertyToID("_Color");
    public Transform mousePoint;//マウスの座標
    [SerializeField] Transform leftUp;//画面外座標
    [SerializeField] Transform rightDown;//画面外座標
    [SerializeField] ParticleSystem particleSystemKill;//吹っ飛びエフェクト
    public Transform expPositionTransform;//経験値終点座標
    [SerializeField] BackGroundManager[] backgroundManagers;//背景プレハブ
    BackGroundManager backgroundManager;//背景
    Label backstep;//バックステップボタン
    Label guard;//ガードボタン
    Tweener tweener;//ゴールド表示
    [SerializeField] Material transitionMaterial;//倒されたときの視界
    [SerializeField] Material stageTransitionMaterial;//ステージクリア時のトランジション
    [SerializeField] Material blurMaterial;//倒されたときのブラー
    readonly int transitionProp = Shader.PropertyToID("_progress");
    readonly int blurProp = Shader.PropertyToID("_blur");
    readonly int isBlurProp = Shader.PropertyToID("_isBlur");
    readonly float transitionSpeed = 0.5f;//倒されたときの視界のスピード
    readonly float transitionReSpeed = 0.2f;//蘇生時の視界のスピード
    float transitionCount;//倒されたときのトランジションカウント
    [SerializeField] Clear clear;
    [SerializeField] GameObject clearDoorObject;
    public static GameObject clearDoor;
    public static int stage;
    readonly float transition0 = -0.2f;//トランジションオフセット
    readonly float transition1 = 1.2f;//トランジションオフセット
    readonly float transitionStageSpeed = 2f;//倒されたときの視界のスピード
    public int gold 
    { 
        get 
        { 
            return gold0; 
        } 
        set
        {
            //徐々に変化
            gold0 = value;
            tweener?.Kill();
            tweener = DOTween.To(() => goldLabelValue, x => 
            {
                goldLabelValue = x;
                goldLabel.text = $"{x.ToString("#,0")}P";
                
            }, gold0, 1f)
                .SetEase(Ease.OutSine);
            StatusSet(character, gold0);
            StatusSet(characterSub, gold0);
        } 
    }
    int goldLabelValue;//表示ゴールドの値
    readonly float hiddenPosition = -338f;//アップグレードの非表示位置
    public readonly Vector2 hanten = new Vector2(1f, -1f);
    public readonly Vector2 hantenLine = new Vector2(-1f, 1f);
    //蘇生トランジション
    public IEnumerator Transition()
    {
        transitionCount = transition1;
        blurMaterial.SetInt(isBlurProp, 1);
        yield return TransitionSub(0.7f,transitionSpeed);
        do
        {
            yield return TransitionSub(Random.Range(0.2f, 0.3f), transitionReSpeed);
            yield return TransitionSub(Random.Range(0.35f, 0.45f), transitionReSpeed);
        } while (playerManagerSub.skill && playerManagerSub.gameObject.activeSelf);
        yield return TransitionSub(transition0, transitionSpeed);
        if (!playerManagerSub.gameObject.activeSelf)
        {
            playerManagerSub.gameObject.SetActive(true);
            playerManagerSub.SetAvtive(true);
        }
        StartCoroutine(ResetStage(false));
        yield return new WaitForSeconds(1f);
        playerManagerSub.Resuscitation();
        do
        {
            yield return TransitionSub(Random.Range(0.4f, 0.5f), transitionReSpeed);
            yield return TransitionSub(Random.Range(0.25f, 0.35f), transitionReSpeed);
        } while (playerManager.hp <= 0);
        yield return TransitionSub(transition1, transitionSpeed);
        blurMaterial.SetInt(isBlurProp, 0);
    }
    //倒された時、ステージクリアしたとき
    IEnumerator ResetStage(bool next)
    {
        playerManager.distance = 0f;
        yield return EnemyDestroy(true);
        if (next)
        {
            stage++;
            Destroy(backgroundManager.gameObject);
            backgroundManager = Instantiate(backgroundManagers[stage], new Vector3(cameraManager.transform.position.x, 0f, 0f), Quaternion.identity);
            backgroundManager.Start0(cameraManager.transform);
        } 
        enemyManager.ClearStage();
        cameraManager.ResetCamera();
    }
    //敵の削除
    public IEnumerator EnemyDestroy(bool all)
    {
        bool end = true;
        while (end)
        {
            end = enemyManager.DestroyEnemy(all);
            yield return null;
        }
    }
    //トランジション関数
    IEnumerator TransitionSub(float value, float speed)
    {
        if(transitionCount > value)
        {
            do
            {
                yield return null;
                transitionCount -= Time.deltaTime * speed;
                transitionMaterial.SetFloat(transitionProp, transitionCount);
                blurMaterial.SetFloat(blurProp, (1f - transitionCount) * 0.002f);
            } while (transitionCount > value);
        }
        else
        {
            do
            {
                yield return null;
                transitionCount += Time.deltaTime * transitionSpeed;
                transitionMaterial.SetFloat(transitionProp, transitionCount);
                blurMaterial.SetFloat(blurProp, (1f - transitionCount) * 0.002f);
            } while (transitionCount <= value);
        }
    }
    void Awake()
    {
        if (gameManager != null)
        {
            Destroy(gameObject);
            return;
        }
        stage = 3;
        DontDestroyOnLoad(gameObject);
        gameManager = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        //状態異常のセット
        effect = new Effect[(int)AttackCollisionValue.Effect.Length - 1];
        for(int i = 0; i < (int)AttackCollisionValue.Effect.Length - 1; i++)
        {
            effect[i] = new Effect((AttackCollisionValue.Effect)i + 1); 
        }
        //UIの設定
        List<Label> buttons = uIDocument.rootVisualElement.Query<Label>().ToList();
        for(int i = 0; i < buttons.Count - 2; i++)
        {
            if(i < 5) character.skills[i].skillButton = buttons[i];
            else characterSub.skills[i - 5].skillButton = buttons[i];
            buttons[i].RegisterCallback<MouseDownEvent, int>(ClickEventSkill, i);
        }
        backstep = buttons[buttons.Count - 2];
        backstep.RegisterCallback<MouseDownEvent>(ClickEventBackStep);
        guard = buttons[buttons.Count - 1];
        guard.RegisterCallback<MouseDownEvent>(ClickEventGuard);
        uIDocument.rootVisualElement.RegisterCallback<MouseDownEvent>(ClickEventAttack);
        //ステータス表UIToolkit
        List<Label> labelList = uIDocumentUpgrade.rootVisualElement.Query<Label>().ToList();
        character.nameLabel = labelList[72];
        character.level.lvLabel = labelList[73];
        character.unlockLabel = labelList[140];
        character.unlockValueLabel = labelList[141];
        characterSub.nameLabel = labelList[1];
        characterSub.level.lvLabel = labelList[2];
        characterSub.unlockLabel = labelList[69];
        characterSub.unlockValueLabel = labelList[70];
        for (int i = 0; i < 3; i++)
        {
            character.status[i].nameLabel = labelList[74 + i * 2];
            character.status[i].pointLabel = labelList[75 + i * 2];
            characterSub.status[i].nameLabel = labelList[3 + i * 2];
            characterSub.status[i].pointLabel = labelList[4 + i * 2];
            character.status[i].SetStatus(character);
            characterSub.status[i].SetStatus(characterSub);
        }
        for (int i = 0; i < 5; i++)
        {
            character.skills[i].autoLabel = labelList[81 + i * 12];
            character.skills[i].nameLabel = labelList[82 + i * 12];
            character.skills[i].level.lvLabel = labelList[83 + i * 12];
            characterSub.skills[i].autoLabel = labelList[10 + i * 12];
            characterSub.skills[i].nameLabel = labelList[11 + i * 12];
            characterSub.skills[i].level.lvLabel = labelList[12 + i * 12];
            for (int j = 0; j < 3; j++)
            {
                character.skills[i].status[j].nameLabel = labelList[84 + i * 12 + j * 2];
                character.skills[i].status[j].pointLabel = labelList[85 + i * 12 + j * 2];
                characterSub.skills[i].status[j].nameLabel = labelList[13 + i * 12 + j * 2];
                characterSub.skills[i].status[j].pointLabel = labelList[14 + i * 12 + j * 2];
                character.skills[i].status[j].SetStatus(character.skills[i], j == 2);
                characterSub.skills[i].status[j].SetStatus(characterSub.skills[i], j == 2);
            }
            character.skills[i].unlockLabel = labelList[90 + i * 12];
            character.skills[i].unlockValueLabel = labelList[91 + i * 12];
            characterSub.skills[i].unlockLabel = labelList[19 + i * 12];
            characterSub.skills[i].unlockValueLabel = labelList[20 + i * 12];
        }
        arrowLabel = labelList[142];
        goldLabel = labelList[143];
        buttons.Clear();
        labelList.Clear();
        //クリックイベント追加
        arrowLabel.RegisterCallback<MouseDownEvent>(DisplayStatus);
        character.unlockValueLabel.RegisterCallback<MouseDownEvent, Character>(CharacterUnlock, character);
        characterSub.unlockValueLabel.RegisterCallback<MouseDownEvent, Character>(CharacterUnlock, characterSub);
        for (int i = 0; i < 3; i++)
        {
            character.status[i].pointLabel.RegisterCallback<MouseDownEvent, Status>(StatusPoint, character.status[i]);
            characterSub.status[i].pointLabel.RegisterCallback<MouseDownEvent, Status>(StatusPoint, characterSub.status[i]);
        }
        for (int i = 0; i < 5; i++)
        {
            character.skills[i].autoLabel.RegisterCallback<MouseDownEvent, Skill>(SkillAuto, character.skills[i]);
            characterSub.skills[i].autoLabel.RegisterCallback<MouseDownEvent, Skill>(SkillAuto, characterSub.skills[i]);
            for (int j = 0; j < 3; j++)
            {
                character.skills[i].status[j].pointLabel.RegisterCallback<MouseDownEvent, Status>(StatusPoint, character.skills[i].status[j]);
                characterSub.skills[i].status[j].pointLabel.RegisterCallback<MouseDownEvent, Status>(StatusPoint, characterSub.skills[i].status[j]);
            }
            character.skills[i].unlockValueLabel.RegisterCallback<MouseDownEvent, Skill>(SkillUnlock, character.skills[i]);
            characterSub.skills[i].unlockValueLabel.RegisterCallback<MouseDownEvent, Skill>(SkillUnlock, characterSub.skills[i]);
        }
        //初期化
        transitionMaterial.SetFloat(transitionProp, transition1);
        stageTransitionMaterial.SetFloat(transitionProp, transition0);
        blurMaterial.SetInt(isBlurProp, 0);
    }
    //カメラの表示範囲かどうか
    public bool CheckInScreen(Transform _transform)
    {
        return leftUp.position.x < _transform.position.x && _transform.position.x <= rightDown.position.x && rightDown.position.y < _transform.position.y && _transform.position.y <= leftUp.position.y;
    }
    //表示範囲の左右の範囲
    public (float, float) LeftRight()
    {
        return (leftUp.position.x, rightDown.position.x);
    }
    //表示範囲の上下の範囲
    public (float, float) TopDown()
    {
        return (leftUp.position.y, rightDown.position.y);
    }
    //倒したときの吹っ飛びエフェクト
    public void KillEffect(Enemy enemy, Vector3 vector3)
    {
        particleSystemKill.transform.position = enemy.transform.position - vector3 * 10f;
        var main = particleSystemKill.main;
        main.startRotation = Mathf.Acos(vector3.x);
        particleSystemKill.Play();
    }
    //サブプレイヤーのアンロック
    void CharacterUnlock(MouseDownEvent mouseDownEvent, Character character)
    {
        if (character.unlockValue > gold) return;
        character.unlock = true;
        gold -= character.unlockValue;
        tutorialManager?.SetActive(4, true);
    }
    //スキルのアンロック
    void SkillUnlock(MouseDownEvent mouseDownEvent, Skill skill)
    {
        if (skill.unlockValue > gold) return;
        skill.unlock = true;
        gold -= skill.unlockValue;
    }
    //ステータスの上昇
    void StatusPoint(MouseDownEvent mouseDownEvent, Status status)
    {
        if (status.point.Length <= status.lv) return;
        if (status.point[status.lv] > gold) return;
        status.lv++;
        if(characterSub.status[0] == status)
        {
            SetCast(status);
        }
        else if (character.status[0] == status)
        {
            SetHP(status);
        }
        gold -= status.point[status.lv - 1];
    }
    //キャスト速度の設定
    void SetCast(Status status)
    {
        playerManagerSub.SetCastSpeed(status.value[status.lv]);
        playerManager.SetCastSpeed(status.value[status.lv]);
    }
    //HPのセット
    void SetHP(Status status)
    {
        playerManager.SetHP(status.value[status.lv]);
    }
    //自動手動のセット
    void SkillAuto(MouseDownEvent mouseDownEvent, Skill skill)
    {
        skill.auto = !skill.auto;
    }
    //ステータスのセット
    void StatusSet(Character character, bool unlock, PlayerManager playerManager)
    {
        character.playerManager = playerManager;
        character.nameLabel.text = character.name;
        character.level.lv = 1;
        character.unlockValueLabel.text = $"{character.unlockValue.ToString("#,#")}P";
        for (int i = 0; i < 3; i++)
        {
            character.status[i].lv = 0;
        }
        for (int i = 0; i < 5; i++)
        {
            character.skills[i].unlock = i < 2;
            character.skills[i].auto = false;
            character.skills[i].nameLabel.text = character.skills[i].name;
            character.skills[i].level.lv = 1;
            for (int j = 0; j < 3; j++)
            {
                character.skills[i].status[j].lv = 0;
            }
            character.skills[i].unlockValueLabel.text = $"{character.skills[i].unlockValue.ToString("#,#")}P";
            character.unlock = unlock;
        }
    }
    //ゴールドのセット
    public void SetExp(int num)
    {
        gold += num;
        if (gold >= 100) tutorialManager.SetActive(2, true);
    }
    //購入できるかの表示
    void StatusSet(Character character, int gold)
    {
        character.unlockValueLabel.style.unityBackgroundImageTintColor = character.unlockValue <= gold ? Color.green : Color.red;
        for (int j = 0; j < 3; j++)
        {
            if(character.status[j].point.Length <= character.status[j].lv)
            {
                character.status[j].pointLabel.style.unityBackgroundImageTintColor =  Color.yellow;
                continue;
            }
            character.status[j].pointLabel.style.unityBackgroundImageTintColor = character.status[j].point[character.status[j].lv] <= gold ? Color.green : Color.red;
        }
        for (int i = 0; i < 5; i++)
        {
            character.skills[i].unlockValueLabel.style.unityBackgroundImageTintColor = character.skills[i].unlockValue <= gold ? Color.green : Color.red;
            for (int j = 0; j < 3; j++)
            {
                if (character.skills[i].status[j].point.Length <= character.skills[i].status[j].lv)
                {
                    character.skills[i].status[j].pointLabel.style.unityBackgroundImageTintColor = Color.yellow;
                    continue;
                }
                character.skills[i].status[j].pointLabel.style.unityBackgroundImageTintColor = character.skills[i].status[j].point[character.skills[i].status[j].lv] <= gold ? Color.green : Color.red;
            }
        }
    }
    //レベルアップウィンドウの表示非表示
    void DisplayStatus(MouseDownEvent mouseDownEvent)
    {
        if(uIDocumentUpgrade.rootVisualElement.style.top == hiddenPosition)
        {
            uIDocumentUpgrade.rootVisualElement.style.top = 0f;
            arrowLabel.style.scale = hanten;
            tutorialManager.SetActive(2, false);
        }
        else
        {
            uIDocumentUpgrade.rootVisualElement.style.top = hiddenPosition;
            arrowLabel.style.scale = Vector2.one;
            tutorialManager.SetActive(5, false);
            tutorialManager.SetActive(6, false);
        }
    }
    //スキルボタンのクールタイムセット
    public void SetButton(PlayerManager playerManager0, int i, float coolTime, bool skill)
    {
        Character character0 = playerManager0 == playerManager ? character : characterSub;
        //if (playerManager0 != playerManager) i += 6;
        ChangeNumIn(ref i, playerManager0);
        if (i < 0) return;
        if (coolTime > 0f)
        {
            character0.skills[i].skillButton.style.unityBackgroundImageTintColor = gray;
            character0.skills[i].skillButton.text = coolTime.ToString("0");
        }
        else
        {
            character0.skills[i].skillButton.style.unityBackgroundImageTintColor = skill ? gray : Color.white;
            character0.skills[i].skillButton.text = "";
        }
    }
    //バックステップのボタンカラー
    public void SetBackstepColor(bool skill)
    {
        backstep.style.unityBackgroundImageTintColor = skill ? gray : Color.white;
    }
    //スキルボタンクリック時
    void ClickEventSkill(MouseDownEvent mouseDownEvent, int i)
    {
        bool b = ChangeNumOut(ref i);
        if (i < 0) return;
        if (b) skillPlayer = playerManager.Skill(i);
        else skillPlayerSub = playerManagerSub.Skill(i);
    }
    //カットインの表示
    public void SetCutIn(int num, PlayerManager playerManager0)
    {
        Character character0 = playerManager0 == playerManager ? character : characterSub;
        ChangeNumIn(ref num, playerManager0);
        StartCoroutine(cutIn.SetCutIn(character0.skills[num].nameLabel.text, playerManager0.skillGradient[num].Evaluate(1f),playerManager0));
    }
    //カットインの表示
    //IEnumerator SetCutInMorning()
    //{
    //    yield return new WaitForSeconds(6.5f); 
    //    cutIn.SetCutIn("朝活", playerManager.skillGradient[1].Evaluate(1f), playerManager);
    //}
    //画面クリック時通常攻撃
    void ClickEventAttack(MouseDownEvent mouseDownEvent)
    {
        if (mouseDownEvent.pressedButtons % 2 == 1) playerManager.Attack(!skillPlayer && !skillPlayerSub, false);
        if (mouseDownEvent.pressedButtons / 2 == 1) playerManagerSub.Attack(!skillPlayerSub && !skillPlayer, false);
        skillPlayer = false;
        skillPlayerSub = false;
    }
    //バックステップボタンの処理
    void ClickEventBackStep(MouseDownEvent mouseDownEvent)
    {
        playerManager.BackStep();
    }   
    //ガードボタンの処理
    void ClickEventGuard(MouseDownEvent mouseDownEvent)
    {
        playerManager.BackStep();
    }
    //プレイヤーのスキルIDからスキルボタンのIDに変更
    void ChangeNumIn(ref int i, PlayerManager playerManager)
    {
        if (playerManager == GameManager.playerManager)
        {
            i = i switch
            {
                0 => 0,
                1 => 3,
                2 => 1,
                3 => 2,
                4 => -1,
                5 => 4,
                _ => -1
            };
        }
        else
        {
            i = i switch
            {
                0 => 1,
                1 => 0,
                2 => 2,
                3 => -1,
                4 => 3,
                5 => 4,
                _ => -1
            };
        }
    }
    //スキルボタンのIDからプレイヤーのスキルIDに変更
    bool ChangeNumOut(ref int i)
    {
        bool b = i < 5;
        i = i switch
        {
            0 => 0,
            1 => 2,
            2 => 3,
            3 => 1,
            4 => 5,
            5 => 1,
            6 => 0,
            7 => 2,
            8 => 4,
            9 => 5,
            _ => -1
        };
        return b;
    }
    //スキルボタンのIDからプレイヤーのスキルIDに変更
    public int ChangeNumOut(int i, PlayerManager playerManager)
    {
        if (playerManager != GameManager.playerManager) i += 5;
        ChangeNumOut(ref i);
        return i;
    }
    //プレイヤーの攻撃力の取得
    public float GetDamage(AttackCollisionValue attackCollisionValue)
    {
        Character character = attackCollisionValue.sub ? characterSub : this.character;
        float damage = character.status[1].value[character.status[1].lv] * attackCollisionValue.damageRate;
        if (attackCollisionValue.skillId >= 0) damage *= character.skills[attackCollisionValue.skillId].status[0].value[character.skills[attackCollisionValue.skillId].status[0].lv] / 100f;
        return damage;
    }
    //クールタイムの取得
    public float GetCoolTime(int i, bool sub)
    {
        Character character = sub ? characterSub : this.character;
        ChangeNumIn(ref i, sub ? playerManagerSub : playerManager);
        if (i < 0) return -1;
        return character.skills[i].status[1].value[character.skills[i].status[1].lv];
    }
    //自動クリックタイムの取得
    public float AutoClickTime(PlayerManager playerManager)
    {
        Character character = playerManager == GameManager.playerManager ? this.character : characterSub;
        return character.status[2].value[character.status[2].lv] > 0 ? 1f / character.status[2].value[character.status[2].lv] : -1f;
    }
    //進化スキルかどうか取得
    public bool GetEvo(PlayerManager playerManager, int skillId)
    {
        if (playerManager == null) return false;
        ChangeNumIn(ref skillId, playerManager);
        if (skillId < 0) return false;
        Character character = playerManager == GameManager.playerManager ? this.character : characterSub;
        return character.skills[skillId].status[2].lv > 0;
    }
    //進化スキルかどうか取得
    public bool GetEvo(PlayerManager playerManager, AttackCollisionValue attackCollisionValue)
    {
        Character character = playerManager == GameManager.playerManager ? this.character : characterSub;
        return character.skills[attackCollisionValue.skillId].status[2].lv > 0;
    }
    void Update()
    {
        playerManager?.Update0();
        if(playerManagerSub != null && playerManagerSub.gameObject.activeSelf) playerManagerSub.Update0();
        cameraManager?.Update0();
        enemyManager?.Update0();
        backgroundManager?.Update0();
    }
    //ステージクリア
    public void Clear()
    {
        if(stage > backgroundManagers.Length - 2)
        {
            clear.gameObject.SetActive(true);
            return;
        }
        clearDoor = Instantiate(clearDoorObject, new Vector3(playerManager.transform.position.x + 16f, -5.5f, 0f), Quaternion.identity);
        clearDoor.GetComponent<SpriteRenderer>().DOFade(1f, 1f);
        Clear(true);
        StartCoroutine(StageTransition(playerManagerSub.gameObject.activeSelf));
    }
    //ステージクリアトランジション
    public IEnumerator StageTransition(bool subUnlock)
    {
        while (playerManager.gameObject.activeSelf || playerManagerSub.gameObject.activeSelf)
        {
            yield return null;
        }
        transitionCount = transition0;
        while (transitionCount <= transition1)
        {
            yield return null;
            transitionCount += Time.deltaTime * transitionStageSpeed;
            stageTransitionMaterial.SetFloat(transitionProp, transitionCount);
        }
        yield return ResetStage(true);
        playerManager.gameObject.SetActive(true);
        playerManager.SetAvtive(true);
        if (subUnlock)
        {
            playerManagerSub.gameObject.SetActive(true);
            playerManagerSub.SetAvtive(true);
        }
        Clear(false);
        while (transitionCount >= transition0)
        {
            yield return null;
            transitionCount -= Time.deltaTime * transitionStageSpeed;
            stageTransitionMaterial.SetFloat(transitionProp, transitionCount);
        }
    }
    //クリア処理
    void Clear(bool isBool)
    {
        playerManager.ClearAnimation(isBool);
        playerManagerSub.ClearAnimation(isBool);
        CameraManager.clear = isBool;
    }
    //シーン読み込み時
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DOTween.SetTweensCapacity(2000, 50);
        DOTween.Init();
        playerManager = GameObject.Find("MainPlayer").GetComponent<PlayerManager>();
        playerManager.Start0();
        playerManagerSub = FindAnyObjectByType<PlayerManagerSub>();
        playerManagerSub.Start0();
        cameraManager = FindAnyObjectByType<CameraManager>();
        cameraManager.Start0();
        enemyManager = FindAnyObjectByType<EnemyManager>();
        enemyManager.Start0();
        damageManager = FindAnyObjectByType<DamageManager>();
        damageManager.Start0();
        backgroundManager = Instantiate(backgroundManagers[stage]);
        backgroundManager.Start0(cameraManager.transform);
        expManagers = new ExpManager[2];
        expManagers[0] = GameObject.Find("ExpManager").GetComponent<ExpManager>();
        expManagers[1] = GameObject.Find("ExpManager1").GetComponent<ExpManager>();
        foreach (var expManager in expManagers)
        {
            expManager.Start0();
        }
        singleHitManager = GameObject.Find("SingleHitManager").GetComponent<SingleHitManager>();
        singleHitManager.Start0();
        poisonManager = GameObject.Find("SingleHitManagerPoison").GetComponent<SingleHitManager>();
        poisonManager.Start0();
        attackCollisionManager = GameObject.Find("CollisionAttackManager").GetComponent<AttackCollisionManager>();
        attackCollisionManager.Start0();
        attackCollisionManagerBoss = GameObject.Find("CollisionAttackManagerBoss").GetComponent<AttackCollisionManager>();
        attackCollisionManagerBoss.Start0();
        tutorialManager = FindAnyObjectByType<TutorialManager>();
        tutorialManager?.Start0();
        //ステータスのセット
        StatusSet(character, true, playerManager);
        StatusSet(characterSub, false, playerManagerSub);
        SetCast(characterSub.status[0]);
        SetHP(character.status[0]);
        gold = 1000000;
        popTextManager = FindAnyObjectByType<PopTextManager>();
        popTextManager.Start0();
        cutIn = FindAnyObjectByType<CutIn>();
        cutIn.Start0();
        tutorialManager?.SetActive(0, true);
        DisplayStatus(null);
        //StartCoroutine(SetCutInMorning());
    }
    //エフェクト情報
    [System.Serializable]
    public class Effect
    {
        public float effectInterval;
        public AttackCollisionValue attackCollisionValue;
        public Effect(AttackCollisionValue.Effect effect)
        {
            attackCollisionValue = new AttackCollisionValue();
            attackCollisionValue.autoAtk = true;
            attackCollisionValue.down = 0f;
            attackCollisionValue.force = Vector3.right * 0.1f;
            if (effect == AttackCollisionValue.Effect.Poison)
            {
                effectInterval = 1f;
                attackCollisionValue.damageRate = 0.1f;
                attackCollisionValue.sub = true;
                attackCollisionValue.skillId = 2;
            }
            else if (effect == AttackCollisionValue.Effect.Star)
            {
                effectInterval = 100f;
            }
            else if (effect == AttackCollisionValue.Effect.Kanden)
            {
                effectInterval = 100f;
                attackCollisionValue.damageRate = 0.1f;
                attackCollisionValue.skillId = 4;
                attackCollisionValue.special = AttackCollisionValue.Special.Kanden;
            }
        }
    }
    //プレイヤー情報
    [System.Serializable]
    public class Character
    {
        public PlayerManager playerManager { get; set; }
        public string name;
        public Label nameLabel { get; set; }
        public Level level;
        public Status[] status;
        public Skill[] skills;
        bool unlock0;
        public bool unlock
        {
            get
            {
                return unlock0;
            }
            set
            {
                unlock0 = value;
                unlockLabel.visible = !unlock0;
                if (unlock0)
                {
                    //アンロック時スキルの開放状態に合わせてスキルボタンの表示
                    for (int i = 0; i < skills.Length; i++)
                    {
                        skills[i].skillButton.visible = skills[i].unlock;
                    }
                }
                else
                {
                    //ロック時スキルボタン非表示
                    for (int i = 0; i < skills.Length; i++)
                    {
                        skills[i].skillButton.visible = unlock0;
                    }
                }
                if (playerManager != null && playerManager.gameObject.activeSelf != unlock0)
                {
                    //ロック時非表示
                    if(unlock0) playerManager.gameObject.SetActive(unlock);
                    playerManager.SetAvtive(unlock);
                    if (!unlock0) playerManager.gameObject.SetActive(unlock);
                }
            }
        }
        public Label unlockLabel { get; set; }
        public int unlockValue;
        public Label unlockValueLabel { get; set; }
        public Character()
        {
            level = new Level();
            status = new Status[3];
            for(int i = 0; i < status.Length; i++)
            {
                status[i] = new Status();
            }
            skills = new Skill[5];
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i] = new Skill();
            }
        }
    }
    //プレイヤーのステータス
    [System.Serializable]
    public class Status
    {
        public string name;
        public Label nameLabel { get; set; }
        int lv0;
        [System.NonSerialized] Level parentLevel;
        [System.NonSerialized] Skill parentSkill;
        public int lv {
            get
            { 
                return lv0; 
            }
            set 
            {
                if (parentSkill == null) parentLevel.lv += value - lv0;
                lv0 = value;
                if(lv0 + 1 >= this.value.Length) nameLabel.text = $"{name}\n{(lv0 < this.value.Length ? this.value[lv0] : "")}{unit}";
                else nameLabel.text = $"{name}\n{this.value[lv0]}{unit}→<color=#00dddd>{this.value[lv0 + 1]}{unit}</color>";
                pointLabel.text = lv0 < point.Length ? $"{point[lv0].ToString("#,#")}P" : "Max";
                if (parentSkill != null && lv0 >= point.Length)
                {
                    tutorialManager.SetActive(6, true);
                    parentSkill.ChangeSkillName(unit);
                }
            }
        }
        public int[] value;
        public string unit;
        public int[] point;
        public Label pointLabel { get; set; }
        public void SetStatus(Level level, Skill skill = null)
        {
            parentLevel = level;
            parentSkill = skill;
        }
        public void SetStatus(Character character)
        {
            SetStatus(character.level);
        }
        public void SetStatus(Skill skill, bool evo)
        {
            SetStatus(skill.level, evo ? skill : null);
        }
    }
    //レベルラベル
    public class Level
    {
        int lv0;
        public int lv
        {
            get
            {
                return lv0;
            }
            set
            {
                lv0 = value;
                lvLabel.text = $"レベル：{lv0}";
            }
        }
        [System.NonSerialized]
        public Label lvLabel;
    }
    //スキル
    [System.Serializable]
    public class Skill
    {
        public string name;
        public Label nameLabel { get; set; }
        public Level level { get; set; }
        public Label skillButton { get; set; }
        public Status[] status;
        bool unlock0;
        public bool unlock 
        {
            get
            {
                return unlock0;
            }
            set
            {
                unlock0 = value;
                unlockLabel.visible = !unlock0;
                skillButton.visible = unlock0;
            }
        }
        public Label unlockLabel { get; set; }
        public int unlockValue;
        public Label unlockValueLabel { get; set; }
        bool auto0;
        public bool auto
        {
            get
            {
                return auto0;
            }
            set
            {
                auto0 = value;
                autoLabel.style.unityTextAlign = auto0 ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                autoLabel.text = auto0 ? "自動" : "手動";
                autoLabel.style.backgroundImage = auto? gameManager.texture2DAuto : gameManager.texture2DManual;
            }
        }        
        public Label autoLabel { get; set; }
        public Skill()
        {
            level = new Level();
            status = new Status[3]; 
            for (int i = 0; i < status.Length; i++)
            {
                status[i] = new Status();
            }
        }
        //進化時名前変更
        public void ChangeSkillName(string name)
        {
            Debug.Log(name);
            nameLabel.text = name;
        }
    }
}
