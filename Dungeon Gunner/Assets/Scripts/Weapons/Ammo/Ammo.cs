using UnityEngine;

[DisallowMultipleComponent]
public class Ammo : MonoBehaviour, IFireable
{
    #region Tooltip
    [Tooltip("Populate with child TrailRanderer component")]
    #endregion
    [SerializeField] private TrailRenderer trailRenderer;

    private float ammoRange = 0f; // the range of each ammo
    private float ammoSpeed;
    private Vector3 fireDirectionVector;
    private float fireDirectionAngle;
    private SpriteRenderer spriteRenderer;
    private AmmoDetailsSO ammoDetails;
    private float ammoChargeTimer;
    private bool isAmmoMaterialSet = false;
    private bool overrideAmmoMovement;
    private bool isColliding = false;


    private void Awake()
    {
        // cache sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Ammo charge effect
        if (ammoChargeTimer > 0f)
        {
            ammoChargeTimer -= Time.deltaTime;
            return;
        }
        else if (!isAmmoMaterialSet)
        {
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }

        // Don't move ammo if movement has been overriden - e.g. this ammo is part of an ammo pattern
        if (!overrideAmmoMovement)
        {
            // Calculate distance vector to move ammo
            // 거리벡터 설정
            Vector3 distanceVector = fireDirectionVector * ammoSpeed * Time.deltaTime;

            // 탄의 위치에 거리벡터가 추가됨으로서 움직임
            transform.position += distanceVector;

            // 거리벡터의 크기만큼 탄의 범위를 줄임
            // Disable after max range reached
            ammoRange -= distanceVector.magnitude;

            // 탄이 최대거리 이동후 비활성화 - 거리가 0보다 작은데 탄이 계속 남아있는 것 방지
            if (ammoRange < 0f)
            {
                if (ammoDetails.isPlayerAmmo)
                {
                    // no multiplier
                    StaticEventHandler.CallMultiplierEvent(false);
                }

                DisableAmmo();
            }

        }


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If already colliding with something return
        // 이미 다른것과 충돌했다면 다시 반환
        if (isColliding) return;

        // Deal Damage To Collision Object
        // 충돌된 물체에게 데미지 줌
        DealDamage(collision);

        // Show ammo hit effect
        AmmoHitEffect();

        DisableAmmo();
    }

    /// <summary>
    /// 충돌된 물체에게 데미지 줌 -- ammoDetails에 있는 데미지 만큼
    /// </summary>
    private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();

        bool enemyHit = false;

        if (health != null)
        {
            // Set isColliding to prevent ammo dealing damage multiple times
            isColliding = true;

            health.TakeDamage(ammoDetails.ammoDamage);

            // Enemy hit
            if (health.enemy != null)
            {
                enemyHit = true;
            }
        }

        // If player ammo then update multiplier
        if (ammoDetails.isPlayerAmmo)
        {
            if (enemyHit)
            {
                // multiplier
                StaticEventHandler.CallMultiplierEvent(true);
            }
            else
            {
                // no multiplier
                StaticEventHandler.CallMultiplierEvent(false);
            }

        }
    }

    /// <summary>
    /// Initialise the ammo being fired - using the ammoDetails, the aimangle, weaponAngle, and weaponAimDirectionVector
    /// If this ammo is part of a pattern the ammo movement can be overriden by setting overrideAmmoMovement to true
    /// ammoDetails, the aimangle, weaponAngle, and weaponAimDirectionVector 와 같은 변수들을 사용하여 사용되는 탄을 초기화 함
    /// 탄이 패턴의 일부일 경우 overrideAmmoMovement 를 true 로 바꾸어서 탄의 이동을 재정의(override) 할 수 있다
    /// </summary>
    public void InitialiseAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement = false)
    {
        #region Ammo
        // 탄약 정보 멤버 변수 감지 - 탄약 세부 사항에서 과거와 동일하게 만듦
        this.ammoDetails = ammoDetails;

        // initialise isColliding
        isColliding = false;

        // Set fire direction - 발사 방향 설정
        SetFireDirection(ammoDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        // Set ammo sprite
        spriteRenderer.sprite = ammoDetails.ammoSprite;

        // set initial ammo material depending on whethere there is an ammo charge period
        // 탄약 충전 시간이 있으면 탄약 material 을 기반으로 설정
        if (ammoDetails.ammoChargeTime > 0f)
        {
            // Set ammo charge timer
            ammoChargeTimer = ammoDetails.ammoChargeTime;
            SetAmmoMaterial(ammoDetails.ammoChargeMaterial);
            isAmmoMaterialSet = false;
        }
        else
        {
            ammoChargeTimer = 0f;
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }

        // Set ammo range - 탄 범위 설정
        ammoRange = ammoDetails.ammoRange;

        // Set ammo speed
        this.ammoSpeed = ammoSpeed;

        // Override ammo movement - 패턴이 있는 공격이면 개별 탄을 무시하고 패턴에 의해 이동되도록 설정
        this.overrideAmmoMovement = overrideAmmoMovement;

        // Activate ammo gameobject
        gameObject.SetActive(true);

        #endregion

        #region Trail

        if (ammoDetails.isAmmoTrail)
        {
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.emitting = true;
            trailRenderer.material = ammoDetails.ammoTrailMaterial;
            trailRenderer.startWidth = ammoDetails.ammoTrailStartWidth;
            trailRenderer.endWidth = ammoDetails.ammoTrailEndWidth;
            trailRenderer.time = ammoDetails.ammoTrailTime;
        }
        else
        {
            trailRenderer.emitting = false;
            trailRenderer.gameObject.SetActive(false);
        }


        #endregion

    }

    /// <summary>
    /// Set ammo fire direction and angle based on the input angle and direction adjusted by the random spread
    /// 랜덤 확산으로 조정된 입력 각도와 방향을 기반으로 탄 발사 방향 및 각도 설정
    /// </summary>
    private void SetFireDirection(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        // calculate random spread angle between min and max
        float randomSpread = Random.Range(ammoDetails.ammoSpreadMin, ammoDetails.ammoSpreadMax);

        // 무작위 확산으로 탄이 목표위치 위 또는 아래로 이동해야 함
        // 1 or -1 의 값을 가져야 함
        // Get a random spread toggle of 1 or -1
        int spreadToggle = Random.Range(0, 2) * 2 - 1;

        // magnitude 는 벡터의 크기 length 
        if (weaponAimDirectionVector.magnitude < Settings.useAimAngleDistance)
        {
            fireDirectionAngle = aimAngle;
        }
        else
        {
            fireDirectionAngle = weaponAimAngle;
        }

        // Adjust ammo fire angle - by random spread
        fireDirectionAngle += spreadToggle * randomSpread;

        // Set ammo rotation
        // 탄이 회전해햐하니 발사방향각도를 기반으로 기본적으로 새 벡터와 같도록 한다 - 오일러 각도를 이용
        transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);

        // Set ammo fire direction
        // fireDirectionAngle 발사 방향 각도를 이용하여 단위 발사 방향 벡터를 반환
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);

    }


    /// <summary>
    /// Disable the ammo - thus returning it to the object pool
    /// 탄을 비활성화 함으로서 오브젝트 풀에 반환한다 - 오브젝트 풀링 기법
    /// </summary>
    private void DisableAmmo()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Display the ammo hit effect
    /// </summary>
    private void AmmoHitEffect()
    {
        // Process if a hit effect has been specified
        // 탄 충돌 이펙트와 탄 충돌 이펙트 프리팹이 둘다 있으면
        if (ammoDetails.ammoHitEffect != null && ammoDetails.ammoHitEffect.ammoHitEffectPrefab != null)
        {
            // Get ammo hit effect gameobject from the pool (with particle system component)
            // 오브젝트 풀에서 탄 충돌 게임오브젝트를 꺼낸다 (파티클 시스템과 같이)
            AmmoHitEffect ammoHitEffect = (AmmoHitEffect)PoolManager.Instance.ReuseComponent
                (ammoDetails.ammoHitEffect.ammoHitEffectPrefab, transform.position, Quaternion.identity);

            // Set Hit Effect
            // 탄 충돌 이펙트 설정
            ammoHitEffect.SetHitEffect(ammoDetails.ammoHitEffect);

            // Set gameobject active (the particle system is set to automatically disable the gameobject once finished)
            ammoHitEffect.gameObject.SetActive(true);

        }
    }


    private void SetAmmoMaterial(Material material)
    {
        spriteRenderer.material = material;
    }

    // 개별 탄이 아닌 탄 패턴 gameobject 를 반환
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    // 렌더러가 지정되었는지 유효성 검사
    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(trailRenderer), trailRenderer);
    }
#endif
    #endregion

}
