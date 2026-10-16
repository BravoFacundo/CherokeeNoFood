using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{   
    private enum ShootMode { BowPosition, InputPosition, HUDPosition }

    [Header("Configuration")]
    [SerializeField] private ShootMode shootMode; 
    public float shootForce;
    private float defaultShootForce;
    [SerializeField] private float shootChargeSpeed, shootMaxCharge, shootReloadTime;
    private ForceMode forceMode;

    [Header("Debug")]
    public bool isReloading;
    private bool isCharging;
    [SerializeField] private float currentChargeTime;

    [Header("Bow Rotation")]
    [SerializeField] private float maxBowYRotation;
    private float mouseXpos;
    private float bowYRotation;

    [Header("References")]
    [SerializeField] private ParticleManager particleManager;
    [SerializeField] private GameObject bowObject;

    [Header("Local References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject enemyAttack;
    private Animator bowAnimator;
    private Camera cam;

    [Header("Prefabs")]
    [SerializeField] private Rigidbody arrowPrefab;
    [SerializeField] private Rigidbody arrowForkPrefab;

    private void Awake()
    {
        cam = Camera.main;
        bowAnimator = bowObject.GetComponent<Animator>();
    }

    void Start()
    {
        bowAnimator.SetFloat("Arrow_ChargeSpeed", 1 / shootMaxCharge);
        bowAnimator.SetFloat("Arrow_ReloadSpeed", 1 / shootReloadTime);
        defaultShootForce = shootForce;
        isReloading = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // Paused: clicks belong to the UI
        Mouse_Inputs();
        RotateBowByMouseXPosition();
    }

    private void Mouse_Inputs()
    {
        if (Input.GetMouseButtonDown(2)) bowAnimator.SetTrigger("Arrow_Reload");
        if (Input.GetMouseButtonDown(1))
        {
            var f = bowAnimator.GetFloat("Arrow_ChargeSpeed");
            bowAnimator.SetFloat("Arrow_ChargeSpeed", f * -1);
        }

        if (Input.GetMouseButtonDown(0)) BeginCharging();
        if (Input.GetMouseButton(0)) UpdateCharging();
        if (Input.GetMouseButtonUp(0)) EndCharging();
    }

    private void BeginCharging()
    {
        currentChargeTime = 0;
        bowAnimator.SetBool("Arrow_Charge", true);
    }
    private void UpdateCharging()
    {
        isCharging = true;
        bowAnimator.SetBool("Arrow_Charge", true);
        if (isCharging && !isReloading && currentChargeTime < shootMaxCharge)
        {
            currentChargeTime += Time.deltaTime * shootChargeSpeed;
        }
    }
    private void EndCharging()
    {
        if (currentChargeTime <= shootMaxCharge * 0.50) CancelCharging();
        else 
        if (currentChargeTime >= shootMaxCharge * 0.50 && currentChargeTime <= shootMaxCharge * 0.75) StartCoroutine(nameof(ShootArrow), 1);
        else 
        if (currentChargeTime >= shootMaxCharge * 0.75 && currentChargeTime <= shootMaxCharge + 1.00) StartCoroutine(nameof(ShootArrow), 2);

        bowAnimator.SetBool("Arrow_Charge", false);
    }

    private void CancelCharging()
    {
        currentChargeTime = 0;
        bowAnimator.SetBool("Arrow_Charge", false);
        bowAnimator.SetTrigger("Arrow_CancelCharge");
    }
    private void ExecuteShoot()
    {
        isCharging = false;
        currentChargeTime = 0;
        isReloading = true;

        bowAnimator.SetBool("Arrow_Charge", false);
        bowAnimator.SetTrigger("Arrow_Shoot");
    }
    private void ReloadShoot()
    {
        bowAnimator.SetTrigger("Arrow_Reload");
        shootForce = defaultShootForce;
        isReloading = false;
    }

    private void RotateBowByMouseXPosition()
    {
        mouseXpos = Mathf.InverseLerp(0, Screen.width, Input.mousePosition.x);
        bowYRotation = Mathf.Lerp(0, maxBowYRotation, mouseXpos);
        bowObject.transform.rotation = Quaternion.Euler(new Vector3(0, bowYRotation, 0));
    }

    IEnumerator ShootArrow(int shootDamage)
    {
        ExecuteShoot();

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Quaternion rotation = Quaternion.LookRotation(ray.direction);

        var enemySpriteRenderer = enemyAttack.GetComponent<SpriteRenderer>();

        ApplyShootDamageMultiplier(shootDamage);
        SetShootMode(enemySpriteRenderer);

        Vector3 spawnPosition = CalculateSpawnPosition();
        CreateArrow(rotation, spawnPosition, shootDamage);

        yield return new WaitForSeconds(shootReloadTime);
        ReloadShoot();
    }

    private void ApplyShootDamageMultiplier(int shootDamage)
    {
        if (shootDamage == 2) shootForce *= 1.25f;
    }

    private void SetShootMode(SpriteRenderer enemySpriteRenderer)
    {
        if (enemySpriteRenderer.sprite != null)
            shootMode = CheckSpriteAlpha(enemySpriteRenderer) ? ShootMode.InputPosition : ShootMode.HUDPosition;
        else shootMode = ShootMode.BowPosition;
    }

    private Vector3 CalculateSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;

        switch (shootMode)
        {
            case ShootMode.BowPosition:
                Vector2 bowSizeDelta = bowObject.GetComponent<RectTransform>().sizeDelta;
                Vector2 pivotPosition = new(
                    bowObject.transform.position.x - (bowSizeDelta.x * 0.5f),
                    bowObject.transform.position.y + (bowSizeDelta.y * 0.5f));
                spawnPosition = cam.ScreenToWorldPoint(new Vector3(pivotPosition.x, pivotPosition.y, cam.nearClipPlane + .6f));
                break;

            case ShootMode.InputPosition:
                spawnPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane + .6f));
                break;

            case ShootMode.HUDPosition:
                spawnPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane + .9f));
                break;
        }

        return spawnPosition;
    }

    private Rigidbody CreateArrow(Quaternion rotation, Vector3 spawnPosition, float damage)
    {
        Rigidbody arrow = (shootMode == ShootMode.HUDPosition) ? arrowForkPrefab : arrowPrefab;
        Rigidbody newArrow = Instantiate(arrow, spawnPosition, rotation);
        newArrow.transform.Rotate(0, 0, Random.Range(0, 180), Space.Self);
        newArrow.GetComponent<ArrowData>().arrowDamage = damage;

        if (shootMode == ShootMode.HUDPosition)
        {
            newArrow.isKinematic = true;
            Destroy(newArrow.gameObject, .5f);
            particleManager.ImpactExplosion(spawnPosition + new Vector3(0, 0, -.1f), Quaternion.identity, false);
            StartCoroutine(playerController.StopReceivingDamage());
        }
        else
        {
            newArrow.AddForce(rotation * Vector3.forward * shootForce, ForceMode.VelocityChange);
        }

        return newArrow;
    }

    private bool CheckSpriteAlpha(SpriteRenderer enemySpriteRenderer)
    {
        Texture2D attackTexture = (Texture2D)enemySpriteRenderer.sprite.texture;
        if (attackTexture == null) return false;

        Vector2 pixelUV = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        int uvX = (int)pixelUV.x;
        int uvY = (int)pixelUV.y;
        
        Color hitColor = attackTexture.GetPixel(uvX, uvY);
        if (hitColor.a > 0.05)
        {
            return false; //Debug.Log("La coordenada " + uvX + "/" + uvY + " en " + "Enemigo" + " NO ES ALPHA");
        }
        else
        {
            return true; //Debug.Log("La coordenada " + uvX + "/" + uvY + " en " + "Enemigo" + " ES ALPHA");
        }
    }

}