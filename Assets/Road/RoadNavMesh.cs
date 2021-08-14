using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Road
{
    [RequireComponent(typeof(Tilemap))]
    public class RoadNavMesh : MonoBehaviour
    {
        public List<TileNavigationPoints> TileNavigationPoints;

        public bool RecreateCopiedTileNavigationPoints;

        public bool RecreateRoadNavMesh;

        public bool NavigationMeshInitialized;

        public List<NavigationPoint> NavigationPoints;

        private List<Junction> _junctions;

        private void Start()
        {
            CreateRoadNavMesh();
            NavigationMeshInitialized = true;
        }

        private void Update()
        {
            foreach (var junction in _junctions)
            {
                junction.Update(Time.deltaTime);
            }
        }

        private void OnValidate()
        {
            if (RecreateCopiedTileNavigationPoints)
            {
                RecreateCopiedTileNavigationPoints = false;
                CreateCopiedTileNavigationPoints();
            }

            if (RecreateRoadNavMesh)
            {
                RecreateRoadNavMesh = false;
                CreateRoadNavMesh();
            }
        }

        public NavigationPoint GetNextOnMesh(Vector3 position)
        {
            NavigationPoint closestPoint = null;
            var closestPointDistance = float.MaxValue;
            foreach (var navigationPoint in NavigationPoints)
            {
                var distance = Vector3.Distance(position, navigationPoint.Position);
                if (distance < closestPointDistance)
                {
                    closestPointDistance = distance;
                    closestPoint = navigationPoint;
                }
            }

            return closestPoint;
        }

        private void CreateCopiedTileNavigationPoints()
        {
            foreach (var tileNavigationPoints in TileNavigationPoints)
            {
                if (tileNavigationPoints.Copy)
                {
                    tileNavigationPoints.NavigationPoints.Clear();
                    var copiedTileNavigationPoints = TileNavigationPoints[tileNavigationPoints.CopiedIndex];
                    tileNavigationPoints.TrafficLightPhases = new List<TrafficLightPhase>();
                    foreach (var trafficLightPhase in copiedTileNavigationPoints.TrafficLightPhases)
                    {
                        tileNavigationPoints.TrafficLightPhases.Add(new TrafficLightPhase()
                        {
                            BlockedNavigationPointsIndices = trafficLightPhase.BlockedNavigationPointsIndices,
                            TimeInPhase = trafficLightPhase.TimeInPhase,
                            TimeUntilNextPhase = trafficLightPhase.TimeUntilNextPhase
                        });
                    }

                    foreach (var navigationPoint in copiedTileNavigationPoints.NavigationPoints)
                    {
                        var newNavigationPoint = new NavigationPoint
                        {
                            Position = navigationPoint.Position,
                            Occupied = navigationPoint.Occupied,
                            ParkingLot = navigationPoint.ParkingLot,
                            NextPointsIndices = new List<int>(navigationPoint.NextPointsIndices),
                            Highway = navigationPoint.Highway,
                        };
                        newNavigationPoint.Position = RotatePointAroundPivot(newNavigationPoint.Position, tileNavigationPoints.CopiedRotatedBy);
                        tileNavigationPoints.NavigationPoints.Add(newNavigationPoint);
                    }
                }
            }
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, float angle)
        {
            var pivot = new Vector3(3, 0, 3);
            var dir = point - pivot;
            dir = Quaternion.Euler(0, angle, 0) * dir;
            point = dir + pivot;
            return point;
        }

        private void CreateRoadNavMesh()
        {
            var navigationPointsByTile = new Dictionary<TileBase, TileNavigationPoints>();
            foreach (var tileNavigationPoints in TileNavigationPoints)
            {
                navigationPointsByTile[tileNavigationPoints.Tile] = tileNavigationPoints;
            }

            NavigationPoints = new List<NavigationPoint>();
            _junctions = new List<Junction>();

            var tilemap = GetComponent<Tilemap>();
            var tilemapBounds = tilemap.cellBounds;
            var allTiles = tilemap.GetTilesBlock(tilemapBounds);
            var tilemapBoundsSize = tilemapBounds.size;
            var tilemapOffset = tilemap.CellToWorld(tilemapBounds.position);
            for (int x = 0; x < tilemapBoundsSize.x; x++)
            {
                for (int y = 0; y < tilemapBoundsSize.y; y++)
                {
                    TileBase tile = allTiles[x + y * tilemapBoundsSize.x];
                    if (tile != null && navigationPointsByTile.ContainsKey(tile))
                    {
                        var tileOrigin = tilemap.CellToWorld(new Vector3Int(x, y, 0)) + tilemapOffset;

                        var tileNavigationPoints = navigationPointsByTile[tile];
                        var navigationPoints = tileNavigationPoints.NavigationPoints;
                        var tilesNewNavigationPoints = new NavigationPoint[navigationPoints.Count];
                        for (var i = 0; i < navigationPoints.Count; i++)
                        {
                            var navigationPoint = navigationPoints[i];
                            var newNavigationPoint = new NavigationPoint
                            {
                                Position = tileOrigin + navigationPoint.Position,
                                NextPoints = new List<NavigationPoint>(),
                                PreviousPoints = new List<NavigationPoint>(),
                                Occupied = navigationPoint.Occupied,
                                ParkingLot = navigationPoint.ParkingLot,
                                InitialIndex = i,
                                Highway = navigationPoint.Highway
                            };
                            newNavigationPoint.Position = new Vector3(
                                Round(newNavigationPoint.Position.x),
                                0,
                                Round(newNavigationPoint.Position.z)
                            );
                            NavigationPoints.Add(newNavigationPoint);
                            tilesNewNavigationPoints[i] = newNavigationPoint;
                        }

                        // Connect tiles points
                        for (var i = 0; i < navigationPoints.Count; i++)
                        {
                            var navigationPoint = navigationPoints[i];
                            foreach (var navigationPointConnectedPointsIndex in navigationPoint.NextPointsIndices)
                            {
                                tilesNewNavigationPoints[i].NextPoints.Add(tilesNewNavigationPoints[navigationPointConnectedPointsIndex]);
                                tilesNewNavigationPoints[navigationPointConnectedPointsIndex].PreviousPoints.Add(tilesNewNavigationPoints[i]);
                            }
                        }

                        if (tileNavigationPoints.TrafficLightPhases.Count > 0)
                        {
                            // Create Junction
                            var allJunctionPoints = new List<NavigationPoint>(tilesNewNavigationPoints);
                            var phases = new List<TrafficLightPhaseResolved>();

                            foreach (var trafficLightPhase in tileNavigationPoints.TrafficLightPhases)
                            {
                                var blockedNavigationPoints = new List<NavigationPoint>();

                                foreach (var blockedNavigationPointsIndex in trafficLightPhase.BlockedNavigationPointsIndices)
                                {
                                    blockedNavigationPoints.Add(tilesNewNavigationPoints[blockedNavigationPointsIndex]);
                                }

                                phases.Add(new TrafficLightPhaseResolved
                                {
                                    BlockedNavigationPoints = blockedNavigationPoints,
                                    TimeInPhase = trafficLightPhase.TimeInPhase,
                                    TimeUntilNextPhase = trafficLightPhase.TimeUntilNextPhase
                                });
                            }

                            _junctions.Add(new Junction(allJunctionPoints, phases));
                        }
                    }
                }
            }

            // Merge Points
            var pointsToBeRemoved = new Dictionary<NavigationPoint, NavigationPoint>();
            for (var i = 0; i < NavigationPoints.Count; i++)
            {
                var point = NavigationPoints[i];
                if (!pointsToBeRemoved.ContainsKey(point))
                {
                    for (var otherIndex = i + 1; otherIndex < NavigationPoints.Count; otherIndex++)
                    {
                        var otherPoint = NavigationPoints[otherIndex];
                        if (!pointsToBeRemoved.ContainsKey(otherPoint))
                        {
                            if (Vector3.Distance(point.Position, otherPoint.Position) < 0.05F)
                            {
                                foreach (var previousPoint in otherPoint.PreviousPoints)
                                {
                                    previousPoint.NextPoints.Remove(otherPoint);
                                    previousPoint.NextPoints.Add(point);
                                }

                                foreach (var junction in _junctions)
                                {
                                    junction.ReplaceNavigationPoint(otherPoint, point);
                                }

                                point.Highway = point.Highway || otherPoint.Highway;
                                point.NextPoints.AddRange(otherPoint.NextPoints);
                                point.PreviousPoints.AddRange(otherPoint.PreviousPoints);
                                pointsToBeRemoved.Add(otherPoint, point);
                            }
                        }
                    }
                }
            }

            for (var i = NavigationPoints.Count - 1; i >= 0; i--)
            {
                if (pointsToBeRemoved.ContainsKey(NavigationPoints[i]))
                {
                    NavigationPoints.RemoveAt(i);
                }
            }
        }

        public float Round(float value)
        {
            return Mathf.Round(value * 10F) / 10F;
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (NavigationPoints != null)
            {
                foreach (var navigationPoint in NavigationPoints)
                {
                    Gizmos.color = navigationPoint.Occupied ? Color.red : Color.green;
                    Gizmos.DrawWireSphere(navigationPoint.Position, 0.25F);
                    Handles.Label(navigationPoint.Position, "" + navigationPoint.InitialIndex);

                    if (navigationPoint.NextPoints != null)
                    {
                        foreach (var navigationPointConnectedPoint in navigationPoint.NextPoints)
                        {
                            Gizmos.color = navigationPoint.Highway ? Color.cyan : Color.green;
                            Gizmos.DrawLine(navigationPoint.Position, navigationPointConnectedPoint.Position);
                            Gizmos.DrawLine(navigationPointConnectedPoint.Position,
                                navigationPointConnectedPoint.Position + Quaternion.Euler(0, 10F, 0) *
                                (navigationPoint.Position - navigationPointConnectedPoint.Position).normalized / 2F);
                            if (NavigationPoints.Contains(navigationPointConnectedPoint))
                            {
                                Gizmos.color = Color.magenta;
                                Gizmos.DrawWireSphere(navigationPointConnectedPoint.Position, 0.15F);
                            }
                        }
                    }
                }
            }
#endif
        }
    }
}
