using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject clickEffectPrefab;
    [SerializeField] private float effectCoolDown = 0.3f;
    private Animator animator;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private float lastEffectTime = 0f;
    private bool isColliding = false;
    private float stuckTimer = 0f;
    private float maxStuckTime = 5f;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 inputPosition = Vector2.zero;
        bool inputDown = false;
        bool inputHeld = false;
        #if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (!GameUIManager.Instance.IsPointerOverUIElement) return;
            inputPosition = Camera.main.ScreenToWorldPoint(touch.position);
            if (touch.phase == TouchPhase.Began)
            {
                inputDown = true;
            }
            if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                inputHeld = true;
            }
        }
        #else
        if (Input.GetMouseButtonDown(0) && !GameUIManager.Instance.IsPointerOverUIElement)
        {
            inputPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            inputDown = true;
            inputHeld = true;
        }
        else if (Input.GetMouseButton(0) && !GameUIManager.Instance.IsPointerOverUIElement)
        {
            inputPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            inputHeld = true;
        }
        #endif
        if (inputDown)
        {
            SetTarget(inputPosition);
            SpawnClickEffect(inputPosition);
        }
        else if (inputHeld && Time.time - lastEffectTime > effectCoolDown)
        {
            SetTarget(inputPosition);
            SpawnClickEffect(inputPosition);
        }
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
            animator.SetBool("isMoving", true);
            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
            {
                isMoving = false;
                GameUIManager.Instance.isMoving = false;
                animator.SetBool("isMoving", false);
            }
        }
        if (isColliding && (Vector2)transform.position != targetPosition)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= maxStuckTime)
            {
                Debug.Log("Movement Cancelled Because Of Stuck Too Long");
                targetPosition = transform.position;
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        if (!isMoving && animator.GetBool("isMoving"))
        {
            animator.SetBool("isMoving", false);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Block Object"))
        {
            isColliding = true;
            isMoving = false;
            animator.SetBool("isMoving", false);
            targetPosition = transform.position;
        }
        else if (collision.gameObject.GetComponent<InteractableObject>() != null)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
            targetPosition = transform.position;
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Block Object"))
        {
            isColliding = false;
        }
    }
    private void SetTarget(Vector2 position)
    {
        targetPosition = position;
        isMoving = true;
        GameUIManager.Instance.isMoving = true;
    }
    private void SpawnClickEffect(Vector2 position)
    {
        if (clickEffectPrefab != null)
        {
            GameObject effect = Instantiate(clickEffectPrefab, position, Quaternion.Euler(-90f, 0f, 0f));
            ParticleSystemRenderer psRenderer = effect.GetComponent<ParticleSystemRenderer>();
            psRenderer.sortingLayerName = "Layer Foreground 1";
            Destroy(effect, 1f);
            lastEffectTime = Time.time;
        }
    }
}
