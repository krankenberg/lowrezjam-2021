using System.Collections.Generic;
using System.Linq;
using Road;
using UnityEngine;

namespace Car
{
    public class DriveOnNavMesh : MonoBehaviour
    {
        public Vector3 CurrentTarget;
        public float Speed = 0.5F;
        public float PositionReachedDistance = 0.5F;
        public float MaxRotationSpeed = 1F;
        public int LockingAngle = 15;
        public float LookAheadRange = 2F;
        public float MaxAcceleration = 0.1F;
        public float TrafficLightStoppingRange = 0.8F;

        private RoadNavMesh _roadNavMesh;
        private Transform _transform;
        private NavigationPoint _currentTargetNavigationPoint;
        private Rigidbody _rigidbody;
        private RaycastHit[] _hits;
        private Queue<NavigationPoint> _nextTargets;
        private NavigationPoint _lastEnqueuedPoint;

        private void Start()
        {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
            _roadNavMesh = GameObject.FindWithTag("Tilemap").GetComponent<RoadNavMesh>();
            _hits = new RaycastHit[5];
            _nextTargets = new Queue<NavigationPoint>();
        }

        private void Update()
        {
            if (_roadNavMesh.NavigationMeshInitialized)
            {
                var transformPosition = _transform.position;
                if (_currentTargetNavigationPoint == null)
                {
                    _currentTargetNavigationPoint = _roadNavMesh.GetNextOnMesh(transformPosition);
                    _lastEnqueuedPoint = EnqueueRandomPoint(_currentTargetNavigationPoint);
                    _lastEnqueuedPoint = EnqueueRandomPoint(_lastEnqueuedPoint);
                    _lastEnqueuedPoint = EnqueueRandomPoint(_lastEnqueuedPoint);
                    _lastEnqueuedPoint = EnqueueRandomPoint(_lastEnqueuedPoint);
                    _lastEnqueuedPoint = EnqueueRandomPoint(_lastEnqueuedPoint);
                }
                else
                {
                    var distanceToTarget = Vector3.Distance(transformPosition, _currentTargetNavigationPoint.Position);
                    if (distanceToTarget < PositionReachedDistance)
                    {
                        _currentTargetNavigationPoint = _nextTargets.Dequeue();
                        _lastEnqueuedPoint = EnqueueRandomPoint(_lastEnqueuedPoint);
                        distanceToTarget = Vector3.Distance(transformPosition, _currentTargetNavigationPoint.Position);
                    }

                    var direction = (_currentTargetNavigationPoint.Position - transformPosition).normalized;
                    var angle = SnapRotationToLockingAngle(Vector3.SignedAngle(Vector3.forward, direction, Vector3.up));

                    var reachedRedTrafficLight = distanceToTarget <= TrafficLightStoppingRange && _nextTargets.Peek().Occupied;

                    var collisionBox = new Vector3(.5F, .5F, 1);
                    var collisionCount = Physics.BoxCastNonAlloc(
                        transformPosition + _transform.forward * 0.1F, collisionBox, direction, _hits, _transform.rotation, LookAheadRange
                    );
                    if (reachedRedTrafficLight || collisionCount > 0 && _hits.Take(collisionCount).Any(raycastHit => raycastHit.transform != _transform))
                    {
                        _rigidbody.velocity = Vector3.zero;
                        _rigidbody.angularVelocity = Vector3.zero;
                    }
                    else
                    {
                        _rigidbody.velocity = direction * Mathf.MoveTowards(_rigidbody.velocity.magnitude, Speed, MaxAcceleration * Time.deltaTime);
                        _rigidbody.angularVelocity = Vector3.zero;
                        _rigidbody.MoveRotation(Quaternion.RotateTowards(_transform.rotation, Quaternion.Euler(0, angle, 0), MaxRotationSpeed));
                    }
                }
            }
        }

        private NavigationPoint EnqueueRandomPoint(NavigationPoint currentPoint)
        {
            var position = Random.Range(0, currentPoint.NextPoints.Count);
            var enqueuedPoint = currentPoint.NextPoints[position];
            _nextTargets.Enqueue(enqueuedPoint);
            return enqueuedPoint;
        }

        private float SnapRotationToLockingAngle(float angle)
        {
            return Mathf.RoundToInt(angle / LockingAngle) * LockingAngle;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(CurrentTarget, .25F);
        }
    }
}
