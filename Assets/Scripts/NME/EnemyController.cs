using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState 
{
    Ideling,
    Walking,
    Attacking,
    HitReacting,
    Isdead
}

    

public class EnemyController : MonoBehaviour
{
    //UI purpose
    public Sprite enemyImage;
    public string enemyName;

    private Rigidbody rigidBody;
    private SpriteRenderer spriteRenderer;
    public float damageTime = 0.05f;
    public int maxHealth;
    public int currentHealth;
    public float attackRate = 1f;
    private float nextAttack;
    private float damageTimer;

    private PlayerContoller player;
    private UIManager hud;
    public EnemyState enemyState = EnemyState.Ideling;
    //Animation
    private Animator animator;

    //HitBoxes
    public GameObject hitBoxPunch;
    private Vector3 originalHitBoxPosition;
    bool isFacingLeft = true;

    //movement
    public bool facingRight = false;
    private float currentSpeed = 2;
    public float maxSpeed;
    private float zDirection;
    private float hDirection;
    private float walkTimer;
    //distance of the character to stop
    public float stopDistanceX = 2f;
    public float stopDistanceZ = 1f;
    private Transform playerTarget;
    private Vector3 targetDistance;
    //For limit of the Enemy movement up and down
    public float minHeight, maxHeight;

    //Audio
    private AudioSource audioSource;
    public AudioClip[] hurtSound, dieSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerTarget = FindAnyObjectByType<PlayerContoller>().transform;
        player = FindAnyObjectByType<PlayerContoller>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        hud = FindObjectOfType<UIManager>();
        targetDistance = playerTarget.position - transform.position;
        originalHitBoxPosition = hitBoxPunch.transform.localPosition;

        //ignore collision between enemies
        int enemyLayer = LayerMask.NameToLayer("NME");
        Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
    }

    void Update()
    {
       
        //function to flip the sprite
        FlipSprite();

        //distance to player
        targetDistance = playerTarget.position - transform.position;

        //set walking timer
        walkTimer += Time.deltaTime;
    }

    
    private void FlipSprite()
    {
        if (enemyState != EnemyState.Attacking && !IsDead())
        {

            facingRight = (playerTarget.position.x <= transform.position.x) ? false : true;

            if (facingRight && isFacingLeft)
            {
                spriteRenderer.flipX = true;
                hitBoxPunch.transform.localPosition = new Vector3(-originalHitBoxPosition.x, originalHitBoxPosition.y, originalHitBoxPosition.z);
                isFacingLeft = false;
            }
            else if(!facingRight && !isFacingLeft)
            {
                spriteRenderer.flipX = false;
                hitBoxPunch.transform.localPosition = originalHitBoxPosition;
                isFacingLeft = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsDead())
        {
            if (!playerTarget.GetComponent<PlayerContoller>().IsDead())
            {
                
                if (enemyState == EnemyState.Walking)
                {
                   //direction of the movement
                    hDirection = targetDistance.x / Mathf.Abs(targetDistance.x);
                    zDirection = targetDistance.z / Mathf.Abs(targetDistance.z);

                    if (Mathf.Abs(targetDistance.x) <= stopDistanceX)
                    {
                    hDirection = 0;
                    }

                     //move enemy
                     rigidBody.velocity = new Vector3(hDirection * currentSpeed, 0, zDirection * currentSpeed);

                     //play animation
                     animator.SetFloat("Speed", Mathf.Abs(currentSpeed));

                }
                //set enemyState to walking if player is too far 
                if(Mathf.Abs(targetDistance.x) <= stopDistanceX && Mathf.Abs(targetDistance.z) <= stopDistanceZ)
                {
                ToEnemyStateAttacking();
                animator.SetTrigger("Attack");
                }
                else ToEnemyStateWalking();
            }
        }
            //limit up and down of enemy
            rigidBody.position = new Vector3(rigidBody.position.x,
                                            rigidBody.position.y,
                                            Mathf.Clamp(rigidBody.position.z, minHeight, maxHeight));
    }

    public void ResetSpeed()
    {
        currentSpeed = maxSpeed;
    }

    public void HandleDamage(int damageAmount)
    {
        if(!IsDead())
        {
            currentHealth -= damageAmount;
            animator.SetTrigger("IsHit");
            PlaySFX(hurtSound);

            FindObjectOfType<UIManager>().UpdateEnemyUI(maxHealth,currentHealth,enemyName,enemyImage);

            if(currentHealth <= 0)
            {
                if (!facingRight)
                {
                    //force to push the enemy when is dead
                    rigidBody.AddRelativeForce(new Vector3(3, 4, 0), ForceMode.Impulse);
                }
                else
                {
                    rigidBody.AddRelativeForce(new Vector3(-3, 4, 0), ForceMode.Impulse);
                }
                    Die();
                    PlaySFX(dieSound);
                    GameManager.instance.EnemyCount();
            }
        }
    }
    public void Die()
    {
        animator.SetBool("IsDead", true);
        enemyState = EnemyState.Isdead;
    }

    public bool IsDead()
    {
        return enemyState == EnemyState.Isdead;
    }
    public void ToEnemyStateWalking()
    {
        enemyState = EnemyState.Walking;
        currentSpeed = maxSpeed;
    }
    public void ToEnemyStateIsDead()
    {
        enemyState = EnemyState.Isdead;
    }
    public void ToEnemyStateIdeling()
    {
        enemyState = EnemyState.Ideling;
    }
    public void ToEnemyStateGetHit()
    {
        enemyState = EnemyState.HitReacting;
        currentSpeed = 0;
    }
    public void ToEnemyStateAttacking()
    {
        enemyState = EnemyState.Attacking;
        currentSpeed = 0;
    }
    public void DisableEnemy()
    {
        //deactivate enemy
        gameObject.SetActive(false);
    }

    public void PlaySFX(AudioClip[] clip)
    {
        int indexNumber = Random.Range(0, clip.Length);
        audioSource.clip = clip[indexNumber];
        audioSource.Play();
    }

}
