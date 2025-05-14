using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 블록 입력 처리를 담당하는 핸들러
    /// </summary>
    [RequireComponent(typeof(BlockDragHandler))]
    public class BlockInputHandler : MonoBehaviour
    {
        private BlockDragHandler dragHandler;
        private BlockPhysicsHandler physicsHandler;
        private BlockGridHandler gridHandler;

        private Camera mainCamera;
        private Rigidbody rb;
        private Outline outline;

        private bool isDragging = false;
        private Vector3 offset;
        private float zDistanceToCamera;

        private void Start()
        {
            dragHandler = GetComponent<BlockDragHandler>();
            physicsHandler = GetComponent<BlockPhysicsHandler>();
            gridHandler = GetComponent<BlockGridHandler>();

            mainCamera = Camera.main;
            rb = GetComponent<Rigidbody>();

            // 아웃라인 컴포넌트 초기화
            InitializeOutline();
        }

        /// <summary>
        /// 아웃라인 컴포넌트 초기화
        /// </summary>
        private void InitializeOutline()
        {
            outline = gameObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineColor = Color.yellow;
                outline.OutlineWidth = 2f;
            }
            outline.enabled = false;
        }

        /// <summary>
        /// 마우스 클릭 시 블록 드래그 시작
        /// </summary>
        private void OnMouseDown()
        {
            if (!dragHandler.Enabled) return;

            // 드래그 상태 설정
            isDragging = true;
            dragHandler.IsDragging = true;
            rb.isKinematic = false;
            outline.enabled = true;

            // 카메라와의 Z축 거리 계산
            zDistanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);

            // 마우스와 오브젝트 간의 오프셋 저장
            offset = transform.position - GetMouseWorldPosition();

            // 충돌 상태 초기화
            physicsHandler.ResetCollisionState();
        }

        /// <summary>
        /// 마우스 버튼 해제 시 블록 드래그 종료
        /// </summary>
        private void OnMouseUp()
        {
            // 드래그 종료 상태 설정
            isDragging = false;
            dragHandler.IsDragging = false;
            outline.enabled = false;

            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // 최종 블록 위치 설정
            gridHandler.SetBlockPosition(true);
            physicsHandler.ResetCollisionState();
        }

        /// <summary>
        /// 매 프레임 입력 상태 업데이트
        /// </summary>
        private void Update()
        {
            // 드래그 중 매 프레임 마우스 위치 업데이트
            if (isDragging)
            {
                UpdateDragPosition();
            }
        }

        /// <summary>
        /// 드래그 위치 업데이트
        /// </summary>
        private void UpdateDragPosition()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 targetPosition = mouseWorldPos + offset;

            // 이동 벡터 계산 및 전달
            Vector3 moveVector = targetPosition - transform.position;
            physicsHandler.SetMoveVector(moveVector, targetPosition);
        }

        /// <summary>
        /// 마우스 화면 좌표를 월드 좌표로 변환
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = zDistanceToCamera;
            return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        }

        /// <summary>
        /// 입력 강제 해제
        /// </summary>
        public void ReleaseInput()
        {
            if (dragHandler.col != null)
                dragHandler.col.enabled = false;

            isDragging = false;
            dragHandler.IsDragging = false;
            outline.enabled = false;

            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        /// <summary>
        /// 아웃라인 표시 여부 설정
        /// </summary>
        public void SetOutlineVisible(bool visible)
        {
            if (outline != null)
            {
                outline.enabled = visible;
            }
        }
    }
}