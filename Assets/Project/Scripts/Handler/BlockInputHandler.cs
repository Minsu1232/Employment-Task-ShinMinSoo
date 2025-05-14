using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ��� �Է� ó���� ����ϴ� �ڵ鷯
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

            // �ƿ����� ������Ʈ �ʱ�ȭ
            InitializeOutline();
        }

        /// <summary>
        /// �ƿ����� ������Ʈ �ʱ�ȭ
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
        /// ���콺 Ŭ�� �� ��� �巡�� ����
        /// </summary>
        private void OnMouseDown()
        {
            if (!dragHandler.Enabled) return;

            // �巡�� ���� ����
            isDragging = true;
            dragHandler.IsDragging = true;
            rb.isKinematic = false;
            outline.enabled = true;

            // ī�޶���� Z�� �Ÿ� ���
            zDistanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);

            // ���콺�� ������Ʈ ���� ������ ����
            offset = transform.position - GetMouseWorldPosition();

            // �浹 ���� �ʱ�ȭ
            physicsHandler.ResetCollisionState();
        }

        /// <summary>
        /// ���콺 ��ư ���� �� ��� �巡�� ����
        /// </summary>
        private void OnMouseUp()
        {
            // �巡�� ���� ���� ����
            isDragging = false;
            dragHandler.IsDragging = false;
            outline.enabled = false;

            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // ���� ��� ��ġ ����
            gridHandler.SetBlockPosition(true);
            physicsHandler.ResetCollisionState();
        }

        /// <summary>
        /// �� ������ �Է� ���� ������Ʈ
        /// </summary>
        private void Update()
        {
            // �巡�� �� �� ������ ���콺 ��ġ ������Ʈ
            if (isDragging)
            {
                UpdateDragPosition();
            }
        }

        /// <summary>
        /// �巡�� ��ġ ������Ʈ
        /// </summary>
        private void UpdateDragPosition()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 targetPosition = mouseWorldPos + offset;

            // �̵� ���� ��� �� ����
            Vector3 moveVector = targetPosition - transform.position;
            physicsHandler.SetMoveVector(moveVector, targetPosition);
        }

        /// <summary>
        /// ���콺 ȭ�� ��ǥ�� ���� ��ǥ�� ��ȯ
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = zDistanceToCamera;
            return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        }

        /// <summary>
        /// �Է� ���� ����
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
        /// �ƿ����� ǥ�� ���� ����
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