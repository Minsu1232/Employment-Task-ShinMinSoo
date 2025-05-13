using Project.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Config;
using Project.Scripts.Events;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// �÷��̾� ��� ������ ����ϴ� ���丮 Ŭ����
    /// </summary>
    public class BlockFactory : MonoBehaviour
    {
        private GameConfig gameConfig;

        /// <summary>
        /// GameConfig�� ���� �ʱ�ȭ
        /// </summary>
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;
        }

        /// <summary>
        /// �÷��̾� ��� ����
        /// </summary>
        public async Task CreatePlayingBlocksAsync(
            int stageIdx,
            Dictionary<(int x, int y), BoardBlockObject> boardBlockDic,
            int boardWidth,
            int boardHeight,
            GameObject playingBlockParent)
        {
            for (int i = 0; i < gameConfig.stageDatas[stageIdx].playingBlocks.Count; i++)
            {
                var pbData = gameConfig.stageDatas[stageIdx].playingBlocks[i];

                CreateBlockGroup(pbData, boardBlockDic, boardWidth, boardHeight, playingBlockParent);
            }

            await Task.Yield();
        }

        /// <summary>
        /// ��� �׷� ����
        /// </summary>
        private void CreateBlockGroup(
            PlayingBlockData pbData,
            Dictionary<(int x, int y), BoardBlockObject> boardBlockDic,
            int boardWidth,
            int boardHeight,
            GameObject playingBlockParent)
        {
            float blockDistance = gameConfig.boardConfig.blockDistance;

            GameObject blockGroupObject = Instantiate(gameConfig.blockConfig.blockGroupPrefab, playingBlockParent.transform);
            blockGroupObject.transform.position = new Vector3(
                pbData.center.x * blockDistance,
                0.33f,
                pbData.center.y * blockDistance
            );

            // ���� ���� ���� ����
            var configInjector = blockGroupObject.GetComponent<ConfigInjector>() ?? blockGroupObject.AddComponent<ConfigInjector>();
            configInjector.SetGameConfig(gameConfig);

            // �ʿ��� �ڵ鷯 ������Ʈ �߰�
            BlockDragHandler dragHandler = blockGroupObject.GetComponent<BlockDragHandler>();
            if (dragHandler == null)
            {
                dragHandler = blockGroupObject.AddComponent<BlockDragHandler>();
            }

            // �⺻ �Է�, ����, �׸��� �ڵ鷯 �߰�
            BlockInputHandler inputHandler = blockGroupObject.GetComponent<BlockInputHandler>() ?? blockGroupObject.AddComponent<BlockInputHandler>();
            BlockPhysicsHandler physicsHandler = blockGroupObject.GetComponent<BlockPhysicsHandler>() ?? blockGroupObject.AddComponent<BlockPhysicsHandler>();
            BlockGridHandler gridHandler = blockGroupObject.GetComponent<BlockGridHandler>() ?? blockGroupObject.AddComponent<BlockGridHandler>();

            if (dragHandler != null)
            {
                dragHandler.blocks = new List<BlockObject>();
                dragHandler.uniqueIndex = pbData.uniqueIndex;

                // ��� ����
                foreach (var gimmick in pbData.gimmicks)
                {
                    if (Enum.TryParse(gimmick.gimmickType, out ObjectPropertiesEnum.BlockGimmickType gimmickType))
                    {
                        dragHandler.gimmickType.Add(gimmickType);
                    }
                }
            }

            // ��� ���� ��� ����
            int maxX = 0;
            int minX = boardWidth;
            int maxY = 0;
            int minY = boardHeight;

            // ���� ��� ����
            foreach (var shape in pbData.shapes)
            {
                GameObject singleBlock = Instantiate(gameConfig.blockConfig.blockPrefab, blockGroupObject.transform);

                singleBlock.transform.localPosition = new Vector3(
                    shape.offset.x * blockDistance,
                    0f,
                    shape.offset.y * blockDistance
                );

                if (dragHandler != null)
                {
                    dragHandler.blockOffsets.Add(new Vector2(shape.offset.x, shape.offset.y));
                }

                // ��� ��Ƽ���� ����
                var renderer = singleBlock.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null && pbData.colorType >= 0)
                {
                    renderer.material = gameConfig.blockConfig.testBlockMaterials[(int)pbData.colorType];
                }

                // ��� ������Ʈ ����
                if (singleBlock.TryGetComponent(out BlockObject blockObj))
                {
                    blockObj.colorType = pbData.colorType;
                    blockObj.x = pbData.center.x + shape.offset.x;
                    blockObj.y = pbData.center.y + shape.offset.y;
                    blockObj.offsetToCenter = new Vector2(shape.offset.x, shape.offset.y);

                    if (dragHandler != null)
                    {
                        dragHandler.blocks.Add(blockObj);
                    }

                    // ���� ��� ����
                    if (boardBlockDic.TryGetValue(((int)blockObj.x, (int)blockObj.y), out BoardBlockObject boardBlock))
                    {
                        boardBlock.playingBlock = blockObj;
                        blockObj.preBoardBlockObject = boardBlock;
                    }

                    // ���� ������Ʈ
                    if (minX > blockObj.x) minX = (int)blockObj.x;
                    if (minY > blockObj.y) minY = (int)blockObj.y;
                    if (maxX < blockObj.x) maxX = (int)blockObj.x;
                    if (maxY < blockObj.y) maxY = (int)blockObj.y;
                }
            }

            // �巡�� �ڵ鷯 ũ�� ����
            if (dragHandler != null)
            {
                dragHandler.horizon = maxX - minX + 1;
                dragHandler.vertical = maxY - minY + 1;
            }
                    
        }
    }
}