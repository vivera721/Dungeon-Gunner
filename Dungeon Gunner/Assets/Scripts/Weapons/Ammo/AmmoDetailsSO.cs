using UnityEngine;

[CreateAssetMenu(fileName = "AmmoDetails_", menuName = "Scriptable Objects/Weapons/Ammo Details")]
public class AmmoDetailsSO : ScriptableObject
{
    #region Header BASIC AMMO DETAILS
    [Space(10)]
    [Header("BASIC AMMO DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("Name for the ammo")]
    #endregion
    public string ammoName;
    public bool isPlayerAmmo;

    #region Header AMMO SPRITE, PREFAB & MATERIAL
    [Space(10)]
    [Header("AMMO SPRITE, PREFAB & MATERIAL")]
    #endregion
    #region Tooltip
    [Tooltip("Sprite to be used for the ammo")]
    #endregion
    public Sprite ammoSprite;
    #region Tooltip
    [Tooltip("Popublate with the prefab to be used for the ammo. If multiple prefabs are specified then a random prefab from the array will be selected" +
        "The prefab can be an ammo patern - as long as it conforms to the IFireable interface.")]
    #endregion
    public GameObject[] ammoPrefabArray;
    #region Tooltip
    [Tooltip("The material to be used for the ammo")]
    #endregion
    public Material ammoMaterial;
    #region Tooltip
    [Tooltip("The material to be used for the ammo")]
    #endregion
    public float ammoChargeTime = 0.1f;
    #region Tooltip
    [Tooltip("If the ammo has a charge time then specify what material should be used to render the ammo while charging")]
    #endregion
    public Material ammoChargeMaterial;

    #region Header AMMO BASE PARAMETERS
    [Space(10)]
    [Header("AMMO BASE PARAMETERS")]
    #endregion
    #region Tooltip
    [Tooltip("The damage each ammo deals")]
    #endregion
    public int ammoDamage = 1;
    // 탄의 속도가 동일하다면 속도는 고정될것이고, 탄의 속도가 다르다면 - min, max 값이 - 임의의 속도를 가진다
    #region Tooltip
    [Tooltip("The minimun speed of the ammo - the speed will be a random value between the min and max")]
    #endregion
    public float ammoSpeedMin = 20f;
    #region Tooltip
    [Tooltip("The maximum speed of the ammo - the speed will be a random value between the min and max")]
    #endregion
    public float ammoSpeedMax = 20f;
    #region Tooltip
    [Tooltip("The range of the ammo ( or ammo pattern) in unity units")]
    #endregion
    public float ammoRange = 20f;
    #region Tooltip
    [Tooltip("The rotation speed in degrees per second of the ammo pattern")]
    #endregion
    public float ammoRotationSpeed = 1f;

    #region Header AMMO SPREAD DETAILS
    [Space(10)]
    [Header("AMMO SPREAD DETAILS")]
    #endregion
    // 탄약의 최소 / 최대 확산 각도. 산포도가 높을수록 정확도가 떨어진다. 최소값과 최대값 사이에 랜덤 스프레드가 계산됨
    // 값이 같다면 하나의 값만 가질것이고 다르다면 둘 사이의 임의의 값을 가지게 됨
    #region Tooltip
    [Tooltip("This is the minimum spread angle of the ammo. A higher spread means less accuracy. A random spread is calculated between the min and max values")]
    #endregion
    public float ammoSpreadMin = 0f;
    #region Tooltip
    [Tooltip("This is the maximum spread angle of the ammo. A higher spread means less accuracy. A random spread is calculated between the min and max values")]
    #endregion
    public float ammoSpreadMax = 0f;

    #region Header AMMO SPAWN DETAILS
    [Space(10)]
    [Header("AMMO SPAWN DETAILS")]
    #endregion
    // 탄 생성 숫자 . 일반 총은 하나씩 나가지만 샷건의 경우 여러개가 나갈것
    #region Tooltip
    [Tooltip("This is the minimum number of ammo that are spawned per shot. A random number of ammo are spawned between the minimum and maximum values")]
    #endregion
    public int ammoSpawnAmountMin = 1;
    #region Tooltip
    [Tooltip("This is the maximum number of ammo that are spawned per shot. A random number of ammo are spawned between the minimum and maximum values")]
    #endregion
    public int ammoSpawnAmountMax = 1;
    // 탄약 생성간의 시간 간격
    #region Tooltip
    [Tooltip("Minimum spawn interval time. The time interval in seconds between spawned ammo is a random value between the minimum and maximum values specified")]
    #endregion
    public float ammoSpawnIntervalMin = 0f;
    #region Tooltip
    [Tooltip("Maximum spawn interval time. The time interval in seconds between spawned ammo is a random value between the minimum and maximum values specified")]
    #endregion
    public float ammoSpawnIntervalMax = 0f;

    // 탄 궤적 (궤도) - 레이저와 같은 무기, 권총같은 무기는 총알이 날라갈때 탄 궤적이 남지 않음
    #region Header AMMO TRAIL DETAILS
    [Space(10)]
    [Header("AMMO TRAIL DETAILS")]
    #endregion

    #region Tooltip
    [Tooltip("Selected if an ammo trail is required, otherwise deselect. If selected then the rest of the ammo trail values should be populated.")]
    #endregion
    public bool isAmmoTrail = false;
    // 탄 궤적이 남는 시간 - 긴 궤적 == 큰 값
    #region Tooltip
    [Tooltip("Ammo trail lifetime in seconds")]
    #endregion
    public float ammoTrailTime = 3f;
    #region Tooltip
    [Tooltip("Ammo trail material")]
    #endregion
    public Material ammoTrailMaterial;
    #region Tooltip
    [Tooltip("The starting width for the ammo trail")]
    #endregion
    [Range(0f, 1f)] public float ammoTrailStartWidth;
    #region Tooltip
    [Tooltip("The ending width for the ammo trail")]
    #endregion
    [Range(0f, 1f)] public float ammoTrailEndWidth;


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(ammoName), ammoName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoSprite), ammoSprite);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoPrefabArray), ammoPrefabArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoMaterial), ammoMaterial);
        if (ammoChargeTime > 0)
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoChargeMaterial), ammoChargeMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoDamage), ammoDamage, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpeedMin), ammoSpeedMin, nameof(ammoSpeedMax), ammoSpeedMax, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoRange), ammoRange, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpreadMin), ammoSpreadMin, nameof(ammoSpreadMax), ammoSpreadMax, true);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnAmountMin), ammoSpawnAmountMin, nameof(ammoSpawnAmountMax), ammoSpawnAmountMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnIntervalMin), ammoSpawnIntervalMin, nameof(ammoSpawnIntervalMax), ammoSpawnIntervalMax, true);
        if (isAmmoTrail)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailTime), ammoTrailTime, false);
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoTrailMaterial), ammoTrailMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailStartWidth), ammoTrailStartWidth, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailEndWidth), ammoTrailEndWidth, false);
        }
    }
#endif
    #endregion



}
