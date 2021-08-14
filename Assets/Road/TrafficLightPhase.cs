using System;

namespace Road
{
    [Serializable]
    public class TrafficLightPhase
    {
        public float TimeInPhase;
        public float TimeUntilNextPhase;
        public int[] BlockedNavigationPointsIndices;
    }
}
