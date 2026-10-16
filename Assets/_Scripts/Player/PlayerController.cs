using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] int playerHealth = 11;
    [SerializeField] bool beingAttacked = false;
    [SerializeField] GameObject currentAttackingEnemy;
    private bool checkFirst = false;
    
    [Header("References")]
    [SerializeField] private Animator healthAnimator;
    [SerializeField] private Animator bowAnimator;
    [SerializeField] private Animator enemyAttackAnimator;
    [SerializeField] private Animator enemyImpactAnimator;
    private PlayerShoot playerShoot;

    void Start()
    {
        healthAnimator.SetInteger("Health", playerHealth);
        playerShoot = GetComponentInChildren<PlayerShoot>();
    }

    private void Update()
    {
        Mathf.Clamp(playerHealth, 0, 12);

        Debug_Inputs();
    }

    private IEnumerator HealPlayer(int healPoints)
    {
        var newHealth = Mathf.Clamp(playerHealth + healPoints, 0, 12);
        for (int i = playerHealth; i < newHealth + 1; i++)
        {
            playerHealth = i;
            healthAnimator.SetInteger("Health", playerHealth);
            yield return new WaitForSeconds(.2f);
        }
    }
    public IEnumerator DamagePlayer(int damagePoints)
    {
        var newHealth = Mathf.Clamp(playerHealth - damagePoints, 0, 12);
        healthAnimator.SetTrigger("ShakeBar");
        yield return new WaitForSeconds(.6f);
        for (int i = playerHealth; i > newHealth - 1; i--)
        {
            playerHealth = i;
            healthAnimator.SetInteger("Health", playerHealth);
            yield return new WaitForSeconds(.2f);
        }
    }

    public IEnumerator EnemyAttack(int damageReceived, string character, GameObject enemyObj)
    {
        currentAttackingEnemy = enemyObj;
        if (beingAttacked && !checkFirst)
        {
            GameManager.instance.SetGameState("pause", true);
            GameManager.instance.SetGameState("Loose", true);
            checkFirst = true;
        }
        else
        {
            beingAttacked = true;
            enemyAttackAnimator.SetBool($"{character}_Attacking", beingAttacked);
            StartCoroutine(nameof(ReceivingDamage));

            if (character != "Thug" && character != "Sumo")
            {
                Debug.LogError(enemyObj.name + " reached player. This should not be possible.");
                Destroy(enemyObj);
            }
        }            
        yield break;
    }

    private IEnumerator ReceivingDamage()
    {
        //shootAtClickPosition.shootMode = ShootAtClickPosition.ShootMode.ShootToHUDPosition;
        for (int i = 0; i < 12; i++)
        {
            if (beingAttacked)
            {
                StartCoroutine(nameof(DamagePlayer), 1);
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                beingAttacked = false; checkFirst = false;
                enemyAttackAnimator.SetBool("SumoDoingDamage", beingAttacked);
                StopCoroutine(nameof(DamagePlayer));
                StopCoroutine(nameof(ReceivingDamage));
                //shootAtClickPosition.shootMode = ShootAtClickPosition.ShootMode.ShootFromBowPosition;
            }
        }
        beingAttacked = false; checkFirst = false;
        enemyAttackAnimator.SetBool("SumoDoingDamage", beingAttacked);
        StopCoroutine(nameof(DamagePlayer));
        StopCoroutine(nameof(ReceivingDamage));
        //shootAtClickPosition.shootMode = ShootAtClickPosition.ShootMode.ShootFromBowPosition;
    }

    public IEnumerator StopReceivingDamage()
    {
        if (beingAttacked)
        {
            beingAttacked = false; checkFirst = false;
            yield return new WaitForSeconds(.5f);

            enemyAttackAnimator.SetBool("Sumo_Attacking", beingAttacked);
            enemyAttackAnimator.SetBool("Thug_Attacking", beingAttacked);
            StopCoroutine(nameof(DamagePlayer));
            StopCoroutine(nameof(ReceivingDamage));

            currentAttackingEnemy.GetComponent<Enemy>().EnemyDelete();
            currentAttackingEnemy = null;
        }
        yield break;
    }

    //-------------------------------------------------------------------------------------------------------------------------

    private void Debug_Inputs()
    {
        if (Input.GetKeyDown("m"))
        {
            playerHealth++;
            healthAnimator.SetTrigger("ShakeBar");
            healthAnimator.SetInteger("Health", playerHealth);
        }
        if (Input.GetKeyDown("n"))
        {
            playerHealth--;
            healthAnimator.SetTrigger("ShakeBar");
            healthAnimator.SetInteger("Health", playerHealth);
        }

        if (Input.GetKeyDown("b"))
        {
            StartCoroutine(nameof(HealPlayer), 3);

        }
        if (Input.GetKeyDown("v"))
        {
            StartCoroutine(nameof(DamagePlayer), 3);

        }
    }
}
