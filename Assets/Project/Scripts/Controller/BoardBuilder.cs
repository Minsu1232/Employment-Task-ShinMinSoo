using Project.Scripts.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Config;
using Project.Scripts.Events;
using static Project.Scripts.Model.BoardBlockData;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ���� ���� ���� �� ������ ����ϴ� ���� Ŭ����
    /// </summary>
    public class BoardBuilder : MonoBehaviour
    {
        private GameConfig gameConfig;

        // ���� ������
        public Dictionary<(int x, int y), BoardBlockObject> boardBlockDic { get; private set; }
        private Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();

        /// <summary>
        /// GameConfig�� ���� �ʱ�ȭ
        /// </summary>
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;

            // ���� ������ �ʱ�ȭ
            boardBlockDic = new Dictionary<(int x, int y), BoardBlockObject>();
            standardBlockDic.Clear();
        }

        /// <summary>
        /// ���ҽ� ����
        /// </summary>
        private void OnDestroy()
        {
            boardBlockDic?.Clear();
            standardBlockDic?.Clear();
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        public async Task<(Dictionary<(int x, int y), BoardBlockObject> boardBlockDic, int boardWidth, int boardHeight, Dictionary<(int, bool), BoardBlockObject> standardBlocks)> CreateBoardAsync(
           int stageIdx,
           Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> wallCoorInfoDic,
           GameObject boardParent)
        {
            int standardBlockIndex = -1;

            // ���� ��� ����
            for (int i = 0; i < gameConfig.stageDatas[stageIdx].boardBlocks.Count; i++)
            {
                BoardBlockData data = gameConfig.stageDatas[stageIdx].boardBlocks[i];

                GameObject blockObj = Instantiate(gameConfig.boardConfig.boardBlockPrefab, boardParent.transform);
                blockObj.transform.localPosition = new Vector3(
                    data.x * gameConfig.boardConfig.blockDistance,
                    0,
                    data.y * gameConfig.boardConfig.blockDistance
                );

                if (blockObj.TryGetComponent(out BoardBlockObject boardBlock))
                {
                    // ���� ���� ������Ʈ ����
                    var configInjector = blockObj.GetComponent<ConfigInjector>() ?? blockObj.AddComponent<ConfigInjector>();
                    configInjector.SetGameConfig(gameConfig);

                    // ���� ��� �Ӽ� ����
                    boardBlock.x = data.x;
                    boardBlock.y = data.y;

                    // ���� �´� ���׸��� ����
                    if (data.ColorType != ColorType.None)
                    {
                        Material blockMaterial = GetBoardBlockMaterial((int)data.ColorType);
                        if (blockMaterial != null)
                        {
                            boardBlock.SetMaterial(blockMaterial, data.ColorType != ColorType.None);
                        }
                    }

                    if (wallCoorInfoDic.ContainsKey((boardBlock.x, boardBlock.y)))
                    {
                        for (int k = 0; k < wallCoorInfoDic[(boardBlock.x, boardBlock.y)].Count; k++)
                        {
                            boardBlock.colorType.Add(wallCoorInfoDic[(boardBlock.x, boardBlock.y)].Keys.ElementAt(k).Item2);
                            boardBlock.len.Add(wallCoorInfoDic[(boardBlock.x, boardBlock.y)].Values.ElementAt(k));

                            DestroyWallDirection dir = wallCoorInfoDic[(boardBlock.x, boardBlock.y)].Keys.ElementAt(k).Item1;
                            bool horizon = dir == DestroyWallDirection.Up || dir == DestroyWallDirection.Down;
                            boardBlock.isHorizon.Add(horizon);

                            standardBlockDic.Add((++standardBlockIndex, horizon), boardBlock);
                        }
                        boardBlock.isCheckBlock = true;
                    }
                    else
                    {
                        boardBlock.isCheckBlock = false;
                    }

                    boardBlockDic.Add((data.x, data.y), boardBlock);
                }
            }
            await Task.Yield();

            // ���� ũ�� ���
            int boardWidth = boardBlockDic.Keys.Max(k => k.x);
            int boardHeight = boardBlockDic.Keys.Max(k => k.y);

            // ���� ���� ��ȯ
            return (boardBlockDic, boardWidth, boardHeight, standardBlockDic);
        }
        /// <summary>
        /// ���� ��� ���׸��� ��ȯ
        /// </summary>
        public Material GetBoardBlockMaterial(int colorTypeIndex)
        {
            if (gameConfig != null &&
                gameConfig.boardConfig != null &&
                gameConfig.wallConfig.wallMaterials != null &&
                colorTypeIndex >= 0 &&
                colorTypeIndex < gameConfig.wallConfig.wallMaterials.Length)
            {
                return gameConfig.wallConfig.wallMaterials[colorTypeIndex];
            }
            return null;
        }
        /// <summary>
        /// ������ ��ϵ��� �Ӽ� ����
        /// </summary>
        private void SetupAdjacentBlocks()
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
        /// ���� �ʱ�ȭ (���ο� �������� ��ȯ ��)
        /// </summary>
        public void Reset()
        {
            boardBlockDic?.Clear();
            standardBlockDic?.Clear();
        }
    }

    /// <summary>
    /// GameConfig ������ ������Ʈ�� �����ϱ� ���� ���� Ŭ����
    /// </summary>
    public class ConfigInjector : MonoBehaviour
    {
        private GameConfig gameConfig;

        public void SetGameConfig(GameConfig config)
        {
            this.gameConfig = config;
        }

        public GameConfig GetGameConfig()
        {
            return gameConfig;
        }
    }
}