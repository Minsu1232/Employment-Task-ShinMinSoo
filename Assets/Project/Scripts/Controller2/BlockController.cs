using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Model;
using Project.Scripts.Presenter;
using Project.Scripts.View;
using DG.Tweening;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 블록 생성 및 관리를 담당하는 컨트롤러
    /// </summary>
    public class BlockController : MonoBehaviour
    {
        [Header("프리팹 참조")]
        [SerializeField] private GameObject blockGroupPrefab;
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private Material[] blockMaterials;
        [SerializeField] private ParticleSystem destroyParticlePrefab;

        [Header("블록 설정")]
        [SerializeField] private float blockDistance = 0.79f;
        [SerializeField] private float blockMoveSpeed = 20f;

        private GameController gameController;
        private BoardController boardController;
        private GameObject blocksParent;

        // 현재 선택된 블록과 드래그 상태
        private BlockObject selectedBlock;
        private Vector3 dragOffset;
        private bool isDragging;

        // 블록 컬렉션
        private Dictionary<int, BlockDragHandler> blockHandlers = new Dictionary<int, BlockDragHandler>();

        // 기믹 컬렉션
        private Dictionary<int, List<IGimmickPresenter>> blockGimmicks = new Dictionary<int, List<IGimmickPresenter>>();

        public void Initialize(GameController controller)
        {
            gameController = controller;
        }

        public void SetBoardController(BoardController controller)
        {
            boardController = controller;
        }

        public async Task CreateBlocksAsync(List<PlayingBlockData> playingBlocks)
        {
            // 이전 블록 제거
            ClearBlocks();

            // 새 블록 부모 오브젝트 생성
            blocksParent = new GameObject("BlocksParent");
            blocksParent.transform.SetParent(transform);

            // 블록 생성
            foreach (var pbData in playingBlocks)
            {
                await CreateBlockGroupAsync(pbData);
            }
        }

        private async Task CreateBlockGroupAsync(PlayingBlockData pbData)
        {
            if (blockGroupPrefab == null || blockPrefab == null)
            {
                Debug.LogError("블록 프리팹이 설정되지 않았습니다!");
                return;
            }

            // 블록 그룹 생성
            GameObject blockGroupObject = Instantiate(blockGroupPrefab, blocksParent.transform);
            blockGroupObject.transform.position = new Vector3(
                pbData.center.x * blockDistance,
                0.33f,
                pbData.center.y * blockDistance
            );

            // 블록 핸들러 설정
            BlockDragHandler dragHandler = blockGroupObject.GetComponent<BlockDragHandler>();
            if (dragHandler == null)
            {
                dragHandler = blockGroupObject.AddComponent<BlockDragHandler>();
            }

            dragHandler.Initialize(this);
            dragHandler.blocks = new List<BlockObject>();
            dragHandler.blockOffsets = new List<Vector2>();
            dragHandler.uniqueIndex = pbData.uniqueIndex;

            // 기믹 설정
            if (pbData.gimmicks != null && pbData.gimmicks.Count > 0)
            {
                // 기믹 프레젠터 생성
                List<IGimmickPresenter> gimmickPresenters =
                    GimmickFactory.Instance.CreateGimmicks(pbData.gimmicks, blockGroupObject);

                // 기믹 저장
                blockGimmicks[pbData.uniqueIndex] = gimmickPresenters;
            }

            // 블록 조각 생성
            int maxX = 0, minX = int.MaxValue;
            int maxY = 0, minY = int.MaxValue;

            foreach (var shape in pbData.shapes)
            {
                GameObject singleBlock = Instantiate(blockPrefab, blockGroupObject.transform);

                singleBlock.transform.localPosition = new Vector3(
                    shape.offset.x * blockDistance,
                    0f,
                    shape.offset.y * blockDistance
                );

                dragHandler.blockOffsets.Add(new Vector2(shape.offset.x, shape.offset.y));

                // 블록 뷰 컴포넌트 추가
                BlockView blockView = singleBlock.AddComponent<BlockView>();

                // 블록 내부 요소 설정
                var renderer = singleBlock.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null && (int)pbData.ColorType < blockMaterials.Length)
                {
                    renderer.material = blockMaterials[(int)pbData.ColorType];
                    if (blockView != null)
                    {
                        blockView.SetMaterial(blockMaterials[(int)pbData.ColorType]);
                    }
                }

                // 블록 오브젝트 설정
                if (singleBlock.TryGetComponent(out BlockObject blockObj))
                {
                    blockObj.colorType = pbData.ColorType;
                    blockObj.x = pbData.center.x + shape.offset.x;
                    blockObj.y = pbData.center.y + shape.offset.y;
                    blockObj.offsetToCenter = new Vector2(shape.offset.x, shape.offset.y);
                    blockObj.dragHandler = dragHandler;

                    dragHandler.blocks.Add(blockObj);

                    // 보드 블록 연결
                    var boardBlock = boardController.GetBoardBlockAt((int)blockObj.x, (int)blockObj.y);
                    if (boardBlock != null)
                    {
                        boardBlock.playingBlock = blockObj;
                        blockObj.preBoardBlockObject = boardBlock;
                    }

                    // 크기 계산
                    if (blockObj.x < minX) minX = (int)blockObj.x;
                    if (blockObj.y < minY) minY = (int)blockObj.y;
                    if (blockObj.x > maxX) maxX = (int)blockObj.x;
                    if (blockObj.y > maxY) maxY = (int)blockObj.y;
                }
            }

            // 블록 그룹 크기 설정
            dragHandler.horizon = maxX - minX + 1;
            dragHandler.vertical = maxY - minY + 1;

            // 블록 핸들러 저장
            blockHandlers[pbData.uniqueIndex] = dragHandler;

            await Task.Yield();
        }

        public void ClearBlocks()
        {
            if (blocksParent != null)
            {
                Destroy(blocksParent);
            }

            blockHandlers.Clear();
            blockGimmicks.Clear();
            selectedBlock = null;
            isDragging = false;
        }

        // 입력 처리 메서드들
        public void OnBlockSelected(BlockObject block)
        {
            if (block == null || !block.dragHandler.Enabled) return;

            selectedBlock = block;
            isDragging = true;

            // 블록 뷰에 선택 효과 적용
            BlockView blockView = block.GetComponent<BlockView>();
            if (blockView != null)
            {
                blockView.ShowOutline(true);
            }
            else
            {
                // 기존 방식(드래그 핸들러에 아웃라인 적용)
                block.dragHandler.ShowOutline(true);
            }
        }

        public void MoveBlock(BlockObject block, Vector3 targetPosition)
        {
            if (block == null || !isDragging || block != selectedBlock) return;

            // 기믹 이동 체크
            if (CheckGimmickMove(block, targetPosition) == false)
            {
                return; // 이동 불가
            }

            Vector3 newPosition = new Vector3(
                targetPosition.x,
                block.dragHandler.transform.position.y,
                targetPosition.z
            );

            // 블록 이동
            block.dragHandler.transform.position = Vector3.Lerp(
                block.dragHandler.transform.position,
                newPosition,
                Time.deltaTime * blockMoveSpeed
            );

            // 보드 블록과 블록 위치 동기화
            UpdateBlockPosition(block);
        }

        // 기믹 이동 체크
        private bool CheckGimmickMove(BlockObject block, Vector3 targetPosition)
        {
            if (block == null || block.dragHandler == null) return true;

            int uniqueIndex = block.dragHandler.uniqueIndex;

            // 해당 블록에 기믹이 없으면 항상 이동 가능
            if (!blockGimmicks.ContainsKey(uniqueIndex))
                return true;

            // 모든 기믹에서 이동 가능 여부 확인
            foreach (var gimmick in blockGimmicks[uniqueIndex])
            {
                if (!gimmick.IsActive) continue;

                // 하나라도 이동 불가면 전체 이동 불가
                if (!gimmick.OnBlockMove(targetPosition))
                    return false;
            }

            return true;
        }

        public void PlaceBlock(BlockObject block)
        {
            if (block == null || !isDragging || block != selectedBlock) return;

            isDragging = false;
            selectedBlock = null;

            // 블록 뷰에 선택 해제 효과 적용
            BlockView blockView = block.GetComponent<BlockView>();
            if (blockView != null)
            {
                blockView.ShowOutline(false);
            }
            else
            {
                // 기존 방식(드래그 핸들러에 아웃라인 적용)
                block.dragHandler.ShowOutline(false);
            }

            // 최종 위치 설정
            FinalizeBlockPosition(block);

            // 블록 배치 후 기믹 알림
            NotifyGimmickBlockPlaced(block);
        }

        // 기믹에 블록 배치 알림
        private void NotifyGimmickBlockPlaced(BlockObject block)
        {
            if (block == null || block.dragHandler == null) return;

            int uniqueIndex = block.dragHandler.uniqueIndex;

            // 해당 블록에 기믹이 없으면 아무것도 안 함
            if (!blockGimmicks.ContainsKey(uniqueIndex))
                return;

            // 모든 기믹에 블록 배치 알림
            foreach (var gimmick in blockGimmicks[uniqueIndex])
            {
                if (!gimmick.IsActive) continue;

                gimmick.OnBlockPlace(block.dragHandler.transform.position);
            }
        }

        private void UpdateBlockPosition(BlockObject block)
        {
            // 블록 위치 업데이트 로직
            foreach (var blockObj in block.dragHandler.blocks)
            {
                // grid 좌표 계산 및 업데이트
                Vector3 worldPos = blockObj.transform.position;
                var gridPos = boardController.WorldToGridPosition(worldPos);
                blockObj.x = gridPos.x;
                blockObj.y = gridPos.y;
            }
        }

        private void FinalizeBlockPosition(BlockObject block)
        {
            // 블록 최종 위치 결정 및 배치 로직

            // 블록 아래의 보드 블록 확인
            Ray ray = new Ray(block.transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out BoardBlockObject boardBlock))
                {
                    // 블록 스냅 처리
                    Vector3 snapPosition = boardController.GridToWorldPosition(
                        (int)block.x, (int)block.y);
                    snapPosition.y = block.dragHandler.transform.position.y;

                    block.dragHandler.transform.position = snapPosition;

                    // 블록-보드 블록 관계 업데이트
                    UpdateBlockBoardRelationship(block, boardBlock);

                    // 블록 파괴 가능 여부 확인
                    CheckBlockDestroy(block, boardBlock);
                }
            }
        }

        private void UpdateBlockBoardRelationship(BlockObject block, BoardBlockObject boardBlock)
        {
            // 블록과 보드 블록 관계 업데이트 로직
            foreach (var blockObj in block.dragHandler.blocks)
            {
                int gridX = (int)blockObj.x;
                int gridY = (int)blockObj.y;

                var targetBoardBlock = boardController.GetBoardBlockAt(gridX, gridY);
                if (targetBoardBlock != null)
                {
                    // 이전 관계 정리
                    if (blockObj.preBoardBlockObject != null && blockObj.preBoardBlockObject != targetBoardBlock)
                    {
                        blockObj.preBoardBlockObject.playingBlock = null;
                    }

                    // 새 관계 설정
                    targetBoardBlock.playingBlock = blockObj;
                    blockObj.preBoardBlockObject = targetBoardBlock;
                }
            }
        }

        private void CheckBlockDestroy(BlockObject block, BoardBlockObject boardBlock)
        {
            // 블록 파괴 가능 여부 확인 및 처리
            if (boardBlock.isCheckBlock)
            {
                // 파괴 가능 여부 기믹에서 확인
                if (!CheckGimmickDestroyAttempt(block))
                    return;

                // 보드 컨트롤러에서 파괴 가능 여부 확인
                if (boardController.CheckCanDestroy(boardBlock, block))
                {
                    DestroyBlock(block, boardBlock);
                }
            }
        }

        // 기믹 파괴 가능 여부 체크
        private bool CheckGimmickDestroyAttempt(BlockObject block)
        {
            if (block == null || block.dragHandler == null) return true;

            int uniqueIndex = block.dragHandler.uniqueIndex;

            // 해당 블록에 기믹이 없으면 항상 파괴 가능
            if (!blockGimmicks.ContainsKey(uniqueIndex))
                return true;

            // 모든 기믹에서 파괴 가능 여부 확인
            foreach (var gimmick in blockGimmicks[uniqueIndex])
            {
                if (!gimmick.IsActive) continue;

                // 하나라도 파괴 불가면 전체 파괴 불가
                if (!gimmick.OnDestroyAttempt())
                    return false;
            }

            return true;
        }

        public void DestroyBlock(BlockObject block, BoardBlockObject boardBlock)
        {
            // 블록 파괴 처리
            Vector3 destroyPosition = CalculateDestroyPosition(block, boardBlock);

            // 파괴 파티클 생성
            CreateDestroyParticle(block, boardBlock);

            // 파괴 시 기믹 알림
            NotifyGimmickBlockDestroyed(block);

            // 블록 제거 애니메이션
            PlayDestroyAnimation(block, destroyPosition);
        }

        // 기믹에 블록 파괴 알림
        private void NotifyGimmickBlockDestroyed(BlockObject block)
        {
            if (block == null || block.dragHandler == null) return;

            int uniqueIndex = block.dragHandler.uniqueIndex;

            // 해당 블록에 기믹이 없으면 아무것도 안 함
            if (!blockGimmicks.ContainsKey(uniqueIndex))
                return;

            // 모든 기믹에 블록 파괴 알림
            foreach (var gimmick in blockGimmicks[uniqueIndex])
            {
                if (!gimmick.IsActive) continue;

                gimmick.OnDestroyed();
            }

            // 기믹 목록에서 제거
            blockGimmicks.Remove(uniqueIndex);
        }

        private Vector3 CalculateDestroyPosition(BlockObject block, BoardBlockObject boardBlock)
        {
            // 파괴 위치 계산
            Vector3 position = block.dragHandler.transform.position;

            // 파괴 방향 결정
            LaunchDirection direction = GetLaunchDirection(boardBlock);

            // 방향에 따른 위치 계산
            switch (direction)
            {
                case LaunchDirection.Up:
                    position += Vector3.forward * 2f;
                    break;
                case LaunchDirection.Down:
                    position += Vector3.back * 2f;
                    break;
                case LaunchDirection.Left:
                    position += Vector3.left * 2f;
                    break;
                case LaunchDirection.Right:
                    position += Vector3.right * 2f;
                    break;
            }

            return position;
        }

        private LaunchDirection GetLaunchDirection(BoardBlockObject boardBlock)
        {
            // 보드 블록 위치를 기반으로 방향 결정
            int x = boardBlock.x;
            int y = boardBlock.y;

            // 모서리 케이스
            if (x == 0 && y == 0)
                return boardBlock.isHorizon.Count > 0 && boardBlock.isHorizon[0] ?
                    LaunchDirection.Down : LaunchDirection.Left;

            if (x == 0 && y == boardController.BoardHeight)
                return boardBlock.isHorizon.Count > 0 && boardBlock.isHorizon[0] ?
                    LaunchDirection.Up : LaunchDirection.Left;

            if (x == boardController.BoardWidth && y == 0)
                return boardBlock.isHorizon.Count > 0 && boardBlock.isHorizon[0] ?
                    LaunchDirection.Down : LaunchDirection.Right;

            if (x == boardController.BoardWidth && y == boardController.BoardHeight)
                return boardBlock.isHorizon.Count > 0 && boardBlock.isHorizon[0] ?
                    LaunchDirection.Up : LaunchDirection.Right;

            // 경계 케이스
            if (x == 0)
                return LaunchDirection.Left;

            if (y == 0)
                return LaunchDirection.Down;

            if (x == boardController.BoardWidth)
                return LaunchDirection.Right;

            if (y == boardController.BoardHeight)
                return LaunchDirection.Up;

            // 기본값
            return LaunchDirection.Up;
        }

        private void CreateDestroyParticle(BlockObject block, BoardBlockObject boardBlock)
        {
            if (destroyParticlePrefab == null) return;

            // 파괴 위치
            Vector3 position = block.dragHandler.transform.position;
            position.y += 0.2f;

            // 파괴 방향
            LaunchDirection direction = GetLaunchDirection(boardBlock);
            Quaternion rotation = Quaternion.identity;

            switch (direction)
            {
                case LaunchDirection.Up:
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case LaunchDirection.Down:
                    rotation = Quaternion.identity;
                    break;
                case LaunchDirection.Left:
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case LaunchDirection.Right:
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
            }

            // 파티클 생성
            ParticleSystem particle = Instantiate(destroyParticlePrefab, position, rotation);

            // 블록 색상에 맞게 파티클 색상 설정
            ParticleSystem.MainModule main = particle.main;
            main.startColor = GetColorForType(block.colorType);

            // 블록 크기에 맞게 파티클 크기 조정
            int blockLength = block.dragHandler.isHorizon.Count > 0 && block.dragHandler.isHorizon[0] ?
                block.dragHandler.horizon : block.dragHandler.vertical;
            particle.transform.localScale = new Vector3(blockLength * 0.4f, 0.5f, blockLength * 0.4f);

            // 파티클 자동 제거
            Destroy(particle.gameObject, 2f);
        }

        private void PlayDestroyAnimation(BlockObject block, Vector3 targetPosition)
        {
            // 파괴 애니메이션
            int uniqueIndex = block.dragHandler.uniqueIndex;

            // 블록 이동 애니메이션
            block.dragHandler.transform.DOMove(targetPosition, 1f)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    // 블록 완전 제거
                    blockHandlers.Remove(uniqueIndex);
                    Destroy(block.dragHandler.gameObject);

                    // 보드 상태 업데이트
                    UpdateBoardAfterDestroy();

                    // 게임 종료 조건 체크
                    CheckGameEnd();
                });

            // 블록 페이드 아웃
            foreach (var blockObj in block.dragHandler.blocks)
            {
                BlockView blockView = blockObj.GetComponent<BlockView>();
                if (blockView != null)
                {
                    blockView.PlayDestroyAnimation(targetPosition, 1f);
                }
                else
                {
                    // 기존 방식
                    var renderer = blockObj.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        Color startColor = renderer.material.color;
                        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

                        DOTween.To(() => startColor, x => renderer.material.color = x, endColor, 1f);
                    }
                }
            }
        }

        private void UpdateBoardAfterDestroy()
        {
            // 블록 파괴 후 보드 상태 업데이트
            // 예: 추가 블록 생성, 효과 발동 등
        }

        private void CheckGameEnd()
        {
            // 게임 종료 조건 체크
            if (blockHandlers.Count == 0)
            {
                // 모든 블록 제거됨 - 레벨 클리어
                OnLevelCleared();
            }
            else if (IsPlayerStuck())
            {
                // 더 이상 가능한 이동이 없음 - 게임 오버
                OnGameOver();
            }
        }

        private bool IsPlayerStuck()
        {
            // 플레이어가 더 이상 이동할 수 없는지 체크
            // 현재 단순화되어 있음
            return false;
        }

        private void OnLevelCleared()
        {
            // 레벨 클리어 로직
            Debug.Log("레벨 클리어!");

            // 이벤트 발생
            GameEvents.OnLevelCompleted?.Invoke();

            // 다음 레벨로 진행
            gameController.GoToNextLevelAsync();
        }

        private void OnGameOver()
        {
            // 게임 오버 로직
            Debug.Log("게임 오버!");

            // 이벤트 발생
            GameEvents.OnGameOver?.Invoke();
        }

        private Color GetColorForType(ColorType colorType)
        {
            switch (colorType)
            {
                case ColorType.Red:
                    return new Color(1, 0, 0);
                case ColorType.Orange:
                    return new Color(1, 0.5f, 0);
                case ColorType.Yellow:
                    return new Color(1, 1, 0);
                case ColorType.Green:
                    return new Color(0, 1, 0);
                case ColorType.Blue:
                    return new Color(0, 0, 1);
                case ColorType.Purple:
                    return new Color(0.5f, 0, 0.5f);
                default:
                    return Color.white;
            }
        }
    }
}