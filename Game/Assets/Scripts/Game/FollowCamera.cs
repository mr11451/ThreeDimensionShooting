using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// カメラを自機(Player)に追従させる。一人称視点。
    /// カメラは自機の位置・回転に追従し、機首方向を映す。
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Offset (local)")]
        [SerializeField] private Vector3 _localOffset = new Vector3(0f, 0.5f, -0.5f);
        [SerializeField] private bool _matchRotation = true;

        [Header("Smoothing")]
        [SerializeField] private float _positionLerp = 20f;
        [SerializeField] private float _rotationLerp = 20f;

        private void Start()
        {
            if (_target == null)
            {
                var pc = FindFirstObjectByType<PlayerShipController>();
                if (pc != null) _target = pc.transform;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPos = _target.TransformPoint(_localOffset);

            if (_positionLerp > 0f)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * _positionLerp);
            }
            else
            {
                transform.position = desiredPos;
            }

            if (_matchRotation)
            {
                if (_rotationLerp > 0f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, Time.deltaTime * _rotationLerp);
                }
                else
                {
                    transform.rotation = _target.rotation;
                }
            }
        }
    }
}
