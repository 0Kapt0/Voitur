using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _controller;
    [SerializeField] private GameObject[] _drivingObjects; // canvas, caméra...
    [SerializeField] private Transform _exitPoint;

    public Transform ExitPoint => _exitPoint;

    private void Awake()
    {
        SetDriving(false);
    }

    public void SetDriving(bool isDriving)
    {
        _controller.enabled = isDriving;

        foreach (GameObject drivingObject in _drivingObjects)
        {
            drivingObject.SetActive(isDriving);
        }
    }
}