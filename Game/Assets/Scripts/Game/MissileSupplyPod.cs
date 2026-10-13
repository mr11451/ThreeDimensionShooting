using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 接触した自機へロックオンミサイルを補給するポッド。
    /// </summary>
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class MissileSupplyPod : MonoBehaviour
    {
        [SerializeField] private int _supplyAmount = 8;

        private bool _collected;

        public static readonly System.Collections.Generic.List<MissileSupplyPod> Active =
            new System.Collections.Generic.List<MissileSupplyPod>();

        private void OnEnable() => Active.Add(this);

        private void OnDisable() => Active.Remove(this);

        /// <summary>自機側へ引き寄せる。</summary>
        public void Pull(Vector3 target, float speed)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }

        private void Awake()
        {
            var pickupCollider = GetComponent<SphereCollider>();
            pickupCollider.isTrigger = true;

            var rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, 45f * Time.deltaTime, Space.World);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_collected) return;

            var weapons = other.GetComponentInParent<PlayerWeapons>();
            if (weapons == null) return;
            if (!weapons.AddMissileStock(_supplyAmount)) return;

            _collected = true;
            Destroy(gameObject);
        }
    }
}
