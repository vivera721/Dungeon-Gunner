using UnityEngine;

public class AmmoPattern : MonoBehaviour, IFireable
{
    #region Tooltip
    [Tooltip("Populate the array with the child ammo gameobjects")]
    #endregion
    [SerializeField] private Ammo[] ammoArray;

    private float ammoRange;
    private float ammoSpeed;
    private Vector3 fireDirectionVector;
    private float fireDirectionAngle;
    private AmmoDetailsSO ammoDetails;
    private float ammoChargeTimer;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void InitialiseAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement)
    {
        this.ammoDetails = ammoDetails;

        this.ammoSpeed = ammoSpeed;

        // Set fire direction
        SetFireDirection(ammoDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        // Set ammo range
        ammoRange = ammoDetails.ammoRange;

        // Activate ammo pattern gameobject
        gameObject.SetActive(true);

        foreach (Ammo ammo in ammoArray)
        {
            ammo.InitialiseAmmo(ammoDetails, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector, true);
        }

        // Set ammo charge timer - this will hold the ammo briefly
        if (ammoDetails.ammoChargeTime > 0f)
        {
            ammoChargeTimer = ammoDetails.ammoChargeTime;
        }
        else
        {
            ammoChargeTimer = 0f;
        }
    }

    private void Update()
    {
        // ammo charge effect
        if (ammoChargeTimer > 0f)
        {
            ammoChargeTimer -= Time.deltaTime;
            return;
        }

        // calculate distance vector to move ammo
        // 움직이는 탄약을 통해 거리벡터를 구함
        Vector3 distanceVector = fireDirectionVector * ammoSpeed * Time.deltaTime;

        transform.position += distanceVector;

        // Rotate ammo
        transform.Rotate(new Vector3(0f, 0f, ammoDetails.ammoRotationSpeed * Time.deltaTime));

        // Disable after max range reached
        ammoRange -= distanceVector.magnitude;

        if (ammoRange < 0f)
        {
            DisableAmmo();
        }
    }

    /// <summary>
    /// Set ammo fire direction based on the input angle and direction adjusted by the random spread
    /// 랜덤 확산으로 조정된 입력 방향을 기반으로 탄 발사 방향 및 각도 설정
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

        // Set ammo fire direction
        // fireDirectionAngle 발사 방향 각도를 이용하여 단위 발사 방향 벡터를 반환
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);

    }

    /// <summary>
    /// Disable the ammo - thus returning it to the object pool
    /// </summary>
    private void DisableAmmo()
    {
        // Disable the ammo pattern game object
        gameObject.SetActive(false);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoArray), ammoArray);
    }
#endif
    #endregion

}
