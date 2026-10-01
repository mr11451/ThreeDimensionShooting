using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 敵の弾。直進し、プレイヤーのシールドに当たるとダメージを与える。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyBullet : MonoBehaviour
    {
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _speed = 50f;
        [SerializeField] private float _lifeSeconds = 5f;
        [SerializeField] private float _rotationSpeed = 360f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            CreateTriangularPyramid();
        }

        private void Start()
        {
            _rb.linearVelocity = transform.forward * _speed;
            Destroy(gameObject, _lifeSeconds);
        }

        private void Update()
        {
            transform.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime, Space.Self);
        }

        private void CreateTriangularPyramid()
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) return;

            var mesh = new Mesh { name = "EnemyBulletTriangularPyramid" };
            mesh.vertices = new[]
            {
                new Vector3(0f, 0.5f, 0.45f),
                new Vector3(-0.45f, -0.25f, -0.35f),
                new Vector3(0.45f, -0.25f, -0.35f),
                new Vector3(0f, 0.5f, -0.35f),
            };
            mesh.triangles = new[]
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 1,
                1, 3, 2,
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerShield>(out var shield))
            {
                shield.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}
