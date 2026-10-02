using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Rigidbody _rigidBody;

    public float moveSmoothness;
    public float rotSmoothness;

    public Vector3 moveOffset;
    public Vector3 rotOffset;

    public Transform carTarget;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void HandleMovement()
    {
        //Vector3 targetPos = carTarget.TransformPoint(moveOffset);

        //transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, Mathf.Clamp(60 + _rigidBody.linearVelocity.magnitude * 2, 60f, 115f), moveSmoothness * Time.deltaTime);

    }

    void HandleRotation()
    {
        var direction = carTarget.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSmoothness * Time.deltaTime);
    }

    void Start()
    {
        
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }
}
