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
    /// üũ ��� �׷� ���� �� �ı� ���� ���� Ȯ���� ����ϴ� �Ŵ��� Ŭ����
    /// </summary>
    public class CheckBlockGroupManager : MonoBehaviour
    {
        // �̱��� ���� ����
        public static CheckBlockGroupManager Instance { get; private set; }

        // ���� ������ ����
        private Dictionary<(int x, int y), BoardBlockObject> boardBlockDic;
        private Dictionary<int, List<BoardBlockObject>> checkBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();

        private Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();

        // ���� ũ�� ����
        private int boardWidth;
        private int boardHeight;

        // ���� ���� ����
        private GameConfig gameConfig;

        // �������� ������ ���� �߰�
        private StageData currentStageData;

        private void Awake()
        {
            // �̱��� ����
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        /// <summary>
        /// �ʱ�ȭ �޼���
        /// </summary>
        public void Initialize(GameConfig config, Dictionary<(int x, int y), BoardBlockObject> boardBlocks, int width, int height, StageData stageData)
        {
            this.gameConfig = config;
            this.boardBlockDic = boardBlocks;
            this.boardWidth = width;
            this.boardHeight = height;
            this.currentStageData = stageData;

            // ��� �׷� �ʱ�ȭ
            checkBlockGroupDic.Clear();
            standardBlockDic.Clear();

            // BoardBlockObject �̺�Ʈ �ڵ鷯 ���
            RegisterBoardBlockEvents();
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
            // ���� �̺�Ʈ �ڵ鷯 ����
            BoardBlockObject.OnCheckDestroy -= CheckCanDestroy;
            BoardBlockObject.OnGetDestroyParticle -= GetDestroyParticle;
            BoardBlockObject.OnGetMaterial -= GetMaterial;
            BoardBlockObject.OnGetBoardSize -= GetBoardSize;
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
            Debug.Log(new Vector2Int(boardWidth, boardHeight));
            return new Vector2Int(boardWidth, boardHeight);
        }

        /// <summary>
        /// ��� �ı� ���� ���� Ȯ�� (�̺�Ʈ �ڵ鷯 �� ���� ����)
        /// </summary>
        public bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
        {
            // ��ȿ�� �˻� - �׷� �ε��� Ȯ��
            foreach (var checkGroupIdx in boardBlock.checkGroupIdx)
            {
                if (!boardBlock.isCheckBlock && !checkBlockGroupDic.ContainsKey(checkGroupIdx))
                {
                    Debug.Log($"[DestroyManager] Group index {checkGroupIdx} not found in dictionary");
                    return false;
                }
            }

            // ����� ��� ���
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

            // ���⺰ üũ ��� �з�
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

            // ��� ���� �� ���� ��ġ Ȯ��
            float blockDistance = gameConfig.boardConfig.blockDistance;
            int matchingIndex = boardBlock.colorType.FindIndex(color => color == block.colorType);

            // ������ ��ġ���� ������ �ı� �Ұ�
            if (matchingIndex == -1)
            {
                return false;
            }

            bool hor = boardBlock.isHorizon[matchingIndex];

            // ���� ���� üũ
            if (hor)
            {
                // üũ ����� x ��� ���
                int minX = boardWidth;
                int maxX = -1;
                foreach (var coordinate in horizonBoardBlocks)
                {
                    if (coordinate.x < minX) minX = (int)coordinate.x;
                    if (coordinate.x > maxX) maxX = (int)coordinate.x;
                }

                // ����� ��踦 ����� �ı� �Ұ�
                if (pBlockminX < minX - blockDistance / 2 || pBlockmaxX > maxX + blockDistance / 2)
                {
                    return false;
                }

                // �� üũ ��ġ�� �˻�
                (int, int)[] blockCheckCoors = new (int, int)[horizonBoardBlocks.Count];

                for (int i = 0; i < horizonBoardBlocks.Count; i++)
                {
                    // ���� �߽��� ����
                    if (horizonBoardBlocks[i].y <= boardHeight / 2)
                    {
                        int maxY = -1;

                        // �÷��̾� ��� �� üũ ��ϰ� ���� y��ǥ�� ���� ��� ã��
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

                        // ��� ���� ��� ��� Ȯ��
                        for (int l = blockCheckCoors[i].Item2; l <= horizonBoardBlocks[i].y; l++)
                        {
                            if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                                continue;

                            (int, int) key = (blockCheckCoors[i].Item1, l);

                            // ��λ� �ٸ� ������ ����� ������ �ı� �Ұ�
                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                            {
                                return false;
                            }
                        }
                    }
                    // ���� �߽��� �Ʒ���
                    else
                    {
                        int minY = 100;

                        // �÷��̾� ��� �� üũ ��ϰ� ���� y��ǥ�� ���� ��� ã��
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

                        // ��� ���� ��� ��� Ȯ��
                        for (int l = blockCheckCoors[i].Item2; l >= horizonBoardBlocks[i].y; l--)
                        {
                            if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                                continue;
                            (int, int) key = (blockCheckCoors[i].Item1, l);

                            // ��λ� �ٸ� ������ ����� ������ �ı� �Ұ�
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
            // ���� ���� üũ
            else
            {
                // üũ ����� y ��� ���
                int minY = boardHeight;
                int maxY = -1;

                foreach (var coordinate in verticalBoardBlocks)
                {
                    if (coordinate.y < minY) minY = (int)coordinate.y;
                    if (coordinate.y > maxY) maxY = (int)coordinate.y;

                    // ����� ��踦 ����� �ı� �Ұ�
                    if (pBlockminY < minY - blockDistance / 2 || pBlockmaxY > maxY + blockDistance / 2)
                    {
                        return false;
                    }

                    // �� üũ ��ġ�� �˻�
                    (int, int)[] blockCheckCoors = new (int, int)[verticalBoardBlocks.Count];

                    for (int i = 0; i < verticalBoardBlocks.Count; i++)
                    {
                        // ���� �߽��� ����
                        if (verticalBoardBlocks[i].x <= boardWidth / 2)
                        {
                            int maxX = int.MinValue;

                            // �÷��̾� ��� �� üũ ��ϰ� ���� y��ǥ�� ���� ��� ã��
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

                            // ��� ���� ��� ��� Ȯ��
                            for (int l = blockCheckCoors[i].Item1; l >= verticalBoardBlocks[i].x; l--)
                            {
                                if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                                {
                                    continue;
                                }

                                (int, int) key = (l, blockCheckCoors[i].Item2);

                                // ��λ� �ٸ� ������ ����� ������ �ı� �Ұ�
                                if (boardBlockDic.ContainsKey(key) &&
                                    boardBlockDic[key].playingBlock != null &&
                                    boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                                {
                                    return false;
                                }
                            }
                        }
                        // ���� �߽��� ������
                        else
                        {
                            int minX = 100;

                            // �÷��̾� ��� �� üũ ��ϰ� ���� y��ǥ�� ���� ��� ã��
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

                            // ��� ���� ��� ��� Ȯ��
                            for (int l = blockCheckCoors[i].Item1; l <= verticalBoardBlocks[i].x; l++)
                            {
                                if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                                {
                                    continue;
                                }

                                (int, int) key = (l, blockCheckCoors[i].Item2);

                                // ��λ� �ٸ� ������ ����� ������ �ı� �Ұ�
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
            }

            return true;
        }

        /// <summary>
        /// ���ο� �������� �ε带 ���� ����
        /// </summary>
        public void Reset()
        {
            checkBlockGroupDic.Clear();
            standardBlockDic.Clear();
        }
    }
}