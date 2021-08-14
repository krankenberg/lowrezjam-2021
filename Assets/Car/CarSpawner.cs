using System.Collections.Generic;
using System.Linq;
using Road;
using UnityEngine;

namespace Car
{
    public class CarSpawner : MonoBehaviour
    {
        public RoadNavMesh RoadNavMesh;

        public int CarCount;

        public GameObject CarPrefab;

        public float MinDistance = 5F;

        public int _spawnedCars = 0;
        private List<Vector3> _spawnedPositions;

        private void Start()
        {
            _spawnedCars = 0;
            _spawnedPositions = new List<Vector3>();
        }

        private void Update()
        {
            if (_spawnedCars <= CarCount)
            {
                for (int i = 0; i < 5; i++)
                {
                    var navigationPoint = RoadNavMesh.NavigationPoints[Random.Range(0, RoadNavMesh.NavigationPoints.Count)];
                    if (_spawnedPositions.Count == 0
                        || _spawnedPositions.Min(position => Vector3.Distance(position, navigationPoint.Position)) > MinDistance)
                    {
                        var nextPoint = navigationPoint.NextPoints[0];
                        var rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, nextPoint.Position - navigationPoint.Position, Vector3.up),
                            Vector3.up);

                        Instantiate(CarPrefab, navigationPoint.Position, rotation);

                        _spawnedPositions.Add(navigationPoint.Position);
                        _spawnedCars++;
                        return;
                    }
                }
            }
        }
    }
}
