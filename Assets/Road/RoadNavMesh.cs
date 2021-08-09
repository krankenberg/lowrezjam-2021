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

        public List<NavigationPoint> NavigationPoints;

        private void Start()
        {
            CreateRoadNavMesh();
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

        private void CreateCopiedTileNavigationPoints()
        {
            foreach (var tileNavigationPoints in TileNavigationPoints)
            {
                if (tileNavigationPoints.Copy)
                {
                    tileNavigationPoints.NavigationPoints.Clear();
                    var copiedTileNavigationPoints = TileNavigationPoints[tileNavigationPoints.CopiedIndex];
                    foreach (var navigationPoint in copiedTileNavigationPoints.NavigationPoints)
                    {
                        var newNavigationPoint = new NavigationPoint
                        {
                            Position = navigationPoint.Position,
                            Occupied = navigationPoint.Occupied,
                            ParkingLot = navigationPoint.ParkingLot,
                            NextPointsIndices = new List<int>(navigationPoint.NextPointsIndices),
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
            var navigationPointsByTile = new Dictionary<TileBase, List<NavigationPoint>>();
            foreach (var tileNavigationPoints in TileNavigationPoints)
            {
                navigationPointsByTile[tileNavigationPoints.Tile] = tileNavigationPoints.NavigationPoints;
            }

            NavigationPoints = new List<NavigationPoint>();

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

                        var navigationPoints = navigationPointsByTile[tile];
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
                                InitialIndex = i
                            };
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
                    }
                }
            }

            // Merge Points
            var pointsToBeRemoved = new List<int>();
            for (var i = 0; i < NavigationPoints.Count; i++)
            {
                if (!pointsToBeRemoved.Contains(i))
                {
                    var point = NavigationPoints[i];
                    for (var otherIndex = i + 1; otherIndex < NavigationPoints.Count; otherIndex++)
                    {
                        if (!pointsToBeRemoved.Contains(otherIndex))
                        {
                            var otherPoint = NavigationPoints[otherIndex];
                            if (Vector3.Distance(point.Position, otherPoint.Position) < 0.05F)
                            {
                                point.NextPoints.AddRange(otherPoint.NextPoints);
                                point.PreviousPoints.AddRange(otherPoint.PreviousPoints);
                                pointsToBeRemoved.Add(otherIndex);
                            }
                        }
                    }
                }
            }

            for (var i = NavigationPoints.Count - 1; i >= 0; i--)
            {
                if (pointsToBeRemoved.Contains(i))
                {
                    NavigationPoints.RemoveAt(i);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (NavigationPoints != null)
            {
                foreach (var navigationPoint in NavigationPoints)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(navigationPoint.Position, 0.25F);
                    Handles.Label(navigationPoint.Position, "" + navigationPoint.InitialIndex);

                    if (navigationPoint.NextPoints != null)
                    {
                        foreach (var navigationPointConnectedPoint in navigationPoint.NextPoints)
                        {
                            Gizmos.DrawLine(navigationPoint.Position, navigationPointConnectedPoint.Position);
                            Gizmos.DrawLine(navigationPointConnectedPoint.Position,
                                navigationPointConnectedPoint.Position + Quaternion.Euler(0, 10F, 0) *
                                (navigationPoint.Position - navigationPointConnectedPoint.Position).normalized / 2F);
                        }
                    }
                }
            }
        }
    }
}
