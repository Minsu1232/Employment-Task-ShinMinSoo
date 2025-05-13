using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Config;
using Project.Scripts.Events;
using Project.Scripts.Model;
using static Project.Scripts.Model.BoardBlockData;
using Project.Scripts.View;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ��� �ı� ���� ������ ó���ϴ� �Ŵ��� Ŭ����
    /// </summary>
    public class BlockDestroyManager : MonoBehaviour, IGameEventListener<(BoardBlockObject, BlockObject)>
    {
        // ���� �� ����
        private GameConfig gameConfig;

        // ���� ������ ����
        private Dictionary<(int x, int y), BoardBlockObject> boardBlockDic;
        private Dictionary<int, List<BoardBlockObject>> checkBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();
        private Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();

        // ���� ũ�� ����
        private int boardWidth;
        private int boardHeight;

        /// <summary>
        /// GameConfig�� ���� �ʱ�ȭ
        /// </summary>
        public void Initialize(GameConfig config, Dictionary<(int x, int y), BoardBlockObject> boardBlocks, int width, int height)
        {
            this.gameConfig = config;
            this.boardBlockDic = boardBlocks;
            this.boardWidth = width;
            this.boardHeight = height;

            // ��� �׷� �ʱ�ȭ
            checkBlockGroupDic.Clear();
            standardBlockDic.Clear();

            // �̺�Ʈ ���
            RegisterEvents();

            // BoardBlockObject �̺�Ʈ �ڵ鷯 ���
            RegisterBoardBlockEvents();
        }

        /// <summary>
        /// �̺�Ʈ ���
        /// </summary>
        private void RegisterEvents()
        {
            if (gameConfig != null && gameConfig.gameEvents != null)
            {
                gameConfig.gameEvents.onCheckDestroy.RegisterListener(this);
            }
        }

        /// <summary>
        /// BoardBlockObject ���� �̺�Ʈ ���
        /// </summary>
        private void RegisterBoardBlockEvents()
        {
            // �ı� üũ �̺�Ʈ �ڵ鷯 ���
            BoardBlockObject.OnCheckDestroy += CheckCanDestroy;

            // ��ƼŬ �������� �̺�Ʈ �ڵ鷯
            BoardBlockObject.OnGetDestroyParticle += GetDestroyParticle;

            // ���� �������� �̺�Ʈ �ڵ鷯
            BoardBlockObject.OnGetMaterial += GetMaterial;

            // ���� ũ�� �������� �̺�Ʈ �ڵ鷯
            BoardBlockObject.OnGetBoardSize += GetBoardSize;
        }

        /// <summary>
        /// �̺�Ʈ ����
        /// </summary>
        private void OnDestroy()
        {
            if (gameConfig != null && gameConfig.gameEvents != null)
            {
                gameConfig.gameEvents.onCheckDestroy.UnregisterListener(this);
            }

            // ���� �̺�Ʈ �ڵ鷯 ����
            BoardBlockObject.OnCheckDestroy -= CheckCanDestroy;
            BoardBlockObject.OnGetDestroyParticle -= GetDestroyParticle;
            BoardBlockObject.OnGetMaterial -= GetMaterial;
            BoardBlockObject.OnGetBoardSize -= GetBoardSize;
        }

        /// <summary>
        /// üũ ��Ʈ���� �̺�Ʈ ó��
        /// </summary>
        public void OnEventRaised((BoardBlockObject, BlockObject) data)
        {
            var (boardBlock, block) = data;
            bool canDestroy = CheckCanDestroy(boardBlock, block);

            // �ı� �����ϸ� ��� �ı� �̺�Ʈ �߻�
            if (canDestroy && block != null)
            {
                gameConfig.gameEvents.onBlockDestroy.Raise(block);
            }
        }

        /// <summary>
        /// ǥ�� ��� ���� ����
        /// </summary>
        public void SetStandardBlockData(Dictionary<(int, bool), BoardBlockObject> standardBlocks)
        {
            this.standardBlockDic = standardBlocks;

            // ǥ�� ��ϰ� üũ �׷� ����
            SetupStandardBlocks();
            CreateCheckBlockGroups();
        }

        /// <summary>
        /// ǥ�� ��� ����
        /// </summary>
        private void SetupStandardBlocks()
        {
            foreach (var kv in standardBlockDic)
            {
                BoardBlockObject boardBlockObject = kv.Value;
                for (int i = 0; i < boardBlockObject.colorType.Count; i++)
                {
                    if (kv.Key.Item2) // ���� ����
                    {
                        for (int j = boardBlockObject.x + 1; j < boardBlockObject.x + boardBlockObject.len[i]; j++)
                        {
                            if (boardBlockDic.TryGetValue((j, boardBlockObject.y), out BoardBlockObject targetBlock))
                            {
                                targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                                targetBlock.len.Add(boardBlockObject.len[i]);
                                targetBlock.isHorizon.Add(kv.Key.Item2);
                                targetBlock.isCheckBlock = true;
                            }
                        }
                    }
                    else // ���� ����
                    {
                        for (int k = boardBlockObject.y + 1; k < boardBlockObject.y + boardBlockObject.len[i]; k++)
                        {
                            if (boardBlockDic.TryGetValue((boardBlockObject.x, k), out BoardBlockObject targetBlock))
                            {
                                targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                                targetBlock.len.Add(boardBlockObject.len[i]);
                                targetBlock.isHorizon.Add(kv.Key.Item2);
                                targetBlock.isCheckBlock = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// üũ ��� �׷� ����
        /// </summary>
        private void CreateCheckBlockGroups()
        {
            int checkBlockIndex = -1;
            checkBlockGroupDic.Clear();

            foreach (var blockPos in boardBlockDic.Keys)
            {
                BoardBlockObject boardBlock = boardBlockDic[blockPos];

                for (int j = 0; j < boardBlock.colorType.Count; j++)
                {
                    if (boardBlock.isCheckBlock && boardBlock.colorType[j] != ColorType.None)
                    {
                        // �� ����� �̹� �׷쿡 �����ִ��� Ȯ��
                        if (boardBlock.checkGroupIdx.Count <= j)
                        {
                            if (boardBlock.isHorizon[j])
                            {
                                // ���� ��� Ȯ��
                                (int x, int y) leftPos = (boardBlock.x - 1, boardBlock.y);
                                if (boardBlockDic.TryGetValue(leftPos, out BoardBlockObject leftBlock) &&
                                    j < leftBlock.colorType.Count &&
                                    leftBlock.colorType[j] == boardBlock.colorType[j] &&
                                    leftBlock.checkGroupIdx.Count > j)
                                {
                                    int grpIdx = leftBlock.checkGroupIdx[j];
                                    checkBlockGroupDic[grpIdx].Add(boardBlock);
                                    boardBlock.checkGroupIdx.Add(grpIdx);
                                }
                                else
                                {
                                    checkBlockIndex++;
                                    checkBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                                    checkBlockGroupDic[checkBlockIndex].Add(boardBlock);
                                    boardBlock.checkGroupIdx.Add(checkBlockIndex);
                                }
                            }
                            else
                            {
                                // ���� ��� Ȯ��
                                (int x, int y) upPos = (boardBlock.x, boardBlock.y - 1);
                                if (boardBlockDic.TryGetValue(upPos, out BoardBlockObject upBlock) &&
                                    j < upBlock.colorType.Count &&
                                    upBlock.colorType[j] == boardBlock.colorType[j] &&
                                    upBlock.checkGroupIdx.Count > j)
                                {
                                    int grpIdx = upBlock.checkGroupIdx[j];
                                    checkBlockGroupDic[grpIdx].Add(boardBlock);
                                    boardBlock.checkGroupIdx.Add(grpIdx);
                                }
                                else
                                {
                                    checkBlockIndex++;
                                    checkBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                                    checkBlockGroupDic[checkBlockIndex].Add(boardBlock);
                                    boardBlock.checkGroupIdx.Add(checkBlockIndex);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �ı� ��ƼŬ ��ȯ (�̺�Ʈ �ڵ鷯)
        /// </summary>
        private ParticleSystem GetDestroyParticle()
        {
            if (gameConfig != null && gameConfig.visualConfig != null)
            {
                return gameConfig.visualConfig.destroyParticlePrefab;
            }
            return null;
        }

        /// <summary>
        /// ���� ��ȯ (�̺�Ʈ �ڵ鷯)
        /// </summary>
        private Material GetMaterial(int index)
        {
            if (gameConfig != null && gameConfig.wallConfig != null)
            {
                if (index >= 0 && index < gameConfig.wallConfig.wallMaterials.Length)
                {
                    return gameConfig.wallConfig.wallMaterials[index];
                }
            }
            return null;
        }

        /// <summary>
        /// ���� ũ�� ��ȯ (�̺�Ʈ �ڵ鷯)
        /// </summary>
        private Vector2Int GetBoardSize()
        {
            return new Vector2Int(boardWidth, boardHeight);
        }

        /// <summary>
        /// ��� �ı� ���� ���� Ȯ�� (�̺�Ʈ �ڵ鷯 �� ���� ����)
        /// </summary>
        public bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
        {
            // üũ �׷� ��ȿ�� �˻�
            foreach (var checkGroupIdx in boardBlock.checkGroupIdx)
            {
                if (!boardBlock.isCheckBlock && !checkBlockGroupDic.ContainsKey(checkGroupIdx)) return false;
            }

            // �÷��̾� ��� ���� ���
            int pBlockminX = boardWidth;
            int pBlockmaxX = -1;
            int pBlockminY = boardHeight;
            int pBlockmaxY = -1;

            List<BlockObject> blocks = block.dragHandler.blocks;

            foreach (var playingBlock in blocks)
            {
                if (playingBlock.x <= pBlockminX) pBlockminX = (int)playingBlock.x;
                if (playingBlock.y <= pBlockminY) pBlockminY = (int)playingBlock.y;
                if (playingBlock.x >= pBlockmaxX) pBlockmaxX = (int)playingBlock.x;
                if (playingBlock.y >= pBlockmaxY) pBlockmaxY = (int)playingBlock.y;
            }

            List<BoardBlockObject> horizonBoardBlocks = new List<BoardBlockObject>();
            List<BoardBlockObject> verticalBoardBlocks = new List<BoardBlockObject>();

            foreach (var checkIndex in boardBlock.checkGroupIdx)
            {
                foreach (var boardBlockObj in checkBlockGroupDic[checkIndex])
                {
                    foreach (var horizon in boardBlockObj.isHorizon)
                    {
                        if (horizon) horizonBoardBlocks.Add(boardBlockObj);
                        else verticalBoardBlocks.Add(boardBlockObj);
                    }
                }
            }

            int matchingIndex = boardBlock.colorType.FindIndex(color => color == block.colorType);
            bool hor = boardBlock.isHorizon[matchingIndex];

            // Horizon
            if (hor)
            {
                int minX = boardWidth;
                int maxX = -1;
                foreach (var coordinate in horizonBoardBlocks)
                {
                    if (coordinate.x < minX) minX = (int)coordinate.x;
                    if (coordinate.x > maxX) maxX = (int)coordinate.x;
                }

                // ���� ��ǥ�� �������� ���θ� �Ǵ�
                if (pBlockminX < minX - gameConfig.boardConfig.blockDistance / 2 ||
                    pBlockmaxX > maxX + gameConfig.boardConfig.blockDistance / 2)
                {
                    return false;
                }

                (int, int)[] blockCheckCoors = new (int, int)[horizonBoardBlocks.Count];

                for (int i = 0; i < horizonBoardBlocks.Count; i++)
                {
                    if (horizonBoardBlocks[i].y <= boardHeight / 2)
                    {
                        int maxY = -1;

                        for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                        {
                            var currentBlock = block.dragHandler.blocks[k];

                            if (currentBlock.y == horizonBoardBlocks[i].y)
                            {
                                if (currentBlock.y > maxY)
                                {
                                    maxY = (int)currentBlock.y;
                                }
                            }
                        }

                        blockCheckCoors[i] = ((int)horizonBoardBlocks[i].x, maxY);

                        for (int l = blockCheckCoors[i].Item2; l <= horizonBoardBlocks[i].y; l++)
                        {
                            if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                                continue;

                            (int, int) key = (blockCheckCoors[i].Item1, l);

                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                            {
                                return false;
                            }
                        }
                    }
                    else // up to downside
                    {
                        int minY = 100;

                        for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                        {
                            var currentBlock = block.dragHandler.blocks[k];

                            if (currentBlock.y == horizonBoardBlocks[i].y)
                            {
                                if (currentBlock.y < minY)
                                {
                                    minY = (int)currentBlock.y;
                                }
                            }
                        }

                        blockCheckCoors[i] = ((int)horizonBoardBlocks[i].x, minY);

                        for (int l = blockCheckCoors[i].Item2; l >= horizonBoardBlocks[i].y; l--)
                        {
                            if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                                continue;
                            (int, int) key = (blockCheckCoors[i].Item1, l);

                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            // Vertical
            else
            {
                int minY = boardHeight;
                int maxY = -1;

                foreach (var coordinate in verticalBoardBlocks)
                {
                    if (coordinate.y < minY) minY = (int)coordinate.y;
                    if (coordinate.y > maxY) maxY = (int)coordinate.y;
                }

                if (pBlockminY < minY - gameConfig.boardConfig.blockDistance / 2 ||
                    pBlockmaxY > maxY + gameConfig.boardConfig.blockDistance / 2)
                {
                    return false;
                }

                (int, int)[] blockCheckCoors = new (int, int)[verticalBoardBlocks.Count];

                for (int i = 0; i < verticalBoardBlocks.Count; i++)
                {
                    //x exist in left
                    if (verticalBoardBlocks[i].x <= boardWidth / 2)
                    {
                        int maxX = int.MinValue;

                        for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                        {
                            var currentBlock = block.dragHandler.blocks[k];

                            if (currentBlock.y == verticalBoardBlocks[i].y)
                            {
                                if (currentBlock.x > maxX)
                                {
                                    maxX = (int)currentBlock.x;
                                }
                            }
                        }

                        // Ʃ�ÿ� y�� maxX�� ����
                        blockCheckCoors[i] = (maxX, (int)verticalBoardBlocks[i].y);

                        for (int l = blockCheckCoors[i].Item1; l >= verticalBoardBlocks[i].x; l--)
                        {
                            if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                                continue;
                            (int, int) key = (l, blockCheckCoors[i].Item2);

                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                            {
                                return false;
                            }
                        }
                    }
                    else // x exist in right
                    {
                        int minX = 100;

                        for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                        {
                            var currentBlock = block.dragHandler.blocks[k];

                            if (currentBlock.y == verticalBoardBlocks[i].y)
                            {
                                if (currentBlock.x < minX)
                                {
                                    minX = (int)currentBlock.x;
                                }
                            }
                        }

                        // Ʃ�ÿ� y�� minX�� ����
                        blockCheckCoors[i] = (minX, (int)verticalBoardBlocks[i].y);

                        for (int l = blockCheckCoors[i].Item1; l <= verticalBoardBlocks[i].x; l++)
                        {
                            if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                                continue;
                            (int, int) key = (l, blockCheckCoors[i].Item2);

                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// ��� ���� �ı� �޼��� (�ʿ�� �ܺο��� ȣ�� ����)
        /// </summary>
        public void DestroyBlockWithEffect(
    BlockObject block,
    Vector3 movePosition,     // �̵� ��ǥ ��ġ
    Vector3 effectPosition,   // �̹� ���� ��ƼŬ ��ġ
    LaunchDirection direction, // ���� (���� ����)
    ColorType colorType,      // ����
    Quaternion rotation)      // ȸ��
        {
            if (block == null || block.dragHandler == null) return;

            // VisualEffectManager ��������
            VisualEffectManager visualEffectManager = StageController.Instance.GetVisualEffectManager();

            // ��� �ı� �̺�Ʈ �߻�
            gameConfig.gameEvents.onBlockDestroy.Raise(block);

            // ��� �ı� ���� ����
            block.dragHandler.ReleaseInput();

            foreach (var blockObject in block.dragHandler.blocks)
            {
                if (blockObject.preBoardBlockObject != null)
                {
                    blockObject.preBoardBlockObject.playingBlock = null;
                }
                blockObject.ColliderOff();
            }

            block.dragHandler.enabled = false;
            GameObject blockGroup = block.transform.parent.gameObject; // ����ġ�� ����
            // ���ؽ� ���ٽ� ȿ�� ���� (���� �߰�)
            if (visualEffectManager != null)
            {
                visualEffectManager.ApplyWallClippingToBlock(blockGroup, effectPosition, (global::LaunchDirection)direction);
            }
            // ��� ���� ��� (�ʿ��� ����)
            int blockLength = (direction == LaunchDirection.Up || direction == LaunchDirection.Down)
                        ? block.dragHandler.horizon    // ���� ���� �߻�� ���� ���� ���
                        : block.dragHandler.vertical;  // ���� ���� �߻�� ���� ���� ���

            // ��ƼŬ ���� �� �̵� �ִϸ��̼�
            if (visualEffectManager != null)
            {
                ParticleSystem particle = visualEffectManager.CreateParticleEffect(
                    effectPosition,    // �̹� ���� ��ġ 
                    rotation,          // �̹� ���� ȸ��
                    colorType,         // ����
                    blockLength        // ����
                );

                // ��� �̵� �ִϸ��̼� ����
                block.dragHandler.DestroyMove(movePosition, particle);
            }
            else
            {
                // VisualEffectManager�� ���� ��� �⺻ �̵� �ִϸ��̼� ����
                block.dragHandler.DestroyMove(movePosition, null);
            }
        }
       
        /// <summary>
        /// ���ο� �������� �ε��� ���� ����
        /// </summary>
        public void Reset()
        {
            checkBlockGroupDic.Clear();
            standardBlockDic.Clear();
        }
    }
}