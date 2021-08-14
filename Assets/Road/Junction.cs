using System.Collections.Generic;

namespace Road
{
    public class Junction
    {
        private readonly List<NavigationPoint> _allJunctionPoints;
        private readonly List<NavigationPoint> _blockedPointsBetweenPhases;
        private readonly List<TrafficLightPhaseResolved> _phases;

        private int _currentPhase;
        private float _timeInCurrentPhase;
        private bool _inAfterPhase;

        public Junction(List<NavigationPoint> allJunctionPoints, List<TrafficLightPhaseResolved> phases)
        {
            _allJunctionPoints = allJunctionPoints;
            _phases = phases;
            _blockedPointsBetweenPhases = new List<NavigationPoint>();

            foreach (var trafficLightPhaseResolved in _phases)
            {
                foreach (var blockedNavigationPoint in trafficLightPhaseResolved.BlockedNavigationPoints)
                {
                    if (!_blockedPointsBetweenPhases.Contains(blockedNavigationPoint))
                    {
                        _blockedPointsBetweenPhases.Add(blockedNavigationPoint);
                    }
                }
            }

            _timeInCurrentPhase = 500;
        }

        public void ReplaceNavigationPoint(NavigationPoint original, NavigationPoint replacement)
        {
            ReplaceInList(_allJunctionPoints, original, replacement);
            ReplaceInList(_blockedPointsBetweenPhases, original, replacement);
            foreach (var trafficLightPhaseResolved in _phases)
            {
                ReplaceInList(trafficLightPhaseResolved.BlockedNavigationPoints, original, replacement);
            }
        }

        private void ReplaceInList(IList<NavigationPoint> array, NavigationPoint original, NavigationPoint replacement)
        {
            for (var i = 0; i < array.Count; i++)
            {
                if (array[i] == original)
                {
                    array[i] = replacement;
                }
            }
        }

        public void Update(float deltaTime)
        {
            _timeInCurrentPhase += deltaTime;

            var currentPhase = _phases[_currentPhase];

            if (_timeInCurrentPhase > currentPhase.TimeInPhase + currentPhase.TimeUntilNextPhase)
            {
                _inAfterPhase = false;
                _timeInCurrentPhase = 0;
                _currentPhase = (_currentPhase + 1) % _phases.Count;

                foreach (var allJunctionPoint in _allJunctionPoints)
                {
                    allJunctionPoint.Occupied = false;
                }

                var nextPhase = _phases[_currentPhase];
                foreach (var blockedNavigationPoint in nextPhase.BlockedNavigationPoints)
                {
                    blockedNavigationPoint.Occupied = true;
                }
            }
            else if (!_inAfterPhase && _timeInCurrentPhase > currentPhase.TimeInPhase)
            {
                _inAfterPhase = true;

                foreach (var allJunctionPoint in _allJunctionPoints)
                {
                    allJunctionPoint.Occupied = false;
                }

                foreach (var blockedNavigationPoint in _blockedPointsBetweenPhases)
                {
                    blockedNavigationPoint.Occupied = true;
                }
            }
        }
    }
}
