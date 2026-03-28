using UnityEngine;

namespace Tomino.View
{
    public class BlockView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        internal void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        public void SetPosition(Vector3 position)
        {
            transform.localPosition = position;
            transform.localRotation = Quaternion.identity;
        }

        public void SetColor(Color color)
        {
            // Sprite orijinal rengini koru - renk değişimi yok
            _spriteRenderer.color = color;
        }

        public void SetSize(float size)
        {
            var sprite = _spriteRenderer.sprite;
            if (sprite == null || sprite.rect.width <= 0) 
            {
                transform.localScale = Vector3.one * size;
                return;
            }
            var scale = sprite.pixelsPerUnit / sprite.rect.width * size;
            transform.localScale = new Vector3(scale, scale);
        }
    }
}
