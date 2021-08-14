using System;
using System.Collections.Generic;
using Road;
using UnityEngine;
using UnityEngine.UI;

namespace Score
{
    public class ScoreUI : MonoBehaviour
    {
        public Text Text;
        public Text HighScoreText;

        public int TopTextsTop = 1;
        public int BottomTextsTop = 56;

        public int Score;

        public int HighScore;

        public Rigidbody CarRigidbody;

        public float MaxSpeed = 10;
        public float MinSpeedToGainPoints = 2;
        public float ScoreModifierForMaxSpeed = 1;
        public float PointsLostPerSecondStanding = 0.5F;
        public float BasePointsLostForCrash = 100;
        public float MaxDistanceToStreetForGettingPoints = 1F;

        private RectTransform _rectTransform;

        private float _score;

        private Queue<PointEvent> _pointEvents;

        private RoadNavMesh _roadNavMesh;

        private List<NavigationPoint> _otherPoints;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _pointEvents = new Queue<PointEvent>();
            _roadNavMesh = GameObject.FindWithTag("Tilemap").GetComponent<RoadNavMesh>();
            _otherPoints = new List<NavigationPoint>();
        }

        private void LateUpdate()
        {
            var carRotation = CarRigidbody.rotation;
            var carRotationY = carRotation.eulerAngles.y;
            if (carRotationY > 115 && carRotationY < 245)
            {
                _rectTransform.offsetMax = new Vector2(0, -TopTextsTop);
            }
            else
            {
                _rectTransform.offsetMax = new Vector2(0, -BottomTextsTop);
            }

            CalculateScore();

            Score = Mathf.RoundToInt(_score);
            HighScore = Math.Max(Score, HighScore);

            Text.text = Score.ToString("D6");
            HighScoreText.text = HighScore.ToString("D6");
        }

        private void CalculateScore()
        {
            var velocity = CarRigidbody.velocity;
            var speed = velocity.magnitude;

            var distanceToRoad = CalculateDistanceToRoad();

            if (speed < MinSpeedToGainPoints || distanceToRoad > MaxDistanceToStreetForGettingPoints)
            {
                _score -= Time.deltaTime * PointsLostPerSecondStanding;
            }
            else
            {
                var scoreModifier = 1 + (speed - MinSpeedToGainPoints) / (MaxSpeed - MinSpeedToGainPoints) * ScoreModifierForMaxSpeed;
                _score += Time.deltaTime * scoreModifier;
            }

            while (_pointEvents.Count > 0)
            {
                var pointEvent = _pointEvents.Dequeue();
                Debug.Log("Processing pointEvent: " + pointEvent);
                _score += pointEvent.Points;
            }

            _score = Mathf.Clamp(_score, 0, 999999);
        }

        private float CalculateDistanceToRoad()
        {
            var carPosition = CarRigidbody.position;
            var nextNavigationPoint = _roadNavMesh.GetNextOnMesh(carPosition);

            _otherPoints.Clear();

            _otherPoints.AddRange(nextNavigationPoint.NextPoints);
            _otherPoints.AddRange(nextNavigationPoint.PreviousPoints);

            var carVector2 = new Vector2(carPosition.x, carPosition.z);
            var nextNavigationPointVector2 = new Vector2(nextNavigationPoint.Position.x, nextNavigationPoint.Position.z);

            var lowestDistance = float.MaxValue;
            foreach (var navigationPoint in _otherPoints)
            {
                var nearestPointOnLine = FindNearestPointOnLine(
                    nextNavigationPointVector2,
                    new Vector2(navigationPoint.Position.x, navigationPoint.Position.z),
                    carVector2
                );
                var distance = Vector2.Distance(carVector2, nearestPointOnLine);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                }
            }

            return lowestDistance;
        }

        private Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 end, Vector2 point)
        {
            Vector2 heading = end - origin;
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            Vector2 lhs = point - origin;
            float dotP = Vector2.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return origin + heading * dotP;
        }

        public void Crash(float velocityDifference, Vector3 position)
        {
            _pointEvents.Enqueue(new PointEvent
            {
                Position = position,
                Points = -BasePointsLostForCrash * (velocityDifference / MaxSpeed)
            });
        }
    }
}
