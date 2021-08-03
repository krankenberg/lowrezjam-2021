using System;
using Car;
using UnityEngine;

namespace Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowObjectWithVelocity : MonoBehaviour
    {
        private const float CameraDistanceToFloor = 9.237604F; // x = (32 / tan(fovDeg / 2)) / PixelsPerUnit

        public float MaxLookAhead = 5F;
        public float MaxVelocity = 10F;
        public float CollisionDistance = 1F;

        private float _currentLookAhead;
        private UnityEngine.Camera _camera;
        private CarMovement _carMovement;
        private Transform _carTransform;
        private Transform _cameraTransform;
        private RaycastHit[] _hits;

        private void Start()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _cameraTransform = _camera.transform;
            _carMovement = GameObject.FindWithTag("Player").GetComponent<CarMovement>();
            _carTransform = _carMovement.transform;
            _hits = new RaycastHit[10];
        }

        private void FixedUpdate()
        {
            var currentVelocity = _carMovement.GetCurrentVelocity();
            var directionModifier = currentVelocity > 0 ? 1 : -1;
            var carsPosition = _carTransform.position;

            _currentLookAhead = Mathf.Lerp(_currentLookAhead, currentVelocity / MaxVelocity * MaxLookAhead, Time.fixedDeltaTime);

            var collisionBox = new Vector3(CollisionDistance, CollisionDistance * 4, CollisionDistance);
            var collisionCount = Physics.BoxCastNonAlloc(_cameraTransform.position, collisionBox, _carTransform.forward * directionModifier, _hits, Quaternion.identity, Math.Abs(_currentLookAhead));
            if (collisionCount > 0)
            {
                var lowestDistance = float.MaxValue;
                for (int i = 0; i < collisionCount; i++)
                {
                    var distance = _hits[i].distance;
                    if (distance < lowestDistance)
                    {
                        lowestDistance = distance;
                    }
                }

                _currentLookAhead = lowestDistance * directionModifier;
            }

            var carPositionOnCameraLayer = new Vector3(carsPosition.x, CameraDistanceToFloor, carsPosition.z);
            var targetPosition = carPositionOnCameraLayer + _carTransform.forward * _currentLookAhead;
            _cameraTransform.localPosition = targetPosition;
        }
    }
}
