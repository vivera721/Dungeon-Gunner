using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class Door : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion

    #region Tooltip
    [Tooltip("Populate this with the BoxCollider2D component on the DoorCollider gameobject")]
    #endregion
    [SerializeField] private BoxCollider2D doorCollider; // 문 충돌판정

    [HideInInspector] public bool isBossRoomDoor = false; // 보스방은 잠김
    private BoxCollider2D doorTrigger; // 문 트리거 작동 판정
    private bool isOpen = false; // 문 여닫음 판별
    private bool previouslyOpened = false; // 이전에 열린 문 판별
    private Animator animator;

    private void Awake()
    {
        // disable door collider by default
        doorCollider.enabled = false;

        animator = GetComponent<Animator>();
        doorTrigger = GetComponent<BoxCollider2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            OpenDoor();
        }
    }

    /// <summary>
    /// Open the door
    /// </summary>
    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            previouslyOpened = true;
            doorCollider.enabled = false;
            doorTrigger.enabled = false;

            // Set open parameter in animator
            animator.SetBool(Settings.open, true);

        }
    }


    private void OnEnable()
    {
        // [ 플레이어가 방에서 멀어질때 문이 사라지게 됨 (검은 어둠에 의해 보이지 않게 됨) 그 때 ]
        // 부모 gameobject 가 disable (비활성) 되면 animator 상태가 리셋된다
        // 그러므로 animator 상태를 복구해야함
        animator.SetBool(Settings.open, isOpen);
    }

    /// <summary>
    /// Lock the door
    /// </summary>
    public void LockDoor()
    {
        isOpen = false;
        doorCollider.enabled = true;
        doorTrigger.enabled = false;

        // set open to false to close door
        animator.SetBool(Settings.open, false);
    }

    /// <summary>
    /// Unlock the door
    /// </summary>
    public void UnlockDoor()
    {
        doorCollider.enabled = false;
        doorTrigger.enabled = true;
        
        // 예전에 열린 문 이면 
        if (previouslyOpened == true)
        {
            // 문을 연다
            isOpen = false;
            OpenDoor();
        }
    }


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorCollider), doorCollider);
    }
#endif
    #endregion
}
