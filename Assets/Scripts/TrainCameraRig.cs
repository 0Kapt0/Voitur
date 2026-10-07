using UnityEngine;

public class TrainCameraRig : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Transform[] _anchors; // les 4 positions
    [SerializeField] private Vector3 _lookOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Rythme")]
    [SerializeField] private float _holdTime = 6f;
    [SerializeField] private float _blendTime = 3f;

    [Header("Smoothness")]
    [SerializeField] private float _positionSmoothness = 3f;
    [SerializeField] private float _lookSmoothness = 4f;

    private int _currentIndex;
    private int _nextIndex;
    private float _holdTimer;
    private float _blendProgress;
    private bool _isBlending;

    private void LateUpdate()
    {
        UpdateCycle();
        MoveCamera();
        RotateCamera();
    }

    private void MoveCamera()
    {
        Vector3 currentAnchorPosition = _anchors[_currentIndex].position;
        Vector3 nextAnchorPosition = _isBlending ? _anchors[_nextIndex].position : currentAnchorPosition;
        float smoothedProgress = Mathf.SmoothStep(0f, 1f, _blendProgress);
        Vector3 desiredPosition = Vector3.Lerp(currentAnchorPosition, nextAnchorPosition, smoothedProgress);

        float positionFactor = 1f - Mathf.Exp(-_positionSmoothness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionFactor);
    }

    private void RotateCamera()
    {
        Vector3 lookDirection = _target.position + _lookOffset - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

        float lookFactor = 1f - Mathf.Exp(-_lookSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookFactor);
    }

    private void UpdateCycle()
    {
        if (_isBlending)
        {
            _blendProgress += Time.deltaTime / _blendTime;

            if (_blendProgress >= 1f)
            {
                _currentIndex = _nextIndex;
                _blendProgress = 0f;
                _holdTimer = 0f;
                _isBlending = false;
            }
        }
        else
        {
            _holdTimer += Time.deltaTime;

            if (_holdTimer >= _holdTime)
            {
                _nextIndex = (_currentIndex + 1) % _anchors.Length;
                _isBlending = true;
            }
        }
    }
}