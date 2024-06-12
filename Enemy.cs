using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Enemy : MonoBehaviour
{
    protected bool skill;//�X�L���g�p�\��
    [SerializeField] float maxhp;//�ő�HP
    [SerializeField] float deffence;//�h���(������)
    [SerializeField] float exp;//�|�����Ƃ��̌o���l
    float down;//�_�E���l
    [SerializeField] float forceRate;//������ї�
    [SerializeField] float downRate;//�_�E���l��
    [SerializeField] float knockResist;//�m�b�N�o�b�N�y��
    Tweener tweenerX;//�m�b�N�o�b�N�E�_�E���p
    Tweener tweenerY;//�m�b�N�o�b�N�E�_�E���p
    AttackCollisionValue attackCollisionValue;//�q�b�g���������蔻��̒l
    Vector3 vector3;//�ꎞ���p
    float hp;//���݂�HP
    [SerializeField] CapsuleCollider2D capsuleCollider;//���S�������蔻�薳����
    [SerializeField] public Animator animator;
    [SerializeField] float attackArea;//�U���͈�
    [SerializeField] float damage;//�U����
    [SerializeField] protected float attackSpeed;//�U�����x
    [SerializeField] float velocity;//�ړ����x
    readonly protected int idleHash = Animator.StringToHash("idle");
    readonly protected int moveHash = Animator.StringToHash("move");
    readonly int attackHash = Animator.StringToHash("attack");
    readonly int downHash = Animator.StringToHash("down");
    readonly int standupHash = Animator.StringToHash("standup");
    readonly int knockHash = Animator.StringToHash("knock");
    readonly int knockEndHash = Animator.StringToHash("knockEnd");
    readonly int starHash = Animator.StringToHash("star");
    readonly int respawnHash = Animator.StringToHash("respawn");   
    public static Vector2 inversionVector2;//���]���X�P�[��
    public float scale { get; protected set; }//�����X�P�[��
    protected float attackCount;//�U�����x�J�E���g
    float[] effectCounts;//��Ԃ̎������ԃJ�E���g
    [SerializeField] ParticleSystem[] effects;//��Ԃ̃G�t�F�N�g
    [SerializeField] ParticleSystem kandenDamage;//���d�ǉ��U���̃G�t�F�N�g
    [SerializeField] Armor armor;//�n�C�p�[�A�[�}�[�̐ݒ�
    [SerializeField] Kind kind;//�G�̎��
    Material material;//�X�v���C�g�̃}�e���A���A�n�C�p�[�A�[�}�[���Ԃ�����
    bool armorbool;//�n�C�p�[�A�[�}�[��Ԃ�
    [SerializeField] ParticleDelay particleSystemAttack;
    enum Armor
    {
        Null,
        Attack,
        All
    }
    enum Kind
    {
        Normal,
        Fly
    }
    //�m�b�N�o�b�N����
    float holdout { 
        get 
        {
            return holdout0;
        }
        set
        {
            if (holdout0 > 0f)
            {
                holdout0 = value;
            }
            else
            {
                holdout0 = value;
                if (holdout0 > 0f)
                {
                    animator.SetTrigger(knockHash);
                    foreach (var afterimage in afterimages)
                    {
                        afterimage.SetTrigger(knockHash);
                    }
                }
            }
        }
    }
    float holdout0;
    [SerializeField] Transform transformDamage; //�_���[�W�\���ʒu
    [SerializeField] Rigidbody2D _rigidbody2D; //���g�̃��W�b�g�{�f�B
    bool death;//�|���ꂽ��
    float hurimukiCount;//�U��ނ��\���ԃJ�E���g
    public bool right { get; protected set; }//�ǂ���������
    public Afterimage[] afterimages;//�c���̃Z�b�g
    protected Afterimage.Condition condition;//���
    protected bool invincible;//���G��
    Vector2 deathPoint;//������уG�t�F�N�g�p
    Transform spawnPoint;//�X�|�[���ʒu
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //�v���C���[�̕���̂݃g���K�[
        if (!collision.CompareTag("PlayerWeapon")) return;
        //�c���\�����A���o�ł킩��悤�Ɏc���̊g��
        if (condition != Afterimage.Condition.Null || invincible)
        {
            foreach (var afterimage in afterimages)
            {
                afterimage.SetScale();
            }
        }
        //�R���W�����̏����擾/�_���[�W��
        AttackCollision attackCollision = collision.gameObject.GetComponent<AttackCollision>();
        (AttackCollisionValue attackCollisionValue0, SingleHit singleHit) = attackCollision.GetAttackCollisionValue();
        //�A�C�X�U���Ȃ�֐����N��
        if (attackCollisionValue0.special == AttackCollisionValue.Special.Ice)
        {
            GameManager.playerManagerSub.Ice(this, attackCollisionValue0);
            return;
        }
        //�_���[�W�A�m�b�N�o�b�N�A�_�E���̏���
        Damage(attackCollisionValue0, transform.position.x - (attackCollision.transform.position.x + attackCollisionValue0.velocity.x * attackCollisionValue0.interval));
        //�ōU���Ȃ�֐��N��
        if (attackCollisionValue0.special == AttackCollisionValue.Special.Poison)
        {
            GameManager.playerManagerSub.Poison(attackCollision, attackCollisionValue0);
        }
        //���d��ԂȂ�ǉ��_���[�W�ƃG�t�F�N�g
        if (effects[(int)AttackCollisionValue.Effect.Kanden - 1].IsAlive(true) && attackCollisionValue.special != AttackCollisionValue.Special.Kanden)
        {
            Damage(GameManager.effect[(int)AttackCollisionValue.Effect.Kanden - 1].attackCollisionValue, default);
            kandenDamage.transform.position = collision.ClosestPoint(transform.position);
            kandenDamage.Play();
        }
        //�T�u�̒ʏ�U���Ȃ�U������ƃG�t�F�N�g�̍폜�ƃG���h�G�t�F�N�g�̃v���C
        if (singleHit != null)
        {
            singleHit.Stop(attackCollision.transform, attackCollisionValue0);
            attackCollision.End(true);
        }
        //��Ԉُ�̏���
        if ((int)attackCollisionValue0.effect > (int)AttackCollisionValue.Effect.Null)
        {
            effectCounts[(int)attackCollisionValue0.effect - 1] = GameManager.effect[(int)attackCollisionValue0.effect - 1].effectInterval;
            effects[(int)attackCollisionValue0.effect - 1].Play();
        }
    }
    //�m�b�N�o�b�N�^�C���̐ݒ�
    void ResetForce(float duration)
    {
        holdout = duration + (holdout - (tweenerX != null ? tweenerX.Elapsed() : 0f)) / 2;
        ResetForce(false);
    }
    //�_�E�����m�b�N�o�b�N�^�C���̃N���A
    void ResetForce(bool resetHold)
    {
        if (resetHold) holdout = 0f;
        tweenerX?.Kill();
        tweenerY?.Kill();
    }
    protected virtual void SetDamage()
    {

    }
    //�������U���̃G�t�F�N�g
    public void AttackParticle()
    {
        particleSystemAttack.enumerator = ParticlePlay(particleSystemAttack);
        StartCoroutine(particleSystemAttack.enumerator);
    }
    IEnumerator ParticlePlay(ParticleDelay particleDelay)
    {
        yield return new WaitForSeconds(particleDelay.delay);
        particleDelay.particleSystem.Play();
        particleDelay.enumerator = null;
    }
    //�G�t�F�N�g�̒�~
    public void AttackParticlesStop()
    {
        if (particleSystemAttack.enumerator != null) StopCoroutine(particleSystemAttack.enumerator);
        if (particleSystemAttack.dependence && particleSystemAttack.particleSystem.IsAlive()) particleSystemAttack.particleSystem.Stop();
    }
    public void Damage(AttackCollisionValue attackCollisionValue, float rightRate)
    {
        //�c���\�����A���o�ł킩��悤�Ɏc���̊g��
        if (condition != Afterimage.Condition.Null || invincible)
        {
            foreach (var afterimage in afterimages)
            {
                afterimage.SetScale();
            }
        }
        //���G�Ȃ炱���܂�
        if (condition == Afterimage.Condition.Invincible || invincible) return;
        //�_���[�W�̃Z�b�g
        PlayerManager playerManager = attackCollisionValue.sub ? GameManager.playerManagerSub : GameManager.playerManager;
        float damage = GameManager.gameManager.GetDamage(attackCollisionValue);
        this.attackCollisionValue = attackCollisionValue;
        hp -= damage;
        SetDamage();
        //�q�b�g�����Ώۂɗ��𗎂Ƃ��Ƃ��̏���
        Special(attackCollisionValue);
        //�_���[�W�̕\��
        //GameManager.damageManagers[attackCollisionValue.sub ? 1 : 0].SetDamage((int)damage, transformDamage, true);
        if (hp > 0f) GameManager.expManagers[attackCollisionValue.sub ? 1 : 0].SetExp((int)damage, transformDamage, !playerManager.left);
        //�n�C�p�[�A�[�}�[�Ȃ炱���܂�
        if (condition == Afterimage.Condition.Armor || armorbool) return;
        //�_�E���l�̃Z�b�g
        down += attackCollisionValue.down * downRate;
        //�󒆂Ńq�b�g�����Ȃ瑦�_�E��
        if (transform.position.y > 0f && down < 1f && kind == Kind.Normal) down = 1f;
        if (hp > 0f)
        {
            if (down >= 1f)
            {
                //�_�E������
                var duration = attackCollisionValue.force.y * forceRate / 12 / down;
                ResetForce(true);
                vector3.x = transform.position.x + attackCollisionValue.force.x * forceRate * (attackCollisionValue.suction ? rightRate : 1) * (!playerManager.left ? 1 : -1);
                right = !playerManager.left;
                vector3.y = transform.position.y + attackCollisionValue.force.y * forceRate;
                tweenerX = transform.DOMoveX(vector3.x, duration)
                    .SetEase(kind == Kind.Normal ? Ease.OutSine : Ease.Linear)
                    .OnComplete(() =>
                    {
                        tweenerX = null;
                    });
                tweenerY = transform.DOMoveY(vector3.y, duration / 2)
                    .OnComplete(() =>
                    {
                        Down();
                    });
                animator.SetTrigger(downHash); 
                foreach (var afterimage in afterimages)
                {
                    afterimage.SetTrigger(downHash);
                }
            }
            else
            {
                //�m�b�N�o�b�N
                Knockback(rightRate, playerManager);
            }
        }
        else
        {
            //�������
            vector3.x = attackCollisionValue.force.normalized.x * 35f * (right ? 1 : -1);
            vector3.y = attackCollisionValue.force.normalized.y * 35f;
            vector3 += transform.position;
            var duration = 0.3f;
            ResetForce(true);
            tweenerX = transform.DOMove(vector3, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => 
                {
                    tweenerX = null;
                    Spawn();
                })
                .OnStart(() =>
                {
                    DeathHitStop();
                })
                .OnUpdate(() => 
                {
                    if (!death && !GameManager.gameManager.CheckInScreen(transform))
                    {
                        death = true;
                        deathPoint = attackCollisionValue.force.normalized;
                        if (!right) deathPoint.x *= -1;
                        GameManager.gameManager.KillEffect(this, deathPoint);
                    }
                });
            capsuleCollider.enabled = false;
            animator.SetTrigger(downHash);
            foreach (var afterimage in afterimages)
            {
                afterimage.SetTrigger(downHash);
            }
            //�o���l�̃Z�b�g
            GameManager.expManagers[attackCollisionValue.sub ? 1 : 0].SetExp((int)(damage + exp), transformDamage, !playerManager.left);
        }
    }
    public float GetHpRate()
    {
        return hp / maxhp;
    }
    protected virtual void DeathHitStop()
    {
        //if (!attackCollisionValue.autoAtk) GameManager.playerManager.HitStop(0.3f);
        //else GameManager.playerManager.HitStop(0.08f);
    }
    //��΂�ꂽ��̏���
    void Down()
    {
        if (kind == Kind.Normal)
        {
            tweenerY = transform.DOLocalMoveY(0f, transform.localPosition.y /** forceRate*/ / 18 / down)
                .OnComplete(() =>
                {
                    ReMove();
                })
                .SetEase(Ease.InQuad);
        }
        else
        {
            ReMove();
        }
    }
    //�m�b�N�o�b�N����
    void Knockback(float rightRate, PlayerManager playerManager)
    {
        float forceX = Mathf.Abs(attackCollisionValue.force.x) - knockResist;
        var duration = forceX > 0f ? (forceX + attackCollisionValue.force.y / attackCollisionValue.force.x * forceX) /** forceRate*/ / 24 : 0f;
        if (duration <= 0f) return;
        ResetForce(duration);
        vector3 = transform.position + attackCollisionValue.force /** forceRate*/ / 5 * (attackCollisionValue.suction ? rightRate : 1) * (!playerManager.left ? 1 : -1);
        right = !playerManager.left;
        tweenerX = transform.DOMoveX(vector3.x, holdout)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                holdout = 0f; 
                animator.SetTrigger(knockEndHash);
                foreach (var afterimage in afterimages)
                {
                    afterimage.SetTrigger(knockEndHash);
                }
                tweenerX = null;
            });
    }
    //���g�p
    void StandUp()
    {
        animator.SetTrigger(standupHash);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetTrigger(standupHash);
        }
    }
    //���X�|��
    void Spawn()
    {
        if (GameManager.enemyManager.stopSpown)
        {
            gameObject.SetActive(false);
            return;
        }
        animator.SetTrigger(respawnHash);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetTrigger(respawnHash);
        }
        SetSpawn();
        capsuleCollider.enabled = true;
    }
    //������
    void Special(AttackCollisionValue attackCollisionValue)
    {
        if (attackCollisionValue.special == AttackCollisionValue.Special.Thunder)
        {
            GameManager.playerManager.thunder.Set(transform);
        }
        else if (attackCollisionValue.special == AttackCollisionValue.Special.ThunderAll && GameManager.gameManager.GetEvo(GameManager.playerManager, attackCollisionValue))
        {
            GameManager.playerManager.thunder.SetAllOk(transform);
        }
    }
    //������
    public virtual void Start0(Transform _transform)
    {
        scale = transform.localScale.x;
        effectCounts = new float[(int)AttackCollisionValue.Effect.Length - 1];
        spawnPoint = _transform;
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        material = spriteRenderers[0].material;
        foreach(var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sharedMaterial = material;
        }
        if (armor == Armor.All)
        {
            armorbool = true;
            material.SetColor(GameManager.gameManager.colorHash, Color.red);
        }
        SetSpawn();
    }
    //�X�|�����̒l�̐ݒ�
    void SetSpawn()
    {
        holdout = 0f;
        hp = maxhp;
        attackCount = 0f;
        if(kind == Kind.Normal) transform.position = spawnPoint.transform.position + Random.Range(0f, 2f) * Vector3.right;
        else transform.position = (Vector2)spawnPoint.transform.position + Random.Range(-4f, 0f) * inversionVector2 + Vector2.up * 12f;
        death = false;
    }
    //�����̐ݒ�
    public void SetRight()
    {
        if (right != GameManager.playerManager.transform.position.x <= transform.position.x)
        {
            right = !right;
            hurimukiCount = 1f;
        }
    }
    public virtual void Update0()
    {
        int currectAnime = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (GameManager.boss != this)
        {
            (float _left, float _right) = GameManager.gameManager.LeftRight();
            //�J�����͈̔͊O�ɍs�����Ƃ����X�|��
            if (((!GameManager.playerManager.left && transform.position.x <= _left) || (GameManager.playerManager.left && transform.position.x >= _right)) && moveHash == currectAnime)
            {
                SetSpawn();
            }
            //�U�����A�[�}�[�Ȃ�
            if(Armor.Attack == armor)
            {
                if(armorbool == (currectAnime != attackHash))
                {
                    armorbool = !armorbool;
                    material.SetColor(GameManager.gameManager.colorHash, armorbool ? Color.red : Color.black);
                }
            } 
        }
        //�U��ނ��̃Z�b�g
        hurimukiCount -= Time.deltaTime;
        if (hurimukiCount <= 0 && !skill)
        {
            SetRight();
        }
        //�ړ��ƍU��
        if (kind == Kind.Normal)
        {
            Attack(currectAnime);
            if (currectAnime == moveHash)
            {
                transform.position += Time.deltaTime * velocity * (right ? Vector3.left : Vector3.right);
            }
        }
        else
        {
            AttackFly(currectAnime);
            if (currectAnime == moveHash)
            {
                transform.position += Time.deltaTime * velocity * (GameManager.playerManager.GetPlayerCenter() - transform.position).normalized;
            }
        }
        //����
        transform.localScale = scale * (right ? Vector2.one : inversionVector2);
        //�A�^�b�N�J�E���g
        if (attackCount > 0f)
        {
            attackCount -= Time.deltaTime;
        }
        //��Ԉُ�
        for (int i = 0; i < (int)AttackCollisionValue.Effect.Length - 1; i++)
        {
            if (!effects[i].IsAlive(true)) continue;
            if (effectCounts[i] >= 0)
            {
                effectCounts[i] -= Time.deltaTime;
            }
            else
            {
                Effect((AttackCollisionValue.Effect)i + 1, default);
                effectCounts[i] += GameManager.effect[i].effectInterval;
            }
        }
        animator.SetBool(starHash, effects[1].IsAlive(true));
    }
    //�N���オ��
    protected virtual void ReMove()
    {
        animator.SetTrigger(standupHash);
        foreach (var afterimage in afterimages)
        {
            afterimage.SetTrigger(standupHash);
        }
        down = 0f;
    }
    //�U��
    public virtual void Attack(int currectAnime)
    {
        if (currectAnime == idleHash || currectAnime == moveHash)
        {
            if (Mathf.Abs(GameManager.playerManager.transform.position.x - transform.position.x) <= attackArea)
            {
                AttackIdle();
            }
            else
            {
                Move();
            }
        }
    }
    //�U���͈͓�����
    void AttackIdle()
    {
        if (attackCount > 0f)
        {
            animator.SetBool(moveHash, false);
            foreach (var afterimage in afterimages)
            {
                StartCoroutine(afterimage.SetBool(moveHash, false));
            }
        }
        else
        {
            animator.SetTrigger(attackHash);
            attackCount = attackSpeed;
            foreach (var afterimage in afterimages)
            {
                afterimage.SetTrigger(attackHash);
            }
        }
    }
    //�ړ�
    void Move()
    {
        animator.SetBool(moveHash, true);
        foreach (var afterimage in afterimages)
        {
            StartCoroutine(afterimage.SetBool(moveHash, true));
        }
    }
    //��s�����X�^�[�̍U��
    public virtual void AttackFly(int currectAnime)
    {
        if (currectAnime == idleHash || currectAnime == moveHash)
        {
            if ((GameManager.playerManager.transform.position - transform.position).sqrMagnitude <= attackArea * attackArea)
            {
                AttackIdle();
            }
            else
            {
                Move();
            }
        }
    }
    //��Ԉُ�̏���
    public void Effect(AttackCollisionValue.Effect effect, float value)
    {
        if (effect == AttackCollisionValue.Effect.Poison) Damage(GameManager.effect[(int)effect - 1].attackCollisionValue, default);
    }
    protected virtual void OnDestroy()
    {
        tweenerX?.Kill();
        tweenerY?.Kill();
    }
}
