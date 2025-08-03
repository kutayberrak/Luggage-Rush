using UnityEngine;
using System.Collections.Generic;
using GameFolders.Scripts;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayer;
    [SerializeField] private Camera mainCam;
    private float maxDistance = 100f;
    private float liftOffset = 0.5f;    // Height to lift on hold
    private float liftForce = 5f;     // Force to lift object up
    private float hoverForce = 30f;     // Force to maintain hover
    private float holdThreshold = 0.02f;  // Time to distinguish hold vs click (reduced for faster response)

    private ClickableObject currentClickable;
    private MaterialPropertyBlock mpb;
    private Dictionary<ClickableObject, float> originalHeights = new Dictionary<ClickableObject, float>();
    private Dictionary<ClickableObject, float> targetHeights = new Dictionary<ClickableObject, float>();
    private Dictionary<ClickableObject, bool> isLifting = new Dictionary<ClickableObject, bool>();
    private float hoverStartTime;
    private bool isHolding;

    private float gravityY;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        GameEvents.OnGameStart += ClearDicts;
    }
    private void OnDisable()
    {
        GameEvents.OnGameStart -= ClearDicts;
    }
    private void Start()
    {
        gravityY = Mathf.Abs(Physics.gravity.y);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DoRaycast(true);
        }
        else if (Input.GetMouseButton(0))
        {
            DoRaycast(false);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentClickable != null)
            {
                AudioManager.Instance.PlaySFX("CollectSFX_1");
                currentClickable.OnClickedByPlayer();
                ClearOutline(currentClickable);
                currentClickable = null;
                isHolding = false;
            }
        }

        // Apply lift or hover force to held object
        if (isHolding && currentClickable != null)
        {
            ApplyLiftOrHover(currentClickable);
        }
    }

    private void DoRaycast(bool isDown)
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, maxDistance, clickableLayer))
        {
            var obj = hit.collider.GetComponent<ClickableObject>();
            if (obj != currentClickable)
            {
                if (currentClickable != null)
                    ClearOutline(currentClickable);

                currentClickable = obj;
                hoverStartTime = Time.time;

                SetOutline(currentClickable, 1f);
                isHolding = false;
            }

            if (!isHolding)
            {
                if (Time.time - hoverStartTime >= holdThreshold)
                {
                    StartHolding(currentClickable);
                }
            }
            else
            {
                // Object is being held - ApplyHover will handle the physics
            }
        }
        else if (isDown || isHolding)
        {
            if (currentClickable != null)
            {
                ClearOutline(currentClickable);
                currentClickable = null;
                isHolding = false;
            }
        }
    }

    private void StartHolding(ClickableObject obj)
    {
        if (obj == null) return;
        isHolding = true;
        RecordOriginalHeight(obj);
        SetTargetHeight(obj);

        // Start with lifting phase
        isLifting[obj] = true;

    }

    private void SetOutline(ClickableObject obj, float value)
    {
        var rend = obj.GetComponent<Renderer>();
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat("_OutlineMultiplier", value);
        rend.SetPropertyBlock(mpb);
    }

    private void ClearOutline(ClickableObject obj)
    {
        SetOutline(obj, 0f);
        // Clear target height when releasing object
        if (targetHeights.ContainsKey(obj))
        {
            targetHeights.Remove(obj);
        }

        // Clear lifting state
        if (isLifting.ContainsKey(obj))
        {
            isLifting.Remove(obj);
        }

        // DON'T clear original height here - let it persist until object settles
        // Original height will only be updated when object is stationary in RecordOriginalHeight
    }

    private void RecordOriginalHeight(ClickableObject obj)
    {
        if (obj == null) return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;

        bool isObjectMoving = Mathf.Abs(rb.linearVelocity.y) > 0.05f;

        if (originalHeights.ContainsKey(obj) && isObjectMoving)
        {

            return;
        }


        if (!isObjectMoving || !originalHeights.ContainsKey(obj))
        {
            float newOriginalHeight = obj.transform.position.y;

            originalHeights[obj] = newOriginalHeight;
        }
    }

    private void SetTargetHeight(ClickableObject obj)
    {
        if (obj != null && originalHeights.ContainsKey(obj))
        {
            float baseY = originalHeights[obj];
            targetHeights[obj] = baseY + liftOffset;
        }
    }

    private void ApplyLiftOrHover(ClickableObject obj)
    {
        if (obj == null) return;

        // Check if object is in lifting phase or hover phase
        if (isLifting.ContainsKey(obj) && isLifting[obj])
        {
            ApplyLift(obj);
        }
        else
        {
            ApplyHover(obj);
        }
    }

    private void ApplyLift(ClickableObject obj)
    {
        if (obj == null) return;

        if (!targetHeights.ContainsKey(obj)) return;

        float targetY = targetHeights[obj];
        float currentY = obj.transform.position.y;
        float heightDifference = targetY - currentY;

        // Move with constant speed using Transform (since object is kinematic)
        if (heightDifference > 0.05f)
        {
            float constantLiftSpeed = liftForce; // Constant speed in units per second
            Vector3 currentPos = obj.transform.position;
            Vector3 newPos = new Vector3(currentPos.x, currentPos.y + (constantLiftSpeed * Time.deltaTime), currentPos.z);
            obj.transform.position = newPos;
        }
        else
        {
            // Object reached target height, switch to hover mode
            isLifting[obj] = false;
        }
    }

    private void ApplyHover(ClickableObject obj)
    {
        if (obj == null) return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Apply hover force like your example - simple and direct
        if ((Mathf.Abs(rb.linearVelocity.y) < 0.1f) || rb.linearVelocity.y < 0)
        {

            float gravityForce = (rb.mass * gravityY) / hoverForce;
            //  rb.AddForce(Vector3.up * gravityForce, ForceMode.Force);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, gravityForce, rb.linearVelocity.z);
        }
    }

    private void ClearDicts()
    {
        originalHeights.Clear();
        targetHeights.Clear();
        isLifting.Clear();
    }
}
