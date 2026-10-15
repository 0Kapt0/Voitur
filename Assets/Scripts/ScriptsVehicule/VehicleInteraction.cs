using UnityEngine;

public class VehicleInteraction : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private float _interactRadius = 5f;

    private Controls _controls;
    private Vehicle _currentVehicle;

    private void Awake()
    {
        _controls = new Controls();
    }

    private void OnEnable()
    {
        _controls.Player.Enable();
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
    }

    private void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        if (!_controls.Player.Interact.WasPressedThisFrame()) return;

        Debug.Log("E");

        if (_currentVehicle == null)
        {
            TryEnterVehicle();
        }
        else
        {
            ExitVehicle();
        }
    }

    private void TryEnterVehicle()
    {
        Vehicle nearestVehicle = FindNearestVehicle();

        if (nearestVehicle == null) return;

        _currentVehicle = nearestVehicle;
        _player.SetActive(false);
        _currentVehicle.SetDriving(true);
    }

    private void ExitVehicle()
    {
        Transform exitPoint = _currentVehicle.ExitPoint;

        _currentVehicle.SetDriving(false);
        _player.transform.SetPositionAndRotation(exitPoint.position, Quaternion.Euler(0f, exitPoint.eulerAngles.y, 0f));
        _player.SetActive(true);
        _currentVehicle = null;
    }

    private Vehicle FindNearestVehicle()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(_player.transform.position, _interactRadius);
        Vehicle nearestVehicle = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            Vehicle vehicle = nearbyCollider.GetComponentInParent<Vehicle>();

            if (vehicle == null) continue;

            float distance = Vector3.Distance(_player.transform.position, vehicle.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestVehicle = vehicle;
            }
        }

        return nearestVehicle;
    }
}