using System.Collections.Generic;
using UnityEngine;

namespace Score
{
    public class CrashDetector : MonoBehaviour
    {
        public float CollisionToleranceTime = 3F;

        public ScoreUI ScoreUI;

        private Rigidbody _rigidbody;

        private List<Transform> _recentlyCollidedTransforms;
        private List<float> _recentlyCollidedTransformsTimes;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _recentlyCollidedTransforms = new List<Transform>();
            _recentlyCollidedTransformsTimes = new List<float>();
        }

        private void Update()
        {
            for (var i = _recentlyCollidedTransformsTimes.Count - 1; i >= 0; i--)
            {
                _recentlyCollidedTransformsTimes[i] -= Time.deltaTime;
                if (_recentlyCollidedTransformsTimes[i] < 0)
                {
                    _recentlyCollidedTransformsTimes.RemoveAt(i);
                    _recentlyCollidedTransforms.RemoveAt(i);
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            var otherTransform = other.transform;
            if (!_recentlyCollidedTransforms.Contains(otherTransform))
            {
                var velocityDifference = other.relativeVelocity.magnitude;
                ScoreUI.Crash(velocityDifference, otherTransform.position);

                _recentlyCollidedTransforms.Add(otherTransform);
                _recentlyCollidedTransformsTimes.Add(CollisionToleranceTime);
            }
        }
    }
}
