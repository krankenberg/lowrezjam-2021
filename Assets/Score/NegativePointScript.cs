using UnityEngine;
using UnityEngine.UI;

namespace Score
{
    public class NegativePointScript : MonoBehaviour
    {
        public Color VisibleColor;
        public Color InvisibleColor;
        public float FadeOutTime;
        public float FadeInTime;

        public Text Text;

        private Vector3 _worldPosition;
        private float _passedTime;
        private UnityEngine.Camera _mainCamera;
        private RectTransform _rectTransform;

        private void Start()
        {
            _mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<UnityEngine.Camera>();
            _rectTransform = Text.GetComponent<RectTransform>();
        }

        public void Show(Vector3 worldPosition, int score)
        {
            _passedTime = -FadeInTime;
            _worldPosition = worldPosition;
            Text.text = score.ToString();
            UpdatePosition();
        }

        private void Update()
        {
            UpdatePosition();
            if (_passedTime < 0)
            {
                Text.color = Color.Lerp(InvisibleColor, VisibleColor, (FadeInTime + _passedTime) / FadeInTime);
            }
            else if (_passedTime > 0 && _passedTime < FadeOutTime)
            {
                Text.color = Color.Lerp(VisibleColor, InvisibleColor, _passedTime / FadeOutTime);
            }
            else
            {
                Text.color = InvisibleColor;
            }

            _passedTime += Time.deltaTime;
        }

        private void UpdatePosition()
        {
            var screenPoint = _mainCamera.WorldToScreenPoint(_worldPosition);

            _rectTransform.anchoredPosition = new Vector2(screenPoint.x, screenPoint.y);
        }
    }
}
