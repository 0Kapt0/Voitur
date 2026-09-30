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

    // Update is called once per frame
    void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        headbob();

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

        float speed = (transform.position - prevPos).magnitude;
        prevPos = transform.position;

        if (Input.GetKey(KeyCode.W))
        {
            moveX = transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveX = -transform.forward;
        }


        if (Input.GetKey(KeyCode.A))
        {
            moveY = -transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveY = transform.right;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }

        Vector3 mouseDelta = Input.mousePositionDelta * sensitivity;
        transform.rotation *= Quaternion.Euler(0, rotationSpeed * (mouseDelta.x / Screen.width) * Time.deltaTime, 0);

        if (!invertYAxis)
            mouseDelta.y = -mouseDelta.y;

        mainCam.transform.rotation *= Quaternion.Euler(rotationSpeed * (mouseDelta.y / Screen.height) * Time.deltaTime, 0, 0);
    }

    private void FixedUpdate()
    {
        //rb.MovePosition(rb.position + (moveX + moveY).normalized * moveSpeed * Time.deltaTime);
        //todo rb.MoveRotation();

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
                //rb.AddForce((moveX + moveY).normalized * moveSpeed * Time.deltaTime, ForceMode.Force);

                moveX = Vector3.zero;
                moveY = Vector3.zero;
            }
            /*else
            {
                rb.linearVelocity = (moveX + moveY).normalized * airMoveSpeed * Time.deltaTime;
                //rb.AddForce((moveX + moveY).normalized * airMoveSpeed * Time.deltaTime, ForceMode.Force);

                rb.AddForce(Vector3.down * additionalGravity, ForceMode.Force);
            }*/
        }
    }
}
