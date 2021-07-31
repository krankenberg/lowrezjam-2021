using UnityEngine;

public class TestScript : MonoBehaviour
{
    public float RotationSpeed = 1F;
    
    private Transform _transform;

    private void Start()
    {
        _transform = transform;
    }

    private void Update()
    {
        _transform.Rotate(Vector3.up, Time.deltaTime * RotationSpeed, Space.World);
    }
}
