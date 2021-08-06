using System;
using UnityEngine;

namespace Car
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarMovement : MonoBehaviour
    {
        public float AccelerationSpeed = 1F;
        public float NaturalDecelerationSpeed = 1F;
        public float MaxVelocity = 10F;
        public float MaxBackwardsVelocity = 5F;
        public float RotationSpeed = 1F;
        public float RotationDiminishmentPoint = 0.3F;
        public float RotationDiminishmentPointModifier = 2F;
        public int LockingAngle = 45;
        public float RotationDeadZone = 5F;

        private Transform _transform;
        private float _currentVelocity;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            _currentVelocity = GetCurrentVelocity();

            var vertical = Input.GetAxisRaw("Vertical");
            var horizontal = Input.GetAxisRaw("Horizontal");

            if (Math.Abs(vertical) > GlobalGameState.Tolerance)
            {
                _currentVelocity += vertical * AccelerationSpeed * Time.deltaTime;
            }
            else
            {
                _currentVelocity = Mathf.MoveTowards(_currentVelocity, 0, NaturalDecelerationSpeed * Time.deltaTime);
            }

            _currentVelocity = Mathf.Clamp(_currentVelocity, -MaxBackwardsVelocity, MaxVelocity);
            _rigidbody.velocity = _transform.rotation * new Vector3(0, 0, _currentVelocity);

            var drivingBackwards = _currentVelocity < 0;
            var relevantMaxVelocity = drivingBackwards ? MaxBackwardsVelocity : MaxVelocity;

            float rotationModifier;
            var absVelocity = Math.Abs(_currentVelocity);
            if (absVelocity < RotationDiminishmentPoint)
            {
                rotationModifier = RotationDiminishmentPointModifier * absVelocity / RotationDiminishmentPoint;
            }
            else
            {
                rotationModifier = RotationDiminishmentPointModifier -
                                   (absVelocity - RotationDiminishmentPoint) /
                                   ((relevantMaxVelocity - RotationDiminishmentPoint) / (RotationDiminishmentPointModifier - 1));
            }

            rotationModifier *= drivingBackwards ? -1 : 1;

            var rotationValue = rotationModifier * horizontal * RotationSpeed * Time.deltaTime;
            if (Math.Abs(rotationValue) > RotationDeadZone)
            {
                _transform.Rotate(Vector3.up, rotationValue, Space.World);
            }
            else
            {
                var currentYAngle = _transform.rotation.eulerAngles.y;
                var newYAngle = Mathf.RoundToInt(currentYAngle / LockingAngle) * LockingAngle;
                var yAngleDiff = newYAngle - currentYAngle;

                _transform.Rotate(Vector3.up, yAngleDiff, Space.World);
            }
        }

        public float GetCurrentVelocity()
        {
            return _rigidbody.velocity.magnitude * (_currentVelocity > 0 ? 1 : -1);
        }
    }
}
