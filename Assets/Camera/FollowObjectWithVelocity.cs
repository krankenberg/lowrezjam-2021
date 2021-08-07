using System;
using System.Linq;
using Car;
using UnityEngine;

namespace Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowObjectWithVelocity : MonoBehaviour
    {
        public float MaxLookAhead = 5F;
        public float MaxVelocity = 10F;
        public float CollisionDistance = 1F;
        public float DampTime = 0.5F;
        public float DampMaxSpeed = 0.1F;

        private float _cameraDistanceToFloor;
        private UnityEngine.Camera _camera;
        private CarMovement _carMovement;
        private Transform _carTransform;
        private Transform _cameraTransform;
        private RaycastHit[] _hits;
        private Vector3 _currentDampVelocity;

        private void Start()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _cameraTransform = _camera.transform;
            _carMovement = GameObject.FindWithTag("Player").GetComponent<CarMovement>();
            _carTransform = _carMovement.transform;
            _hits = new RaycastHit[10];
            _cameraDistanceToFloor = (32 / Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad / 2)) / GlobalGameState.PixelsPerUnit;
        }

        private void FixedUpdate()
        {
            var currentVelocity = _carMovement.GetCurrentVelocity();

            var desiredLookAhead = currentVelocity / MaxVelocity * MaxLookAhead;
            var lookAhead = ClampDesiredLookAheadToCollidingBuildings(currentVelocity, desiredLookAhead);

            var targetPosition = CalculateTargetPosition(lookAhead);
            SmoothStepCameraToTargetPosition(targetPosition);
        }

        private float ClampDesiredLookAheadToCollidingBuildings(float currentVelocity, float desiredLookAhead)
        {
            var drivingBackwards = currentVelocity > 0;
            var directionModifier = drivingBackwards ? 1 : -1;

            var collisionBox = new Vector3(CollisionDistance, CollisionDistance * 4, CollisionDistance);
            var collisionCount = Physics.BoxCastNonAlloc(_cameraTransform.position, collisionBox, _carTransform.forward * directionModifier, _hits,
                Quaternion.identity, Math.Abs(desiredLookAhead));
            if (collisionCount > 0)
            {
                var lowestDistance = _hits.Take(collisionCount).Min(hit => hit.distance);
                return lowestDistance * directionModifier;
            }

            return desiredLookAhead;
        }

        private Vector3 CalculateTargetPosition(float lookAhead)
        {
            var carsPosition = _carTransform.position;
            var carPositionOnCameraLayer = new Vector3(carsPosition.x, _cameraDistanceToFloor, carsPosition.z);
            var targetPosition = carPositionOnCameraLayer + _carTransform.forward * lookAhead;
            return targetPosition;
        }

        private void SmoothStepCameraToTargetPosition(Vector3 targetPosition)
        {
            _cameraTransform.localPosition = Vector3.SmoothDamp(_cameraTransform.localPosition, targetPosition, ref _currentDampVelocity, DampTime,
                DampMaxSpeed, Time.fixedDeltaTime);
        }
    }
}
