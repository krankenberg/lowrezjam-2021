using UnityEngine;

namespace Score
{
    public class PointEvent
    {
        public Vector3? Position;
        public float Points;

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}, {nameof(Points)}: {Points}";
        }
    }
}
