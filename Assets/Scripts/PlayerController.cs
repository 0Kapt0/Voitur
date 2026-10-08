using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10;
    [SerializeField] private float airMoveSpeed = 5;
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float additionalGravity = 10;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float bouncingPadMultiplier = 10;
    [SerializeField] private float sensitivity = 100;
    [SerializeField] private Transform originTsfm;
    [SerializeField] private LayerMask bouncingPadLayer;

    [Header("Headbob parameters")]
    [SerializeField] private AnimationCurve headbobCurve;
    [SerializeField] private float headbobAmp = 0.05f;
    [SerializeField] private float headbobFreq = 3f;
    [SerializeField] private float headbobTime = 0.5f;

    private bool invertYAxis = false;

    private Controls controls;
    private Camera mainCam;
    private Rigidbody rb;
    private CapsuleCollider coll;

    private float curSpeed;
    private bool jump;
    private bool jumping;
    private Vector3 moveX;
    private Vector3 moveY;
    private Vector3 prevPos;
    private bool grounded;
    private bool onBouncingPad;
    private float runTimer;
    private float headbobTimer;
    private float headHeight;
    private Quaternion upperLimit;
    private Quaternion bottomLimit;

    void Awake()
    {
        controls = new Controls();

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        mainCam = GetComponentInChildren<Camera>();

        curSpeed = moveSpeed;
        moveX = Vector3.zero;
        moveY = Vector3.zero;
        jump = false;
        jumping = false;
        onBouncingPad = false;
        runTimer = 0;
        headbobTimer = 0;
        headHeight = mainCam.transform.localPosition.y;
        upperLimit = Quaternion.Euler(-80, 0, 0);
        bottomLimit = Quaternion.Euler(80, 0, 0);
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    void headbob()
    {
        if (rb.linearVelocity.magnitude > 0.01f)
        {
            mainCam.transform.localPosition = new Vector3(0, headHeight + headbobCurve.Evaluate(headbobTimer / headbobTime) * headbobAmp, 0);
            headbobTimer += Time.deltaTime;
            if (headbobTimer > headbobTime)
            {
                headbobTimer = 0;
            }
        }
    }

    void Update()
    {
        LockCursor();
        headbob();
        CheckGround();
        TrackSpeed();
        GetInput();
        Look();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    private void CheckGround()
    {
        grounded = false;
        if (Physics.SphereCast(originTsfm.position, coll.radius, -transform.up, out RaycastHit hit, 2f, LayerMask.GetMask("Ground", "BouncingPad")))
        {
            if (hit.distance <= (coll.height * 0.5f) - coll.radius + 0.1f)
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("BouncingPad"))
                {
                    onBouncingPad = true;
                }
                else
                {
                    grounded = true;
                }
            }
            else
            {
                jumping = false;
            }
        }
    }

    private void TrackSpeed()
    {
        float speed = (transform.position - prevPos).magnitude;
        prevPos = transform.position;
    }

    private void GetInput()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        moveX = transform.forward * moveInput.y;
        moveY = transform.right * moveInput.x;

        if (controls.Player.Jump.WasPressedThisFrame())
        {
            jump = true;
        }
    }

    private void Look()
    {
        Vector2 mouseDelta = controls.Player.Look.ReadValue<Vector2>() * sensitivity;
        transform.rotation *= Quaternion.Euler(0, rotationSpeed * (mouseDelta.x / Screen.width) * Time.deltaTime, 0);

        if (!invertYAxis)
            mouseDelta.y = -mouseDelta.y;

        mainCam.transform.rotation *= Quaternion.Euler(rotationSpeed * (mouseDelta.y / Screen.height) * Time.deltaTime, 0, 0);
    }

    private void FixedUpdate()
    {
        if (jump)
        {
            if (!jumping)
            {
                if (grounded)
                {
                    rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                }
                else if (onBouncingPad)
                {
                    rb.AddForce(transform.up * jumpForce * bouncingPadMultiplier, ForceMode.Impulse);
                }

                jumping = true;
                jump = false;
            }
        }
        else if (!jumping)
        {
            if (grounded)
            {
                rb.linearVelocity = (moveX + moveY).normalized * moveSpeed * Time.deltaTime;

                moveX = Vector3.zero;
                moveY = Vector3.zero;
            }
        }
    }
}