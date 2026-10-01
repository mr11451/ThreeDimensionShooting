using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 自機の移動・旋回を担当する。ゲームパッド前提の操作。
    /// 左スティック: 上下左右スラスト / 右スティック: 視点(機体の向き)
    /// RB/LB: 前後スラスト。アーケード寄りの機敏な挙動。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerShipController : MonoBehaviour
    {
        [Header("Thrust")]
        [SerializeField] private float _lateralThrust = 40f;
        [SerializeField] private float _forwardThrust = 60f;
        [SerializeField] private float _maxSpeed = 60f;

        [Header("Look")]
        [SerializeField] private float _lookSensitivity = 2.5f;
        [SerializeField] private float _rollLerp = 6f;
        [SerializeField] private float _bankOnStrafeDeg = 25f;

        [Header("Damping (Arcade feel)")]
        [SerializeField] private float _linearDragWhenNoInput = 3f;
        [SerializeField] private float _linearDragNormal = 0.5f;

        private Rigidbody _rb;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _throttleInput; // -1..1 (前後)
        private float _pitch;
        private float _yaw;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void Update()
        {
            ReadInput();
            ApplyLook();
        }

        private void FixedUpdate()
        {
            ApplyThrust();
            ClampSpeed();
        }

        private void ReadInput()
        {
            var pad = Gamepad.current;
            if (pad == null)
            {
                _moveInput = Vector2.zero;
                _lookInput = Vector2.zero;
                _throttleInput = 0f;
                return;
            }

            _moveInput = pad.leftStick.ReadValue();
            _lookInput = pad.rightStick.ReadValue();

            // バンパー: RB=前進, LB=後退。RT はメインショット専用。
            float fwd = pad.rightShoulder.isPressed ? 1f : 0f;
            float back = pad.leftShoulder.isPressed ? 1f : 0f;
            _throttleInput = fwd - back;
        }

        private void ApplyLook()
        {
            _yaw += _lookInput.x * _lookSensitivity;
            _pitch -= _lookInput.y * _lookSensitivity;

            // 横移動時にバンク(見た目の傾き)
            float targetRoll = -_moveInput.x * _bankOnStrafeDeg;
            Quaternion targetRot = Quaternion.Euler(_pitch, _yaw, 0f) * Quaternion.Euler(0f, 0f, targetRoll);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, Time.deltaTime * _rollLerp));
        }

        private void ApplyThrust()
        {
            Vector3 localDir = new Vector3(_moveInput.x, _moveInput.y, _throttleInput);
            bool hasInput = localDir.sqrMagnitude > 0.001f;

            _rb.linearDamping = hasInput ? _linearDragNormal : _linearDragWhenNoInput;
            if (!hasInput) return;

            Vector3 world = transform.TransformDirection(localDir.normalized);
            float power = (Mathf.Abs(localDir.z) > 0.01f) ? _forwardThrust : _lateralThrust;
            _rb.AddForce(world * power, ForceMode.Acceleration);
        }

        private void ClampSpeed()
        {
            if (_rb.linearVelocity.magnitude > _maxSpeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * _maxSpeed;
            }
        }
    }
}
