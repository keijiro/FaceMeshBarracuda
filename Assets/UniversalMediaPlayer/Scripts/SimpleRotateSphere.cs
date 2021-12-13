using UnityEngine;

namespace UMP
{
    public class SimpleRotateSphere : MonoBehaviour
    {
        private const int RMB_ID = 0;

        private Transform _cachedTransform;
        private Vector2? _rmbPrevPos;
        private float _x;
        private float _y;

        [Range(1, 100)]
        [SerializeField]
        private float _rotationSpeed = 4;

        void Awake()
        {
            _cachedTransform = Camera.main.transform;
        }

        void Update()
        {
            TrackRotation();
        }

        private void TrackRotation()
        {
            if (_rmbPrevPos.HasValue)
            {
                if (Input.GetMouseButton(RMB_ID))
                {
                    if (((int)_rmbPrevPos.Value.x != (int)Input.mousePosition.x)
                        || ((int)_rmbPrevPos.Value.y != (int)Input.mousePosition.y))
                    {
                        _x += (_rmbPrevPos.Value.y - Input.mousePosition.y) * Time.deltaTime * _rotationSpeed;
                        _y -= (_rmbPrevPos.Value.x - Input.mousePosition.x) * Time.deltaTime * _rotationSpeed;
                        _cachedTransform.rotation = Quaternion.Euler(_x, _y, 0);
                        _rmbPrevPos = Input.mousePosition;
                    }
                }
                else
                {
                    _rmbPrevPos = null;
                }
            }
            else if (Input.GetMouseButtonDown(RMB_ID))
            {
                _rmbPrevPos = Input.mousePosition;
            }
        }
    }
}
