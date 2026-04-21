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
            if (_spriteRenderer == null)
            {
                Debug.LogError("BlockView.SetSprite: _spriteRenderer NULL! GameObject: " + gameObject.name);
                return;
            }
            
            _spriteRenderer.sprite = sprite;
        }

        public void SetPosition(Vector3 position)
        {
            transform.localPosition = position;
            transform.localRotation = Quaternion.identity;
        }

        public void SetColor(Color color)
        {
            if (_spriteRenderer == null)
            {
                Debug.LogError("BlockView.SetColor: _spriteRenderer NULL!");
                return;
            }

            _spriteRenderer.color = color;
        }

        public void SetSize(float size)
        {
            if (_spriteRenderer == null)
            {
                Debug.LogError("BlockView.SetSize: _spriteRenderer NULL!");
                transform.localScale = Vector3.one * size;
                return;
            }

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
