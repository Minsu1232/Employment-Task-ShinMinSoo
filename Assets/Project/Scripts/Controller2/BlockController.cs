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
    /// ��� ���� �� ������ ����ϴ� ��Ʈ�ѷ�
    /// </summary>
    public class BlockController : MonoBehaviour
    {
        [Header("������ ����")]
        [SerializeField] private GameObject blockGroupPrefab;
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private Material[] blockMaterials;
        [SerializeField] private ParticleSystem destroyParticlePrefab;

        [Header("��� ����")]
        [SerializeField] private float blockDistance = 0.79f;
        [SerializeField] private float blockMoveSpeed = 20f;

        private GameController gameController;
        private BoardController boardController;
        private GameObject blocksParent;

        // ���� ���õ� ��ϰ� �巡�� ����
        private BlockObject selectedBlock;
        private Vector3 dragOffset;
        private bool isDragging;

        // ��� �÷���
        private Dictionary<int, BlockDragHandler> blockHandlers = new Dictionary<int, BlockDragHandler>();

        // ��� �÷���
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
            // ���� ��� ����
            ClearBlocks();

            // �� ��� �θ� ������Ʈ ����
            blocksParent = new GameObject("BlocksParent");
            blocksParent.transform.SetParent(transform);

            // ��� ����
            foreach (var pbData in playingBlocks)
            {
                await CreateBlockGroupAsync(pbData);
            }
        }

        private async Task CreateBlockGroupAsync(PlayingBlockData pbData)
        {
            if (blockGroupPrefab == null || blockPrefab == null)
            {
                Debug.LogError("��� �������� �������� �ʾҽ��ϴ�!");
                return;
            }

            // ��� �׷� ����
            GameObject blockGroupObject = Instantiate(blockGroupPrefab, blocksParent.transform);
            blockGroupObject.transform.position = new Vector3(
                pbData.center.x * blockDistance,
                0.33f,
                pbData.center.y * blockDistance
            );

            // ��� �ڵ鷯 ����
            BlockDragHandler dragHandler = blockGroupObject.GetComponent<BlockDragHandler>();
            if (dragHandler == null)
            {
                dragHandler = blockGroupObject.AddComponent<BlockDragHandler>();
            }

            dragHandler.Initialize(this);
            dragHandler.blocks = new List<BlockObject>();
            dragHandler.blockOffsets = new List<Vector2>();
            dragHandler.uniqueIndex = pbData.uniqueIndex;

            // ��� ����
            if (pbData.gimmicks != null && pbData.gimmicks.Count > 0)
            {
                // ��� �������� ����
                List<IGimmickPresenter> gimmickPresenters =
                    GimmickFactory.Instance.CreateGimmicks(pbData.gimmicks, blockGroupObject);

                // ��� ����
                blockGimmicks[pbData.uniqueIndex] = gimmickPresenters;
            }

            // ��� ���� ����
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

                // ��� �� ������Ʈ �߰�
                BlockView blockView = singleBlock.AddComponent<BlockView>();

                // ��� ���� ��� ����
                var renderer = singleBlock.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null && (int)pbData.ColorType < blockMaterials.Length)
                {
                    renderer.material = blockMaterials[(int)pbData.ColorType];
                    if (blockView != null)
                    {
                        blockView.SetMaterial(blockMaterials[(int)pbData.ColorType]);
                    }
                }

                // ��� ������Ʈ ����
                if (singleBlock.TryGetComponent(out BlockObject blockObj))
                {
                    blockObj.colorType = pbData.ColorType;
                    blockObj.x = pbData.center.x + shape.offset.x;
                    blockObj.y = pbData.center.y + shape.offset.y;
                    blockObj.offsetToCenter = new Vector2(shape.offset.x, shape.offset.y);
                    blockObj.dragHandler = dragHandler;

                    dragHandler.blocks.Add(blockObj);

                    // ���� ��� ����
                    var boardBlock = boardController.GetBoardBlockAt((int)blockObj.x, (int)blockObj.y);
                    if (boardBlock != null)
                    {
                        boardBlock.playingBlock = blockObj;
                        blockObj.preBoardBlockObject = boardBlock;
                    }

                    // ũ�� ���
                    if (blockObj.x < minX) minX = (int)blockObj.x;
                    if (blockObj.y < minY) minY = (int)blockObj.y;
                    if (blockObj.x > maxX) maxX = (int)blockObj.x;
                    if (blockObj.y > maxY) maxY = (int)blockObj.y;
                }
            }

            // ��� �׷� ũ�� ����
            dragHandler.horizon = maxX - minX + 1;
            dragHandler.vertical = maxY - minY + 1;

            // ��� �ڵ鷯 ����
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

        // �Է� ó�� �޼����
        public void OnBlockSelected(BlockObject block)
        {
            if (block == null || !block.dragHandler.Enabled) return;

            selectedBlock = block;
            isDragging = true;

            // ��� �信 ���� ȿ�� ����
            BlockView blockView = block.GetComponent<BlockView>();
            if (blockView != null)
            {
                blockView.ShowOutline(true);
            }
            else
            {
                // ���� ���(�巡�� �ڵ鷯�� �ƿ����� ����)
                block.dragHandler.ShowOutline(true);
            }
        }

        public void MoveBlock(BlockObject block, Vector3 targetPosition)
        {
            if (block == null || !isDragging || block != selectedBlock) return;

            // ��� �̵� üũ
            if (CheckGimmickMove(block, targetPosition) == false)
            {
                return; // �̵� �Ұ�
            }

            Vector3 newPosition = new Vector3(
                targetPosition.x,
                block.dragHandler.transform.position.y,
                targetPosition.z
            );

            // ��� �̵�
            block.dragHandler.transform.position = Vector3.Lerp(
                block.dragHandler.transform.position,
                newPosition,
                Time.deltaTime * blockMoveSpeed
            );

            // ���� ��ϰ� ��� ��ġ ����ȭ
            UpdateBlockPosition(block);
        }

        // ��� �̵� üũ
        private bool CheckGimmickMove(BlockObject block, Vector3 targetPosition)
        {
            if (block == null || block.dragHandler == null) return true;

            int uniqueIndex = block.dragHandler.uniqueIndex;

            // �ش� ��Ͽ� ����� ������ �׻� �̵� ����
            if (!blockGimmicks.ContainsKey(uniqueIndex))
                return true;

            // ��� ��Ϳ��� �̵� ���� ���� Ȯ��
            foreach (var gimmick in blockGimmicks[uniqueIndex])
            {
                if (!gimmick.IsActive) continue;

                // �ϳ��� �̵� �Ұ��� ��ü �̵� �Ұ�
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

            // ��� �信 ���� ���� ȿ�� ����
            BlockView blockView = block.GetComponent<BlockView>();
            if (blockView != null)
            {
                blockView.ShowOutline(false);
            }
            else
            {
                // ���� ���(�巡�� �ڵ鷯�� �ƿ����� ����)
                block.dragHandler.ShowOutline(false);
            }

            // ���� ��ġ ����
            FinalizeBlockPosition(block);

            // ��� ��ġ �� ��� �˸�
            NotifyGimmickBlockPlaced(block);
        }

        // ��Ϳ� ��� ��ġ �˸�
        private void NotifyGimmickBlockPlaced(BlockObject block)
        {
            if (block == null || block.dragHandler == null) return;

            int uniqueIndex = block.dragHandler.uniqueIndex;

            // �ش� ��Ͽ� ����� ������ �ƹ��͵� �� ��
            if (!blockGimmicks.ContainsKey(uniqueIndex))
                return;

            // ��� ��Ϳ� ��� ��ġ �˸�
            foreach (var gimmick in blockGimmicks[uniqueIndex])
            {
                if (!gimmick.IsActive) continue;

                gimmick.OnBlockPlace(block.dragHandler.transform.position);
            }
        }

        private void UpdateBlockPosition(BlockObject block)
        {
            // ��� ��ġ ������Ʈ ����
            foreach (var blockObj in block.dragHandler.blocks)
            {
                // grid ��ǥ ��� �� ������Ʈ
                Vector3 worldPos = blockObj.transform.position;
                var gridPos = boardController.WorldToGridPosition(worldPos);
                blockObj.x = gridPos.x;
                blockObj.y = gridPos.y;
            }
        }

        private void FinalizeBlockPosition(BlockObject block)
        {
            // ��� ���� ��ġ ���� �� ��ġ ����

            // ��� �Ʒ��� ���� ��� Ȯ��
            Ray ray = new Ray(block.transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out BoardBlockObject boardBlock))
                {
                    // ��� ���� ó��
                    Vector3 snapPosition = boardController.GridToWorldPosition(
                        (int)block.x, (int)block.y);
                    snapPosition.y = block.dragHandler.transform.position.y;

                    block.dragHandler.transform.position = snapPosition;

                    // ���-���� ��� ���� ������Ʈ
                    UpdateBlockBoardRelationship(block, boardBlock);

                    // ��� �ı� ���� ���� Ȯ��
                    CheckBlockDestroy(block, boardBlock);
                }
            }
        }

        private void UpdateBlockBoardRelationship(BlockObject block, BoardBlockObject boardBlock)
        {
            // ��ϰ� ���� ��� ���� ������Ʈ ����
            foreach (var blockObj in block.dragHandler.blocks)
            {
                int gridX = (int)blockObj.x;
                int gridY = (int)blockObj.y;

                var targetBoardBlock = boardController.GetBoardBlockAt(gridX, gridY);
                if (targetBoardBlock != null)
                {
                    // ���� ���� ����
                    if (blockObj.preBoardBlockObject != null && blockObj.preBoardBlockObject != targetBoardBlock)
                    {
                        blockObj.preBoardBlockObject.playingBlock = null;
                    }

                    // �� ���� ����
                    targetBoardBlock.playingBlock = blockObj;
                    blockObj.preBoardBlockObject = targetBoardBlock;
                }
            }
        }

        private void CheckBlockDestroy(BlockObject block, BoardBlockObject boardBlock)
        {
            // ��� �ı� ���� ���� Ȯ�� �� ó��
            if (boardBlock.isCheckBlock)
            {
                // �ı� ���� ���� ��Ϳ��� Ȯ��
                if (!CheckGimmickDestroyAttempt(block))
                    return;

                // ���� ��Ʈ�ѷ����� �ı� ���� ���� Ȯ��
                if (boardController.CheckCanDestroy(boardBlock, block))
                {
                    DestroyBlock(block, boardBlock);
                }
            }
        }

        // ��� �ı� ���� ���� üũ
        private bool CheckGimmickDestroyAttempt(BlockObject block)
        {
            if (block == null || block.dragHandler == null) return true;

            int uniqueIndex = block.dragHandler.uniqueIndex;

            // �ش� ��Ͽ� ����� ������ �׻� �ı� ����
            if (!blockGimmicks.ContainsKey(uniqueIndex))
                return true;

            // ��� ��Ϳ��� �ı� ���� ���� Ȯ��
            foreach (var gimmick in blockGimmicks[uniqueIndex])
            {
                if (!gimmick.IsActive) continue;

                // �ϳ��� �ı� �Ұ��� ��ü �ı� �Ұ�
                if (!gimmick.OnDestroyAttempt())
                    return false;
            }

            return true;
        }

        public void DestroyBlock(BlockObject block, BoardBlockObject boardBlock)
        {
            // ��� �ı� ó��
            Vector3 destroyPosition = CalculateDestroyPosition(block, boardBlock);

            // �ı� ��ƼŬ ����
            CreateDestroyParticle(block, boardBlock);

            // �ı� �� ��� �˸�
            NotifyGimmickBlockDestroyed(block);

            // ��� ���� �ִϸ��̼�
            PlayDestroyAnimation(block, destroyPosition);
        }

        // ��Ϳ� ��� �ı� �˸�
        private void NotifyGimmickBlockDestroyed(BlockObject block)
        {
            if (block == null || block.dragHandler == null) return;

            int uniqueIndex = block.dragHandler.uniqueIndex;

            // �ش� ��Ͽ� ����� ������ �ƹ��͵� �� ��
            if (!blockGimmicks.ContainsKey(uniqueIndex))
                return;

            // ��� ��Ϳ� ��� �ı� �˸�
            foreach (var gimmick in blockGimmicks[uniqueIndex])
            {
                if (!gimmick.IsActive) continue;

                gimmick.OnDestroyed();
            }

            // ��� ��Ͽ��� ����
            blockGimmicks.Remove(uniqueIndex);
        }

        private Vector3 CalculateDestroyPosition(BlockObject block, BoardBlockObject boardBlock)
        {
            // �ı� ��ġ ���
            Vector3 position = block.dragHandler.transform.position;

            // �ı� ���� ����
            LaunchDirection direction = GetLaunchDirection(boardBlock);

            // ���⿡ ���� ��ġ ���
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
            // ���� ��� ��ġ�� ������� ���� ����
            int x = boardBlock.x;
            int y = boardBlock.y;

            // �𼭸� ���̽�
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

            // ��� ���̽�
            if (x == 0)
                return LaunchDirection.Left;

            if (y == 0)
                return LaunchDirection.Down;

            if (x == boardController.BoardWidth)
                return LaunchDirection.Right;

            if (y == boardController.BoardHeight)
                return LaunchDirection.Up;

            // �⺻��
            return LaunchDirection.Up;
        }

        private void CreateDestroyParticle(BlockObject block, BoardBlockObject boardBlock)
        {
            if (destroyParticlePrefab == null) return;

            // �ı� ��ġ
            Vector3 position = block.dragHandler.transform.position;
            position.y += 0.2f;

            // �ı� ����
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

            // ��ƼŬ ����
            ParticleSystem particle = Instantiate(destroyParticlePrefab, position, rotation);

            // ��� ���� �°� ��ƼŬ ���� ����
            ParticleSystem.MainModule main = particle.main;
            main.startColor = GetColorForType(block.colorType);

            // ��� ũ�⿡ �°� ��ƼŬ ũ�� ����
            int blockLength = block.dragHandler.isHorizon.Count > 0 && block.dragHandler.isHorizon[0] ?
                block.dragHandler.horizon : block.dragHandler.vertical;
            particle.transform.localScale = new Vector3(blockLength * 0.4f, 0.5f, blockLength * 0.4f);

            // ��ƼŬ �ڵ� ����
            Destroy(particle.gameObject, 2f);
        }

        private void PlayDestroyAnimation(BlockObject block, Vector3 targetPosition)
        {
            // �ı� �ִϸ��̼�
            int uniqueIndex = block.dragHandler.uniqueIndex;

            // ��� �̵� �ִϸ��̼�
            block.dragHandler.transform.DOMove(targetPosition, 1f)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    // ��� ���� ����
                    blockHandlers.Remove(uniqueIndex);
                    Destroy(block.dragHandler.gameObject);

                    // ���� ���� ������Ʈ
                    UpdateBoardAfterDestroy();

                    // ���� ���� ���� üũ
                    CheckGameEnd();
                });

            // ��� ���̵� �ƿ�
            foreach (var blockObj in block.dragHandler.blocks)
            {
                BlockView blockView = blockObj.GetComponent<BlockView>();
                if (blockView != null)
                {
                    blockView.PlayDestroyAnimation(targetPosition, 1f);
                }
                else
                {
                    // ���� ���
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
            // ��� �ı� �� ���� ���� ������Ʈ
            // ��: �߰� ��� ����, ȿ�� �ߵ� ��
        }

        private void CheckGameEnd()
        {
            // ���� ���� ���� üũ
            if (blockHandlers.Count == 0)
            {
                // ��� ��� ���ŵ� - ���� Ŭ����
                OnLevelCleared();
            }
            else if (IsPlayerStuck())
            {
                // �� �̻� ������ �̵��� ���� - ���� ����
                OnGameOver();
            }
        }

        private bool IsPlayerStuck()
        {
            // �÷��̾ �� �̻� �̵��� �� ������ üũ
            // ���� �ܼ�ȭ�Ǿ� ����
            return false;
        }

        private void OnLevelCleared()
        {
            // ���� Ŭ���� ����
            Debug.Log("���� Ŭ����!");

            // �̺�Ʈ �߻�
            GameEvents.OnLevelCompleted?.Invoke();

            // ���� ������ ����
            gameController.GoToNextLevelAsync();
        }

        private void OnGameOver()
        {
            // ���� ���� ����
            Debug.Log("���� ����!");

            // �̺�Ʈ �߻�
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