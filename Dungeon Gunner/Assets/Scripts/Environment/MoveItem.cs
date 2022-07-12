using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MoveItem : MonoBehaviour
{
    #region Header SOUND EFFECT
    [Space(10)]
    [Header("SOUND EFFECT")]
    #endregion

    #region Tooltip
    [Tooltip("The sound effect when this item is moved")]
    #endregion Tooltip
    [SerializeField] private SoundEffectSO moveSoundEffect;
    [HideInInspector] public BoxCollider2D boxCollider2D;
    private Rigidbody2D rigidbody2D;
    private InstantiatedRoom instantiatedRoom;
    private Vector3 previousPosition;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        instantiatedRoom = GetComponentInParent<InstantiatedRoom>();

        instantiatedRoom.moveableItemsList.Add(this);
    }

    /// <summary>
    /// Update the obstacle positions when something comes into contact
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateObstacles();
    }

    /// <summary>
    /// Update the obstacle position
    /// </summary>
    private void UpdateObstacles()
    {
        // Make sure the item stays within the room
        ConfineItemToRoomBounds();

        // Update moveable items in obstacles array
        instantiatedRoom.UpdateMoveableObestacles();

        // capture new position post collision
        previousPosition = transform.position;

        // Play sound if moving
        if (Mathf.Abs(rigidbody2D.velocity.x) > 0.001f || Mathf.Abs(rigidbody2D.velocity.y) > 0.001f)
        {
            // Play moving sound every 10 frames
            if (moveSoundEffect != null && Time.frameCount % 10 == 0)
            {
                SoundEffectManager.Instance.PlaySoundEffect(moveSoundEffect);
            }
        }
    }

    /// <summary>
    /// Confine the item stays within the room bounds
    /// </summary>
    private void ConfineItemToRoomBounds()
    {
        Bounds itemBounds = boxCollider2D.bounds;
        Bounds roomBounds = instantiatedRoom.roomColliderBounds;

        // if the item is being pushed beyond the room bounds then set the item position to its previous position
        if (itemBounds.min.x <= roomBounds.min.x ||
            itemBounds.max.x >= roomBounds.max.x ||
            itemBounds.min.y <= roomBounds.min.y ||
            itemBounds.max.y >= roomBounds.max.y)
        {
            transform.position = previousPosition;
        }
    }



}
