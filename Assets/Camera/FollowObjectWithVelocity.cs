using System;
using Car;
using UnityEngine;

namespace Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowObjectWithVelocity : MonoBehaviour
    {
        public float MaxLookAhead = 5F;
        public float MaxVelocity = 3F;

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
//            var currentVelocity = CarMovement.GetCurrentVelocity();
//            var shipControlTransform = CarMovement.transform;
//            var shipsPosition = shipControlTransform.position;
//
//            _currentLookAhead = Mathf.Lerp(_currentLookAhead, currentVelocity / MaxVelocity * MaxLookAhead, Time.deltaTime);
//            var position = transform.localPosition;
//            transform.localPosition = new Vector3(shipsPosition.x, shipsPosition.y, position.z) + shipControlTransform.up * _currentLookAhead;
//            transform.rotation = shipControlTransform.rotation;
            _cameraTransform.position = _carTransform.position + new Vector3(0, 9.237604F, 0);
        }
    }
}
