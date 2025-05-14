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
    /// 게임 보드 생성 및 관리를 담당하는 빌더 클래스
    /// </summary>
    public class BoardBuilder : MonoBehaviour
    {
        private GameConfig gameConfig;

        // 보드 데이터
        public Dictionary<(int x, int y), BoardBlockObject> boardBlockDic { get; private set; }
        private Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();

        /// <summary>
        /// GameConfig를 통한 초기화
        /// </summary>
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;

            // 보드 데이터 초기화
            boardBlockDic = new Dictionary<(int x, int y), BoardBlockObject>();
            standardBlockDic.Clear();
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        private void OnDestroy()
        {
            boardBlockDic?.Clear();
            standardBlockDic?.Clear();
        }

        /// <summary>
        /// 보드 생성
        /// </summary>
        public async Task<(Dictionary<(int x, int y), BoardBlockObject> boardBlockDic, int boardWidth, int boardHeight, Dictionary<(int, bool), BoardBlockObject> standardBlocks)> CreateBoardAsync(
           int stageIdx,
           Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> wallCoorInfoDic,
           GameObject boardParent)
        {
            int standardBlockIndex = -1;

            // 보드 블록 생성
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
                    // 게임 설정 컴포넌트 전달
                    var configInjector = blockObj.GetComponent<ConfigInjector>() ?? blockObj.AddComponent<ConfigInjector>();
                    configInjector.SetGameConfig(gameConfig);

                    // 보드 블록 속성 설정
                    boardBlock.x = data.x;
                    boardBlock.y = data.y;

                    // 색상에 맞는 메테리얼 적용
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

            // 보드 크기 계산
            int boardWidth = boardBlockDic.Keys.Max(k => k.x);
            int boardHeight = boardBlockDic.Keys.Max(k => k.y);

            // 보드 정보 반환
            return (boardBlockDic, boardWidth, boardHeight, standardBlockDic);
        }
        /// <summary>
        /// 보드 블록 메테리얼 반환
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
        /// 인접한 블록들의 속성 설정
        /// </summary>
        private void SetupAdjacentBlocks()
        {
            foreach (var kv in standardBlockDic)
            {
                BoardBlockObject boardBlockObject = kv.Value;
                for (int i = 0; i < boardBlockObject.colorType.Count; i++)
                {
                    if (kv.Key.Item2) // 가로 방향
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
                    else // 세로 방향
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
        /// 보드 초기화 (새로운 스테이지 전환 시)
        /// </summary>
        public void Reset()
        {
            boardBlockDic?.Clear();
            standardBlockDic?.Clear();
        }
    }

    /// <summary>
    /// GameConfig 참조를 컴포넌트에 주입하기 위한 헬퍼 클래스
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