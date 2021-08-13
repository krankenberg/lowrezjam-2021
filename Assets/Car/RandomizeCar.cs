using System.Collections.Generic;
using UnityEngine;

namespace Car
{
    public class RandomizeCar : MonoBehaviour
    {
        public SpriteRenderer BaseSpriteRenderer;
        public SpriteRenderer TintedSpriteRenderer;

        public List<Color> Colors;
        public List<SpritePair> SpritePairs;

        private void Start()
        {
            var spritePair = SpritePairs[Random.Range(0, SpritePairs.Count)];
            var color = Colors[Random.Range(0, Colors.Count)];

            BaseSpriteRenderer.sprite = spritePair.BaseSprite;
            BaseSpriteRenderer.color = Color.white;

            TintedSpriteRenderer.sprite = spritePair.TintedSprite;
            TintedSpriteRenderer.color = color;
        }
    }
}
