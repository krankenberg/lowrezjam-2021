using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Road
{
    [Serializable]
    public class NavigationPoint
    {
        public Vector3 Position;

        [NonSerialized] public List<NavigationPoint> NextPoints;

        [NonSerialized] public List<NavigationPoint> PreviousPoints;

        [NonSerialized] public int InitialIndex;

        public bool Occupied;

        public bool ParkingLot;

        public bool Highway;

        [FormerlySerializedAs("ConnectedPointsIndices")]
        public List<int> NextPointsIndices;
    }
}
