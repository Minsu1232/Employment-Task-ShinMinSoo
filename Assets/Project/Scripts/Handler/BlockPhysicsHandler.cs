using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ��� ���� ó���� ����ϴ� �ڵ鷯
    /// </summary>
    [RequireComponent(typeof(BlockDragHandler))]
    public class BlockPhysicsHandler : MonoBehaviour
    {
        private BlockDragHandler dragHandler;
        private BlockGridHandler gridHandler;

        private Rigidbody rb;

        // ���� �Ӽ�
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float moveSpeed = 25f;
        [SerializeField] private float followSpeed = 30f;

        // �浹 ó�� ����
        private bool isColliding = false;
        private Vector3 lastCollisionNormal;
        private float collisionResetTime = 0.1f;
        private float lastCollisionTime;

        // ���� �̵� ����
        private Vector3 currentMoveVector;
        private Vector3 currentTargetPosition;

        private void Awake()
        {
            dragHandler = GetComponent<BlockDragHandler>();
            gridHandler = GetComponent<BlockGridHandler>();

            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // ���� ���� �ʱ�ȭ
            InitializePhysics();
        }

        /// <summary>
        /// ���� �Ӽ� �ʱ�ȭ
        /// </summary>
        private void InitializePhysics()
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        /// <summary>
        /// ���� ������Ʈ (���� ������ ����)
        /// </summary>
        private void FixedUpdate()
        {
            if (!dragHandler.Enabled || !dragHandler.IsDragging) return;

            // �׸��� ��ġ ������Ʈ (���� ����)
            gridHandler.SetBlockPosition(false);

            // �浹 �˻� �� �̵� ó��
            ProcessMovement();
        }

        /// <summary>
        /// �� ������ �浹 ���� ������Ʈ
        /// </summary>
        private void Update()
        {
            // �浹 ���� �ڵ� ���� �˻�
            if (isColliding && Time.time - lastCollisionTime > collisionResetTime)
            {
                ResetCollisionState();
            }
        }

        /// <summary>
        /// �̵� ���� ����
        /// </summary>
        public void SetMoveVector(Vector3 moveVector, Vector3 targetPosition)
        {
            currentMoveVector = moveVector;
            currentTargetPosition = targetPosition;
        }

        /// <summary>
        /// �̵� ó��
        /// </summary>
        private void ProcessMovement()
        {
            // �浹 ���¿��� ���콺�� ����� �־����� �浹 ���� ����
            float distanceToTarget = Vector3.Distance(transform.position, currentTargetPosition);
            if (isColliding && distanceToTarget > 0.5f)
            {
                if (Vector3.Dot(currentMoveVector.normalized, lastCollisionNormal) > 0.1f)
                {
                    ResetCollisionState();
                }
            }

            // �浹 ���ο� ���� �ӵ� ���
            Vector3 velocity = CalculateVelocity();

            // �ӵ� ����
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, velocity, Time.fixedDeltaTime * 10f);
            }
        }

        /// <summary>
        /// �ӵ� ���
        /// </summary>
        private Vector3 CalculateVelocity()
        {
            Vector3 velocity;

            if (isColliding)
            {
                // �浹�鿡 ������ �̵� ���� ��� (�浹���� ���� �̲������� �̵�)
                Vector3 projectedMove = Vector3.ProjectOnPlane(currentMoveVector, lastCollisionNormal);
                velocity = projectedMove * moveSpeed;
            }
            else
            {
                // �Ϲ� �̵� ���� ���
                velocity = currentMoveVector * followSpeed;
            }

            // �ִ� �ӵ� ����
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }

            return velocity;
        }

        /// <summary>
        /// �浹 ���� �ʱ�ȭ
        /// </summary>
        public void ResetCollisionState()
        {
            isColliding = false;
            lastCollisionNormal = Vector3.zero;
        }

        /// <summary>
        /// �浹 ���� (Enter)
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision);
        }

        /// <summary>
        /// �浹 ���� (Stay)
        /// </summary>
        private void OnCollisionStay(Collision collision)
        {
            HandleCollision(collision);
        }

        /// <summary>
        /// �浹 ó��
        /// </summary>
        private void HandleCollision(Collision collision)
        {
            if (!dragHandler.IsDragging) return;

            if (collision.contactCount > 0 && collision.gameObject.layer != LayerMask.NameToLayer("Board"))
            {
                Vector3 normal = collision.contacts[0].normal;

                // ���� �浹(�ٴڰ��� �浹)�� ����
                if (Vector3.Dot(normal, Vector3.up) < 0.8f)
                {
                    isColliding = true;
                    lastCollisionNormal = normal;
                    lastCollisionTime = Time.time;
                }
            }
        }

        /// <summary>
        /// �浹 ���� ó��
        /// </summary>
        private void OnCollisionExit(Collision collision)
        {
            // ���� �浹 ���� ������Ʈ�� ������ ���� �浹 ���� ����
            if (collision.contactCount > 0)
            {
                Vector3 normal = collision.contacts[0].normal;

                // ���� ����� �浹 normal�� ������ ��쿡�� ����
                if (Vector3.Dot(normal, lastCollisionNormal) > 0.8f)
                {
                    ResetCollisionState();
                }
            }
        }

        /// <summary>
        /// �浹 ���� Ȯ��
        /// </summary>
        public bool IsColliding()
        {
            return isColliding;
        }
    }
}