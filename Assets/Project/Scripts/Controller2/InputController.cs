using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 입력 처리를 담당하는 컨트롤러
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

            // 마우스 입력 처리
            if (Input.GetMouseButtonDown(0))
            {
                // 블록 선택
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 100f, blockLayer))
                {
                    BlockObject block = hit.collider.GetComponentInParent<BlockObject>();
                    if (block != null && block.dragHandler.Enabled)
                    {
                        selectedBlock = block;

                        // 카메라와의 z축 거리 계산
                        zDistanceToCamera = Vector3.Distance(block.transform.position, mainCamera.transform.position);

                        // 마우스와 오브젝트 간의 오프셋 저장
                        dragOffset = block.dragHandler.transform.position - GetMouseWorldPosition();

                        // 블록 선택 처리
                        blockController.OnBlockSelected(selectedBlock);
                    }
                }
            }
            else if (Input.GetMouseButton(0) && selectedBlock != null)
            {
                // 블록 드래그
                Vector3 targetPosition = GetMouseWorldPosition() + dragOffset;
                blockController.MoveBlock(selectedBlock, targetPosition);
            }
            else if (Input.GetMouseButtonUp(0) && selectedBlock != null)
            {
                // 블록 배치
                blockController.PlaceBlock(selectedBlock);
                selectedBlock = null;
            }
           
            // 키보드 입력 처리 (예: 레벨 이동)
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
                // 현재 레벨 재시작
                gameController.LoadLevelAsync(gameController.GetLevelController().CurrentLevel);
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = zDistanceToCamera;
            return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        }

        // 블록 위에 마우스가 있는지 확인
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

        // UI 입력 관련 메서드들
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