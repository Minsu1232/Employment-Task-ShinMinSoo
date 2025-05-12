using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// �Է� ó���� ����ϴ� ��Ʈ�ѷ�
    /// </summary>
    public class InputController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask blockLayer;

        private GameController gameController;
        private BlockController blockController;

        private BlockObject selectedBlock;
        private Vector3 dragOffset;
        private float zDistanceToCamera;

        public void Initialize(GameController controller)
        {
            gameController = controller;

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        public void SetBlockController(BlockController controller)
        {
            blockController = controller;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (blockController == null) return;

            // ���콺 �Է� ó��
            if (Input.GetMouseButtonDown(0))
            {
                // ��� ����
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 100f, blockLayer))
                {
                    BlockObject block = hit.collider.GetComponentInParent<BlockObject>();
                    if (block != null && block.dragHandler.Enabled)
                    {
                        selectedBlock = block;

                        // ī�޶���� z�� �Ÿ� ���
                        zDistanceToCamera = Vector3.Distance(block.transform.position, mainCamera.transform.position);

                        // ���콺�� ������Ʈ ���� ������ ����
                        dragOffset = block.dragHandler.transform.position - GetMouseWorldPosition();

                        // ��� ���� ó��
                        blockController.OnBlockSelected(selectedBlock);
                    }
                }
            }
            else if (Input.GetMouseButton(0) && selectedBlock != null)
            {
                // ��� �巡��
                Vector3 targetPosition = GetMouseWorldPosition() + dragOffset;
                blockController.MoveBlock(selectedBlock, targetPosition);
            }
            else if (Input.GetMouseButtonUp(0) && selectedBlock != null)
            {
                // ��� ��ġ
                blockController.PlaceBlock(selectedBlock);
                selectedBlock = null;
            }
           
            // Ű���� �Է� ó�� (��: ���� �̵�)
            if (Input.GetKeyDown(KeyCode.N))
            {
                gameController.GoToNextLevelAsync();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                gameController.GoToPreviousLevelAsync();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                // ���� ���� �����
                gameController.LoadLevelAsync(gameController.GetLevelController().CurrentLevel);
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = zDistanceToCamera;
            return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        }

        // ��� ���� ���콺�� �ִ��� Ȯ��
        public bool IsPointerOverBlock(out BlockObject block)
        {
            block = null;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, blockLayer))
            {
                block = hit.collider.GetComponentInParent<BlockObject>();
                return block != null;
            }

            return false;
        }

        // UI �Է� ���� �޼����
        public void OnNextLevelButtonClicked()
        {
            gameController.GoToNextLevelAsync();
        }

        public void OnPreviousLevelButtonClicked()
        {
            gameController.GoToPreviousLevelAsync();
        }

        public void OnRestartButtonClicked()
        {
            gameController.LoadLevelAsync(gameController.GetLevelController().CurrentLevel);
        }
    }
}