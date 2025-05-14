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
    /// 체크 블록 그룹 관리 및 파괴 가능 여부 확인을 담당하는 매니저 클래스
    /// </summary>
    public class CheckBlockGroupManager : MonoBehaviour
    {
        // 싱글톤 패턴 구현
        public static CheckBlockGroupManager Instance { get; private set; }

        // 보드 데이터 참조
        private Dictionary<(int x, int y), BoardBlockObject> boardBlockDic;
        private Dictionary<int, List<BoardBlockObject>> checkBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();

        private Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();

        // 보드 크기 정보
        private int boardWidth;
        private int boardHeight;

        // 게임 설정 참조
        private GameConfig gameConfig;

        // 스테이지 데이터 참조 추가
        private StageData currentStageData;

        private void Awake()
        {
            // 싱글톤 설정
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        /// <summary>
        /// 초기화 메서드
        /// </summary>
        public void Initialize(GameConfig config, Dictionary<(int x, int y), BoardBlockObject> boardBlocks, int width, int height, StageData stageData)
        {
            this.gameConfig = config;
            this.boardBlockDic = boardBlocks;
            this.boardWidth = width;
            this.boardHeight = height;
            this.currentStageData = stageData;

            // 블록 그룹 초기화
            checkBlockGroupDic.Clear();
            standardBlockDic.Clear();

            // BoardBlockObject 이벤트 핸들러 등록
            RegisterBoardBlockEvents();
        }

        /// <summary>
        /// BoardBlockObject 정적 이벤트 등록
        /// </summary>
        private void RegisterBoardBlockEvents()
        {
            // 파괴 체크 이벤트 핸들러 등록
            BoardBlockObject.OnCheckDestroy += CheckCanDestroy;

            // 파티클 가져오기 이벤트 핸들러
            BoardBlockObject.OnGetDestroyParticle += GetDestroyParticle;

            // 재질 가져오기 이벤트 핸들러
            BoardBlockObject.OnGetMaterial += GetMaterial;

            // 보드 크기 가져오기 이벤트 핸들러
            BoardBlockObject.OnGetBoardSize += GetBoardSize;
        }

        /// <summary>
        /// 이벤트 해제
        /// </summary>
        private void OnDestroy()
        {
            // 정적 이벤트 핸들러 해제
            BoardBlockObject.OnCheckDestroy -= CheckCanDestroy;
            BoardBlockObject.OnGetDestroyParticle -= GetDestroyParticle;
            BoardBlockObject.OnGetMaterial -= GetMaterial;
            BoardBlockObject.OnGetBoardSize -= GetBoardSize;
        }

        /// <summary>
        /// 표준 블록 정보 설정
        /// </summary>
        public void SetStandardBlockData(Dictionary<(int, bool), BoardBlockObject> standardBlocks)
        {
            this.standardBlockDic = standardBlocks;

            // 표준 블록과 체크 그룹 설정
            SetupStandardBlocks();
            CreateCheckBlockGroups();
        }

        /// <summary>
        /// 표준 블록 설정
        /// </summary>
        private void SetupStandardBlocks()
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
        /// 체크 블록 그룹 생성
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
                        // 이 블록이 이미 그룹에 속해있는지 확인
                        if (boardBlock.checkGroupIdx.Count <= j)
                        {
                            if (boardBlock.isHorizon[j])
                            {
                                // 왼쪽 블록 확인
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
                                // 위쪽 블록 확인
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
        /// 파괴 파티클 반환 (이벤트 핸들러)
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
        /// 재질 반환 (이벤트 핸들러)
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
        /// 보드 크기 반환 (이벤트 핸들러)
        /// </summary>
        private Vector2Int GetBoardSize()
        {
            Debug.Log(new Vector2Int(boardWidth, boardHeight));
            return new Vector2Int(boardWidth, boardHeight);
        }

        /// <summary>
        /// 블록 파괴 가능 여부 확인 (이벤트 핸들러 및 내부 로직)
        /// </summary>
        public bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
        {
            // 유효성 검사 - 그룹 인덱스 확인
            foreach (var checkGroupIdx in boardBlock.checkGroupIdx)
            {
                if (!boardBlock.isCheckBlock && !checkBlockGroupDic.ContainsKey(checkGroupIdx))
                {
                    Debug.Log($"[DestroyManager] Group index {checkGroupIdx} not found in dictionary");
                    return false;
                }
            }

            // 블록의 경계 계산
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

            // 방향별 체크 블록 분류
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

            // 블록 간격 및 색상 일치 확인
            float blockDistance = gameConfig.boardConfig.blockDistance;
            int matchingIndex = boardBlock.colorType.FindIndex(color => color == block.colorType);

            // 색상이 일치하지 않으면 파괴 불가
            if (matchingIndex == -1)
            {
                return false;
            }

            bool hor = boardBlock.isHorizon[matchingIndex];

            // 가로 방향 체크
            if (hor)
            {
                // 체크 블록의 x 경계 계산
                int minX = boardWidth;
                int maxX = -1;
                foreach (var coordinate in horizonBoardBlocks)
                {
                    if (coordinate.x < minX) minX = (int)coordinate.x;
                    if (coordinate.x > maxX) maxX = (int)coordinate.x;
                }

                // 블록이 경계를 벗어나면 파괴 불가
                if (pBlockminX < minX - blockDistance / 2 || pBlockmaxX > maxX + blockDistance / 2)
                {
                    return false;
                }

                // 각 체크 위치별 검사
                (int, int)[] blockCheckCoors = new (int, int)[horizonBoardBlocks.Count];

                for (int i = 0; i < horizonBoardBlocks.Count; i++)
                {
                    // 보드 중심의 위쪽
                    if (horizonBoardBlocks[i].y <= boardHeight / 2)
                    {
                        int maxY = -1;

                        // 플레이어 블록 중 체크 블록과 같은 y좌표를 가진 블록 찾기
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

                        // 경로 상의 모든 블록 확인
                        for (int l = blockCheckCoors[i].Item2; l <= horizonBoardBlocks[i].y; l++)
                        {
                            if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                                continue;

                            (int, int) key = (blockCheckCoors[i].Item1, l);

                            // 경로상 다른 색상의 블록이 있으면 파괴 불가
                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                            {
                                return false;
                            }
                        }
                    }
                    // 보드 중심의 아래쪽
                    else
                    {
                        int minY = 100;

                        // 플레이어 블록 중 체크 블록과 같은 y좌표를 가진 블록 찾기
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

                        // 경로 상의 모든 블록 확인
                        for (int l = blockCheckCoors[i].Item2; l >= horizonBoardBlocks[i].y; l--)
                        {
                            if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                                continue;
                            (int, int) key = (blockCheckCoors[i].Item1, l);

                            // 경로상 다른 색상의 블록이 있으면 파괴 불가
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
            // 세로 방향 체크
            else
            {
                // 체크 블록의 y 경계 계산
                int minY = boardHeight;
                int maxY = -1;

                foreach (var coordinate in verticalBoardBlocks)
                {
                    if (coordinate.y < minY) minY = (int)coordinate.y;
                    if (coordinate.y > maxY) maxY = (int)coordinate.y;

                    // 블록이 경계를 벗어나면 파괴 불가
                    if (pBlockminY < minY - blockDistance / 2 || pBlockmaxY > maxY + blockDistance / 2)
                    {
                        return false;
                    }

                    // 각 체크 위치별 검사
                    (int, int)[] blockCheckCoors = new (int, int)[verticalBoardBlocks.Count];

                    for (int i = 0; i < verticalBoardBlocks.Count; i++)
                    {
                        // 보드 중심의 왼쪽
                        if (verticalBoardBlocks[i].x <= boardWidth / 2)
                        {
                            int maxX = int.MinValue;

                            // 플레이어 블록 중 체크 블록과 같은 y좌표를 가진 블록 찾기
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

                            // 튜플에 y와 maxX를 저장
                            blockCheckCoors[i] = (maxX, (int)verticalBoardBlocks[i].y);

                            // 경로 상의 모든 블록 확인
                            for (int l = blockCheckCoors[i].Item1; l >= verticalBoardBlocks[i].x; l--)
                            {
                                if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                                {
                                    continue;
                                }

                                (int, int) key = (l, blockCheckCoors[i].Item2);

                                // 경로상 다른 색상의 블록이 있으면 파괴 불가
                                if (boardBlockDic.ContainsKey(key) &&
                                    boardBlockDic[key].playingBlock != null &&
                                    boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                                {
                                    return false;
                                }
                            }
                        }
                        // 보드 중심의 오른쪽
                        else
                        {
                            int minX = 100;

                            // 플레이어 블록 중 체크 블록과 같은 y좌표를 가진 블록 찾기
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

                            // 튜플에 y와 minX를 저장
                            blockCheckCoors[i] = (minX, (int)verticalBoardBlocks[i].y);

                            // 경로 상의 모든 블록 확인
                            for (int l = blockCheckCoors[i].Item1; l <= verticalBoardBlocks[i].x; l++)
                            {
                                if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                                {
                                    continue;
                                }

                                (int, int) key = (l, blockCheckCoors[i].Item2);

                                // 경로상 다른 색상의 블록이 있으면 파괴 불가
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
        /// 새로운 스테이지 로드를 위한 리셋
        /// </summary>
        public void Reset()
        {
            checkBlockGroupDic.Clear();
            standardBlockDic.Clear();
        }
    }
}