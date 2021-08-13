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

        private RoadNavMesh _roadNavMesh;
        private Transform _transform;
        private NavigationPoint _currentTargetNavigationPoint;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
            _roadNavMesh = GameObject.FindWithTag("Tilemap").GetComponent<RoadNavMesh>();
        }

        private void Update()
        {
            if (_roadNavMesh.NavigationMeshInitialized)
            {
                if (_currentTargetNavigationPoint == null)
                {
                    _currentTargetNavigationPoint = _roadNavMesh.GetNextOnMesh(_transform.position);
                }
                else
                {
                    if (Vector3.Distance(_transform.position, _currentTargetNavigationPoint.Position) < PositionReachedDistance)
                    {
                        var position = Random.Range(0, _currentTargetNavigationPoint.NextPoints.Count);
                        _currentTargetNavigationPoint = _currentTargetNavigationPoint.NextPoints[position];
                    }

                    var direction = (_currentTargetNavigationPoint.Position - _transform.position).normalized;
                    _rigidbody.velocity = direction * Speed;
                    var angle = SnapRotationToLockingAngle(Vector3.SignedAngle(Vector3.forward, direction, Vector3.up));
                    _rigidbody.angularVelocity = Vector3.zero;
                    _rigidbody.MoveRotation(Quaternion.RotateTowards(_transform.rotation, Quaternion.Euler(0, angle, 0), MaxRotationSpeed));
                }
            }
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
