using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 블록 물리 처리를 담당하는 핸들러
    /// </summary>
    [RequireComponent(typeof(BlockDragHandler))]
    public class BlockPhysicsHandler : MonoBehaviour
    {
        private BlockDragHandler dragHandler;
        private BlockGridHandler gridHandler;

        private Rigidbody rb;

        // 물리 속성
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float moveSpeed = 25f;
        [SerializeField] private float followSpeed = 30f;

        // 충돌 처리 변수
        private bool isColliding = false;
        private Vector3 lastCollisionNormal;
        private float collisionResetTime = 0.1f;
        private float lastCollisionTime;

        // 현재 이동 정보
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
            // 물리 설정 초기화
            InitializePhysics();
        }

        /// <summary>
        /// 물리 속성 초기화
        /// </summary>
        private void InitializePhysics()
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        /// <summary>
        /// 물리 업데이트 (고정 프레임 간격)
        /// </summary>
        private void FixedUpdate()
        {
            if (!dragHandler.Enabled || !dragHandler.IsDragging) return;

            // 그리드 위치 업데이트 (정렬 없이)
            gridHandler.SetBlockPosition(false);

            // 충돌 검사 및 이동 처리
            ProcessMovement();
        }

        /// <summary>
        /// 매 프레임 충돌 상태 업데이트
        /// </summary>
        private void Update()
        {
            // 충돌 상태 자동 해제 검사
            if (isColliding && Time.time - lastCollisionTime > collisionResetTime)
            {
                ResetCollisionState();
            }
        }

        /// <summary>
        /// 이동 벡터 설정
        /// </summary>
        public void SetMoveVector(Vector3 moveVector, Vector3 targetPosition)
        {
            currentMoveVector = moveVector;
            currentTargetPosition = targetPosition;
        }

        /// <summary>
        /// 이동 처리
        /// </summary>
        private void ProcessMovement()
        {
            // 충돌 상태에서 마우스가 충분히 멀어지면 충돌 상태 해제
            float distanceToTarget = Vector3.Distance(transform.position, currentTargetPosition);
            if (isColliding && distanceToTarget > 0.5f)
            {
                if (Vector3.Dot(currentMoveVector.normalized, lastCollisionNormal) > 0.1f)
                {
                    ResetCollisionState();
                }
            }

            // 충돌 여부에 따른 속도 계산
            Vector3 velocity = CalculateVelocity();

            // 속도 적용
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, velocity, Time.fixedDeltaTime * 10f);
            }
        }

        /// <summary>
        /// 속도 계산
        /// </summary>
        private Vector3 CalculateVelocity()
        {
            Vector3 velocity;

            if (isColliding)
            {
                // 충돌면에 투영된 이동 벡터 계산 (충돌면을 따라 미끄러지듯 이동)
                Vector3 projectedMove = Vector3.ProjectOnPlane(currentMoveVector, lastCollisionNormal);
                velocity = projectedMove * moveSpeed;
            }
            else
            {
                // 일반 이동 벡터 계산
                velocity = currentMoveVector * followSpeed;
            }

            // 최대 속도 제한
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }

            return velocity;
        }

        /// <summary>
        /// 충돌 상태 초기화
        /// </summary>
        public void ResetCollisionState()
        {
            isColliding = false;
            lastCollisionNormal = Vector3.zero;
        }

        /// <summary>
        /// 충돌 감지 (Enter)
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision);
        }

        /// <summary>
        /// 충돌 감지 (Stay)
        /// </summary>
        private void OnCollisionStay(Collision collision)
        {
            HandleCollision(collision);
        }

        /// <summary>
        /// 충돌 처리
        /// </summary>
        private void HandleCollision(Collision collision)
        {
            if (!dragHandler.IsDragging) return;

            if (collision.contactCount > 0 && collision.gameObject.layer != LayerMask.NameToLayer("Board"))
            {
                Vector3 normal = collision.contacts[0].normal;

                // 수직 충돌(바닥과의 충돌)은 무시
                if (Vector3.Dot(normal, Vector3.up) < 0.8f)
                {
                    isColliding = true;
                    lastCollisionNormal = normal;
                    lastCollisionTime = Time.time;
                }
            }
        }

        /// <summary>
        /// 충돌 종료 처리
        /// </summary>
        private void OnCollisionExit(Collision collision)
        {
            // 현재 충돌 중인 오브젝트가 떨어질 때만 충돌 상태 해제
            if (collision.contactCount > 0)
            {
                Vector3 normal = collision.contacts[0].normal;

                // 현재 저장된 충돌 normal과 유사한 경우에만 해제
                if (Vector3.Dot(normal, lastCollisionNormal) > 0.8f)
                {
                    ResetCollisionState();
                }
            }
        }

        /// <summary>
        /// 충돌 상태 확인
        /// </summary>
        public bool IsColliding()
        {
            return isColliding;
        }
    }
}