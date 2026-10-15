using UnityEngine;
using UnityEngine.Splines;

public class TrainCameraRig : MonoBehaviour
{
    [SerializeField] private TrainController _train;
    [SerializeField] private SplineContainer _cameraSpline;
    [SerializeField] private Vector3 _lookOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Smoothness")]
    [SerializeField] private float _positionSmoothness = 3f;
    [SerializeField] private float _lookSmoothness = 4f;

    private void LateUpdate()
    {
        MoveCamera();
        RotateCamera();
    }

    private void MoveCamera()
    {
        Vector3 desiredPosition = _cameraSpline.EvaluatePosition(_train.Progress);

        float positionFactor = 1f - Mathf.Exp(-_positionSmoothness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionFactor);
    }

    private void RotateCamera()
    {
        Vector3 lookDirection = _train.transform.position + _lookOffset - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

        float lookFactor = 1f - Mathf.Exp(-_lookSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookFactor);
    }
}