using System;
using Car;
using UnityEngine;

namespace Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowObjectWithVelocity : MonoBehaviour
    {
        public float MaxLookAhead = 5F;
        public float MaxVelocity = 10F;

        private float _currentLookAhead;
        private UnityEngine.Camera _camera;
        private CarMovement _carMovement;
        private Transform _carTransform;
        private Transform _cameraTransform;

        private void Start()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _cameraTransform = _camera.transform;
            _carMovement = GameObject.FindWithTag("Player").GetComponent<CarMovement>();
            _carTransform = _carMovement.transform;
        }

        private void LateUpdate()
        {
            var currentVelocity = _carMovement.GetCurrentVelocity();
            var carsPosition = _carTransform.position;

            _currentLookAhead = Mathf.Lerp(_currentLookAhead, currentVelocity / MaxVelocity * MaxLookAhead, Time.deltaTime);
            _cameraTransform.localPosition = new Vector3(carsPosition.x, 9.237604F, carsPosition.z) + _carTransform.forward * _currentLookAhead;
        }
    }
}
