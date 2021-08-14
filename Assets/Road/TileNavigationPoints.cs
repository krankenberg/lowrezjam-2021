using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Road
{
    [System.Serializable]
    public class TileNavigationPoints
    {
        public TileBase Tile;
        public bool Copy;
        public int CopiedIndex;
        public int CopiedRotatedBy;
        public List<NavigationPoint> NavigationPoints;
        public List<TrafficLightPhase> TrafficLightPhases;
    }
}
