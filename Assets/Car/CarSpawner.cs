using Road;
using UnityEngine;

namespace Car
{
    public class CarSpawner : MonoBehaviour
    {
        public RoadNavMesh RoadNavMesh;

        public int CarCount;

        public GameObject CarPrefab;

        private int _spawnedCars = 0;

        private void Update()
        {
            while (_spawnedCars <= CarCount)
            {
                var navigationPoint = RoadNavMesh.NavigationPoints[Random.Range(0, RoadNavMesh.NavigationPoints.Count)];
                var nextPoint = navigationPoint.NextPoints[0];
                var rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, nextPoint.Position - navigationPoint.Position, Vector3.up),
                    Vector3.up);

                Instantiate(CarPrefab, navigationPoint.Position, rotation);

                _spawnedCars++;
            }
        }
    }
}
