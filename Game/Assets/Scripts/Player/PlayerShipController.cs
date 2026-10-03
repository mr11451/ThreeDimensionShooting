using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 自機の移動・旋回を担当する。ゲームパッド前提の操作。
    /// 左スティック: 上下左右スラスト / 右スティック: 視点(機体の向き)
    /// RT/LT: 前後スラスト。アーケード寄りの機敏な挙動。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerShipController : MonoBehaviour
    {
        [Header("Thrust")]
        [SerializeField] private float _lateralThrust = 40f;
        [SerializeField] private float _forwardThrust = 60f;
        [SerializeField] private float _maxSpeed = 60f;
        [Tooltip("トリガーの遊び(0..0.9)")]
        [SerializeField] private float _triggerDeadZone = 0.05f;

        [Header("Look")]
        [SerializeField] private float _lookSensitivity = 2.5f;
        [SerializeField] private float _rollLerp = 6f;
        [SerializeField] private float _bankOnStrafeDeg = 25f;

        [Header("Inertia")]
        [Tooltip("入力がないときの減速度(小さいほど慣性が強い)")]
        [SerializeField] private float _coastDeceleration = 8f;

        private Vector3 _velocity;
        private Rigidbody _rb;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _throttleInput; // -1..1 (前後)
        private float _pitch;
        private float _yaw;
        private PlayerShield _playerShield;

        private void Awake()
        {
            ApplyExternalSettings();
            _rb = GetComponent<Rigidbody>();
            _playerShield = GetComponent<PlayerShield>();
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void ApplyExternalSettings()
        {
            var v = FlightSettingsFile.Load();
            if (v.TryGetValue("LateralThrust", out float f)) _lateralThrust = f;
            if (v.TryGetValue("ForwardThrust", out f)) _forwardThrust = f;
            if (v.TryGetValue("MaxSpeed", out f)) _maxSpeed = f;
            if (v.TryGetValue("TriggerDeadZone", out f)) _triggerDeadZone = Mathf.Clamp(f, 0f, 0.9f);
            if (v.TryGetValue("CoastDeceleration", out f)) _coastDeceleration = Mathf.Max(0f, f);
            if (v.TryGetValue("LookSensitivity", out f)) _lookSensitivity = f;
            if (v.TryGetValue("RollLerp", out f)) _rollLerp = f;
            if (v.TryGetValue("BankOnStrafeDeg", out f)) _bankOnStrafeDeg = f;
        }

        private void Update()
        {
            if (_playerShield != null && _playerShield.IsDowned)
            {
                _moveInput = Vector2.zero;
                _lookInput = Vector2.zero;
                _throttleInput = 0f;
                return;
            }

            ReadInput();
            ApplyLook();
        }

        private void FixedUpdate()
        {
            transform.position = Vector3.zero;

            if (_playerShield != null && _playerShield.IsDowned)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                return;
            }

            ApplyThrust();
            ClampSpeed();
        }

        private void OnCollisionEnter(Collision collision)
        {
            var backdrop = collision.collider.GetComponentInParent<NavigationBackdrop>();
            if (backdrop != null)
            {
                backdrop.HandleStarCollision(collision.collider);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<EnemyBase>() != null)
            {
                GetComponent<PlayerShield>()?.TakeDamage(100f);
            }
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

            // トリガー: RT=前進, LT=後退(押し込み量 0..1 のアナログ)。RB/LB は武装専用。
            float fwd = ApplyDeadZone(pad.rightTrigger.ReadValue());
            float back = ApplyDeadZone(pad.leftTrigger.ReadValue());
            _throttleInput = fwd - back;
        }

        private float ApplyDeadZone(float value)
        {
            if (value <= _triggerDeadZone) return 0f;
            return Mathf.Clamp01((value - _triggerDeadZone) / (1f - _triggerDeadZone));
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
            // 前後はトリガーの押し込み量(0..1)に比例し、ForwardThrust を最大加速度とする。
            Vector2 lateral = Vector2.ClampMagnitude(_moveInput, 1f);
            Vector3 localAccel = new Vector3(
                lateral.x * _lateralThrust,
                lateral.y * _lateralThrust,
                _throttleInput * _forwardThrust);
            bool hasInput = localAccel.sqrMagnitude > 0.0001f;

            if (hasInput)
            {
                _velocity += transform.TransformDirection(localAccel) * Time.fixedDeltaTime;
            }
            else
            {
                _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _coastDeceleration * Time.fixedDeltaTime);
            }

            _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);

            WorldRelativeMotion.ApplyDelta(-_velocity * Time.fixedDeltaTime);
            _rb.linearVelocity = Vector3.zero;
        }

        private void ClampSpeed()
        {
            // 自機自体は原点固定。世界の相対移動で速度感を表現するため、Rigidbody の速度を抑制しない。
        }
    }
}
