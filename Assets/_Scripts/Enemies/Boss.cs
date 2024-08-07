using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    [Header("Configuration")]
    [SerializeField] private float jumpMaxHeight;
    [SerializeField] private float landRecoveryTime;

    [Header("Prefabs")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private GameObject landSmokePrefab;

    private bool _isGroundedLock = false;

    public override void Start()
    {
        base.Start();
        StartCoroutine(nameof(BossSpawn));
    }

    public override void Update()
    {
        base.Update();

        if (isGrounded && !_isGroundedLock) StartCoroutine(nameof(BossLand));
        else if (!isGrounded) _isGroundedLock = false;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Arrow"))
        {
            if (isColliding) return;
            isColliding = true;
            print(hp + " - " + 2 + " = " + (hp - 2));
            hp -= 2;
            if (hp <= 0) StartCoroutine(EnemyDeath(col.gameObject));
            else StartCoroutine(EnemyHit(col.gameObject));
        }
        else
        if (col.name == "Trigger_EnemyCenterToAttack")
        {

        }

        if (col.name == "Trigger_EnemyStartAI")
        {

        }
        else
        if (col.name == "Trigger_EnemyCenterToDrop")
        {
            StopCoroutine(nameof(BossDodge));
            EnemyMoveCenter();
        }

    }

    private IEnumerator BossDodge()
    {
        while (!moveCenter)
        {
            string[] actions = { "MoveForward", "MoveRight", "MoveLeft", "Jump", "JumpAttack" };
            int random = Random.Range(0, actions.Length);
            float moveTime = Random.Range(3f, 6f);
            yield return new WaitForSeconds(moveTime);

            switch (actions[random])
            {
                case "MoveForward":
                    moveRight = false;
                    moveLeft = false;
                    break;
                case "MoveRight":
                    moveRight = true;
                    moveLeft = false;
                    break;
                case "MoveLeft":
                    moveRight = false;
                    moveLeft = true;
                    break;
                case "Jump":
                    moveRight = false;
                    moveLeft = false;
                    if (canMove && isGrounded)
                    {
                        //moveForward = false;
                        rb.velocity = Vector3.zero;
                        rb.AddForce(10f * jumpForce * Vector3.up, ForceMode.Impulse);
                        yield return new WaitForSeconds(3f);
                    }
                    break;
                case "JumpAttack":
                    moveRight = false;
                    moveLeft = false;
                    if (canMove && isGrounded)
                    {
                        //moveForward = false;
                        rb.velocity = Vector3.zero;
                        rb.AddForce(10f * jumpForce * Vector3.up, ForceMode.Impulse);
                        yield return new WaitForSeconds(3f);
                    }
                    break;
            }
        }
    }

    private IEnumerator BossSpawn()
    {
        rb.AddForce(2f * jumpForce * Vector3.up, ForceMode.Impulse);
        yield return new WaitForSeconds(.3f);
        levitate = true;
        yield return new WaitForSeconds(2f);
        canMove = false;
        levitate = false;
    }

    private IEnumerator BossLand()
    {
        _isGroundedLock = true;
        animator.SetTrigger("isLanding");
        var ar = new Vector3(transform.position.x, 0, transform.position.z);
        Instantiate(landSmokePrefab, ar, Quaternion.identity);
        yield return new WaitForSeconds(2f);
        canMove = true;
        animator.SetTrigger("isRunning");
        StartCoroutine(nameof(BossDodge));
    }

}
