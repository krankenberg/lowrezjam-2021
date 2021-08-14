using System;
using System.Collections.Generic;
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

        private RectTransform _rectTransform;

        private float _score;

        private Queue<PointEvent> _pointEvents;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _pointEvents = new Queue<PointEvent>();
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

            if (speed > MinSpeedToGainPoints)
            {
                var scoreModifier = 1 + (speed - MinSpeedToGainPoints) / (MaxSpeed - MinSpeedToGainPoints) * ScoreModifierForMaxSpeed;
                _score += Time.deltaTime * scoreModifier;
            }
            else
            {
                _score -= Time.deltaTime * PointsLostPerSecondStanding;
            }

            while (_pointEvents.Count > 0)
            {
                var pointEvent = _pointEvents.Dequeue();
                Debug.Log("Processing pointEvent: " + pointEvent);
                _score += pointEvent.Points;
            }

            _score = Mathf.Clamp(_score, 0, 999999);
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
