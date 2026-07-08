using UnityEngine;

namespace FoodTruckKiller.Assassination
{
    /// <summary>
    /// 简易 FX 衰减器：缩放从 1 缓动到 endScale，alpha 1→0，duration 秒后销毁。
    /// 沙箱无 Animator 时的轻量替代。
    /// </summary>
    public class SimpleFXFader : MonoBehaviour
    {
        public float duration = 0.5f;
        public float endScale = 2.0f;

        private float _t;
        private SpriteRenderer _sr;
        private Color _startColor;
        private Vector3 _startScale;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _startColor = _sr.color;
            _startScale = transform.localScale;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            float p = Mathf.Clamp01(_t / duration);
            transform.localScale = Vector3.Lerp(_startScale, _startScale * endScale, p);
            if (_sr != null)
            {
                var c = _startColor;
                c.a = Mathf.Lerp(1f, 0f, p);
                _sr.color = c;
            }
            if (_t >= duration) Destroy(gameObject);
        }
    }
}
